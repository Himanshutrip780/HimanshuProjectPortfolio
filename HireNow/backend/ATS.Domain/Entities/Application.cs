using System;
using System.Collections.Generic;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class Application : BaseEntity
    {
        public Guid JobId { get; set; }
        public Job Job { get; set; }

        public Guid CandidateId { get; set; }
        public Candidate Candidate { get; set; }

        public string CurrentStage { get; set; }
        public string Status { get; set; } // Active, Hired, Rejected

        public ICollection<ApplicationStage> StagesHistory { get; set; } = new List<ApplicationStage>();
        public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public ICollection<AIScore> AIScores { get; set; } = new List<AIScore>();
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }
}
