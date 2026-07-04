using TaskApi.Model.Dto;

namespace TaskApi.Services
{
    public interface ITaskJiraFeatureService
    {
        Task<TaskResponseDto?> GetTaskByIssueKeyAsync(
            string issueKey,
            Guid userId);

        Task<TaskResponseDto?> UpdateBacklogRankAsync(
            Guid taskId,
            Guid userId,
            UpdateBacklogRankRequestDto request);

        Task<ComponentResponseDto> CreateComponentAsync(
            Guid projectId,
            Guid userId,
            CreateComponentRequestDto request);

        Task<IEnumerable<ComponentResponseDto>> GetComponentsAsync(
            Guid projectId,
            Guid userId);

        Task<ReleaseVersionResponseDto> CreateVersionAsync(
            Guid projectId,
            Guid userId,
            CreateReleaseVersionRequestDto request);

        Task<IEnumerable<ReleaseVersionResponseDto>> GetVersionsAsync(
            Guid projectId,
            Guid userId);

        Task<ReleaseVersionResponseDto?> ReleaseVersionAsync(
            Guid versionId,
            Guid userId);

        Task<TaskLinkResponseDto?> CreateLinkAsync(
            Guid sourceTaskId,
            Guid userId,
            CreateTaskLinkRequestDto request);

        Task<IEnumerable<TaskLinkResponseDto>?> GetLinksAsync(
            Guid taskId,
            Guid userId);

        Task<bool> DeleteLinkAsync(
            Guid linkId,
            Guid userId);

        Task<WorkLogResponseDto?> AddWorkLogAsync(
            Guid taskId,
            Guid userId,
            CreateWorkLogRequestDto request);

        Task<IEnumerable<WorkLogResponseDto>?> GetWorkLogsAsync(
            Guid taskId,
            Guid userId);

        Task<BulkTaskUpdateResponseDto> BulkUpdateAsync(
            Guid projectId,
            Guid userId,
            BulkTaskUpdateRequestDto request);

        Task<SavedFilterResponseDto> CreateSavedFilterAsync(
            Guid? projectId,
            Guid userId,
            CreateSavedFilterRequestDto request);

        Task<IEnumerable<SavedFilterResponseDto>> GetSavedFiltersAsync(
            Guid? projectId,
            Guid userId);

        Task<IEnumerable<TaskResponseDto>?> ExecuteSavedFilterAsync(
            Guid filterId,
            Guid userId);

        Task<SprintResponseDto?> UpdateSprintAsync(
            Guid sprintId,
            Guid userId,
            UpdateSprintRequestDto request);

        Task<bool> DeleteSprintAsync(
            Guid sprintId,
            Guid userId);
    }
}
