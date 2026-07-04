namespace TaskApi.Model.Domain.Entities
{
    public class Epic
    {
        public Guid EpicId { get; private set; }

        public Guid ProjectId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        private Epic()
        {
        }

        public Epic(
            Guid projectId,
            string name,
            string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Epic name is required");
            }

            EpicId = Guid.NewGuid();

            ProjectId = projectId;

            Name = name.Trim();

            Description = description?.Trim();

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
