using AutoMapper;
using Microsoft.AspNetCore.Http;
using TaskApi.Clients;
using TaskApi.Model.Domain.Entities;
using TaskApi.Model.Domain.Enums;
using TaskApi.Model.Dto;
using TaskApi.Repositories;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.Exceptions;
using Workflow.IO.Shared.IntegrationEvents;
using Workflow.IO.Shared.Middleware;
using Workflow.IO.Shared.Persistence;

namespace TaskApi.Services
{
    public class TaskService
        : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        private readonly IMapper _mapper;

        private readonly IProjectAccessClient _projectAccessClient;

        private readonly IIntegrationEventPublisher _eventPublisher;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IProjectMetadataClient _projectMetadataClient;

        private readonly ITenantContext _tenantContext;

        public TaskService(
            ITaskRepository taskRepository,
            IMapper mapper,
            IProjectAccessClient projectAccessClient,
            IProjectMetadataClient projectMetadataClient,
            IIntegrationEventPublisher eventPublisher,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            ITenantContext tenantContext)
        {
            _taskRepository = taskRepository;

            _mapper = mapper;

            _projectAccessClient = projectAccessClient;

            _projectMetadataClient = projectMetadataClient;

            _eventPublisher = eventPublisher;

            _unitOfWork = unitOfWork;

            _httpContextAccessor = httpContextAccessor;

            _tenantContext = tenantContext;
        }

        public async Task<TaskResponseDto>
            CreateTaskAsync(
                Guid projectId,
                Guid reporterId,
            CreateTaskRequestDto request)
        {
            await EnsureProjectMemberAsync(
                projectId,
                reporterId);

            if (request.AssigneeId.HasValue)
            {
                await EnsureProjectMemberAsync(
                    projectId,
                    request.AssigneeId.Value);
            }

            var metadata =
                await _projectMetadataClient
                    .GetProjectMetadataAsync(projectId)
                ?? throw new NotFoundException(
                    "Project was not found");

            var (projectKey, issueNumber) =
                await _taskRepository.AllocateIssueNumberAsync(
                    projectId,
                    metadata.Key);

            var backlogRank =
                await _taskRepository.GetNextBacklogRankAsync(
                    projectId);

            if (request.ParentTaskId.HasValue)
            {
                var parent = await _taskRepository.GetByIdAsync(request.ParentTaskId.Value);
                if (parent == null)
                {
                    throw new NotFoundException("Parent task was not found.");
                }
                if (parent.ProjectId != projectId)
                {
                    throw new ArgumentException("Parent task must belong to the same project.");
                }
            }

            var task = new TaskItem(
                projectId,
                projectKey,
                issueNumber,
                request.Title,
                request.Description,
                request.IssueType,
                request.Priority,
                request.AssigneeId,
                reporterId,
                request.DueDate,
                request.ParentTaskId,
                request.ComponentId,
                request.FixVersionId,
                request.OriginalEstimateMinutes,
                backlogRank,
                request.FeDeveloper,
                request.BeDeveloper,
                request.QaEngineer,
                request.InitialEta,
                request.LatestEta,
                organizationId: _tenantContext.CurrentOrganizationId);

            if (request.TeamId.HasValue)
            {
                task.AssignTeam(request.TeamId.Value);
            }

            await _taskRepository
                .CreateTaskAsync(task);

            await PublishTaskEventAsync(
                "TaskCreated",
                task,
                reporterId,
                request.AssigneeId,
                $"Task '{task.Title}' was created");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<IEnumerable<TaskResponseDto>>
            GetProjectTasksAsync(
                Guid projectId,
                Guid userId)
        {
            await EnsureProjectMemberAsync(
                projectId,
                userId,
                requireContributor: false);

            var tasks =
                await _taskRepository
                    .GetProjectTasksAsync(projectId);

            return _mapper.Map<
                IEnumerable<TaskResponseDto>>(tasks);
        }

        public async Task<IEnumerable<TaskResponseDto>> SearchProjectTasksAsync(
            Guid projectId,
            Guid userId,
            TaskSearchRequestDto request)
        {
            await EnsureProjectMemberAsync(
                projectId,
                userId,
                requireContributor: false);

            var tasks =
                await _taskRepository
                    .SearchProjectTasksAsync(
                        projectId,
                        request);

            return _mapper.Map<IEnumerable<TaskResponseDto>>(
                tasks);
        }

        public async Task<TaskResponseDto?>
            GetTaskByIdAsync(
                Guid taskId,
                Guid userId)
        {
            var task =
                await _taskRepository
                    .GetByIdAsync(taskId);

            if (task == null)
            {
                return null;
            }

            await EnsureProjectMemberAsync(
                task.ProjectId,
                userId,
                requireContributor: false);

            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<TaskResponseDto?>
            UpdateTaskAsync(
                Guid taskId,
                Guid userId,
                UpdateTaskRequestDto request)
        {
            var task =
                await _taskRepository
                    .GetByIdAsync(taskId);

            if (task == null)
            {
                return null;
            }

            await EnsureProjectMemberAsync(
                task.ProjectId,
                userId);

            if (request.ParentTaskId != task.ParentTaskId)
            {
                if (request.ParentTaskId.HasValue)
                {
                    await ValidateParentTaskAsync(taskId, request.ParentTaskId.Value, task.ProjectId);
                }
                task.SetParentTask(request.ParentTaskId);
            }

            task.Update(
                request.Title,
                request.Description,
                request.Priority,
                request.DueDate,
                request.ComponentId,
                request.FixVersionId,
                request.OriginalEstimateMinutes,
                request.RemainingEstimateMinutes,
                request.FeDeveloper,
                request.BeDeveloper,
                request.QaEngineer,
                request.InitialEta,
                request.LatestEta,
                request.TeamId);

            await PublishTaskEventAsync(
                "TaskUpdated",
                task,
                userId,
                null,
                $"Task '{task.Title}' was updated");

            await NotifyWatchersAsync(
                task,
                userId,
                "TaskUpdated",
                $"Task '{task.Title}' was updated");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<TaskResponseDto?>
            ChangeStatusAsync(
                Guid taskId,
                Guid userId,
                ChangeTaskStatusRequestDto request)
        {
            var task =
                await _taskRepository
                    .GetByIdAsync(taskId);

            if (task == null)
            {
                return null;
            }

            await EnsureProjectMemberAsync(
                task.ProjectId,
                userId);

            task.ChangeStatus(
                request.Status,
                request.Resolution);

            await PublishTaskEventAsync(
                "TaskStatusChanged",
                task,
                userId,
                task.AssigneeId,
                $"Task '{task.IssueKey}' moved to {task.Status}");

            await NotifyWatchersAsync(
                task,
                userId,
                "TaskStatusChanged",
                $"Task '{task.Title}' moved to {task.Status}");

            // Mock Automation check
            try
            {
                var rules = await _taskRepository.GetProjectAutomationRulesAsync(task.ProjectId);
                foreach (var rule in rules)
                {
                    if (rule.IsEnabled && rule.TriggerType.Equals("StatusChanged", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(rule.TriggerValue) || rule.TriggerValue.Equals(request.Status.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"[AUTOMATION TRIGGERED] Rule '{rule.Name}' ({rule.AutomationRuleId}) executed for Task {task.TaskId}. Action: {rule.ActionType} ({rule.ActionValue})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUTOMATION ERROR] Failed to check automation rules: {ex.Message}");
            }

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<TaskResponseDto?>
            AssignTaskAsync(
                Guid taskId,
                Guid userId,
                AssignTaskRequestDto request)
        {
            var task =
                await _taskRepository
                    .GetByIdAsync(taskId);

            if (task == null)
            {
                return null;
            }

            await EnsureProjectMemberAsync(
                task.ProjectId,
                userId);

            if (request.AssigneeId.HasValue)
            {
                await EnsureProjectMemberAsync(
                    task.ProjectId,
                    request.AssigneeId.Value);
            }

            task.AssignTo(request.AssigneeId);

            await PublishTaskEventAsync(
                "TaskAssigned",
                task,
                userId,
                request.AssigneeId,
                $"Task '{task.Title}' was assigned");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<bool>
            DeleteTaskAsync(
                Guid taskId,
                Guid userId)
        {
            var task =
                await _taskRepository
                    .GetByIdAsync(taskId);

            if (task == null)
            {
                return false;
            }

            await EnsureProjectMemberAsync(
                task.ProjectId,
                userId);

            var deleted =
                await _taskRepository
                .DeleteTaskAsync(taskId);

            if (deleted)
            {
                await PublishTaskEventAsync(
                    "TaskDeleted",
                    task,
                    userId,
                    null,
                    $"Task '{task.Title}' was deleted");

                await _unitOfWork.SaveChangesAsync();
            }

            return deleted;
        }

        public async Task<TaskLabelResponseDto?> AddLabelAsync(
            Guid taskId,
            Guid userId,
            AddTaskLabelRequestDto request)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId);

            if (task == null)
            {
                return null;
            }

            var label = new TaskLabel(
                taskId,
                request.Name,
                request.Color);

            await _taskRepository.AddLabelAsync(label);

            await PublishTaskEventAsync(
                "TaskLabelAdded",
                task,
                userId,
                null,
                $"Label '{label.Name}' was added to task '{task.Title}'");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaskLabelResponseDto>(label);
        }

        public async Task<IEnumerable<TaskLabelResponseDto>?> GetLabelsAsync(
            Guid taskId,
            Guid userId)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId,
                    requireContributor: false);

            if (task == null)
            {
                return null;
            }

            var labels =
                await _taskRepository
                    .GetLabelsAsync(taskId);

            return _mapper.Map<IEnumerable<TaskLabelResponseDto>>(
                labels);
        }

        public async Task<bool> RemoveLabelAsync(
            Guid taskId,
            Guid labelId,
            Guid userId)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId);

            if (task == null)
            {
                return false;
            }

            var label =
                await _taskRepository
                    .GetLabelByIdAsync(labelId);

            if (label == null ||
                label.TaskId != taskId)
            {
                return false;
            }

            await _taskRepository.RemoveLabelAsync(label);

            await PublishTaskEventAsync(
                "TaskLabelRemoved",
                task,
                userId,
                null,
                $"Label '{label.Name}' was removed from task '{task.Title}'");

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<SubTaskResponseDto?> CreateSubTaskAsync(
            Guid taskId,
            Guid userId,
            CreateSubTaskRequestDto request)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId);

            if (task == null)
            {
                return null;
            }

            var subTask =
                new SubTask(
                    taskId,
                    request.Title);

            await _taskRepository.CreateSubTaskAsync(subTask);

            await PublishTaskEventAsync(
                "SubTaskCreated",
                task,
                userId,
                null,
                $"Subtask '{subTask.Title}' was added to task '{task.Title}'");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SubTaskResponseDto>(subTask);
        }

        public async Task<IEnumerable<SubTaskResponseDto>?> GetSubTasksAsync(
            Guid taskId,
            Guid userId)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId,
                    requireContributor: false);

            if (task == null)
            {
                return null;
            }

            var subTasks =
                await _taskRepository
                    .GetSubTasksAsync(taskId);

            return _mapper.Map<IEnumerable<SubTaskResponseDto>>(
                subTasks);
        }

        public async Task<SubTaskResponseDto?> ChangeSubTaskCompletionAsync(
            Guid subTaskId,
            Guid userId,
            ChangeSubTaskCompletionRequestDto request)
        {
            var subTask =
                await _taskRepository
                    .GetSubTaskByIdAsync(subTaskId);

            if (subTask == null)
            {
                return null;
            }

            var task =
                await GetAuthorizedTaskAsync(
                    subTask.TaskId,
                    userId);

            if (task == null)
            {
                return null;
            }

            if (request.IsCompleted)
            {
                subTask.Complete();
            }
            else
            {
                subTask.Reopen();
            }

            await PublishTaskEventAsync(
                "SubTaskCompletionChanged",
                task,
                userId,
                null,
                $"Subtask '{subTask.Title}' completion changed");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SubTaskResponseDto>(subTask);
        }

        public async Task<bool> DeleteSubTaskAsync(
            Guid subTaskId,
            Guid userId)
        {
            var subTask =
                await _taskRepository
                    .GetSubTaskByIdAsync(subTaskId);

            if (subTask == null)
            {
                return false;
            }

            var task =
                await GetAuthorizedTaskAsync(
                    subTask.TaskId,
                    userId);

            if (task == null)
            {
                return false;
            }

            await _taskRepository.RemoveSubTaskAsync(subTask);

            await PublishTaskEventAsync(
                "SubTaskDeleted",
                task,
                userId,
                null,
                $"Subtask '{subTask.Title}' was deleted");

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<BoardResponseDto> CreateBoardAsync(
            Guid projectId,
            Guid userId,
            CreateBoardRequestDto request)
        {
            await EnsureProjectMemberAsync(
                projectId,
                userId);

            var existingBoard =
                await _taskRepository
                    .GetBoardByProjectIdAsync(projectId);

            if (existingBoard != null)
            {
                throw new ConflictException(
                    "Project already has a board");
            }

            var board =
                new Board(
                    projectId,
                    request.Name);

            await _taskRepository.CreateBoardAsync(board);

            await _taskRepository.AddBoardColumnsAsync(
                CreateDefaultBoardColumns(board.BoardId));

            await PublishPlanningEventAsync(
                "BoardCreated",
                "Board",
                board.BoardId,
                userId,
                $"Board '{board.Name}' was created");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BoardResponseDto>(board);
        }

        public async Task<BoardViewResponseDto?>             GetBoardViewAsync(
            Guid projectId,
            Guid userId)
        {
            await EnsureProjectMemberAsync(
                projectId,
                userId,
                requireContributor: false);

            var board =
                await _taskRepository
                    .GetBoardByProjectIdAsync(projectId);

            if (board == null)
            {
                return null;
            }

            var columns =
                await _taskRepository
                    .GetBoardColumnsAsync(board.BoardId);

            var tasksByStatus =
                await _taskRepository
                    .GetProjectTasksGroupedByStatusAsync(
                        projectId);

            return new BoardViewResponseDto
            {
                Board = _mapper.Map<BoardResponseDto>(board),
                Columns = columns.Select(column =>
                    new BoardColumnViewDto
                    {
                        Column =
                            _mapper.Map<BoardColumnResponseDto>(
                                column),
                        Tasks =
                            _mapper.Map<IEnumerable<TaskResponseDto>>(
                                tasksByStatus.TryGetValue(
                                    column.Status,
                                    out var columnTasks)
                                    ? columnTasks
                                    : new List<TaskItem>())
                    })
            };
        }

        public async Task<SprintResponseDto> CreateSprintAsync(
            Guid projectId,
            Guid userId,
            CreateSprintRequestDto request)
        {
            await EnsureProjectMemberAsync(
                projectId,
                userId);

            var sprint =
                new Sprint(
                    projectId,
                    request.Name,
                    request.StartDate,
                    request.EndDate);

            await _taskRepository.CreateSprintAsync(sprint);

            await PublishPlanningEventAsync(
                "SprintCreated",
                "Sprint",
                sprint.SprintId,
                userId,
                $"Sprint '{sprint.Name}' was created");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SprintResponseDto>(sprint);
        }

        public async Task<IEnumerable<SprintResponseDto>>             GetProjectSprintsAsync(
            Guid projectId,
            Guid userId)
        {
            await EnsureProjectMemberAsync(
                projectId,
                userId,
                requireContributor: false);

            var sprints =
                await _taskRepository
                    .GetProjectSprintsAsync(projectId);

            return _mapper.Map<IEnumerable<SprintResponseDto>>(
                sprints);
        }

        public async Task<SprintResponseDto?> StartSprintAsync(
            Guid sprintId,
            Guid userId)
        {
            var sprint =
                await GetAuthorizedSprintAsync(
                    sprintId,
                    userId);

            if (sprint == null)
            {
                return null;
            }

            var otherActive =
                await _taskRepository
                    .GetActiveSprintForProjectAsync(
                        sprint.ProjectId,
                        sprint.SprintId);

            try
            {
                sprint.Start(otherActive != null);
            }
            catch (InvalidOperationException exception)
            {
                throw new ConflictException(exception.Message);
            }

            await PublishPlanningEventAsync(
                "SprintStarted",
                "Sprint",
                sprint.SprintId,
                userId,
                $"Sprint '{sprint.Name}' was started");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SprintResponseDto>(sprint);
        }

        public async Task<SprintResponseDto?> CompleteSprintAsync(
            Guid sprintId,
            Guid userId)
        {
            var sprint =
                await GetAuthorizedSprintAsync(
                    sprintId,
                    userId);

            if (sprint == null)
            {
                return null;
            }

            try
            {
                sprint.Complete();
            }
            catch (InvalidOperationException exception)
            {
                throw new ConflictException(exception.Message);
            }

            await PublishPlanningEventAsync(
                "SprintCompleted",
                "Sprint",
                sprint.SprintId,
                userId,
                $"Sprint '{sprint.Name}' was completed");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SprintResponseDto>(sprint);
        }

        public async Task<TaskResponseDto?> MoveTaskToSprintAsync(
            Guid taskId,
            Guid userId,
            MoveTaskToSprintRequestDto request)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId);

            if (task == null)
            {
                return null;
            }

            if (request.SprintId.HasValue)
            {
                var sprint =
                    await _taskRepository
                        .GetSprintByIdAsync(
                            request.SprintId.Value);

                if (sprint == null ||
                    sprint.ProjectId != task.ProjectId)
                {
                    throw new NotFoundException(
                        "Sprint was not found");
                }
            }

            task.MoveToSprint(request.SprintId);

            await PublishTaskEventAsync(
                request.SprintId.HasValue
                    ? "TaskMovedToSprint"
                    : "TaskMovedToBacklog",
                task,
                userId,
                task.AssigneeId,
                request.SprintId.HasValue
                    ? $"Task '{task.Title}' was moved to a sprint"
                    : $"Task '{task.Title}' was moved to backlog");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<BacklogResponseDto>             GetBacklogAsync(
            Guid projectId,
            Guid userId)
        {
            await EnsureProjectMemberAsync(
                projectId,
                userId,
                requireContributor: false);

            var backlogTasks =
                await _taskRepository
                    .GetBacklogTasksAsync(projectId);

            var sprints =
                await _taskRepository
                    .GetProjectSprintsAsync(projectId);

            var sprintBacklogs =
                new List<SprintBacklogResponseDto>();

            foreach (var sprint in sprints)
            {
                var sprintTasks =
                    await _taskRepository
                        .GetSprintTasksAsync(sprint.SprintId);

                sprintBacklogs.Add(
                    new SprintBacklogResponseDto
                    {
                        Sprint =
                            _mapper.Map<SprintResponseDto>(
                                sprint),
                        Tasks =
                            _mapper.Map<IEnumerable<TaskResponseDto>>(
                                sprintTasks)
                    });
            }

            return new BacklogResponseDto
            {
                ProjectId = projectId,
                BacklogTasks =
                    _mapper.Map<IEnumerable<TaskResponseDto>>(
                        backlogTasks),
                Sprints = sprintBacklogs
            };
        }

        public async Task<EpicResponseDto> CreateEpicAsync(
            Guid projectId,
            Guid userId,
            CreateEpicRequestDto request)
        {
            await EnsureProjectMemberAsync(
                projectId,
                userId);

            var epic =
                new Epic(
                    projectId,
                    request.Name,
                    request.Description);

            await _taskRepository.CreateEpicAsync(epic);

            await PublishPlanningEventAsync(
                "EpicCreated",
                "Epic",
                epic.EpicId,
                userId,
                $"Epic '{epic.Name}' was created");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EpicResponseDto>(epic);
        }

        public async Task<IEnumerable<EpicResponseDto>>             GetProjectEpicsAsync(
            Guid projectId,
            Guid userId)
        {
            await EnsureProjectMemberAsync(
                projectId,
                userId,
                requireContributor: false);

            var epics =
                await _taskRepository
                    .GetProjectEpicsAsync(projectId);

            return _mapper.Map<IEnumerable<EpicResponseDto>>(
                epics);
        }

        public async Task<TaskResponseDto?> AssignEpicAsync(
            Guid taskId,
            Guid userId,
            AssignEpicRequestDto request)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId);

            if (task == null)
            {
                return null;
            }

            if (request.EpicId.HasValue)
            {
                var epic =
                    await _taskRepository
                        .GetEpicByIdAsync(
                            request.EpicId.Value);

                if (epic == null ||
                    epic.ProjectId != task.ProjectId)
                {
                    throw new NotFoundException(
                        "Epic was not found");
                }
            }

            task.AssignEpic(request.EpicId);

            await PublishTaskEventAsync(
                request.EpicId.HasValue
                    ? "TaskAssignedToEpic"
                    : "TaskRemovedFromEpic",
                task,
                userId,
                task.AssigneeId,
                request.EpicId.HasValue
                    ? $"Task '{task.Title}' was assigned to an epic"
                    : $"Task '{task.Title}' was removed from its epic");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<TaskResponseDto?> UpdateStoryPointsAsync(
            Guid taskId,
            Guid userId,
            UpdateStoryPointsRequestDto request)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId);

            if (task == null)
            {
                return null;
            }

            task.UpdateStoryPoints(request.StoryPoints);

            await PublishTaskEventAsync(
                "TaskStoryPointsUpdated",
                task,
                userId,
                task.AssigneeId,
                $"Story points for task '{task.Title}' were updated");

            await NotifyWatchersAsync(
                task,
                userId,
                "TaskStoryPointsUpdated",
                $"Story points for task '{task.Title}' were updated");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<TaskWatcherResponseDto?> WatchTaskAsync(
            Guid taskId,
            Guid userId)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId);

            if (task == null)
            {
                return null;
            }

            var existing =
                await _taskRepository
                    .GetWatcherAsync(
                        taskId,
                        userId);

            if (existing != null)
            {
                return _mapper.Map<TaskWatcherResponseDto>(
                    existing);
            }

            var watcher =
                new TaskWatcher(
                    taskId,
                    userId);

            await _taskRepository.AddWatcherAsync(watcher);

            await PublishTaskEventAsync(
                "TaskWatcherAdded",
                task,
                userId,
                null,
                $"A watcher was added to task '{task.Title}'");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaskWatcherResponseDto>(watcher);
        }

        public async Task<IEnumerable<TaskWatcherResponseDto>?> GetWatchersAsync(
            Guid taskId,
            Guid userId)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId,
                    requireContributor: false);

            if (task == null)
            {
                return null;
            }

            var watchers =
                await _taskRepository
                    .GetWatchersAsync(taskId);

            return _mapper.Map<IEnumerable<TaskWatcherResponseDto>>(
                watchers);
        }

        public async Task<bool> UnwatchTaskAsync(
            Guid taskId,
            Guid userId)
        {
            var task =
                await GetAuthorizedTaskAsync(
                    taskId,
                    userId);

            if (task == null)
            {
                return false;
            }

            var watcher =
                await _taskRepository
                    .GetWatcherAsync(
                        taskId,
                        userId);

            if (watcher == null)
            {
                return false;
            }

            await _taskRepository.RemoveWatcherAsync(watcher);

            await PublishTaskEventAsync(
                "TaskWatcherRemoved",
                task,
                userId,
                null,
                $"A watcher was removed from task '{task.Title}'");

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private async Task ValidateParentTaskAsync(Guid taskId, Guid? parentTaskId, Guid projectId)
        {
            if (!parentTaskId.HasValue) return;

            if (parentTaskId.Value == taskId)
            {
                throw new ArgumentException("A task cannot be its own parent.");
            }

            var parent = await _taskRepository.GetByIdAsync(parentTaskId.Value);
            if (parent == null)
            {
                throw new NotFoundException("Parent task was not found.");
            }

            if (parent.ProjectId != projectId)
            {
                throw new ArgumentException("Parent task must belong to the same project.");
            }

            // Loop checking
            var current = parent;
            while (current.ParentTaskId.HasValue)
            {
                if (current.ParentTaskId.Value == taskId)
                {
                    throw new ArgumentException("Circular reference detected: Parent task cannot be a descendant of the current task.");
                }
                current = await _taskRepository.GetByIdAsync(current.ParentTaskId.Value);
                if (current == null) break;
            }
        }

        private async Task EnsureProjectMemberAsync(
            Guid projectId,
            Guid userId,
            bool requireContributor = true)
        {
            var allowed =
                requireContributor
                    ? await _projectAccessClient.CanContributeAsync(
                        projectId,
                        userId)
                    : await _projectAccessClient.IsProjectMemberAsync(
                        projectId,
                        userId);

            if (!allowed)
            {
                throw new ForbiddenException(
                    requireContributor
                        ? "User cannot modify data in this project"
                        : "User is not a member of this project");
            }
        }

        private async Task<TaskItem?> GetAuthorizedTaskAsync(
            Guid taskId,
            Guid userId,
            bool requireContributor = true)
        {
            var task =
                await _taskRepository
                    .GetByIdAsync(taskId);

            if (task == null)
            {
                return null;
            }

            await EnsureProjectMemberAsync(
                task.ProjectId,
                userId,
                requireContributor);

            return task;
        }

        private async Task<Sprint?> GetAuthorizedSprintAsync(
            Guid sprintId,
            Guid userId)
        {
            var sprint =
                await _taskRepository
                    .GetSprintByIdAsync(sprintId);

            if (sprint == null)
            {
                return null;
            }

            await EnsureProjectMemberAsync(
                sprint.ProjectId,
                userId,
                requireContributor: true);

            return sprint;
        }

        private static IEnumerable<BoardColumn> CreateDefaultBoardColumns(
            Guid boardId)
        {
            return new List<BoardColumn>
            {
                new(
                    boardId,
                    "Todo",
                    Model.Domain.Enums.TaskStatus.Todo,
                    1),
                new(
                    boardId,
                    "In Progress",
                    Model.Domain.Enums.TaskStatus.InProgress,
                    2),
                new(
                    boardId,
                    "Review",
                    Model.Domain.Enums.TaskStatus.Review,
                    3),
                new(
                    boardId,
                    "Blocked",
                    Model.Domain.Enums.TaskStatus.Blocked,
                    4),
                new(
                    boardId,
                    "Done",
                    Model.Domain.Enums.TaskStatus.Done,
                    5)
            };
        }

        private async Task PublishPlanningEventAsync(
            string eventType,
            string entityType,
            Guid entityId,
            Guid actorId,
            string description)
        {
            await _eventPublisher.PublishAsync(
                new IntegrationEventRequest
                {
                    EventId = Guid.NewGuid(),
                    CorrelationId = GetCorrelationId(),
                    EventType = eventType,
                    EntityType = entityType,
                    EntityId = entityId,
                    ActorId = actorId,
                    Description = description
                });
        }

        private string? GetCorrelationId() =>
            _httpContextAccessor.HttpContext?
                .Items[CorrelationIdMiddleware.HeaderName]
                ?.ToString();

        private async Task NotifyWatchersAsync(
            TaskItem task,
            Guid actorId,
            string eventType,
            string description)
        {
            var watchers =
                await _taskRepository
                    .GetWatchersAsync(task.TaskId);

            foreach (var watcher in watchers)
            {
                if (watcher.UserId == actorId)
                {
                    continue;
                }

                await PublishTaskEventAsync(
                    eventType,
                    task,
                    actorId,
                    watcher.UserId,
                    description);
            }
        }

        private async Task PublishTaskEventAsync(
            string eventType,
            TaskItem task,
            Guid actorId,
            Guid? recipientId,
            string description)
        {
            await _eventPublisher.PublishAsync(
                new IntegrationEventRequest
                {
                    EventId = Guid.NewGuid(),
                    CorrelationId = GetCorrelationId(),
                    EventType = eventType,
                    EntityType = "Task",
                    EntityId = task.TaskId,
                    ActorId = actorId,
                    RecipientId =
                        recipientId == actorId
                            ? null
                            : recipientId,
                    Description = description,
                    PayloadJson =
                        $$"""
                        {
                          "taskId": "{{task.TaskId}}",
                          "issueKey": "{{task.IssueKey}}",
                          "projectId": "{{task.ProjectId}}",
                          "issueType": "{{task.IssueType}}",
                          "status": "{{task.Status}}",
                          "resolution": "{{task.Resolution}}",
                          "priority": "{{task.Priority}}",
                          "assigneeId": "{{task.AssigneeId}}",
                          "sprintId": "{{task.SprintId}}",
                          "epicId": "{{task.EpicId}}",
                          "storyPoints": {{task.StoryPoints?.ToString() ?? "null"}},
                          "dueDate": {{(task.DueDate.HasValue ? $"\"{task.DueDate.Value:O}\"" : "null")}}
                        }
                        """
                });
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(Guid userId)
        {
            var tasks = await _taskRepository.GetAllTasksAsync();
            return _mapper.Map<IEnumerable<TaskResponseDto>>(tasks);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetChildTasksAsync(Guid taskId, Guid userId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Task not found");
            }
            await EnsureProjectMemberAsync(task.ProjectId, userId, requireContributor: false);

            var childTasks = await _taskRepository.GetChildTasksAsync(taskId);
            return _mapper.Map<IEnumerable<TaskResponseDto>>(childTasks);
        }

        public async Task<TaskResponseDto?> AssignTeamAsync(
            Guid taskId,
            Guid userId,
            AssignTeamRequestDto request)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                return null;
            }

            await EnsureProjectMemberAsync(task.ProjectId, userId);

            task.AssignTeam(request.TeamId);

            await PublishTaskEventAsync(
                "TaskTeamUpdated",
                task,
                userId,
                null,
                $"Task '{task.Title}' team was updated");

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<AutomationRuleResponseDto> CreateAutomationRuleAsync(
            Guid projectId,
            Guid userId,
            CreateAutomationRuleRequestDto request)
        {
            await EnsureProjectMemberAsync(projectId, userId);

            var rule = new AutomationRule(
                projectId,
                request.Name,
                request.TriggerType,
                request.TriggerValue,
                request.ActionType,
                request.ActionValue);

            await _taskRepository.CreateAutomationRuleAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AutomationRuleResponseDto>(rule);
        }

        public async Task<IEnumerable<AutomationRuleResponseDto>> GetProjectAutomationRulesAsync(
            Guid projectId,
            Guid userId)
        {
            await EnsureProjectMemberAsync(projectId, userId, requireContributor: false);

            var rules = await _taskRepository.GetProjectAutomationRulesAsync(projectId);
            return _mapper.Map<IEnumerable<AutomationRuleResponseDto>>(rules);
        }

        public async Task<bool> DeleteAutomationRuleAsync(
            Guid ruleId,
            Guid userId)
        {
            var rule = await _taskRepository.GetAutomationRuleByIdAsync(ruleId);
            if (rule == null) return false;

            await EnsureProjectMemberAsync(rule.ProjectId, userId);

            await _taskRepository.RemoveAutomationRuleAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<AutomationRuleResponseDto?> ToggleAutomationRuleAsync(
            Guid ruleId,
            Guid userId,
            bool isEnabled)
        {
            var rule = await _taskRepository.GetAutomationRuleByIdAsync(ruleId);
            if (rule == null) return null;

            await EnsureProjectMemberAsync(rule.ProjectId, userId);

            rule.Toggle(isEnabled);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AutomationRuleResponseDto>(rule);
        }
    }
}
