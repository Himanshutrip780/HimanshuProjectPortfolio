using TaskApi.Model.Dto;

namespace TaskApi.Services
{
    public interface ITaskService
    {
        Task<TaskResponseDto>
            CreateTaskAsync(
                Guid projectId,
                Guid reporterId,
                CreateTaskRequestDto request);

        Task<IEnumerable<TaskResponseDto>>
            GetProjectTasksAsync(
                Guid projectId,
                Guid userId);

        Task<IEnumerable<TaskResponseDto>> SearchProjectTasksAsync(
            Guid projectId,
            Guid userId,
            TaskSearchRequestDto request);

        Task<TaskResponseDto?>
            GetTaskByIdAsync(
                Guid taskId,
                Guid userId);

        Task<TaskResponseDto?>
            UpdateTaskAsync(
                Guid taskId,
                Guid userId,
                UpdateTaskRequestDto request);

        Task<TaskResponseDto?>
            ChangeStatusAsync(
                Guid taskId,
                Guid userId,
                ChangeTaskStatusRequestDto request);

        Task<TaskResponseDto?>
            AssignTaskAsync(
                Guid taskId,
                Guid userId,
                AssignTaskRequestDto request);

        Task<bool>
            DeleteTaskAsync(
                Guid taskId,
                Guid userId);

        Task<TaskLabelResponseDto?> AddLabelAsync(
            Guid taskId,
            Guid userId,
            AddTaskLabelRequestDto request);

        Task<IEnumerable<TaskLabelResponseDto>?> GetLabelsAsync(
            Guid taskId,
            Guid userId);

        Task<bool> RemoveLabelAsync(
            Guid taskId,
            Guid labelId,
            Guid userId);

        Task<SubTaskResponseDto?> CreateSubTaskAsync(
            Guid taskId,
            Guid userId,
            CreateSubTaskRequestDto request);

        Task<IEnumerable<SubTaskResponseDto>?> GetSubTasksAsync(
            Guid taskId,
            Guid userId);

        Task<SubTaskResponseDto?> ChangeSubTaskCompletionAsync(
            Guid subTaskId,
            Guid userId,
            ChangeSubTaskCompletionRequestDto request);

        Task<bool> DeleteSubTaskAsync(
            Guid subTaskId,
            Guid userId);

        Task<BoardResponseDto> CreateBoardAsync(
            Guid projectId,
            Guid userId,
            CreateBoardRequestDto request);

        Task<BoardViewResponseDto?> GetBoardViewAsync(
            Guid projectId,
            Guid userId);

        Task<SprintResponseDto> CreateSprintAsync(
            Guid projectId,
            Guid userId,
            CreateSprintRequestDto request);

        Task<IEnumerable<SprintResponseDto>> GetProjectSprintsAsync(
            Guid projectId,
            Guid userId);

        Task<SprintResponseDto?> StartSprintAsync(
            Guid sprintId,
            Guid userId);

        Task<SprintResponseDto?> CompleteSprintAsync(
            Guid sprintId,
            Guid userId);

        Task<TaskResponseDto?> MoveTaskToSprintAsync(
            Guid taskId,
            Guid userId,
            MoveTaskToSprintRequestDto request);

        Task<BacklogResponseDto> GetBacklogAsync(
            Guid projectId,
            Guid userId);

        Task<EpicResponseDto> CreateEpicAsync(
            Guid projectId,
            Guid userId,
            CreateEpicRequestDto request);

        Task<IEnumerable<EpicResponseDto>> GetProjectEpicsAsync(
            Guid projectId,
            Guid userId);

        Task<TaskResponseDto?> AssignEpicAsync(
            Guid taskId,
            Guid userId,
            AssignEpicRequestDto request);

        Task<TaskResponseDto?> UpdateStoryPointsAsync(
            Guid taskId,
            Guid userId,
            UpdateStoryPointsRequestDto request);

        Task<TaskWatcherResponseDto?> WatchTaskAsync(
            Guid taskId,
            Guid userId);

        Task<IEnumerable<TaskWatcherResponseDto>?> GetWatchersAsync(
            Guid taskId,
            Guid userId);

        Task<bool> UnwatchTaskAsync(
            Guid taskId,
            Guid userId);

        Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(Guid userId);
        Task<IEnumerable<TaskResponseDto>> GetChildTasksAsync(Guid taskId, Guid userId);

        Task<AutomationRuleResponseDto> CreateAutomationRuleAsync(
            Guid projectId,
            Guid userId,
            CreateAutomationRuleRequestDto request);

        Task<IEnumerable<AutomationRuleResponseDto>> GetProjectAutomationRulesAsync(
            Guid projectId,
            Guid userId);

        Task<bool> DeleteAutomationRuleAsync(
            Guid ruleId,
            Guid userId);

        Task<AutomationRuleResponseDto?> ToggleAutomationRuleAsync(
            Guid ruleId,
            Guid userId,
            bool isEnabled);

        Task<TaskResponseDto?> AssignTeamAsync(
            Guid taskId,
            Guid userId,
            AssignTeamRequestDto request);
    }
}
