using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class ApplicationStage : BaseEntity
    {
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; }

        public string StageName { get; set; }
        public string Status { get; set; } // Current, Completed, Left
        public DateTime EnteredDate { get; set; } = DateTime.UtcNow;
        public DateTime? LeftDate { get; set; }
        public int SequenceNumber { get; set; }
    }
}
