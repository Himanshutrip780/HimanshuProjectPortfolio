using Workflow.IO.Shared.Contracts;

namespace AnalyticsApi.Services
{
    public interface IAnalyticsProjectionService
    {
        Task ProjectAsync(
            IntegrationEventRequest integrationEvent,
            CancellationToken cancellationToken = default);
    }
}
