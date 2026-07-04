using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.Contracts;

namespace Workflow.IO.Shared.IntegrationEvents
{
    public class OutboxIntegrationEventPublisher<TDbContext>
        : IIntegrationEventPublisher
        where TDbContext : DbContext, IOutboxDbContext
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        private readonly TDbContext _context;

        public OutboxIntegrationEventPublisher(
            TDbContext context)
        {
            _context = context;
        }

        public async Task PublishAsync(
            IntegrationEventRequest integrationEvent,
            CancellationToken cancellationToken = default)
        {
            if (integrationEvent.EventId == Guid.Empty)
            {
                integrationEvent.EventId = Guid.NewGuid();
            }

            if (integrationEvent.OccurredAtUtc == default)
            {
                integrationEvent.OccurredAtUtc = DateTime.UtcNow;
            }

            var payload =
                JsonSerializer.Serialize(
                    integrationEvent,
                    JsonOptions);

            await _context.OutboxMessages.AddAsync(
                new OutboxMessage(
                    integrationEvent.EventId,
                    integrationEvent.EventType,
                    payload),
                cancellationToken);
        }
    }
}
