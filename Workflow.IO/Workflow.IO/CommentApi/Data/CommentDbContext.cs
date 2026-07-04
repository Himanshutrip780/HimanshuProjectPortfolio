using CommentApi.Model.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.IntegrationEvents;

namespace CommentApi.Data
{
    public class CommentDbContext : DbContext, IOutboxDbContext
    {
        public CommentDbContext(
            DbContextOptions<CommentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<CommentMention> CommentMentions { get; set; }

        public DbSet<CommentReaction> CommentReactions { get; set; }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("comment");

            modelBuilder.Entity<Comment>()
                .HasKey(x => x.CommentId);

            modelBuilder.Entity<Comment>()
                .Property(x => x.Body)
                .IsRequired()
                .HasMaxLength(4000);

            modelBuilder.Entity<Comment>()
                .HasIndex(x => x.TaskId);

            modelBuilder.Entity<Comment>()
                .HasIndex(x => x.AuthorId);

            modelBuilder.Entity<Comment>()
                .HasIndex(x => x.ParentCommentId);

            modelBuilder.Entity<CommentMention>()
                .HasKey(x => x.CommentMentionId);

            modelBuilder.Entity<CommentMention>()
                .HasIndex(x =>
                    new
                    {
                        x.CommentId,
                        x.MentionedUserId
                    })
                .IsUnique();

            modelBuilder.Entity<CommentReaction>()
                .HasKey(x => x.CommentReactionId);

            modelBuilder.Entity<CommentReaction>()
                .HasIndex(x => new { x.CommentId, x.UserId, x.Emoji })
                .IsUnique();

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
