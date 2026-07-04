using Microsoft.EntityFrameworkCore;
using ProjectApi.Data;
using ProjectApi.Model.Domain.Entities;

namespace ProjectApi.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ProjectDbContext _context;

        public ProjectRepository(ProjectDbContext context)
        {
            _context = context;
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            await _context.Projects.AddAsync(project);

            return project;
        }

        public async Task<IEnumerable<Project>> GetProjectsByUserAsync(
            Guid userId)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(p => p.OwnerId == userId || _context.ProjectMembers.Any(pm => pm.ProjectId == p.ProjectId && pm.UserId == userId))
                .ToListAsync();
        }

        public async Task<int> GetCountByOrgIdAsync(Guid organizationId)
        {
            return await _context.Projects
                .IgnoreQueryFilters()
                .CountAsync(p => p.OrganizationId == organizationId && p.Status != ProjectApi.Model.Domain.Enums.ProjectStatus.Archived);
        }

        public async Task<Project?> GetByIdAsync(Guid projectId)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(
                    x => x.ProjectId == projectId);
        }

        public async Task<Project?> GetByKeyAsync(string key)
        {
            var normalized =
                key.Trim().ToUpperInvariant();

            return await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Key == normalized);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            var normalized =
                key.Trim().ToUpperInvariant();

            return await _context.Projects
                .AnyAsync(x => x.Key == normalized);
        }


        public async Task<Project?> UpdateProjectAsync(
            Guid projectId,
            string name,
            string? description)
        {
            var project =
                await _context.Projects
                    .FirstOrDefaultAsync(
                        x => x.ProjectId == projectId);

            if (project == null)
            {
                return null;
            }

            project.Update(name, description);

            return project;
        }

        public async Task<bool> DeleteProjectAsync(Guid projectId)
        {
            var project =
                await _context.Projects
                    .FirstOrDefaultAsync(
                        x => x.ProjectId == projectId);

            if (project == null)
            {
                return false;
            }

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsMemberAsync(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public async Task<IEnumerable<ProjectMember>> GetMembersAsync(Guid projectId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public async Task AddMemberAsync(ProjectMember member)
        {
            await _context.ProjectMembers.AddAsync(member);
        }

        public void RemoveMember(ProjectMember member)
        {
            _context.ProjectMembers.Remove(member);
        }

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
