using Microsoft.EntityFrameworkCore;
using TaskApi.Model.Domain.Entities;
using TaskApi.Model.Domain.Enums;
using TaskApi.Model.Domain.Enums;
using Workflow.IO.Shared.IntegrationEvents;
using Workflow.IO.Shared.Contracts;

namespace TaskApi.Data
{
    public class TaskDbContext : DbContext, IOutboxDbContext
    {
        private readonly ITenantContext _tenantContext;

        public TaskDbContext(DbContextOptions<TaskDbContext> options, ITenantContext tenantContext)
            : base(options)
        {
            _tenantContext = tenantContext;
        }

        public Guid? CurrentTenantId => _tenantContext.CurrentOrganizationId;

        public DbSet<TaskItem> Tasks { get; set; }

        public DbSet<TaskLabel> TaskLabels { get; set; }

        public DbSet<SubTask> SubTasks { get; set; }

        public DbSet<Board> Boards { get; set; }

        public DbSet<BoardColumn> BoardColumns { get; set; }

        public DbSet<Sprint> Sprints { get; set; }

        public DbSet<Epic> Epics { get; set; }

        public DbSet<TaskWatcher> TaskWatchers { get; set; }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        public DbSet<ProjectIssueCounter> ProjectIssueCounters { get; set; }

        public DbSet<Component> Components { get; set; }

        public DbSet<ReleaseVersion> ReleaseVersions { get; set; }

        public DbSet<TaskLink> TaskLinks { get; set; }

        public DbSet<WorkLog> WorkLogs { get; set; }

        public DbSet<SavedFilter> SavedFilters { get; set; }

        public DbSet<CalendarDay> CalendarDays { get; set; }

        public DbSet<DailyUpdateState> DailyUpdateStates { get; set; }

        public DbSet<Team> Teams { get; set; }

        public DbSet<TeamMember> TeamMembers { get; set; }

        public DbSet<AutomationRule> AutomationRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("task");

            modelBuilder.Entity<TaskItem>()
                .HasKey(x => x.TaskId);

            modelBuilder.Entity<TaskItem>()
                .HasQueryFilter(x => x.OrganizationId == CurrentTenantId);

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.Description)
                .HasMaxLength(4000);

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(40);

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.Priority)
                .HasConversion<string>()
                .HasMaxLength(40);

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.IssueType)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.Resolution)
                .HasConversion<string>()
                .HasMaxLength(30);

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.IssueKey)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => x.IssueKey)
                .IsUnique();

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => new { x.ProjectId, x.IssueNumber })
                .IsUnique();

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.BacklogRank)
                .HasPrecision(18, 4);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => x.BacklogRank);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => x.ParentTaskId);

            modelBuilder.Entity<TaskItem>()
                .Ignore(x => x.IsOverdue);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => x.ProjectId);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => x.AssigneeId);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => x.SprintId);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => x.EpicId);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => x.Status);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => x.TeamId);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => new { x.ProjectId, x.Status });

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => new { x.ProjectId, x.SprintId });

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.UpdatedAt)
                .IsConcurrencyToken();

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.FeDeveloper)
                .HasMaxLength(100);

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.BeDeveloper)
                .HasMaxLength(100);

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.QaEngineer)
                .HasMaxLength(100);

            modelBuilder.Entity<OutboxMessage>()
                .Property(x => x.EventId)
                .IsRequired();

            modelBuilder.Entity<TaskLabel>()
                .HasKey(x => x.TaskLabelId);

            modelBuilder.Entity<TaskLabel>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(80);

            modelBuilder.Entity<TaskLabel>()
                .Property(x => x.Color)
                .HasMaxLength(30);

            modelBuilder.Entity<TaskLabel>()
                .HasIndex(x => new { x.TaskId, x.Name })
                .IsUnique();

            modelBuilder.Entity<SubTask>()
                .HasKey(x => x.SubTaskId);

            modelBuilder.Entity<SubTask>()
                .Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<SubTask>()
                .HasIndex(x => x.TaskId);

            modelBuilder.Entity<Board>()
                .HasKey(x => x.BoardId);

            modelBuilder.Entity<Board>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(120);

            modelBuilder.Entity<Board>()
                .HasIndex(x => x.ProjectId)
                .IsUnique();

            modelBuilder.Entity<BoardColumn>()
                .HasKey(x => x.BoardColumnId);

            modelBuilder.Entity<BoardColumn>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(80);

            modelBuilder.Entity<BoardColumn>()
                .Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(40);

            modelBuilder.Entity<BoardColumn>()
                .HasIndex(x => new { x.BoardId, x.Status })
                .IsUnique();

            modelBuilder.Entity<Sprint>()
                .HasKey(x => x.SprintId);

            modelBuilder.Entity<Sprint>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(120);

            modelBuilder.Entity<Sprint>()
                .Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(40);

            modelBuilder.Entity<Sprint>()
                .HasIndex(x => x.ProjectId);

            modelBuilder.Entity<Epic>()
                .HasKey(x => x.EpicId);

            modelBuilder.Entity<Epic>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(160);

            modelBuilder.Entity<Epic>()
                .Property(x => x.Description)
                .HasMaxLength(2000);

            modelBuilder.Entity<Epic>()
                .HasIndex(x => x.ProjectId);

            modelBuilder.Entity<TaskWatcher>()
                .HasKey(x => x.TaskWatcherId);

            modelBuilder.Entity<TaskWatcher>()
                .HasIndex(x => new { x.TaskId, x.UserId })
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

            modelBuilder.Entity<ProjectIssueCounter>()
                .HasKey(x => x.ProjectId);

            modelBuilder.Entity<ProjectIssueCounter>()
                .Property(x => x.ProjectKey)
                .IsRequired()
                .HasMaxLength(10);

            modelBuilder.Entity<Component>()
                .HasKey(x => x.ComponentId);

            modelBuilder.Entity<Component>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(120);

            modelBuilder.Entity<Component>()
                .HasIndex(x => new { x.ProjectId, x.Name })
                .IsUnique();

            modelBuilder.Entity<ReleaseVersion>()
                .HasKey(x => x.ReleaseVersionId);

            modelBuilder.Entity<ReleaseVersion>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(120);

            modelBuilder.Entity<ReleaseVersion>()
                .HasIndex(x => new { x.ProjectId, x.Name })
                .IsUnique();

            modelBuilder.Entity<TaskLink>()
                .HasKey(x => x.TaskLinkId);

            modelBuilder.Entity<TaskLink>()
                .Property(x => x.LinkType)
                .HasConversion<string>()
                .HasMaxLength(30);

            modelBuilder.Entity<TaskLink>()
                .HasIndex(x => new { x.SourceTaskId, x.TargetTaskId, x.LinkType })
                .IsUnique();

            modelBuilder.Entity<WorkLog>()
                .HasKey(x => x.WorkLogId);

            modelBuilder.Entity<WorkLog>()
                .HasIndex(x => x.TaskId);

            modelBuilder.Entity<SavedFilter>()
                .HasKey(x => x.SavedFilterId);

            modelBuilder.Entity<SavedFilter>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(120);

            modelBuilder.Entity<SavedFilter>()
                .Property(x => x.JqlQuery)
                .IsRequired()
                .HasMaxLength(4000);

            modelBuilder.Entity<CalendarDay>()
                .HasKey(x => x.Date);

            modelBuilder.Entity<DailyUpdateState>()
                .HasKey(x => x.ProjectId);

            modelBuilder.Entity<DailyUpdateState>()
                .Property(x => x.ExtraRecipients)
                .HasMaxLength(2000);

            // ✅ Team Configuration
            modelBuilder.Entity<Team>()
                .HasKey(x => x.TeamId);

            modelBuilder.Entity<Team>()
                .Property(x => x.TeamId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Team>()
                .HasQueryFilter(x => x.OrganizationId == CurrentTenantId);

            modelBuilder.Entity<Team>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(120);

            modelBuilder.Entity<Team>()
                .Property(x => x.AvatarUrl)
                .HasMaxLength(500);

            modelBuilder.Entity<Team>()
                .Property(x => x.Visibility)
                .IsRequired()
                .HasMaxLength(30);

            modelBuilder.Entity<Team>()
                .Property(x => x.Description)
                .HasMaxLength(1000);

            modelBuilder.Entity<Team>()
                .HasMany(x => x.Members)
                .WithOne(x => x.Team)
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ TeamMember Configuration
            modelBuilder.Entity<TeamMember>()
                .HasKey(x => x.TeamMemberId);

            modelBuilder.Entity<TeamMember>()
                .Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(40);

            modelBuilder.Entity<TeamMember>()
                .HasIndex(x => new { x.TeamId, x.UserId })
                .IsUnique();

            modelBuilder.Entity<AutomationRule>()
                .HasKey(x => x.AutomationRuleId);

            modelBuilder.Entity<AutomationRule>()
                .HasIndex(x => x.ProjectId);
        }
    }
}
