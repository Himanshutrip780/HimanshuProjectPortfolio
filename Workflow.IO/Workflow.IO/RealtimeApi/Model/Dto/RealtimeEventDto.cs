namespace RealtimeApi.Model.Dto
{
    public class RealtimeEventDto
    {
        public string EventType { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public Guid EntityId { get; set; }

        public Guid? ProjectId { get; set; }

        public Guid? TaskId { get; set; }

        public Guid? ActorId { get; set; }

        public Guid? RecipientId { get; set; }

        public string? Description { get; set; }

        public string? PayloadJson { get; set; }

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
