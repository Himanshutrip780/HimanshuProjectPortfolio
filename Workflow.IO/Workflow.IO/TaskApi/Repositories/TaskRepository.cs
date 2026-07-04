using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.Model.Domain.Entities;
using TaskApi.Model.Domain.Enums;
using TaskApi.Model.Dto;
using TaskStatus = TaskApi.Model.Domain.Enums.TaskStatus;

namespace TaskApi.Repositories
{
    public class TaskRepository
        : ITaskRepository
    {
        private static readonly System.Threading.SemaphoreSlim _issueNumberLock = new System.Threading.SemaphoreSlim(1, 1);
        private readonly TaskDbContext _context;

        public TaskRepository(TaskDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(task);

            return task;
        }

        public async Task<IEnumerable<TaskItem>>
            GetProjectTasksAsync(Guid projectId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.DueDate)
                .ToListAsync();
        }

        public async Task<Dictionary<TaskStatus, List<TaskItem>>>
            GetProjectTasksGroupedByStatusAsync(Guid projectId)
        {
            var tasks = await _context.Tasks
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .ToListAsync();

            return tasks
                .GroupBy(x => x.Status)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(x => x.Priority)
                        .ThenBy(x => x.DueDate)
                        .ToList());
        }

        public async Task<IEnumerable<TaskItem>> SearchProjectTasksAsync(
            Guid projectId,
            TaskSearchRequestDto request)
        {
            var query =
                _context.Tasks
                    .AsNoTracking()
                    .Where(x => x.ProjectId == projectId);

            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var search = request.Query.Trim();

                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    (x.Description != null &&
                     x.Description.Contains(search)));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(x =>
                    x.Status == request.Status.Value);
            }

            if (request.Priority.HasValue)
            {
                query = query.Where(x =>
                    x.Priority == request.Priority.Value);
            }

            if (request.AssigneeId.HasValue)
            {
                query = query.Where(x =>
                    x.AssigneeId == request.AssigneeId.Value);
            }

            if (request.ReporterId.HasValue)
            {
                query = query.Where(x =>
                    x.ReporterId == request.ReporterId.Value);
            }

            if (request.SprintId.HasValue)
            {
                query = query.Where(x =>
                    x.SprintId == request.SprintId.Value);
            }

            if (request.EpicId.HasValue)
            {
                query = query.Where(x =>
                    x.EpicId == request.EpicId.Value);
            }

            if (request.TeamId.HasValue)
            {
                query = query.Where(x =>
                    x.TeamId == request.TeamId.Value);
            }

            if (request.IsOverdue == true)
            {
                var now = DateTime.UtcNow;

                query = query.Where(x =>
                    x.DueDate != null &&
                    x.DueDate < now &&
                    x.Status != TaskStatus.Done);
            }
            else if (request.IsOverdue == false)
            {
                var now = DateTime.UtcNow;

                query = query.Where(x =>
                    x.DueDate == null ||
                    x.DueDate >= now ||
                    x.Status == TaskStatus.Done);
            }

            return await query
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.DueDate)
                .ToListAsync();
        }

        public async Task<TaskItem?>
            GetByIdAsync(Guid taskId)
        {
            return await _context.Tasks
                .FirstOrDefaultAsync(
                    x => x.TaskId == taskId);
        }

        public async Task<TaskLabel> AddLabelAsync(TaskLabel label)
        {
            await _context.TaskLabels.AddAsync(label);

            return label;
        }

        public async Task<IEnumerable<TaskLabel>> GetLabelsAsync(
            Guid taskId)
        {
            return await _context.TaskLabels
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<TaskLabel?> GetLabelByIdAsync(Guid labelId)
        {
            return await _context.TaskLabels
                .FirstOrDefaultAsync(
                    x => x.TaskLabelId == labelId);
        }

        public async Task RemoveLabelAsync(TaskLabel label)
        {
            _context.TaskLabels.Remove(label);
        }

        public async Task<SubTask> CreateSubTaskAsync(SubTask subTask)
        {
            await _context.SubTasks.AddAsync(subTask);

            return subTask;
        }

        public async Task<IEnumerable<SubTask>> GetSubTasksAsync(
            Guid taskId)
        {
            return await _context.SubTasks
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<SubTask?> GetSubTaskByIdAsync(Guid subTaskId)
        {
            return await _context.SubTasks
                .FirstOrDefaultAsync(
                    x => x.SubTaskId == subTaskId);
        }

        public async Task RemoveSubTaskAsync(SubTask subTask)
        {
            _context.SubTasks.Remove(subTask);
        }

        public async Task<Board> CreateBoardAsync(Board board)
        {
            await _context.Boards.AddAsync(board);

            return board;
        }

        public async Task<Board?> GetBoardByProjectIdAsync(
            Guid projectId)
        {
            return await _context.Boards
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ProjectId == projectId);
        }

        public async Task<Board?> GetBoardByIdAsync(Guid boardId)
        {
            return await _context.Boards
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.BoardId == boardId);
        }

        public async Task AddBoardColumnsAsync(
            IEnumerable<BoardColumn> columns)
        {
            await _context.BoardColumns.AddRangeAsync(columns);
        }

        public async Task<IEnumerable<BoardColumn>> GetBoardColumnsAsync(
            Guid boardId)
        {
            return await _context.BoardColumns
                .AsNoTracking()
                .Where(x => x.BoardId == boardId)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }

        public async Task<Sprint> CreateSprintAsync(Sprint sprint)
        {
            await _context.Sprints.AddAsync(sprint);

            return sprint;
        }

        public async Task<IEnumerable<Sprint>> GetProjectSprintsAsync(
            Guid projectId)
        {
            return await _context.Sprints
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Sprint?> GetSprintByIdAsync(Guid sprintId)
        {
            return await _context.Sprints
                .FirstOrDefaultAsync(
                    x => x.SprintId == sprintId);
        }

        public async Task<Sprint?> GetActiveSprintForProjectAsync(
            Guid projectId,
            Guid? excludeSprintId = null)
        {
            var query =
                _context.Sprints.Where(x =>
                    x.ProjectId == projectId &&
                    x.Status == SprintStatus.Active);

            if (excludeSprintId.HasValue)
            {
                query = query.Where(x =>
                    x.SprintId != excludeSprintId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetBacklogTasksAsync(
            Guid projectId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(x =>
                    x.ProjectId == projectId &&
                    x.SprintId == null)
                .OrderBy(x => x.BacklogRank)
                .ThenByDescending(x => x.Priority)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetSprintTasksAsync(
            Guid sprintId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(x => x.SprintId == sprintId)
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Epic> CreateEpicAsync(Epic epic)
        {
            await _context.Epics.AddAsync(epic);

            return epic;
        }

        public async Task<IEnumerable<Epic>> GetProjectEpicsAsync(
            Guid projectId)
        {
            return await _context.Epics
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Epic?> GetEpicByIdAsync(Guid epicId)
        {
            return await _context.Epics
                .FirstOrDefaultAsync(
                    x => x.EpicId == epicId);
        }

        public async Task<TaskWatcher?> GetWatcherAsync(
            Guid taskId,
            Guid userId)
        {
            return await _context.TaskWatchers
                .FirstOrDefaultAsync(x =>
                    x.TaskId == taskId &&
                    x.UserId == userId);
        }

        public async Task<TaskWatcher> AddWatcherAsync(
            TaskWatcher watcher)
        {
            await _context.TaskWatchers.AddAsync(watcher);

            return watcher;
        }

        public async Task<IEnumerable<TaskWatcher>> GetWatchersAsync(
            Guid taskId)
        {
            return await _context.TaskWatchers
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task RemoveWatcherAsync(TaskWatcher watcher)
        {
            _context.TaskWatchers.Remove(watcher);
        }

        public async Task<bool> DeleteTaskAsync(Guid taskId)
        {
            var task =
                await _context.Tasks
                    .FirstOrDefaultAsync(
                        x => x.TaskId == taskId);

            if (task == null)
            {
                return false;
            }

            _context.Tasks.Remove(task);

            return true;
        }

        public async Task<(string ProjectKey, int IssueNumber)>
            AllocateIssueNumberAsync(
                Guid projectId,
                string projectKey)
        {
            await _issueNumberLock.WaitAsync();
            try
            {
                var counter =
                    await _context.ProjectIssueCounters
                        .FirstOrDefaultAsync(
                            x => x.ProjectId == projectId);

                var maxExisting = await _context.Tasks
                    .Where(x => x.ProjectId == projectId)
                    .Select(x => (int?)x.IssueNumber)
                    .MaxAsync() ?? 0;

                if (counter == null)
                {
                    counter = new ProjectIssueCounter(
                        projectId,
                        projectKey);

                    await _context.ProjectIssueCounters.AddAsync(
                        counter);
                }
                else
                {
                    counter.UpdateProjectKey(projectKey);
                }

                counter.SyncLastIssueNumber(maxExisting);

                var issueNumber = counter.AllocateNext();

                await _context.SaveChangesAsync();

                return (counter.ProjectKey, issueNumber);
            }
            finally
            {
                _issueNumberLock.Release();
            }
        }

        public async Task<decimal> GetNextBacklogRankAsync(
            Guid projectId)
        {
            var maxRank =
                await _context.Tasks
                    .Where(x => x.ProjectId == projectId)
                    .Select(x => (decimal?)x.BacklogRank)
                    .MaxAsync();

            return (maxRank ?? 0) + 1000m;
        }

        public async Task<TaskItem?> GetByIssueKeyAsync(
            string issueKey)
        {
            var normalized =
                issueKey.Trim().ToUpperInvariant();

            return await _context.Tasks
                .FirstOrDefaultAsync(
                    x => x.IssueKey == normalized);
        }

        public async Task AddComponentAsync(Component component) =>
            await _context.Components.AddAsync(component);

        public async Task<IEnumerable<Component>> GetComponentsAsync(
            Guid projectId) =>
            await _context.Components
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderBy(x => x.Name)
                .ToListAsync();

        public async Task AddVersionAsync(ReleaseVersion version) =>
            await _context.ReleaseVersions.AddAsync(version);

        public async Task<IEnumerable<ReleaseVersion>> GetVersionsAsync(
            Guid projectId) =>
            await _context.ReleaseVersions
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

        public async Task<ReleaseVersion?> GetVersionByIdAsync(
            Guid versionId) =>
            await _context.ReleaseVersions
                .FirstOrDefaultAsync(
                    x => x.ReleaseVersionId == versionId);

        public async Task AddLinkAsync(TaskLink link) =>
            await _context.TaskLinks.AddAsync(link);

        public async Task<IEnumerable<TaskLinkInfo>> GetLinksForTaskAsync(
            Guid taskId)
        {
            var links =
                await _context.TaskLinks
                    .AsNoTracking()
                    .Where(x =>
                        x.SourceTaskId == taskId ||
                        x.TargetTaskId == taskId)
                    .ToListAsync();

            var result = new List<TaskLinkInfo>();

            foreach (var link in links)
            {
                var targetId =
                    link.SourceTaskId == taskId
                        ? link.TargetTaskId
                        : link.SourceTaskId;

                var targetKey =
                    await _context.Tasks
                        .Where(x => x.TaskId == targetId)
                        .Select(x => x.IssueKey)
                        .FirstOrDefaultAsync()
                    ?? string.Empty;

                result.Add(new TaskLinkInfo(link, targetKey));
            }

            return result;
        }

        public async Task<TaskLink?> GetLinkByIdAsync(Guid linkId) =>
            await _context.TaskLinks
                .FirstOrDefaultAsync(x => x.TaskLinkId == linkId);

        public Task RemoveLinkAsync(TaskLink link)
        {
            _context.TaskLinks.Remove(link);

            return Task.CompletedTask;
        }

        public async Task AddWorkLogAsync(WorkLog workLog) =>
            await _context.WorkLogs.AddAsync(workLog);

        public async Task<IEnumerable<WorkLog>> GetWorkLogsAsync(
            Guid taskId) =>
            await _context.WorkLogs
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.StartedAt)
                .ToListAsync();

        public async Task<int> GetTotalLoggedMinutesAsync(
            Guid taskId) =>
            await _context.WorkLogs
                .Where(x => x.TaskId == taskId)
                .SumAsync(x => x.TimeSpentMinutes);

        public async Task AddSavedFilterAsync(SavedFilter filter) =>
            await _context.SavedFilters.AddAsync(filter);

        public async Task<IEnumerable<SavedFilter>> GetSavedFiltersAsync(
            Guid userId,
            Guid? projectId)
        {
            var query =
                _context.SavedFilters
                    .AsNoTracking()
                    .Where(x => x.UserId == userId);

            if (projectId.HasValue)
            {
                query = query.Where(
                    x => x.ProjectId == projectId);
            }

            return await query
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<SavedFilter?> GetSavedFilterByIdAsync(
            Guid filterId)
        {
            return await _context.SavedFilters
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.SavedFilterId == filterId);
        }

        public async Task<bool> DeleteSprintAsync(Guid sprintId)
        {
            var sprint =
                await _context.Sprints
                    .FirstOrDefaultAsync(
                        x => x.SprintId == sprintId);

            if (sprint == null)
            {
                return false;
            }

            var tasksInSprint =
                await _context.Tasks
                    .Where(x => x.SprintId == sprintId)
                    .ToListAsync();

            foreach (var task in tasksInSprint)
            {
                task.MoveToSprint(null);
            }

            _context.Sprints.Remove(sprint);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return await _context.Tasks
                .AsNoTracking()
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetChildTasksAsync(Guid taskId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(x => x.ParentTaskId == taskId)
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.DueDate)
                .ToListAsync();
        }

        public async Task<AutomationRule> CreateAutomationRuleAsync(AutomationRule rule)
        {
            await _context.AutomationRules.AddAsync(rule);
            return rule;
        }

        public async Task<IEnumerable<AutomationRule>> GetProjectAutomationRulesAsync(Guid projectId)
        {
            return await _context.AutomationRules
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<AutomationRule?> GetAutomationRuleByIdAsync(Guid ruleId)
        {
            return await _context.AutomationRules
                .FirstOrDefaultAsync(x => x.AutomationRuleId == ruleId);
        }

        public async Task RemoveAutomationRuleAsync(AutomationRule rule)
        {
            _context.AutomationRules.Remove(rule);
            await Task.CompletedTask;
        }

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();

        public async Task AddTeamAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
        }

        public async Task<Team?> GetTeamByIdAsync(Guid teamId)
        {
            return await _context.Teams
                .Include(x => x.Members)
                .FirstOrDefaultAsync(x => x.TeamId == teamId);
        }

        public async Task<IEnumerable<Team>> GetVisibleTeamsAsync(Guid userId)
        {
            return await _context.Teams
                .Include(x => x.Members)
                .Where(x => x.Visibility == "Public" || x.Members.Any(m => m.UserId == userId))
                .ToListAsync();
        }

        public async Task<TeamMember?> GetTeamMemberAsync(Guid teamId, Guid userId)
        {
            return await _context.TeamMembers
                .FirstOrDefaultAsync(x => x.TeamId == teamId && x.UserId == userId);
        }

        public async Task AddTeamMemberAsync(TeamMember member)
        {
            await _context.TeamMembers.AddAsync(member);
        }

        public async Task RemoveTeamMemberAsync(TeamMember member)
        {
            _context.TeamMembers.Remove(member);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Guid>> GetUserTeamIdsAsync(Guid userId)
        {
            return await _context.TeamMembers
                .Where(x => x.UserId == userId)
                .Select(x => x.TeamId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetTeamsWithTasksAsync()
        {
            return await _context.Teams
                .Include(x => x.Members)
                .ToListAsync();
        }
    }
}
