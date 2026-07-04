using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class OTPVerificationAttempt : BaseEntity
    {
        public Guid EmailVerificationRequestId { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string OtpAttempt { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
