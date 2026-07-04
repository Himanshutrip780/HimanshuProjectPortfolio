namespace TaskApi.Model.Domain.Entities
{
    public class TaskLabel
    {
        public Guid TaskLabelId { get; private set; }

        public Guid TaskId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Color { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private TaskLabel()
        {
        }

        public TaskLabel(
            Guid taskId,
            string name,
            string? color)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Label name is required");
            }

            TaskLabelId = Guid.NewGuid();

            TaskId = taskId;

            Name = name.Trim();

            Color = color?.Trim();

            CreatedAt = DateTime.UtcNow;
        }
    }
}
