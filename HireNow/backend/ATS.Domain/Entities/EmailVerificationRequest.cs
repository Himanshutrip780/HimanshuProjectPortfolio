using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class EmailVerificationRequest : BaseEntity
    {
        public Guid? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string OtpHash { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string VerificationStatus { get; set; } = "Pending";
        public int AttemptCount { get; set; }
        public int ResendCount { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RegistrationPayload { get; set; }
        public DateTime? LastResentAt { get; set; }
    }
}
