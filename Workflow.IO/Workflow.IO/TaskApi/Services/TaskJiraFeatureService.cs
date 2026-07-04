using AutoMapper;
using TaskApi.Clients;
using TaskApi.Model.Domain.Entities;
using TaskApi.Model.Domain.Enums;
using TaskApi.Model.Dto;
using TaskApi.Repositories;
using Workflow.IO.Shared.Exceptions;
using Workflow.IO.Shared.Persistence;

namespace TaskApi.Services
{
    public class TaskJiraFeatureService : ITaskJiraFeatureService
    {
        private readonly ITaskRepository _taskRepository;

        private readonly ITaskService _taskService;

        private readonly IProjectAccessClient _projectAccessClient;

        private readonly IMapper _mapper;

        private readonly IUnitOfWork _unitOfWork;

        public TaskJiraFeatureService(
            ITaskRepository taskRepository,
            ITaskService taskService,
            IProjectAccessClient projectAccessClient,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _taskRepository = taskRepository;

            _taskService = taskService;

            _projectAccessClient = projectAccessClient;

            _mapper = mapper;

            _unitOfWork = unitOfWork;
        }

        public async Task<TaskResponseDto?> GetTaskByIssueKeyAsync(
            string issueKey,
            Guid userId)
        {
            var task =
                await _taskRepository.GetByIssueKeyAsync(
                    issueKey);

            if (task == null)
            {
                return null;
            }

            return await _taskService.GetTaskByIdAsync(
                task.TaskId,
                userId);
        }

        public async Task<TaskResponseDto?> UpdateBacklogRankAsync(
            Guid taskId,
            Guid userId,
            UpdateBacklogRankRequestDto request)
        {
            var task =
                await _taskRepository.GetByIdAsync(taskId);

            if (task == null)
            {
                return null;
            }

            await EnsureContributorAsync(task.ProjectId, userId);

            task.UpdateBacklogRank(request.BacklogRank);

            await _unitOfWork.SaveChangesAsync();

            return await MapTaskAsync(task);
        }

        public async Task<ComponentResponseDto> CreateComponentAsync(
            Guid projectId,
            Guid userId,
            CreateComponentRequestDto request)
        {
            await EnsureContributorAsync(projectId, userId);

            var component = new Component(
                projectId,
                request.Name,
                request.Description);

            await _taskRepository.AddComponentAsync(component);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ComponentResponseDto>(component);
        }

        public async Task<IEnumerable<ComponentResponseDto>> GetComponentsAsync(
            Guid projectId,
            Guid userId)
        {
            await EnsureMemberAsync(projectId, userId);

            var components =
                await _taskRepository.GetComponentsAsync(projectId);

            return _mapper.Map<IEnumerable<ComponentResponseDto>>(
                components);
        }

        public async Task<ReleaseVersionResponseDto> CreateVersionAsync(
            Guid projectId,
            Guid userId,
            CreateReleaseVersionRequestDto request)
        {
            await EnsureContributorAsync(projectId, userId);

            var version = new ReleaseVersion(
                projectId,
                request.Name,
                request.Description,
                request.ReleaseDate);

            await _taskRepository.AddVersionAsync(version);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReleaseVersionResponseDto>(version);
        }

        public async Task<IEnumerable<ReleaseVersionResponseDto>> GetVersionsAsync(
            Guid projectId,
            Guid userId)
        {
            await EnsureMemberAsync(projectId, userId);

            var versions =
                await _taskRepository.GetVersionsAsync(projectId);

            return _mapper.Map<IEnumerable<ReleaseVersionResponseDto>>(
                versions);
        }

        public async Task<ReleaseVersionResponseDto?> ReleaseVersionAsync(
            Guid versionId,
            Guid userId)
        {
            var version =
                await _taskRepository.GetVersionByIdAsync(versionId);

            if (version == null)
            {
                return null;
            }

            await EnsureContributorAsync(version.ProjectId, userId);

            version.Release(DateTime.UtcNow);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReleaseVersionResponseDto>(version);
        }

        public async Task<TaskLinkResponseDto?> CreateLinkAsync(
            Guid sourceTaskId,
            Guid userId,
            CreateTaskLinkRequestDto request)
        {
            var source =
                await _taskRepository.GetByIdAsync(sourceTaskId);

            if (source == null)
            {
                return null;
            }

            await EnsureContributorAsync(source.ProjectId, userId);

            var target =
                await _taskRepository.GetByIdAsync(
                    request.TargetTaskId);

            if (target == null ||
                target.ProjectId != source.ProjectId)
            {
                throw new NotFoundException("Target task was not found");
            }

            var link = new TaskLink(
                sourceTaskId,
                request.TargetTaskId,
                request.LinkType,
                userId);

            await _taskRepository.AddLinkAsync(link);

            await _unitOfWork.SaveChangesAsync();

            var response = _mapper.Map<TaskLinkResponseDto>(link);

            response.TargetIssueKey = target.IssueKey;

            return response;
        }

        public async Task<IEnumerable<TaskLinkResponseDto>?> GetLinksAsync(
            Guid taskId,
            Guid userId)
        {
            var task =
                await _taskRepository.GetByIdAsync(taskId);

            if (task == null)
            {
                return null;
            }

            await EnsureMemberAsync(task.ProjectId, userId);

            var links =
                await _taskRepository.GetLinksForTaskAsync(taskId);

            return links.Select(x => new TaskLinkResponseDto
            {
                TaskLinkId = x.Link.TaskLinkId,
                SourceTaskId = x.Link.SourceTaskId,
                TargetTaskId = x.Link.TargetTaskId,
                LinkType = x.Link.LinkType,
                TargetIssueKey = x.TargetIssueKey
            });
        }

        public async Task<bool> DeleteLinkAsync(
            Guid linkId,
            Guid userId)
        {
            var link =
                await _taskRepository.GetLinkByIdAsync(linkId);

            if (link == null)
            {
                return false;
            }

            var source =
                await _taskRepository.GetByIdAsync(link.SourceTaskId);

            if (source == null)
            {
                return false;
            }

            await EnsureContributorAsync(source.ProjectId, userId);

            await _taskRepository.RemoveLinkAsync(link);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<WorkLogResponseDto?> AddWorkLogAsync(
            Guid taskId,
            Guid userId,
            CreateWorkLogRequestDto request)
        {
            var task =
                await _taskRepository.GetByIdAsync(taskId);

            if (task == null)
            {
                return null;
            }

            await EnsureContributorAsync(task.ProjectId, userId);

            var workLog = new WorkLog(
                taskId,
                userId,
                request.TimeSpentMinutes,
                request.Comment,
                request.StartedAt ?? DateTime.UtcNow);

            await _taskRepository.AddWorkLogAsync(workLog);

            task.LogWork(request.TimeSpentMinutes);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<WorkLogResponseDto>(workLog);
        }

        public async Task<IEnumerable<WorkLogResponseDto>?> GetWorkLogsAsync(
            Guid taskId,
            Guid userId)
        {
            var task =
                await _taskRepository.GetByIdAsync(taskId);

            if (task == null)
            {
                return null;
            }

            await EnsureMemberAsync(task.ProjectId, userId);

            var logs =
                await _taskRepository.GetWorkLogsAsync(taskId);

            return _mapper.Map<IEnumerable<WorkLogResponseDto>>(logs);
        }

        public async Task<BulkTaskUpdateResponseDto> BulkUpdateAsync(
            Guid projectId,
            Guid userId,
            BulkTaskUpdateRequestDto request)
        {
            await EnsureContributorAsync(projectId, userId);

            var updated = 0;

            foreach (var taskId in request.TaskIds.Distinct())
            {
                var task =
                    await _taskRepository.GetByIdAsync(taskId);

                if (task == null || task.ProjectId != projectId)
                {
                    continue;
                }

                if (request.Status.HasValue)
                {
                    task.ChangeStatus(
                        request.Status.Value,
                        request.Resolution);
                }

                if (request.AssigneeId.HasValue)
                {
                    task.AssignTo(request.AssigneeId);
                }

                if (request.MoveToBacklog)
                {
                    task.MoveToSprint(null);
                }
                else if (request.SprintId.HasValue)
                {
                    task.MoveToSprint(request.SprintId);
                }

                updated++;
            }

            await _unitOfWork.SaveChangesAsync();

            return new BulkTaskUpdateResponseDto
            {
                UpdatedCount = updated
            };
        }

        public async Task<SavedFilterResponseDto> CreateSavedFilterAsync(
            Guid? projectId,
            Guid userId,
            CreateSavedFilterRequestDto request)
        {
            if (projectId.HasValue)
            {
                await EnsureMemberAsync(projectId.Value, userId);
            }

            var filter = new SavedFilter(
                userId,
                projectId,
                request.Name,
                request.JqlQuery);

            await _taskRepository.AddSavedFilterAsync(filter);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SavedFilterResponseDto>(filter);
        }

        public async Task<IEnumerable<SavedFilterResponseDto>> GetSavedFiltersAsync(
            Guid? projectId,
            Guid userId)
        {
            if (projectId.HasValue)
            {
                await EnsureMemberAsync(projectId.Value, userId);
            }

            var filters =
                await _taskRepository.GetSavedFiltersAsync(
                    userId,
                    projectId);

            return _mapper.Map<IEnumerable<SavedFilterResponseDto>>(
                filters);
        }

        public async Task<IEnumerable<TaskResponseDto>?> ExecuteSavedFilterAsync(
            Guid filterId,
            Guid userId)
        {
            var filter =
                await _taskRepository.GetSavedFilterByIdAsync(
                    filterId);

            if (filter == null ||
                filter.UserId != userId)
            {
                return null;
            }

            if (!filter.ProjectId.HasValue)
            {
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        ["projectId"] =
                        [
                            "Filter must be scoped to a project to execute"
                        ]
                    });
            }

            var projectId = filter.ProjectId.Value;

            await EnsureMemberAsync(projectId, userId);

            var tasks =
                await _taskRepository.GetProjectTasksAsync(projectId);

            var teams = await _taskRepository.GetVisibleTeamsAsync(userId);

            var matched =
                JqlFilterParser.Apply(
                    tasks,
                    filter.JqlQuery,
                    userId,
                    teams);

            return _mapper.Map<IEnumerable<TaskResponseDto>>(matched);
        }

        public async Task<SprintResponseDto?> UpdateSprintAsync(
            Guid sprintId,
            Guid userId,
            UpdateSprintRequestDto request)
        {
            var sprint =
                await _taskRepository.GetSprintByIdAsync(sprintId);

            if (sprint == null)
            {
                return null;
            }

            await EnsureContributorAsync(sprint.ProjectId, userId);

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                sprint.UpdateDetails(
                    request.Name,
                    request.StartDate ?? sprint.StartDate,
                    request.EndDate ?? sprint.EndDate);
            }

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SprintResponseDto>(sprint);
        }

        public async Task<bool> DeleteSprintAsync(
            Guid sprintId,
            Guid userId)
        {
            var sprint =
                await _taskRepository.GetSprintByIdAsync(sprintId);

            if (sprint == null)
            {
                return false;
            }

            await EnsureContributorAsync(sprint.ProjectId, userId);

            if (sprint.Status == SprintStatus.Active)
            {
                throw new ConflictException(
                    "Cannot delete an active sprint");
            }

            return await _taskRepository.DeleteSprintAsync(sprintId);
        }

        private async Task<TaskResponseDto> MapTaskAsync(TaskItem task)
        {
            var dto = _mapper.Map<TaskResponseDto>(task);

            dto.TotalLoggedMinutes =
                await _taskRepository.GetTotalLoggedMinutesAsync(
                    task.TaskId);

            return dto;
        }

        private Task EnsureMemberAsync(Guid projectId, Guid userId) =>
            EnsureAccessAsync(projectId, userId, false);

        private Task EnsureContributorAsync(Guid projectId, Guid userId) =>
            EnsureAccessAsync(projectId, userId, true);

        private async Task EnsureAccessAsync(
            Guid projectId,
            Guid userId,
            bool requireContributor)
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
                    "User cannot access this project");
            }
        }
    }
}
