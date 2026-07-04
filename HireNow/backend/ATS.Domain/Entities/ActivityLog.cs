using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class ActivityLog : BaseEntity
    {
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; }

        public Guid CandidateId { get; set; }
        public Candidate Candidate { get; set; }

        public string Action { get; set; }
        public string Details { get; set; }
        public string PerformedBy { get; set; }
    }
}
