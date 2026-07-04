using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class ResumeParsingResult : BaseEntity
    {
        public Guid CandidateId { get; set; }
        public Candidate Candidate { get; set; }

        public string RawText { get; set; }
        public decimal ConfidenceScore { get; set; }
        public string ParsedDataJson { get; set; } // JSON metadata representing name, email, skills, experience, certifications
    }
}
