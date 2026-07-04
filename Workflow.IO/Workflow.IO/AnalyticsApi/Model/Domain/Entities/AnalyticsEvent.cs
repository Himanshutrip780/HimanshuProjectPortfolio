namespace AnalyticsApi.Model.Domain.Entities
{
    public class AnalyticsEvent
    {
        public Guid AnalyticsEventId { get; private set; } = Guid.NewGuid();

        public string EventType { get; private set; } = string.Empty;

        public string EntityType { get; private set; } = string.Empty;

        public Guid EntityId { get; private set; }

        public Guid? ProjectId { get; private set; }

        public Guid? ActorId { get; private set; }

        public Guid? RecipientId { get; private set; }

        public string? Description { get; private set; }

        public string? PayloadJson { get; private set; }

        public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;

        private AnalyticsEvent()
        {
        }

        public AnalyticsEvent(
            string eventType,
            string entityType,
            Guid entityId,
            Guid? projectId,
            Guid? actorId,
            Guid? recipientId,
            string? description,
            string? payloadJson)
        {
            EventType = eventType;
            EntityType = entityType;
            EntityId = entityId;
            ProjectId = projectId;
            ActorId = actorId;
            RecipientId = recipientId;
            Description = description;
            PayloadJson = payloadJson;
        }
    }
}
