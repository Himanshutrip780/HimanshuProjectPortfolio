using ProjectApi.Model.Domain.Entities;

namespace ProjectApi.Repositories
{
    public interface IProjectRepository
    {
        Task<Project> CreateProjectAsync(Project project);

        Task<IEnumerable<Project>>
            GetProjectsByUserAsync(Guid userId);

        Task<Project?>
            GetByIdAsync(Guid projectId);

        Task<Project?> GetByKeyAsync(string key);

        Task<bool> KeyExistsAsync(string key);

        Task<int> GetCountByOrgIdAsync(Guid organizationId);

        Task<Project?> UpdateProjectAsync(
            Guid projectId,
            string name,
            string? description);

        Task<bool> DeleteProjectAsync(
            Guid projectId);

        Task<bool> IsMemberAsync(Guid projectId, Guid userId);
        
        Task<IEnumerable<ProjectMember>> GetMembersAsync(Guid projectId);
        
        Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId);
        
        Task AddMemberAsync(ProjectMember member);
        
        void RemoveMember(ProjectMember member);

        Task SaveChangesAsync();
    }
}
