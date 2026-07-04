namespace Workflow.IO.Shared.IntegrationEvents
{
    public class OutboxMessage
    {
        public Guid OutboxMessageId { get; private set; }

        public Guid EventId { get; private set; }

        public string EventType { get; private set; } = string.Empty;

        public string PayloadJson { get; private set; } = string.Empty;

        public DateTime CreatedAt { get; private set; }

        public DateTime? PublishedAt { get; private set; }

        public int RetryCount { get; private set; }

        public string? LastError { get; private set; }

        private OutboxMessage()
        {
        }

        public OutboxMessage(
            Guid eventId,
            string eventType,
            string payloadJson)
        {
            OutboxMessageId = Guid.NewGuid();

            EventId = eventId;

            EventType = eventType;

            PayloadJson = payloadJson;

            CreatedAt = DateTime.UtcNow;
        }

        public void MarkPublished()
        {
            PublishedAt = DateTime.UtcNow;

            LastError = null;
        }

        public void MarkFailed(string error)
        {
            RetryCount++;

            LastError = error;
        }
    }
}
