using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class TenantVerificationRequest : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string Email { get; set; }
        public string VerificationCode { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
