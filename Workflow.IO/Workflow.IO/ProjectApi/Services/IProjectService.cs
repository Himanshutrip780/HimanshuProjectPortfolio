using ProjectApi.Model.Domain.Entities;
using ProjectApi.Model.Dto;

namespace ProjectApi.Services
{
    public interface IProjectService
    {
        Task<ProjectResponseDto> CreateProjectAsync(
            Guid ownerId,
            CreateProjectRequestDto request);

        Task<IEnumerable<ProjectResponseDto>>
            GetUserProjectsAsync(Guid userId);



        Task<ProjectResponseDto?> GetProjectByIdAsync(
            Guid projectId,
            Guid userId);

        Task<ProjectResponseDto?> GetProjectByKeyAsync(
            string projectKey,
            Guid userId);

        Task<ProjectResponseDto?> UpdateProjectAsync(
            Guid projectId,
            Guid userId,
            UpdateProjectRequestDto request);

        Task<bool> DeleteProjectAsync(
            Guid projectId,
            Guid userId);



        Task<bool> ArchiveProjectAsync(
            Guid projectId,
            Guid userId);
    }
}
