using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class EmailVerificationAuditLog : BaseEntity
    {
        public Guid? UserId { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Details { get; set; }
    }
}
