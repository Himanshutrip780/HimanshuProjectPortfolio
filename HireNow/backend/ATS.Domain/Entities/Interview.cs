using System;
using System.Collections.Generic;
using ATS.Domain.Common;
using ATS.Domain.Enums;

namespace ATS.Domain.Entities
{
    public class Interview : BaseEntity
    {
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; }

        public Guid InterviewerId { get; set; }
        public ApplicationUser Interviewer { get; set; }

        public string Title { get; set; }
        public InterviewType Type { get; set; } = InterviewType.Technical;
        public DateTime ScheduledTime { get; set; }
        public int DurationMinutes { get; set; }
        public string VideoLink { get; set; }
        public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

        public ICollection<InterviewFeedback> Feedbacks { get; set; } = new List<InterviewFeedback>();
    }
}
