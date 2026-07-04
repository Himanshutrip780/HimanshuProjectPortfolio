using System;
using System.ComponentModel.DataAnnotations;

namespace TaskApi.Model.Domain.Entities
{
    public class DailyUpdateState
    {
        [Key]
        public Guid ProjectId { get; set; }

        public DateTime? LastSentAt { get; set; }

        public bool IsTriggeredToday { get; set; }

        public string? ExtraRecipients { get; set; }
    }
}
