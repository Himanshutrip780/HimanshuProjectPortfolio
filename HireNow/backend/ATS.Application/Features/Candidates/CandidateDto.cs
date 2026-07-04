using System;
using System.Collections.Generic;

namespace ATS.Application.Features.Candidates
{
    public class CandidateDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LinkedInUrl { get; set; }
        public string GitHubUrl { get; set; }
        public string PortfolioUrl { get; set; }
        public string ResumePath { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public DateTime CreatedDate { get; set; }
        public string ParsedDataJson { get; set; }
        public decimal? ParsingConfidenceScore { get; set; }
        public string CurrentTitle { get; set; }
        public string YearsOfExperience { get; set; }
        public string Education { get; set; }
        public decimal? ExpectedSalary { get; set; }
        public string LatestApplicationJobTitle { get; set; }
        public string LatestApplicationStage { get; set; }
        public int LatestApplicationMatchScore { get; set; }
    }
}
