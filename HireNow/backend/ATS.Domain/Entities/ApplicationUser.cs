using System;
using Microsoft.AspNetCore.Identity;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>, IMultiTenant
    {
        public bool IsPendingVerification { get; set; } = false;
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
