using System;

namespace ATS.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; }
        public string TableName { get; set; }
        public string Action { get; set; } // Insert, Update, Delete
        public string OldValues { get; set; } // JSON representation
        public string NewValues { get; set; } // JSON representation
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
