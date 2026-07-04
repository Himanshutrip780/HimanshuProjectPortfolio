using AnalyticsApi.Model.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.Extensions;
using Workflow.IO.Shared.IntegrationEvents;

namespace AnalyticsApi.Data
{
    public class AnalyticsDbContext
        : DbContext, IProcessedEventsDbContext
    {
        public AnalyticsDbContext(
            DbContextOptions<AnalyticsDbContext> options)
            : base(options)
        {
        }

        public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; }

        public DbSet<TaskAnalyticsItem> TaskAnalyticsItems { get; set; }

        public DbSet<ProcessedIntegrationEvent> ProcessedEvents { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("analytics");

            modelBuilder.Entity<AnalyticsEvent>()
                .HasKey(x => x.AnalyticsEventId);

            modelBuilder.Entity<AnalyticsEvent>()
                .Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<AnalyticsEvent>()
                .Property(x => x.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<AnalyticsEvent>()
                .Property(x => x.Description)
                .HasMaxLength(4000);

            modelBuilder.Entity<AnalyticsEvent>()
                .HasIndex(x => x.ProjectId);

            modelBuilder.Entity<AnalyticsEvent>()
                .HasIndex(x => x.OccurredAt);

            modelBuilder.Entity<TaskAnalyticsItem>()
                .HasKey(x => x.TaskId);

            modelBuilder.Entity<TaskAnalyticsItem>()
                .Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<TaskAnalyticsItem>()
                .Property(x => x.Priority)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<TaskAnalyticsItem>()
                .HasIndex(x => x.ProjectId);

            modelBuilder.Entity<TaskAnalyticsItem>()
                .HasIndex(x => x.SprintId);

            modelBuilder.Entity<TaskAnalyticsItem>()
                .HasIndex(x => new { x.ProjectId, x.Status });

            modelBuilder.ConfigureProcessedIntegrationEvents();
        }
    }
}
