using Microsoft.EntityFrameworkCore;
using NotificationApi.Model.Domain.Entities;
using Workflow.IO.Shared.Extensions;
using Workflow.IO.Shared.IntegrationEvents;

namespace NotificationApi.Data
{
    public class NotificationDbContext
        : DbContext, IProcessedEventsDbContext
    {
        public NotificationDbContext(
            DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<ProcessedIntegrationEvent> ProcessedEvents { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("notification");

            modelBuilder.Entity<Notification>()
                .HasKey(x => x.NotificationId);

            modelBuilder.Entity<Notification>()
                .Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Notification>()
                .Property(x => x.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Notification>()
                .Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(4000);

            modelBuilder.Entity<Notification>()
                .HasIndex(x => x.RecipientId);

            modelBuilder.ConfigureProcessedIntegrationEvents();
        }
    }
}
