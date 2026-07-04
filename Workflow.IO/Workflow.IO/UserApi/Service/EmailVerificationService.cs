using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using UserApi.Data;
using UserApi.Model.Domian.Entities;
using UserApi.Model.Dto;
using Workflow.IO.Shared.Exceptions;

namespace UserApi.Service
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly UserDbContext _context;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public EmailVerificationService(UserDbContext context, IUserService userService, IConfiguration configuration)
        {
            _context = context;
            _userService = userService;
            _configuration = configuration;
        }

        private string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(bytes);
        }

        private async Task SaveEmailBackupAsync(string email, string subject, string htmlContent)
        {
            try
            {
                var backupsDir = Path.Combine(Directory.GetCurrentDirectory(), "backups", "email-verification");
                if (!Directory.Exists(backupsDir))
                {
                    Directory.CreateDirectory(backupsDir);
                }
                var fileName = $"{email.Replace("@", "_").Replace(".", "_")}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 4)}.html";
                var previewPath = Path.Combine(backupsDir, fileName);
                await File.WriteAllTextAsync(previewPath, htmlContent, Encoding.UTF8);
                Console.WriteLine($"[Email Backup] Verification email successfully written to: {previewPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Error] Failed to write verification email to backups: {ex.Message}");
            }
        }

        private async Task DeliverEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var smtpHost = _configuration["SMTP_HOST"];
            if (!string.IsNullOrWhiteSpace(smtpHost))
            {
                try
                {
                    var smtpPortStr = _configuration["SMTP_PORT"];
                    var smtpPort = int.TryParse(smtpPortStr, out var port) ? port : 587;
                    var smtpUser = _configuration["SMTP_USERNAME"];
                    var smtpPass = _configuration["SMTP_PASSWORD"];
                    var senderEmail = _configuration["SMTP_SENDER_EMAIL"] ?? "no-reply@workflow.io";
                    var senderName = _configuration["SMTP_SENDER_NAME"] ?? "Workflow.IO";

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(senderName, senderEmail));
                    message.To.Add(new MailboxAddress("", toEmail));
                    message.Subject = subject;
                    message.Body = new TextPart("html") { Text = htmlContent };

                    using (var client = new SmtpClient())
                    {
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                        await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.Auto);
                        if (!string.IsNullOrEmpty(smtpUser))
                        {
                            await client.AuthenticateAsync(smtpUser, smtpPass);
                        }
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);
                    }

                    Console.WriteLine($"[Email Success] Real email sent successfully via SMTP ({smtpHost}) to: {toEmail}");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Email Error] Failed to send real email via SMTP to {toEmail}: {ex.Message}");
                    // Fallback to mock on error so user doesn't get completely blocked
                }
            }

            await SaveEmailBackupAsync(toEmail, subject, htmlContent);
        }

        public async Task SendOtpAsync(RegisterUserRequestDTO request, string? ipAddress, string? userAgent)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            // Validate email format and domain
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                throw new ArgumentException("Invalid email format.");
            }

            var domain = email.Split('@').Last().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException("Invalid email domain.");
            }

            // Check if there is an active verification request within 30 minutes to check resend limits
            var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);
            var existingRequest = await _context.EmailVerificationRequests
                .Where(r => r.Email == email && r.CreatedDate >= thirtyMinutesAgo)
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            if (existingRequest != null && existingRequest.ResendCount >= 3)
            {
                throw new ArgumentException("Maximum resend attempts exceeded. Please try again after 30 minutes.");
            }

            // Generate secure 6-digit OTP
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var otpHash = HashOtp(otp);
            var expiry = DateTime.UtcNow.AddMinutes(10);

            EmailVerificationRequest verificationRequest;

            if (existingRequest != null && existingRequest.VerificationStatus == "Pending")
            {
                // Reuse the existing request object but update OTP details
                existingRequest.OtpHash = otpHash;
                existingRequest.ExpiryDate = expiry;
                existingRequest.AttemptCount = 0; // Reset attempts for the new code
                existingRequest.ResendCount += 1;
                existingRequest.LastResentAt = DateTime.UtcNow;
                existingRequest.IPAddress = ipAddress;
                existingRequest.UserAgent = userAgent;
                existingRequest.RegistrationPayload = JsonSerializer.Serialize(request);

                verificationRequest = existingRequest;
                _context.EmailVerificationRequests.Update(existingRequest);
            }
            else
            {
                verificationRequest = new EmailVerificationRequest
                {
                    Email = email,
                    OtpHash = otpHash,
                    CreatedDate = DateTime.UtcNow,
                    ExpiryDate = expiry,
                    VerificationStatus = "Pending",
                    AttemptCount = 0,
                    ResendCount = 0,
                    IPAddress = ipAddress,
                    UserAgent = userAgent,
                    RegistrationPayload = JsonSerializer.Serialize(request)
                };

                await _context.EmailVerificationRequests.AddAsync(verificationRequest);
            }

            // Log OTP generation audit
            var auditLog = new EmailVerificationAuditLog
            {
                EmailAddress = email,
                Action = existingRequest != null ? "OTP_RESENT" : "OTP_GENERATED",
                CreatedDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Details = $"OTP generated. ResendCount: {verificationRequest.ResendCount}"
            };

            await _context.EmailVerificationAuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            // Send OTP email
            var emailBody = GetOtpEmailHtml(request.FirstName, otp, "10 minutes");
            var subject = existingRequest != null ? "Your new verification code" : "Verify your email address";
            
            // Output OTP code to Console for manual/auto test verification
            Console.WriteLine($"[EMAIL OTP LOG] Sent to: {email} | Code: {otp} | Expiry: 10 minutes");

            await DeliverEmailAsync(email, subject, emailBody);
        }

        public async Task ResendOtpAsync(string email, string? ipAddress, string? userAgent)
        {
            var emailNormalized = email.Trim().ToLowerInvariant();

            // Find the most recent pending verification request
            var verificationRequest = await _context.EmailVerificationRequests
                .Where(r => r.Email == emailNormalized && r.VerificationStatus == "Pending")
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            if (verificationRequest == null)
            {
                throw new ArgumentException("No pending verification request found for this email address.");
            }

            var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);
            if (verificationRequest.LastResentAt.HasValue && verificationRequest.LastResentAt.Value >= thirtyMinutesAgo && verificationRequest.ResendCount >= 3)
            {
                throw new ArgumentException("Maximum resend attempts exceeded. Please try again after 30 minutes.");
            }

            // Deserialize payload to read details for the email
            RegisterUserRequestDTO? payload = null;
            if (!string.IsNullOrEmpty(verificationRequest.RegistrationPayload))
            {
                payload = JsonSerializer.Deserialize<RegisterUserRequestDTO>(verificationRequest.RegistrationPayload);
            }

            var firstName = payload?.FirstName ?? "User";

            // Generate secure 6-digit OTP
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var otpHash = HashOtp(otp);
            var expiry = DateTime.UtcNow.AddMinutes(10);

            verificationRequest.OtpHash = otpHash;
            verificationRequest.ExpiryDate = expiry;
            verificationRequest.AttemptCount = 0; // Reset attempts for new OTP
            verificationRequest.ResendCount += 1;
            verificationRequest.LastResentAt = DateTime.UtcNow;
            verificationRequest.IPAddress = ipAddress;
            verificationRequest.UserAgent = userAgent;

            _context.EmailVerificationRequests.Update(verificationRequest);

            // Log OTP resent audit
            var auditLog = new EmailVerificationAuditLog
            {
                EmailAddress = emailNormalized,
                Action = "OTP_RESENT",
                CreatedDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Details = $"OTP resent. ResendCount: {verificationRequest.ResendCount}"
            };

            await _context.EmailVerificationAuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            // Send OTP email
            var emailBody = GetResendEmailHtml(firstName, otp, "10 minutes");
            var subject = "Your new verification code";
            
            Console.WriteLine($"[EMAIL OTP LOG] Resent to: {emailNormalized} | Code: {otp} | Expiry: 10 minutes");

            await DeliverEmailAsync(emailNormalized, subject, emailBody);
        }

        public async Task<UserDto> VerifyOtpAsync(string email, string code, string? ipAddress, string? userAgent)
        {
            var emailNormalized = email.Trim().ToLowerInvariant();

            // Find the most recent pending verification request
            var verificationRequest = await _context.EmailVerificationRequests
                .Where(r => r.Email == emailNormalized && r.VerificationStatus == "Pending")
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            if (verificationRequest == null)
            {
                throw new ArgumentException("No pending verification request found for this email address.");
            }

            // Check brute-force limits
            if (verificationRequest.AttemptCount >= 5)
            {
                verificationRequest.VerificationStatus = "Locked";
                _context.EmailVerificationRequests.Update(verificationRequest);
                await _context.SaveChangesAsync();
                throw new ArgumentException("Maximum verification attempts exceeded. Please register again to request a new verification code.");
            }

            // Check expiry
            if (verificationRequest.ExpiryDate < DateTime.UtcNow)
            {
                verificationRequest.VerificationStatus = "Expired";
                _context.EmailVerificationRequests.Update(verificationRequest);
                await _context.SaveChangesAsync();
                throw new ArgumentException("Verification code has expired. Please request a new code.");
            }

            // Log attempt
            var attempt = new OTPVerificationAttempt
            {
                EmailVerificationRequestId = verificationRequest.Id,
                EmailAddress = emailNormalized,
                OtpAttempt = code,
                CreatedDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent
            };

            var computedHash = HashOtp(code);
            var isSuccessful = computedHash == verificationRequest.OtpHash;

            attempt.IsSuccessful = isSuccessful;
            await _context.OTPVerificationAttempts.AddAsync(attempt);

            if (!isSuccessful)
            {
                verificationRequest.AttemptCount += 1;
                _context.EmailVerificationRequests.Update(verificationRequest);

                // Audit log for failure
                var failureAudit = new EmailVerificationAuditLog
                {
                    EmailAddress = emailNormalized,
                    Action = "OTP_FAILED",
                    CreatedDate = DateTime.UtcNow,
                    IPAddress = ipAddress,
                    UserAgent = userAgent,
                    Details = $"Verification failed. AttemptCount: {verificationRequest.AttemptCount}"
                };

                await _context.EmailVerificationAuditLogs.AddAsync(failureAudit);
                await _context.SaveChangesAsync();

                var remaining = 5 - verificationRequest.AttemptCount;
                throw new ArgumentException($"Invalid verification code. {remaining} attempts remaining.");
            }

            // SUCCESS!
            // check if email exists in database (delayed validation check)
            var exists = await _userService.GetUserByEmailAsync(emailNormalized);
            if (exists != null)
            {
                verificationRequest.VerificationStatus = "Failed";
                _context.EmailVerificationRequests.Update(verificationRequest);

                var conflictAudit = new EmailVerificationAuditLog
                {
                    EmailAddress = emailNormalized,
                    Action = "OTP_FAILED",
                    CreatedDate = DateTime.UtcNow,
                    IPAddress = ipAddress,
                    UserAgent = userAgent,
                    Details = "Verification succeeded but email already registered"
                };

                await _context.EmailVerificationAuditLogs.AddAsync(conflictAudit);
                await _context.SaveChangesAsync();

                throw new ConflictException("Email already exists");
            }

            // Deserialize registration payload and register user
            if (string.IsNullOrEmpty(verificationRequest.RegistrationPayload))
            {
                throw new ArgumentException("Registration payload is missing. Please register again.");
            }

            var request = JsonSerializer.Deserialize<RegisterUserRequestDTO>(verificationRequest.RegistrationPayload);
            if (request == null)
            {
                throw new ArgumentException("Failed to deserialize registration payload.");
            }

            // Create account
            var userDto = await _userService.RegisterUserAsync(request);

            // Update verification status
            verificationRequest.VerificationStatus = "Verified";
            verificationRequest.UserId = userDto.UserId;
            _context.EmailVerificationRequests.Update(verificationRequest);

            // Audit log for success
            var successAudit = new EmailVerificationAuditLog
            {
                UserId = userDto.UserId,
                EmailAddress = emailNormalized,
                Action = "OTP_VERIFIED",
                CreatedDate = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Details = "Email ownership successfully verified. User created."
            };

            await _context.EmailVerificationAuditLogs.AddAsync(successAudit);
            await _context.SaveChangesAsync();

            // Send confirmation success email
            var emailBody = GetSuccessEmailHtml(request.FirstName);
            await DeliverEmailAsync(emailNormalized, "Welcome! Verification Successful", emailBody);

            return userDto;
        }

        public async Task<string> CheckStatusAsync(string email)
        {
            var emailNormalized = email.Trim().ToLowerInvariant();

            var req = await _context.EmailVerificationRequests
                .Where(r => r.Email == emailNormalized)
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            if (req == null)
            {
                return "None";
            }

            return req.VerificationStatus;
        }

        private string GetOtpEmailHtml(string name, string code, string expiry)
        {
            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Verify your email address</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6; margin: 0; padding: 40px 0; color: #1f2937; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; border: 1px solid #e5e7eb; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); padding: 32px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.025em; }}
        .content {{ padding: 40px; line-height: 1.6; }}
        .greeting {{ font-size: 18px; font-weight: 600; margin-bottom: 16px; color: #111827; }}
        .description {{ font-size: 15px; color: #4b5563; margin-bottom: 24px; }}
        .code-container {{ background-color: #f9fafb; border: 2px dashed #e5e7eb; border-radius: 8px; padding: 24px; text-align: center; margin-bottom: 24px; }}
        .code {{ font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace; font-size: 32px; font-weight: 800; letter-spacing: 0.25em; color: #4f46e5; margin: 0; }}
        .expiry {{ font-size: 13px; color: #9ca3af; text-align: center; margin-top: 8px; }}
        .notice {{ font-size: 13px; color: #6b7280; border-top: 1px solid #e5e7eb; padding-top: 24px; margin-top: 24px; }}
        .footer {{ background-color: #f9fafb; padding: 24px; text-align: center; font-size: 13px; color: #9ca3af; border-top: 1px solid #e5e7eb; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Workflow.IO</h1>
        </div>
        <div class='content'>
            <div class='greeting'>Hello {name},</div>
            <div class='description'>Thank you for starting your registration. To complete your account creation and verify ownership of your organization email, please enter the verification code below on the signup page.</div>
            <div class='code-container'>
                <div class='code'>{code}</div>
                <div class='expiry'>This code is valid for {expiry} and can only be used once.</div>
            </div>
            <div class='notice'>If you did not request this verification code, please ignore this email. No account will be created until this code is verified.</div>
        </div>
        <div class='footer'>
            &copy; 2026 Workflow.IO. All rights reserved.
        </div>
    </div>
</body>
</html>";
        }

        private string GetResendEmailHtml(string name, string code, string expiry)
        {
            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Your new verification code</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6; margin: 0; padding: 40px 0; color: #1f2937; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; border: 1px solid #e5e7eb; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); padding: 32px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.025em; }}
        .content {{ padding: 40px; line-height: 1.6; }}
        .greeting {{ font-size: 18px; font-weight: 600; margin-bottom: 16px; color: #111827; }}
        .description {{ font-size: 15px; color: #4b5563; margin-bottom: 24px; }}
        .code-container {{ background-color: #f9fafb; border: 2px dashed #e5e7eb; border-radius: 8px; padding: 24px; text-align: center; margin-bottom: 24px; }}
        .code {{ font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace; font-size: 32px; font-weight: 800; letter-spacing: 0.25em; color: #4f46e5; margin: 0; }}
        .expiry {{ font-size: 13px; color: #9ca3af; text-align: center; margin-top: 8px; }}
        .notice {{ font-size: 13px; color: #6b7280; border-top: 1px solid #e5e7eb; padding-top: 24px; margin-top: 24px; }}
        .footer {{ background-color: #f9fafb; padding: 24px; text-align: center; font-size: 13px; color: #9ca3af; border-top: 1px solid #e5e7eb; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Workflow.IO</h1>
        </div>
        <div class='content'>
            <div class='greeting'>Hello {name},</div>
            <div class='description'>As requested, here is your new verification code. Please use this code to complete your organization email verification.</div>
            <div class='code-container'>
                <div class='code'>{code}</div>
                <div class='expiry'>This code is valid for {expiry} and can only be used once.</div>
            </div>
            <div class='notice'>If you did not request this verification code, please ignore this email.</div>
        </div>
        <div class='footer'>
            &copy; 2026 Workflow.IO. All rights reserved.
        </div>
    </div>
</body>
</html>";
        }

        private string GetSuccessEmailHtml(string name)
        {
            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Welcome to Workflow.IO!</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6; margin: 0; padding: 40px 0; color: #1f2937; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; border: 1px solid #e5e7eb; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 32px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.025em; }}
        .content {{ padding: 40px; line-height: 1.6; }}
        .greeting {{ font-size: 18px; font-weight: 600; margin-bottom: 16px; color: #111827; }}
        .description {{ font-size: 15px; color: #4b5563; margin-bottom: 24px; }}
        .success-box {{ background-color: #ecfdf5; border: 1px solid #d1fae5; border-radius: 8px; padding: 20px; text-align: center; margin-bottom: 24px; color: #065f46; font-weight: 600; font-size: 15px; }}
        .notice {{ font-size: 13px; color: #6b7280; border-top: 1px solid #e5e7eb; padding-top: 24px; margin-top: 24px; }}
        .footer {{ background-color: #f9fafb; padding: 24px; text-align: center; font-size: 13px; color: #9ca3af; border-top: 1px solid #e5e7eb; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Workflow.IO</h1>
        </div>
        <div class='content'>
            <div class='greeting'>Hello {name},</div>
            <div class='success-box'>Email verification successful! Your account is now active.</div>
            <div class='description'>Your organization email ownership has been verified. We have automatically provisioned your organization's workspace, teams, and default project structure so you can get started immediately.</div>
            <div class='notice'>You can now return to the login page and sign in to your dashboard.</div>
        </div>
        <div class='footer'>
            &copy; 2026 Workflow.IO. All rights reserved.
        </div>
    </div>
</body>
</html>";
        }
    }
}
