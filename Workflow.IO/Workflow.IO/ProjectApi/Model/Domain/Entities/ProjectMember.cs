using ProjectApi.Model.Domain.Enums;
using System;

namespace ProjectApi.Model.Domain.Entities
{
    public class ProjectMember
    {
        public Guid ProjectId { get; private set; }
        
        public Guid UserId { get; private set; }
        
        public ProjectRole Role { get; private set; }
        
        public DateTime JoinedAt { get; private set; }
        
        public DateTime UpdatedAt { get; private set; }

        public Project Project { get; private set; } = null!;

        private ProjectMember() { }

        public ProjectMember(Guid projectId, Guid userId, ProjectRole role)
        {
            ProjectId = projectId;
            UserId = userId;
            Role = role;
            JoinedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public ProjectMember(Project project, Guid userId, ProjectRole role)
        {
            Project = project;
            ProjectId = project.ProjectId;
            UserId = userId;
            Role = role;
            JoinedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeRole(ProjectRole role)
        {
            Role = role;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
