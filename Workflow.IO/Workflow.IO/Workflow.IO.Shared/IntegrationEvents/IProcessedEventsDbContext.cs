using Microsoft.EntityFrameworkCore;

namespace Workflow.IO.Shared.IntegrationEvents
{
    public interface IProcessedEventsDbContext
    {
        DbSet<ProcessedIntegrationEvent> ProcessedEvents { get; }
    }
}
