namespace TaskApi.Model.Domain.Entities
{
    public class SubTask
    {
        public Guid SubTaskId { get; private set; }

        public Guid TaskId { get; private set; }

        public string Title { get; private set; } = string.Empty;

        public bool IsCompleted { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        private SubTask()
        {
        }

        public SubTask(
            Guid taskId,
            string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Subtask title is required");
            }

            SubTaskId = Guid.NewGuid();

            TaskId = taskId;

            Title = title.Trim();

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            IsCompleted = true;

            UpdatedAt = DateTime.UtcNow;
        }

        public void Reopen()
        {
            IsCompleted = false;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
