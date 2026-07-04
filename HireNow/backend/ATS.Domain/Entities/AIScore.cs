using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class AIScore : BaseEntity
    {
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; }

        public Guid JobId { get; set; }
        public Job Job { get; set; }

        public Guid CandidateId { get; set; }
        public Candidate Candidate { get; set; }

        public int MatchScore { get; set; } // 0-100
        public decimal SkillMatchPercentage { get; set; }
        public decimal ExperienceMatchPercentage { get; set; }
        public decimal EducationMatchPercentage { get; set; }

        public string MissingSkillsJson { get; set; }
        public string StrengthsJson { get; set; }
        public string WeaknessesJson { get; set; }
        public string AIQuestionsJson { get; set; } = "[]";

        public string Recommendation { get; set; } // Strong Fit, Moderate Fit, Weak Fit
        public string AISummary { get; set; }
    }
}
