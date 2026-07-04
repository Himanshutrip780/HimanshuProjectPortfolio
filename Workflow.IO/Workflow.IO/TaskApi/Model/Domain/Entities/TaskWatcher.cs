namespace TaskApi.Model.Domain.Entities
{
    public class TaskWatcher
    {
        public Guid TaskWatcherId { get; private set; }

        public Guid TaskId { get; private set; }

        public Guid UserId { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private TaskWatcher()
        {
        }

        public TaskWatcher(
            Guid taskId,
            Guid userId)
        {
            TaskWatcherId = Guid.NewGuid();

            TaskId = taskId;

            UserId = userId;

            CreatedAt = DateTime.UtcNow;
        }
    }
}
