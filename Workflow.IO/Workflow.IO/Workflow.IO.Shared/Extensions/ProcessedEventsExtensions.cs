using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.IntegrationEvents;

namespace Workflow.IO.Shared.Extensions
{
    public static class ProcessedEventsExtensions
    {
        public static void ConfigureProcessedIntegrationEvents(
            this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessedIntegrationEvent>()
                .HasKey(x => x.EventId);

            modelBuilder.Entity<ProcessedIntegrationEvent>()
                .Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<ProcessedIntegrationEvent>()
                .HasIndex(x => x.ProcessedAt);
        }
    }
}
