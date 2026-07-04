using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ATS.Application.Features.Candidates;

namespace ATS.Application.Features.Applications
{
    public class ApplicationDto
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string JobTitle { get; set; }
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public string CurrentStage { get; set; }
        public string Stage => CurrentStage;
        public string Status { get; set; } // Active, Hired, Rejected
        public DateTime CreatedDate { get; set; }

        [JsonPropertyName("timeInStageDays")]
        public int TimeInStageDays { get; set; }

        public CandidateDto Candidate { get; set; }

        [JsonPropertyName("aimatchScore")]
        public int? AIMatchScore { get; set; }

        [JsonPropertyName("airecommendation")]
        public string AIRecommendation { get; set; }

        [JsonPropertyName("aisummary")]
        public string AISummary { get; set; }

        [JsonPropertyName("strengths")]
        public List<string> Strengths { get; set; } = new List<string>();

        [JsonPropertyName("weaknesses")]
        public List<string> Weaknesses { get; set; } = new List<string>();

        [JsonPropertyName("missingSkills")]
        public List<string> MissingSkills { get; set; } = new List<string>();

        [JsonPropertyName("aiQuestions")]
        public List<string> AIQuestions { get; set; } = new List<string>();

        [JsonPropertyName("skillsMatch")]
        public List<string> SkillsMatch { get; set; } = new List<string>();


        
        public List<StageHistoryDto> StageHistory { get; set; } = new List<StageHistoryDto>();
    }

    public class StageHistoryDto
    {
        public string StageName { get; set; }
        public string Status { get; set; }
        public DateTime EnteredDate { get; set; }
        public DateTime? LeftDate { get; set; }
        public int SequenceNumber { get; set; }
    }
}
