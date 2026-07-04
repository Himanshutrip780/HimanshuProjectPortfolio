using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client.Events;
using Workflow.IO.Shared.Contracts;

namespace Workflow.IO.Shared.IntegrationEvents
{
    public static class IdempotentEventConsumerHelper
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public static IntegrationEventRequest? Deserialize(
            BasicDeliverEventArgs args)
        {
            var json =
                Encoding.UTF8.GetString(args.Body.ToArray());

            return JsonSerializer.Deserialize<IntegrationEventRequest>(
                json,
                JsonOptions);
        }

        public static async Task<bool> IsAlreadyProcessedAsync(
            DbContext context,
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            if (context is not IProcessedEventsDbContext processedContext)
            {
                return false;
            }

            return await processedContext.ProcessedEvents
                .AnyAsync(
                    x => x.EventId == eventId,
                    cancellationToken);
        }

        public static async Task MarkProcessedAsync(
            DbContext context,
            IntegrationEventRequest integrationEvent,
            CancellationToken cancellationToken = default)
        {
            if (context is not IProcessedEventsDbContext processedContext)
            {
                return;
            }

            await processedContext.ProcessedEvents.AddAsync(
                new ProcessedIntegrationEvent(
                    integrationEvent.EventId,
                    integrationEvent.EventType),
                cancellationToken);
        }
    }
}
