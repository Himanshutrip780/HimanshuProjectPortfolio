namespace NotificationApi.Model.Domain.Entities
{
    public class Notification
    {
        public Guid NotificationId { get; private set; }

        public Guid? RecipientId { get; private set; }

        public string EventType { get; private set; } = string.Empty;

        public string EntityType { get; private set; } = string.Empty;

        public Guid EntityId { get; private set; }

        public string Message { get; private set; } = string.Empty;

        public bool IsRead { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private Notification()
        {
        }

        public Notification(
            Guid? recipientId,
            string eventType,
            string entityType,
            Guid entityId,
            string message)
        {
            NotificationId = Guid.NewGuid();

            RecipientId = recipientId;

            EventType = eventType;

            EntityType = entityType;

            EntityId = entityId;

            Message = message;

            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
