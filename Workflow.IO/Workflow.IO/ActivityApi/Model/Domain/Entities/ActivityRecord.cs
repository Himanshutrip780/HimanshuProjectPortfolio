namespace ActivityApi.Model.Domain.Entities
{
    public class ActivityRecord
    {
        public Guid ActivityRecordId { get; private set; }

        public string EventType { get; private set; } = string.Empty;

        public string EntityType { get; private set; } = string.Empty;

        public Guid EntityId { get; private set; }

        public Guid? ActorId { get; private set; }

        public string? Description { get; private set; }

        public string? PayloadJson { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private ActivityRecord()
        {
        }

        public ActivityRecord(
            string eventType,
            string entityType,
            Guid entityId,
            Guid? actorId,
            string? description,
            string? payloadJson)
        {
            ActivityRecordId = Guid.NewGuid();

            EventType = eventType;

            EntityType = entityType;

            EntityId = entityId;

            ActorId = actorId;

            Description = description;

            PayloadJson = payloadJson;

            CreatedAt = DateTime.UtcNow;
        }
    }
}
