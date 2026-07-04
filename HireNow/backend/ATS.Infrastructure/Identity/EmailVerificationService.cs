using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Application.DTOs.Auth;
using ATS.Domain.Entities;
using ATS.Shared.Models;

namespace ATS.Infrastructure.Identity
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;

        public EmailVerificationService(
            IApplicationDbContext context,
            IIdentityService identityService,
            IEmailService emailService)
        {
            _context = context;
            _identityService = identityService;
            _emailService = emailService;
        }

        private string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(bytes);
        }

        public async Task<Result> SendOtpAsync(RegisterRequest request, string? ipAddress, string? userAgent)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                return Result.Failure("Invalid email format.");
            }

            var domain = email.Split('@').Last().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(domain))
            {
                return Result.Failure("Invalid email domain.");
            }

            // Check resend limits within past 30 minutes
            var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);
            var existingRequest = await _context.EmailVerificationRequests
                .Where(r => r.Email == email && r.CreatedDate >= thirtyMinutesAgo)
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            if (existingRequest != null && existingRequest.ResendCount >= 3)
            {
                return Result.Failure("Maximum resend attempts exceeded. Please try again after 30 minutes.");
            }

            // Generate secure 6-digit OTP
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var otpHash = HashOtp(otp);
            var expiry = DateTime.UtcNow.AddMinutes(10);

            EmailVerificationRequest verificationRequest;

            if (existingRequest != null && existingRequest.VerificationStatus == "Pending")
            {
                existingRequest.OtpHash = otpHash;
                existingRequest.ExpiryDate = expiry;
                existingRequest.AttemptCount = 0; // Reset attempts for new OTP
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
                    RegistrationPayload = JsonSerializer.Serialize(request),
                    CreatedBy = "System"
                };

                await _context.EmailVerificationRequests.AddAsync(verificationRequest);
            }

            // Log OTP generation audit
            var auditLog = new EmailVerificationAuditLog
            {
                EmailAddress = email,
                Action = existingRequest != null ? "OTP_RESENT" : "OTP_GENERATED",
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Details = $"OTP generated. ResendCount: {verificationRequest.ResendCount}",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            await _context.EmailVerificationAuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync(default);

            // Send OTP email
            var emailBody = GetOtpEmailHtml($"{request.FirstName} {request.LastName}".Trim(), otp, "10 minutes");
            var subject = existingRequest != null ? "Your new verification code" : "Verify your email address";

            Console.WriteLine($"[EMAIL OTP LOG] Sent to: {email} | Code: {otp} | Expiry: 10 minutes");

            try
            {
                await _emailService.SendEmailAsync(email, subject, emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Error] Failed to send verification email: {ex.Message}");
            }

            return Result.Success();
        }

        public async Task<Result> ResendOtpAsync(string email, string? ipAddress, string? userAgent)
        {
            var emailNormalized = email.Trim().ToLowerInvariant();

            var verificationRequest = await _context.EmailVerificationRequests
                .Where(r => r.Email == emailNormalized && r.VerificationStatus == "Pending")
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            if (verificationRequest == null)
            {
                return Result.Failure("No pending verification request found for this email address.");
            }

            var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);
            if (verificationRequest.LastResentAt.HasValue && verificationRequest.LastResentAt.Value >= thirtyMinutesAgo && verificationRequest.ResendCount >= 3)
            {
                return Result.Failure("Maximum resend attempts exceeded. Please try again after 30 minutes.");
            }

            RegisterRequest? payload = null;
            if (!string.IsNullOrEmpty(verificationRequest.RegistrationPayload))
            {
                payload = JsonSerializer.Deserialize<RegisterRequest>(verificationRequest.RegistrationPayload);
            }

            var name = payload != null ? $"{payload.FirstName} {payload.LastName}".Trim() : "User";

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

            var auditLog = new EmailVerificationAuditLog
            {
                EmailAddress = emailNormalized,
                Action = "OTP_RESENT",
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Details = $"OTP resent. ResendCount: {verificationRequest.ResendCount}",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            await _context.EmailVerificationAuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync(default);

            var emailBody = GetResendEmailHtml(name, otp, "10 minutes");
            var subject = "Your new verification code";

            Console.WriteLine($"[EMAIL OTP LOG] Resent to: {emailNormalized} | Code: {otp} | Expiry: 10 minutes");

            try
            {
                await _emailService.SendEmailAsync(emailNormalized, subject, emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Error] Failed to resend verification email: {ex.Message}");
            }

            return Result.Success();
        }

        public async Task<Result<AuthResponse>> VerifyOtpAsync(string email, string code, string? ipAddress, string? userAgent)
        {
            var emailNormalized = email.Trim().ToLowerInvariant();

            var verificationRequest = await _context.EmailVerificationRequests
                .Where(r => r.Email == emailNormalized && r.VerificationStatus == "Pending")
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            if (verificationRequest == null)
            {
                return Result<AuthResponse>.Failure("No pending verification request found for this email address.");
            }

            if (verificationRequest.AttemptCount >= 5)
            {
                verificationRequest.VerificationStatus = "Locked";
                _context.EmailVerificationRequests.Update(verificationRequest);
                await _context.SaveChangesAsync(default);
                return Result<AuthResponse>.Failure("Maximum verification attempts exceeded. Please register again to request a new verification code.");
            }

            if (verificationRequest.ExpiryDate < DateTime.UtcNow)
            {
                verificationRequest.VerificationStatus = "Expired";
                _context.EmailVerificationRequests.Update(verificationRequest);
                await _context.SaveChangesAsync(default);
                return Result<AuthResponse>.Failure("Verification code has expired. Please request a new code.");
            }

            // Log attempt
            var attempt = new OTPVerificationAttempt
            {
                EmailVerificationRequestId = verificationRequest.Id,
                EmailAddress = emailNormalized,
                OtpAttempt = code,
                IsSuccessful = false,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            var computedHash = HashOtp(code);
            var isSuccessful = computedHash == verificationRequest.OtpHash;

            attempt.IsSuccessful = isSuccessful;
            await _context.OTPVerificationAttempts.AddAsync(attempt);

            if (!isSuccessful)
            {
                verificationRequest.AttemptCount += 1;
                _context.EmailVerificationRequests.Update(verificationRequest);

                var failureAudit = new EmailVerificationAuditLog
                {
                    EmailAddress = emailNormalized,
                    Action = "OTP_FAILED",
                    IPAddress = ipAddress,
                    UserAgent = userAgent,
                    Details = $"Verification failed. AttemptCount: {verificationRequest.AttemptCount}",
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };

                await _context.EmailVerificationAuditLogs.AddAsync(failureAudit);
                await _context.SaveChangesAsync(default);

                var remaining = 5 - verificationRequest.AttemptCount;
                return Result<AuthResponse>.Failure($"Invalid verification code. {remaining} attempts remaining.");
            }

            // check if email exists in database (delayed validation check)
            var regResult = await _identityService.RegisterAsync(JsonSerializer.Deserialize<RegisterRequest>(verificationRequest.RegistrationPayload!));
            if (!regResult.IsSuccess)
            {
                verificationRequest.VerificationStatus = "Failed";
                _context.EmailVerificationRequests.Update(verificationRequest);

                var conflictAudit = new EmailVerificationAuditLog
                {
                    EmailAddress = emailNormalized,
                    Action = "OTP_FAILED",
                    IPAddress = ipAddress,
                    UserAgent = userAgent,
                    Details = $"Verification succeeded but registration failed: {regResult.Error}",
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };

                await _context.EmailVerificationAuditLogs.AddAsync(conflictAudit);
                await _context.SaveChangesAsync(default);

                return Result<AuthResponse>.Failure(regResult.Error);
            }

            var authResponse = regResult.Value;

            verificationRequest.VerificationStatus = "Verified";
            if (Guid.TryParse(authResponse.Id, out var parsedUserId))
            {
                verificationRequest.UserId = parsedUserId;
            }
            _context.EmailVerificationRequests.Update(verificationRequest);

            var successAudit = new EmailVerificationAuditLog
            {
                UserId = verificationRequest.UserId,
                EmailAddress = emailNormalized,
                Action = "OTP_VERIFIED",
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Details = "Email ownership successfully verified. User registered.",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            await _context.EmailVerificationAuditLogs.AddAsync(successAudit);
            await _context.SaveChangesAsync(default);

            // Send confirmation success email
            var emailBody = GetSuccessEmailHtml($"{authResponse.FirstName} {authResponse.LastName}".Trim());
            try
            {
                await _emailService.SendEmailAsync(emailNormalized, "Welcome! Verification Successful", emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Error] Failed to send success email: {ex.Message}");
            }

            return Result<AuthResponse>.Success(authResponse);
        }

        public async Task<Result<string>> CheckStatusAsync(string email)
        {
            var emailNormalized = email.Trim().ToLowerInvariant();

            var req = await _context.EmailVerificationRequests
                .Where(r => r.Email == emailNormalized)
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            if (req == null)
            {
                return Result<string>.Success("None");
            }

            return Result<string>.Success(req.VerificationStatus);
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
        .header {{ background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); padding: 32px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.025em; }}
        .content {{ padding: 40px; line-height: 1.6; }}
        .greeting {{ font-size: 18px; font-weight: 600; margin-bottom: 16px; color: #111827; }}
        .description {{ font-size: 15px; color: #4b5563; margin-bottom: 24px; }}
        .code-container {{ background-color: #f9fafb; border: 2px dashed #e5e7eb; border-radius: 8px; padding: 24px; text-align: center; margin-bottom: 24px; }}
        .code {{ font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace; font-size: 32px; font-weight: 800; letter-spacing: 0.25em; color: #2563eb; margin: 0; }}
        .expiry {{ font-size: 13px; color: #9ca3af; text-align: center; margin-top: 8px; }}
        .notice {{ font-size: 13px; color: #6b7280; border-top: 1px solid #e5e7eb; padding-top: 24px; margin-top: 24px; }}
        .footer {{ background-color: #f9fafb; padding: 24px; text-align: center; font-size: 13px; color: #9ca3af; border-top: 1px solid #e5e7eb; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>HireNow</h1>
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
            &copy; 2026 HireNow. All rights reserved.
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
        .header {{ background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); padding: 32px; text-align: center; color: #ffffff; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.025em; }}
        .content {{ padding: 40px; line-height: 1.6; }}
        .greeting {{ font-size: 18px; font-weight: 600; margin-bottom: 16px; color: #111827; }}
        .description {{ font-size: 15px; color: #4b5563; margin-bottom: 24px; }}
        .code-container {{ background-color: #f9fafb; border: 2px dashed #e5e7eb; border-radius: 8px; padding: 24px; text-align: center; margin-bottom: 24px; }}
        .code {{ font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace; font-size: 32px; font-weight: 800; letter-spacing: 0.25em; color: #2563eb; margin: 0; }}
        .expiry {{ font-size: 13px; color: #9ca3af; text-align: center; margin-top: 8px; }}
        .notice {{ font-size: 13px; color: #6b7280; border-top: 1px solid #e5e7eb; padding-top: 24px; margin-top: 24px; }}
        .footer {{ background-color: #f9fafb; padding: 24px; text-align: center; font-size: 13px; color: #9ca3af; border-top: 1px solid #e5e7eb; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>HireNow</h1>
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
            &copy; 2026 HireNow. All rights reserved.
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
    <title>Welcome to HireNow!</title>
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
            <h1>Welcome to HireNow</h1>
        </div>
        <div class='content'>
            <div class='greeting'>Hello {name},</div>
            <div class='success-box'>Email verification successful! Your account is now active.</div>
            <div class='description'>Your organization email ownership has been verified. You can now log in to the ATS portal and start managing candidates and jobs.</div>
            <div class='notice'>You can now return to the login page and sign in to your dashboard.</div>
        </div>
        <div class='footer'>
            &copy; 2026 HireNow. All rights reserved.
        </div>
    </div>
</body>
</html>";
        }
    }
}
