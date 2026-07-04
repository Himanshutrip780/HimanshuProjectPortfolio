namespace TaskApi.Model.Domain.Entities
{
    public class Component
    {
        public Guid ComponentId { get; private set; }

        public Guid ProjectId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private Component()
        {
        }

        public Component(
            Guid projectId,
            string name,
            string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Component name is required");
            }

            ComponentId = Guid.NewGuid();

            ProjectId = projectId;

            Name = name.Trim();

            Description = description?.Trim();

            CreatedAt = DateTime.UtcNow;
        }
    }
}
