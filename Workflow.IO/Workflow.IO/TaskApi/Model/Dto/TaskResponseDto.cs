using TaskApi.Model.Domain.Enums;

namespace TaskApi.Model.Dto
{
    public class TaskResponseDto
    {
        public Guid TaskId { get; set; }

        public Guid ProjectId { get; set; }

        public int IssueNumber { get; set; }

        public string IssueKey { get; set; } = string.Empty;

        public IssueType IssueType { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Model.Domain.Enums.TaskStatus Status { get; set; }

        public TaskResolution? Resolution { get; set; }

        public TaskPriority Priority { get; set; }

        public Guid? ParentTaskId { get; set; }

        public Guid? ComponentId { get; set; }

        public Guid? FixVersionId { get; set; }

        public int? OriginalEstimateMinutes { get; set; }

        public int? RemainingEstimateMinutes { get; set; }

        public decimal BacklogRank { get; set; }

        public int TotalLoggedMinutes { get; set; }

        public Guid? AssigneeId { get; set; }

        public Guid ReporterId { get; set; }

        public Guid? SprintId { get; set; }

        public Guid? EpicId { get; set; }

        public int? StoryPoints { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsOverdue { get; set; }

        public string? FeDeveloper { get; set; }

        public string? BeDeveloper { get; set; }

        public string? QaEngineer { get; set; }

        public DateTime? InitialEta { get; set; }

        public DateTime? LatestEta { get; set; }

        public Guid? TeamId { get; set; }
    }
}
