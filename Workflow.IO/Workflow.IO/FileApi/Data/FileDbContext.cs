using FileApi.Model.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.IntegrationEvents;

namespace FileApi.Data
{
    public class FileDbContext : DbContext, IOutboxDbContext
    {
        public FileDbContext(
            DbContextOptions<FileDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileAttachment> FileAttachments { get; set; }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("file");

            modelBuilder.Entity<FileAttachment>()
                .HasKey(x => x.FileAttachmentId);

            modelBuilder.Entity<FileAttachment>()
                .Property(x => x.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<FileAttachment>()
                .Property(x => x.StoredFileName)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<FileAttachment>()
                .Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<FileAttachment>()
                .Property(x => x.StoragePath)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<FileAttachment>()
                .HasIndex(x => x.TaskId);

            modelBuilder.Entity<OutboxMessage>()
                .HasKey(x => x.OutboxMessageId);

            modelBuilder.Entity<OutboxMessage>()
                .Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<OutboxMessage>()
                .Property(x => x.PayloadJson)
                .IsRequired();

            modelBuilder.Entity<OutboxMessage>()
                .HasIndex(x => x.PublishedAt);
        }
    }
}
