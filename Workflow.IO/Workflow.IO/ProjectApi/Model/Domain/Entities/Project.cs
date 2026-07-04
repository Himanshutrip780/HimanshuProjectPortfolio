using ProjectApi.Model.Domain;
using ProjectApi.Model.Domain.Enums;

namespace ProjectApi.Model.Domain.Entities
{
    public class Project
    {
        public Guid ProjectId { get; set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public Guid OwnerId { get; private set; }

        public string Key { get; private set; } = string.Empty;

        public ProjectType ProjectType { get; private set; }

        public ProjectStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public Guid OrganizationId { get; private set; }

        public Guid WorkspaceId { get; private set; }

        // Removed Members collection for single-tenant architecture

        private Project() { }

        public Project(
            string name,
            string? description,
            Guid ownerId,
            string key,
            Guid organizationId,
            Guid workspaceId,
            ProjectType projectType = ProjectType.Scrum)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Project name is required");
            }

            ProjectKeyGenerator.Validate(key);

            ProjectId = Guid.NewGuid();

            Name = name.Trim();

            Description = description?.Trim();

            OwnerId = ownerId;
            
            OrganizationId = organizationId;

            WorkspaceId = workspaceId;

            Key = key.Trim().ToUpperInvariant();

            ProjectType = projectType;

            Status = ProjectStatus.Active;

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            string? description)
        {
            Name = name.Trim();

            Description = description?.Trim();

            UpdatedAt = DateTime.UtcNow;
        }

        public void Archive()
        {
            Status = ProjectStatus.Archived;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
