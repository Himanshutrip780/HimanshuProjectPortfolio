using Workflow.IO.Shared.Contracts;

namespace Workflow.IO.Shared.IntegrationEvents
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync(
            IntegrationEventRequest integrationEvent,
            CancellationToken cancellationToken = default);
    }
}
