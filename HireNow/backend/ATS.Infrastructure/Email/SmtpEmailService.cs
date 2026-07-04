using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Entities;

namespace ATS.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IApplicationDbContext _context;

        public SmtpEmailService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailOutbox = new EmailOutbox
            {
                RecipientEmail = toEmail,
                Subject = subject,
                Body = body,
                IsSent = false,
                RetryCount = 0,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.EmailOutboxes.AddAsync(emailOutbox);
            await _context.SaveChangesAsync(default);
        }

        public async Task SendEmailTemplateAsync(string toEmail, string triggerEvent, Dictionary<string, string> templateVariables)
        {
            var template = await _context.EmailTemplates
                .FirstOrDefaultAsync(t => t.TriggerEvent == triggerEvent);

            if (template == null)
            {
                await SendEmailAsync(toEmail, $"Alert: {triggerEvent}", $"Event {triggerEvent} was triggered with {templateVariables.Count} params.");
                return;
            }

            var subject = template.Subject;
            var body = template.Body;

            foreach (var variable in templateVariables)
            {
                subject = subject.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                body = body.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }

            subject = System.Text.RegularExpressions.Regex.Replace(subject, @"\{\{.*?\}\}", "");
            body = System.Text.RegularExpressions.Regex.Replace(body, @"\{\{.*?\}\}", "");

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
