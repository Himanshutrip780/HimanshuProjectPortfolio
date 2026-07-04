namespace Workflow.IO.Shared.IntegrationEvents
{
    public class ProcessedIntegrationEvent
    {
        public Guid EventId { get; private set; }

        public string EventType { get; private set; } = string.Empty;

        public DateTime ProcessedAt { get; private set; }

        private ProcessedIntegrationEvent()
        {
        }

        public ProcessedIntegrationEvent(
            Guid eventId,
            string eventType)
        {
            EventId = eventId;

            EventType = eventType;

            ProcessedAt = DateTime.UtcNow;
        }
    }
}
