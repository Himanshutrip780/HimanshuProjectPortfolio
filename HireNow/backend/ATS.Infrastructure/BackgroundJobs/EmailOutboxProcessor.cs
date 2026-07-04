using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Entities;

namespace ATS.Infrastructure.BackgroundJobs
{
    public class EmailOutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailOutboxProcessor> _logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _emailsFolder;

        public EmailOutboxProcessor(
            IServiceScopeFactory scopeFactory, 
            ILogger<EmailOutboxProcessor> logger,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
            _emailsFolder = Path.Combine(AppContext.BaseDirectory, "wwwroot", "sent_emails");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Outbox Processor background service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingEmailsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred processing email outbox.");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcessPendingEmailsAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var pendingEmails = await context.EmailOutboxes
                .Where(e => !e.IsSent && e.RetryCount < 3)
                .OrderBy(e => e.CreatedDate)
                .Take(20)
                .ToListAsync(stoppingToken);

            if (pendingEmails.Count == 0) return;

            _logger.LogInformation("Found {Count} pending emails in outbox to process.", pendingEmails.Count);

            foreach (var email in pendingEmails)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    await DeliverEmailAsync(context, email);
                    email.IsSent = true;
                    email.SentDate = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    email.RetryCount++;
                    email.ErrorMessage = ex.Message;
                    _logger.LogError(ex, "Failed to deliver outbox email ID {Id} to {Recipient}", email.Id, email.RecipientEmail);
                }

                await context.SaveChangesAsync(stoppingToken);
            }
        }

        private async Task DeliverEmailAsync(IApplicationDbContext context, EmailOutbox email)
        {
            Guid? companyId = null;
            var user = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email.RecipientEmail);
            if (user != null)
            {
                companyId = user.CompanyId;
            }
            else
            {
                var candidate = await context.Candidates.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Email == email.RecipientEmail);
                if (candidate != null)
                {
                    companyId = candidate.CompanyId;
                }
            }

            if (companyId.HasValue)
            {
                var smtpSettings = await context.TenantSmtpSettings
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.CompanyId == companyId.Value && s.Enabled);

                if (smtpSettings != null)
                {
                    var message = new MimeKit.MimeMessage();
                    message.From.Add(new MimeKit.MailboxAddress(smtpSettings.SenderName ?? "HireNow ATS", smtpSettings.SenderAddress));
                    message.To.Add(new MimeKit.MailboxAddress("", email.RecipientEmail));
                    message.Subject = email.Subject;
                    message.Body = new MimeKit.TextPart("html") { Text = email.Body };

                    using (var client = new MailKit.Net.Smtp.SmtpClient())
                    {
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                        await client.ConnectAsync(smtpSettings.Host, smtpSettings.Port, MailKit.Security.SecureSocketOptions.Auto);
                        if (!string.IsNullOrEmpty(smtpSettings.Username))
                        {
                            await client.AuthenticateAsync(smtpSettings.Username, smtpSettings.Password);
                        }
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);
                    }

                    _logger.LogInformation("Successfully sent email via tenant SMTP ({Host}) to {Recipient}", smtpSettings.Host, email.RecipientEmail);
                    return;
                }
            }

            // Global SMTP Fallback
            var globalSmtpHost = _configuration["SMTP_HOST"];
            if (!string.IsNullOrWhiteSpace(globalSmtpHost))
            {
                var smtpPortStr = _configuration["SMTP_PORT"];
                var smtpPort = int.TryParse(smtpPortStr, out var port) ? port : 587;
                var smtpUser = _configuration["SMTP_USERNAME"];
                var smtpPass = _configuration["SMTP_PASSWORD"];
                var senderEmail = _configuration["SMTP_SENDER_EMAIL"] ?? "no-reply@hirenow.com";
                var senderName = _configuration["SMTP_SENDER_NAME"] ?? "HireNow ATS";

                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress(senderName, senderEmail));
                message.To.Add(new MimeKit.MailboxAddress("", email.RecipientEmail));
                message.Subject = email.Subject;
                message.Body = new MimeKit.TextPart("html") { Text = email.Body };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    await client.ConnectAsync(globalSmtpHost, smtpPort, MailKit.Security.SecureSocketOptions.Auto);
                    if (!string.IsNullOrEmpty(smtpUser))
                    {
                        await client.AuthenticateAsync(smtpUser, smtpPass);
                    }
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation("Successfully sent email via global SMTP ({Host}) to {Recipient}", globalSmtpHost, email.RecipientEmail);
                return;
            }

            if (!Directory.Exists(_emailsFolder))
            {
                Directory.CreateDirectory(_emailsFolder);
            }

            var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.html";
            var filePath = Path.Combine(_emailsFolder, fileName);

            var emailHtml = $@"
<html>
<head><title>{email.Subject}</title></head>
<body>
    <div style='background:#f4f4f4;padding:15px;border-bottom:1px solid #ddd;'>
        <strong>To:</strong> {email.RecipientEmail}<br/>
        <strong>Date:</strong> {DateTime.UtcNow}<br/>
        <strong>Subject:</strong> {email.Subject}
    </div>
    <div style='padding:20px;'>
        {email.Body}
    </div>
</body>
</html>";

            await File.WriteAllTextAsync(filePath, emailHtml);
            _logger.LogInformation("Successfully wrote mock email to path: {Path}", filePath);
        }
    }
}
