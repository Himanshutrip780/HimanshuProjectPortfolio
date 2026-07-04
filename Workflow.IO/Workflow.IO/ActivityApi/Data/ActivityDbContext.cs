using ActivityApi.Model.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.Extensions;
using Workflow.IO.Shared.IntegrationEvents;

namespace ActivityApi.Data
{
    public class ActivityDbContext
        : DbContext, IProcessedEventsDbContext
    {
        public ActivityDbContext(
            DbContextOptions<ActivityDbContext> options)
            : base(options)
        {
        }

        public DbSet<ActivityRecord> Activities { get; set; }

        public DbSet<ProcessedIntegrationEvent> ProcessedEvents { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("activity");

            modelBuilder.Entity<ActivityRecord>()
                .HasKey(x => x.ActivityRecordId);

            modelBuilder.Entity<ActivityRecord>()
                .Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ActivityRecord>()
                .Property(x => x.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ActivityRecord>()
                .Property(x => x.Description)
                .HasMaxLength(4000);

            modelBuilder.Entity<ActivityRecord>()
                .HasIndex(x => new { x.EntityType, x.EntityId });

            modelBuilder.Entity<ActivityRecord>()
                .HasIndex(x => x.CreatedAt);

            modelBuilder.ConfigureProcessedIntegrationEvents();
        }
    }
}
