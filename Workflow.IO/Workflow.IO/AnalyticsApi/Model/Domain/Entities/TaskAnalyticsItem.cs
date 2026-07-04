namespace AnalyticsApi.Model.Domain.Entities
{
    public class TaskAnalyticsItem
    {
        public Guid TaskId { get; private set; }

        public Guid ProjectId { get; private set; }

        public string Status { get; private set; } = "Todo";

        public string Priority { get; private set; } = "Medium";

        public Guid? AssigneeId { get; private set; }

        public Guid? SprintId { get; private set; }

        public Guid? EpicId { get; private set; }

        public int? StoryPoints { get; private set; }

        public DateTime? DueDate { get; private set; }

        public bool IsDeleted { get; private set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        private TaskAnalyticsItem()
        {
        }

        public TaskAnalyticsItem(
            Guid taskId,
            Guid projectId)
        {
            TaskId = taskId;
            ProjectId = projectId;
        }

        public void ApplySnapshot(
            string? status,
            string? priority,
            Guid? assigneeId,
            Guid? sprintId,
            Guid? epicId,
            int? storyPoints,
            DateTime? dueDate)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                Status = status;
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                Priority = priority;
            }

            AssigneeId = assigneeId;
            SprintId = sprintId;
            EpicId = epicId;
            StoryPoints = storyPoints;
            DueDate = dueDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkDeleted()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
