using ProjectApi.Model.Domain.Enums;
using System;

namespace ProjectApi.Model.Domain.Entities
{
    public class Workspace
    {
        public Guid WorkspaceId { get; set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public Guid OrganizationId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Workspace() { }

        public Workspace(string name, string? description, Guid organizationId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Workspace name is required");
            }

            WorkspaceId = Guid.NewGuid();
            Name = name.Trim();
            Description = description?.Trim();
            OrganizationId = organizationId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Workspace name is required");
            }

            Name = name.Trim();
            Description = description?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
