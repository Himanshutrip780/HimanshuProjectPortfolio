using ProjectApi.Model.Domain.Entities;
using ProjectApi.Model.Domain.Enums;
using ProjectApi.Repositories;
using Workflow.IO.Shared.Exceptions;

namespace ProjectApi.Authorization
{
    public class ProjectAuthorizationService
        : IProjectAuthorizationService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectAuthorizationService(
            IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task RequireProjectAccessAsync(
            Guid projectId,
            Guid userId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new ForbiddenException("Project not found or access denied.");
            }
        }

        public async Task<bool> HasAccessAsync(
            Guid projectId,
            Guid userId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            return project != null;
        }
    }
}
