using TaskApi.Model.Domain.Entities;
using TaskApi.Model.Dto;
using TaskStatus = TaskApi.Model.Domain.Enums.TaskStatus;

namespace TaskApi.Repositories
{
    public interface ITaskRepository
    {
        Task<TaskItem> CreateTaskAsync(TaskItem task);

        Task<IEnumerable<TaskItem>>
            GetProjectTasksAsync(Guid projectId);

        Task<Dictionary<TaskStatus, List<TaskItem>>>
            GetProjectTasksGroupedByStatusAsync(Guid projectId);

        Task<IEnumerable<TaskItem>> SearchProjectTasksAsync(
            Guid projectId,
            TaskSearchRequestDto request);

        Task<TaskItem?>
            GetByIdAsync(Guid taskId);

        Task<TaskLabel> AddLabelAsync(TaskLabel label);

        Task<IEnumerable<TaskLabel>> GetLabelsAsync(Guid taskId);

        Task<TaskLabel?> GetLabelByIdAsync(Guid labelId);

        Task RemoveLabelAsync(TaskLabel label);

        Task<SubTask> CreateSubTaskAsync(SubTask subTask);

        Task<IEnumerable<SubTask>> GetSubTasksAsync(Guid taskId);

        Task<SubTask?> GetSubTaskByIdAsync(Guid subTaskId);

        Task RemoveSubTaskAsync(SubTask subTask);

        Task<Board> CreateBoardAsync(Board board);

        Task<Board?> GetBoardByProjectIdAsync(Guid projectId);

        Task<Board?> GetBoardByIdAsync(Guid boardId);

        Task AddBoardColumnsAsync(IEnumerable<BoardColumn> columns);

        Task<IEnumerable<BoardColumn>> GetBoardColumnsAsync(Guid boardId);

        Task<Sprint> CreateSprintAsync(Sprint sprint);

        Task<IEnumerable<Sprint>> GetProjectSprintsAsync(Guid projectId);

        Task<Sprint?> GetSprintByIdAsync(Guid sprintId);

        Task<Sprint?> GetActiveSprintForProjectAsync(
            Guid projectId,
            Guid? excludeSprintId = null);

        Task<IEnumerable<TaskItem>> GetBacklogTasksAsync(Guid projectId);

        Task<IEnumerable<TaskItem>> GetSprintTasksAsync(Guid sprintId);

        Task<Epic> CreateEpicAsync(Epic epic);

        Task<IEnumerable<Epic>> GetProjectEpicsAsync(Guid projectId);

        Task<Epic?> GetEpicByIdAsync(Guid epicId);

        Task<TaskWatcher?> GetWatcherAsync(
            Guid taskId,
            Guid userId);

        Task<TaskWatcher> AddWatcherAsync(TaskWatcher watcher);

        Task<IEnumerable<TaskWatcher>> GetWatchersAsync(Guid taskId);

        Task RemoveWatcherAsync(TaskWatcher watcher);

        Task<bool>
            DeleteTaskAsync(Guid taskId);

        Task<(string ProjectKey, int IssueNumber)> AllocateIssueNumberAsync(
            Guid projectId,
            string projectKey);

        Task<decimal> GetNextBacklogRankAsync(Guid projectId);

        Task<TaskItem?> GetByIssueKeyAsync(string issueKey);

        Task AddComponentAsync(Component component);

        Task<IEnumerable<Component>> GetComponentsAsync(Guid projectId);

        Task AddVersionAsync(ReleaseVersion version);

        Task<IEnumerable<ReleaseVersion>> GetVersionsAsync(Guid projectId);

        Task<ReleaseVersion?> GetVersionByIdAsync(Guid versionId);

        Task AddLinkAsync(TaskLink link);

        Task<IEnumerable<TaskLinkInfo>> GetLinksForTaskAsync(Guid taskId);

        Task<TaskLink?> GetLinkByIdAsync(Guid linkId);

        Task RemoveLinkAsync(TaskLink link);

        Task AddWorkLogAsync(WorkLog workLog);

        Task<IEnumerable<WorkLog>> GetWorkLogsAsync(Guid taskId);

        Task<int> GetTotalLoggedMinutesAsync(Guid taskId);

        Task AddSavedFilterAsync(SavedFilter filter);

        Task<IEnumerable<SavedFilter>> GetSavedFiltersAsync(
            Guid userId,
            Guid? projectId);

        Task<SavedFilter?> GetSavedFilterByIdAsync(Guid filterId);

        Task<bool> DeleteSprintAsync(Guid sprintId);

        Task<IEnumerable<TaskItem>> GetAllTasksAsync();
        Task<IEnumerable<TaskItem>> GetChildTasksAsync(Guid taskId);

        Task AddTeamAsync(Team team);
        Task<Team?> GetTeamByIdAsync(Guid teamId);
        Task<IEnumerable<Team>> GetVisibleTeamsAsync(Guid userId);
        Task<TeamMember?> GetTeamMemberAsync(Guid teamId, Guid userId);
        Task AddTeamMemberAsync(TeamMember member);
        Task RemoveTeamMemberAsync(TeamMember member);
        Task<IEnumerable<Guid>> GetUserTeamIdsAsync(Guid userId);
        Task<IEnumerable<Team>> GetTeamsWithTasksAsync();

        Task<AutomationRule> CreateAutomationRuleAsync(AutomationRule rule);
        Task<IEnumerable<AutomationRule>> GetProjectAutomationRulesAsync(Guid projectId);
        Task<AutomationRule?> GetAutomationRuleByIdAsync(Guid ruleId);
        Task RemoveAutomationRuleAsync(AutomationRule rule);

        Task SaveChangesAsync();
    }

    public record TaskLinkInfo(TaskLink Link, string TargetIssueKey);
}
