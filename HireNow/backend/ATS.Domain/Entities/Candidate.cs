using System;
using System.Collections.Generic;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class Candidate : BaseEntity, IMultiTenant
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public string? PortfolioUrl { get; set; }

        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
        public string? ResumePath { get; set; }
        public string Source { get; set; } = "Organic";
        public string? LoginToken { get; set; }
        public DateTime? LoginTokenExpiry { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public decimal? ExpectedSalary { get; set; }
        public int? YearsOfExperience { get; set; }

        public ICollection<CandidateSkill> Skills { get; set; } = new List<CandidateSkill>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<CandidateNote> Notes { get; set; } = new List<CandidateNote>();
        public ResumeParsingResult ParsingResult { get; set; }
    }
}
