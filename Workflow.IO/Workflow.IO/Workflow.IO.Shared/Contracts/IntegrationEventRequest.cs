using System.ComponentModel.DataAnnotations;

namespace Workflow.IO.Shared.Contracts
{
    public class IntegrationEventRequest
    {
        public Guid EventId { get; set; } = Guid.NewGuid();

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public Guid EntityId { get; set; }

        public Guid? ActorId { get; set; }

        public Guid? RecipientId { get; set; }

        [MaxLength(4000)]
        public string? Description { get; set; }

        public string? PayloadJson { get; set; }

        public string? CorrelationId { get; set; }
    }
}
