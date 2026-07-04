namespace TaskApi.Model.Domain.Entities
{
    public class WorkLog
    {
        public Guid WorkLogId { get; private set; }

        public Guid TaskId { get; private set; }

        public Guid UserId { get; private set; }

        public int TimeSpentMinutes { get; private set; }

        public string? Comment { get; private set; }

        public DateTime StartedAt { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private WorkLog()
        {
        }

        public WorkLog(
            Guid taskId,
            Guid userId,
            int timeSpentMinutes,
            string? comment,
            DateTime startedAt)
        {
            if (timeSpentMinutes <= 0)
            {
                throw new ArgumentException(
                    "Time spent must be greater than zero");
            }

            WorkLogId = Guid.NewGuid();

            TaskId = taskId;

            UserId = userId;

            TimeSpentMinutes = timeSpentMinutes;

            Comment = comment?.Trim();

            StartedAt = startedAt;

            CreatedAt = DateTime.UtcNow;
        }
    }
}
