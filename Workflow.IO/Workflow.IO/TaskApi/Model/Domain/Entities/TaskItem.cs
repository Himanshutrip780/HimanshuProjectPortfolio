using TaskApi.Model.Domain;
using TaskApi.Model.Domain.Enums;
using Workflow.IO.Shared.Exceptions;

namespace TaskApi.Model.Domain.Entities
{
    public class TaskItem
    {
        public Guid TaskId { get; private set; }

        public Guid? OrganizationId { get; private set; }

        public Guid ProjectId { get; private set; }

        public int IssueNumber { get; private set; }

        public string IssueKey { get; private set; } = string.Empty;

        public IssueType IssueType { get; private set; }

        public string Title { get; private set; }

        public string? Description { get; private set; }

        public Enums.TaskStatus Status { get; private set; }

        public TaskPriority Priority { get; private set; }

        public TaskResolution? Resolution { get; private set; }

        public Guid? AssigneeId { get; private set; }

        public Guid ReporterId { get; private set; }

        public Guid? SprintId { get; private set; }

        public Guid? EpicId { get; private set; }

        public Guid? ParentTaskId { get; private set; }

        public Guid? ComponentId { get; private set; }

        public Guid? FixVersionId { get; private set; }

        public Guid? TeamId { get; private set; }

        public int? StoryPoints { get; private set; }

        public int? OriginalEstimateMinutes { get; private set; }

        public int? RemainingEstimateMinutes { get; private set; }

        public decimal BacklogRank { get; private set; }

        public DateTime? DueDate { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public string? FeDeveloper { get; private set; }

        public string? BeDeveloper { get; private set; }

        public string? QaEngineer { get; private set; }

        public DateTime? InitialEta { get; private set; }

        public DateTime? LatestEta { get; private set; }

        public bool IsOverdue =>
            DueDate.HasValue &&
            DueDate.Value < DateTime.UtcNow &&
            Status != Enums.TaskStatus.Done;

        private TaskItem()
        {
            Title = string.Empty;
        }

        public TaskItem(
            Guid projectId,
            string projectKey,
            int issueNumber,
            string title,
            string? description,
            IssueType issueType,
            TaskPriority priority,
            Guid? assigneeId,
            Guid reporterId,
            DateTime? dueDate,
            Guid? parentTaskId,
            Guid? componentId,
            Guid? fixVersionId,
            int? originalEstimateMinutes,
            decimal backlogRank,
            string? feDeveloper = null,
            string? beDeveloper = null,
            string? qaEngineer = null,
            DateTime? initialEta = null,
            DateTime? latestEta = null,
            Guid? organizationId = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Task title is required");
            }

            if (issueType == IssueType.SubTask &&
                !parentTaskId.HasValue)
            {
                throw new ArgumentException(
                    "Sub-tasks require a parent task");
            }

            TaskId = Guid.NewGuid();

            OrganizationId = organizationId;

            ProjectId = projectId;

            IssueNumber = issueNumber;

            IssueKey = $"{projectKey.Trim().ToUpperInvariant()}-{issueNumber}";

            IssueType = issueType;

            Title = title.Trim();

            Description = description?.Trim();

            Status = Enums.TaskStatus.Todo;

            Priority = priority;

            AssigneeId = assigneeId;

            ReporterId = reporterId;

            ParentTaskId = parentTaskId;

            ComponentId = componentId;

            FixVersionId = fixVersionId;

            OriginalEstimateMinutes = originalEstimateMinutes;

            RemainingEstimateMinutes = originalEstimateMinutes;

            BacklogRank = backlogRank;

            DueDate = dueDate;

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;

            FeDeveloper = feDeveloper;

            BeDeveloper = beDeveloper;

            QaEngineer = qaEngineer;

            InitialEta = initialEta;

            LatestEta = latestEta;
        }

        public void Update(
            string title,
            string? description,
            TaskPriority priority,
            DateTime? dueDate,
            Guid? componentId,
            Guid? fixVersionId,
            int? originalEstimateMinutes,
            int? remainingEstimateMinutes,
            string? feDeveloper = null,
            string? beDeveloper = null,
            string? qaEngineer = null,
            DateTime? initialEta = null,
            DateTime? latestEta = null,
            Guid? teamId = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Task title is required");
            }

            Title = title.Trim();

            Description = description?.Trim();

            Priority = priority;

            DueDate = dueDate;

            ComponentId = componentId;

            FixVersionId = fixVersionId;

            OriginalEstimateMinutes = originalEstimateMinutes;

            if (remainingEstimateMinutes.HasValue)
            {
                RemainingEstimateMinutes = remainingEstimateMinutes;
            }

            FeDeveloper = feDeveloper;

            BeDeveloper = beDeveloper;

            QaEngineer = qaEngineer;

            InitialEta = initialEta;

            LatestEta = latestEta;

            TeamId = teamId;

            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeStatus(
            Enums.TaskStatus status,
            TaskResolution? resolution = null)
        {
            TaskWorkflow.EnsureTransition(Status, status);

            if (status == Enums.TaskStatus.Done)
            {
                Resolution = resolution ?? TaskResolution.Done;
            }
            else if (Status == Enums.TaskStatus.Done &&
                     status != Enums.TaskStatus.Done)
            {
                Resolution = null;
            }

            Status = status;

            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeStatusForColumn(
            Enums.TaskStatus status,
            TaskResolution? resolution = null)
        {
            if (!TaskWorkflow.IsValidColumnStatus(status))
            {
                throw new ArgumentException(
                    "Status is not valid for a board column");
            }

            ChangeStatus(status, resolution);
        }

        public void AssignTo(Guid? assigneeId)
        {
            AssigneeId = assigneeId;

            UpdatedAt = DateTime.UtcNow;
        }

        public void MoveToSprint(Guid? sprintId)
        {
            SprintId = sprintId;

            UpdatedAt = DateTime.UtcNow;
        }

        public void AssignEpic(Guid? epicId)
        {
            EpicId = epicId;

            UpdatedAt = DateTime.UtcNow;
        }

        public void AssignTeam(Guid? teamId)
        {
            TeamId = teamId;

            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStoryPoints(int? storyPoints)
        {
            if (storyPoints < 0)
            {
                throw new ArgumentException(
                    "Story points cannot be negative");
            }

            StoryPoints = storyPoints;

            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateBacklogRank(decimal backlogRank)
        {
            BacklogRank = backlogRank;

            UpdatedAt = DateTime.UtcNow;
        }

        public void SetParentTask(Guid? parentTaskId)
        {
            if (parentTaskId.HasValue && parentTaskId.Value == TaskId)
            {
                throw new ArgumentException("A task cannot be its own parent.");
            }
            ParentTaskId = parentTaskId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void LogWork(int minutesLogged)
        {
            if (minutesLogged <= 0)
            {
                throw new ArgumentException(
                    "Logged time must be positive");
            }

            RemainingEstimateMinutes =
                RemainingEstimateMinutes.HasValue
                    ? Math.Max(
                        0,
                        RemainingEstimateMinutes.Value - minutesLogged)
                    : null;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
