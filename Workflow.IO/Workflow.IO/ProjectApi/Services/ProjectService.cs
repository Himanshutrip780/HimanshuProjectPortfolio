using AutoMapper;
using ProjectApi.Authorization;
using ProjectApi.Model.Domain;
using ProjectApi.Model.Domain.Entities;
using ProjectApi.Model.Domain.Enums;
using ProjectApi.Model.Dto;
using ProjectApi.Repositories;
using Workflow.IO.Shared.Exceptions;
using Workflow.IO.Shared.Persistence;
using Workflow.IO.Shared.Contracts;

namespace ProjectApi.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectAuthorizationService _authorization;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public ProjectService(
            IProjectRepository projectRepository,
            IProjectAuthorizationService authorization,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ITenantContext tenantContext)
        {
            _projectRepository = projectRepository;
            _authorization = authorization;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<ProjectResponseDto> CreateProjectAsync(
            Guid ownerId,
            CreateProjectRequestDto request)
        {
            var key =
                string.IsNullOrWhiteSpace(request.Key)
                    ? ProjectKeyGenerator.FromName(request.Name)
                    : request.Key.Trim().ToUpperInvariant();

            if (await _projectRepository.KeyExistsAsync(key))
            {
                throw new ConflictException(
                    $"Project key '{key}' is already in use");
            }

            if (!_tenantContext.CurrentOrganizationId.HasValue)
            {
                throw new InvalidOperationException(
                    "Organization context is missing when creating a project");
            }

            if (!_tenantContext.CurrentWorkspaceId.HasValue)
            {
                throw new InvalidOperationException(
                    "Workspace context is missing when creating a project");
            }

            var project = new Project(
                request.Name,
                request.Description,
                ownerId,
                key,
                _tenantContext.CurrentOrganizationId.Value,
                _tenantContext.CurrentWorkspaceId.Value,
                request.ProjectType);

            await _projectRepository.CreateProjectAsync(project);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProjectResponseDto>(project);
        }

        public async Task<IEnumerable<ProjectResponseDto>>
            GetUserProjectsAsync(Guid userId)
        {
            var projects =
                await _projectRepository
                    .GetProjectsByUserAsync(userId);

            return _mapper.Map<IEnumerable<ProjectResponseDto>>(
                projects);
        }



        public async Task<ProjectResponseDto?> GetProjectByIdAsync(
            Guid projectId,
            Guid userId)
        {

            var project =
                await _projectRepository.GetByIdAsync(projectId);

            if (project == null)
            {
                return null;
            }

            if (project.OwnerId != userId)
            {
                var isMember = await _projectRepository.IsMemberAsync(projectId, userId);
                if (!isMember)
                {
                    return null;
                }
            }

            return _mapper.Map<ProjectResponseDto>(project);
        }

        public async Task<ProjectResponseDto?> GetProjectByKeyAsync(
            string projectKey,
            Guid userId)
        {
            var project =
                await _projectRepository.GetByKeyAsync(projectKey);

            if (project == null)
            {
                return null;
            }

            return await GetProjectByIdAsync(
                project.ProjectId,
                userId);
        }

        public async Task<ProjectResponseDto?> UpdateProjectAsync(
            Guid projectId,
            Guid userId,
            UpdateProjectRequestDto request)
        {

            var project =
                await _projectRepository.GetByIdAsync(projectId);

            if (project == null)
            {
                return null;
            }

            project.Update(request.Name, request.Description);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProjectResponseDto>(project);
        }

        public async Task<bool> DeleteProjectAsync(
            Guid projectId,
            Guid userId)
        {

            return await _projectRepository.DeleteProjectAsync(
                projectId);
        }



        public async Task<bool> ArchiveProjectAsync(
            Guid projectId,
            Guid userId)
        {

            var project =
                await _projectRepository.GetByIdAsync(projectId);

            if (project == null)
            {
                return false;
            }

            project.Archive();

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
