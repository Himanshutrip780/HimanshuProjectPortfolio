using System;
using System.Collections.Generic;
using ATS.Domain.Enums;

namespace ATS.Application.Features.Interviews
{
    public class InterviewDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string CandidateName { get; set; }
        public string JobTitle { get; set; }
        public Guid InterviewerId { get; set; }
        public string InterviewerName { get; set; }
        public string Title { get; set; }
        public InterviewType Type { get; set; }
        public DateTime ScheduledTime { get; set; }
        public int DurationMinutes { get; set; }
        public string VideoLink { get; set; }
        public InterviewStatus Status { get; set; }
        public List<FeedbackDto> Feedbacks { get; set; } = new List<FeedbackDto>();
    }

    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public Guid InterviewerId { get; set; }
        public string InterviewerName { get; set; }
        public int CommunicationScore { get; set; }
        public int ProblemSolvingScore { get; set; }
        public int CodingScore { get; set; }
        public int SystemDesignScore { get; set; }
        public int CultureFitScore { get; set; }
        public string FeedbackText { get; set; }
        public RecommendationType Recommendation { get; set; }
        public DateTime SubmittedDate { get; set; }
    }
}
