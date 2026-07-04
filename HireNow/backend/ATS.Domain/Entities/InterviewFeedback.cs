using System;
using ATS.Domain.Common;
using ATS.Domain.Enums;

namespace ATS.Domain.Entities
{
    public class InterviewFeedback : BaseEntity
    {
        public Guid InterviewId { get; set; }
        public Interview Interview { get; set; }

        public Guid InterviewerId { get; set; }
        public ApplicationUser Interviewer { get; set; }

        public int CommunicationScore { get; set; } // 1-5
        public int ProblemSolvingScore { get; set; } // 1-5
        public int CodingScore { get; set; } // 1-5
        public int SystemDesignScore { get; set; } // 1-5
        public int CultureFitScore { get; set; } // 1-5

        public string FeedbackText { get; set; }
        public RecommendationType Recommendation { get; set; } = RecommendationType.Neutral;
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    }
}
