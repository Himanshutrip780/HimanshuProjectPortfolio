using System;
using System.Collections.Generic;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class Company : BaseEntity
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string SubscriptionPlan { get; set; }

        // SSO Configuration
        public bool SsoEnabled { get; set; } = false;
        public string? SsoProvider { get; set; }
        public string? SsoRedirectUrl { get; set; }
        public string? SsoIssuer { get; set; }
        public string? SsoClientId { get; set; }

        // Branding Configuration
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? FontFamily { get; set; }
        public string? CustomCss { get; set; }

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
    }
}
