using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class EmailOutbox : BaseEntity
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsSent { get; set; } = false;
        public int RetryCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }
        public DateTime? SentDate { get; set; }
    }
}
