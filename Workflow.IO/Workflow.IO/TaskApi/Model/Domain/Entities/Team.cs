using System;
using System.Collections.Generic;

namespace TaskApi.Model.Domain.Entities
{
    public class Team
    {
        public Guid TeamId { get; set; }
        public string Name { get; private set; }
        public string? AvatarUrl { get; private set; }
        public Guid LeadId { get; private set; }
        public string Visibility { get; private set; } // "Public" or "Private"
        public string? Description { get; private set; }
        public bool IsArchived { get; private set; }
        public Guid OrganizationId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public ICollection<TeamMember> Members { get; private set; } = new List<TeamMember>();

        private Team()
        {
            Name = string.Empty;
            Visibility = "Public";
        }

        public Team(string name, string? avatarUrl, Guid leadId, string visibility, string? description, Guid organizationId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Team name is required");
            }

            TeamId = Guid.NewGuid();
            Name = name.Trim();
            AvatarUrl = avatarUrl;
            LeadId = leadId;
            Visibility = string.IsNullOrWhiteSpace(visibility) ? "Public" : visibility.Trim();
            Description = description?.Trim();
            OrganizationId = organizationId;
            IsArchived = false;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string? avatarUrl, Guid leadId, string visibility, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Team name is required");
            }

            Name = name.Trim();
            AvatarUrl = avatarUrl;
            LeadId = leadId;
            Visibility = string.IsNullOrWhiteSpace(visibility) ? "Public" : visibility.Trim();
            Description = description?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetLead(Guid leadId)
        {
            LeadId = leadId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Archive()
        {
            IsArchived = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsArchived = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
