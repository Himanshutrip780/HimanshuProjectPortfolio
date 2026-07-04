using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskApi.Model.Dto;
using TaskApi.Services;
using Workflow.IO.Shared.Contracts;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        private readonly ILogger<TaskController> _logger;

        public TaskController(
            ITaskService taskService,
            ILogger<TaskController> logger)
        {
            _taskService = taskService;

            _logger = logger;
        }

        [HttpPost("projects/{projectId:guid}/tasks")]
        public async Task<IActionResult>
            CreateTask(
                Guid projectId,
                [FromBody]
                CreateTaskRequestDto request)
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized();
            }

            var reporterId = Guid.Parse(userIdClaim);

            var task =
                await _taskService
                    .CreateTaskAsync(
                        projectId,
                        reporterId,
                        request);

            _logger.LogInformation(
                "Task {TaskId} created in project {ProjectId} by user {ReporterId}",
                task.TaskId,
                projectId,
                reporterId);

            return Ok(
                ApiResponse<TaskResponseDto>.Ok(
                    task,
                    "Task created successfully"));
        }

        [HttpGet("tasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            var userId = GetCurrentUserId();

            var tasks = await _taskService.GetAllTasksAsync(userId);

            return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks));
        }

        [HttpGet("projects/{projectId:guid}/tasks")]
        public async Task<IActionResult>
            GetProjectTasks(Guid projectId)
        {
            var userId = GetCurrentUserId();

            var tasks =
                await _taskService
                    .GetProjectTasksAsync(
                        projectId,
                        userId);

            return Ok(
                ApiResponse<IEnumerable<TaskResponseDto>>.Ok(
                    tasks));
        }

        [HttpGet("projects/{projectId:guid}/tasks/search")]
        public async Task<IActionResult> SearchProjectTasks(
            Guid projectId,
            [FromQuery] TaskSearchRequestDto request)
        {
            var tasks =
                await _taskService.SearchProjectTasksAsync(
                    projectId,
                    GetCurrentUserId(),
                    request);

            return Ok(
                ApiResponse<IEnumerable<TaskResponseDto>>.Ok(
                    tasks));
        }

        [HttpPost("projects/{projectId:guid}/boards")]
        public async Task<IActionResult> CreateBoard(
            Guid projectId,
            [FromBody] CreateBoardRequestDto request)
        {
            var board =
                await _taskService.CreateBoardAsync(
                    projectId,
                    GetCurrentUserId(),
                    request);

            return Ok(
                ApiResponse<BoardResponseDto>.Ok(
                    board,
                    "Board created successfully"));
        }

        [HttpGet("projects/{projectId:guid}/board")]
        public async Task<IActionResult> GetBoardView(
            Guid projectId)
        {
            var board =
                await _taskService.GetBoardViewAsync(
                    projectId,
                    GetCurrentUserId());

            if (board == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<BoardViewResponseDto>.Ok(
                    board));
        }

        [HttpGet("projects/{projectId:guid}/backlog")]
        public async Task<IActionResult> GetBacklog(
            Guid projectId)
        {
            var backlog =
                await _taskService.GetBacklogAsync(
                    projectId,
                    GetCurrentUserId());

            return Ok(
                ApiResponse<BacklogResponseDto>.Ok(
                    backlog));
        }

        [HttpPost("projects/{projectId:guid}/sprints")]
        public async Task<IActionResult> CreateSprint(
            Guid projectId,
            [FromBody] CreateSprintRequestDto request)
        {
            var sprint =
                await _taskService.CreateSprintAsync(
                    projectId,
                    GetCurrentUserId(),
                    request);

            return Ok(
                ApiResponse<SprintResponseDto>.Ok(
                    sprint,
                    "Sprint created successfully"));
        }

        [HttpGet("projects/{projectId:guid}/sprints")]
        public async Task<IActionResult> GetProjectSprints(
            Guid projectId)
        {
            var sprints =
                await _taskService.GetProjectSprintsAsync(
                    projectId,
                    GetCurrentUserId());

            return Ok(
                ApiResponse<IEnumerable<SprintResponseDto>>.Ok(
                    sprints));
        }

        [HttpPost("projects/{projectId:guid}/epics")]
        public async Task<IActionResult> CreateEpic(
            Guid projectId,
            [FromBody] CreateEpicRequestDto request)
        {
            var epic =
                await _taskService.CreateEpicAsync(
                    projectId,
                    GetCurrentUserId(),
                    request);

            return Ok(
                ApiResponse<EpicResponseDto>.Ok(
                    epic,
                    "Epic created successfully"));
        }

        [HttpGet("projects/{projectId:guid}/epics")]
        public async Task<IActionResult> GetProjectEpics(
            Guid projectId)
        {
            var epics =
                await _taskService.GetProjectEpicsAsync(
                    projectId,
                    GetCurrentUserId());

            return Ok(
                ApiResponse<IEnumerable<EpicResponseDto>>.Ok(
                    epics));
        }

        [HttpGet("tasks/{taskId:guid}")]
        public async Task<IActionResult>
            GetTaskById(Guid taskId)
        {
            var userId = GetCurrentUserId();

            var task =
                await _taskService
                    .GetTaskByIdAsync(
                        taskId,
                        userId);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<TaskResponseDto>.Ok(task));
        }

        [HttpPut("tasks/{taskId:guid}")]
        public async Task<IActionResult>
            UpdateTask(
                Guid taskId,
                [FromBody]
                UpdateTaskRequestDto request)
        {
            var userId = GetCurrentUserId();

            var task =
                await _taskService
                    .UpdateTaskAsync(
                        taskId,
                        userId,
                        request);

            if (task == null)
            {
                return NotFound();
            }

            _logger.LogInformation(
                "Task {TaskId} updated",
                taskId);

            return Ok(
                ApiResponse<TaskResponseDto>.Ok(
                    task,
                    "Task updated successfully"));
        }

        [HttpPatch("tasks/{taskId:guid}/status")]
        public async Task<IActionResult>
            ChangeStatus(
                Guid taskId,
                [FromBody]
                ChangeTaskStatusRequestDto request)
        {
            var userId = GetCurrentUserId();

            var task =
                await _taskService
                    .ChangeStatusAsync(
                        taskId,
                        userId,
                        request);

            if (task == null)
            {
                return NotFound();
            }

            _logger.LogInformation(
                "Task {TaskId} status changed to {Status}",
                taskId,
                task.Status);

            return Ok(
                ApiResponse<TaskResponseDto>.Ok(
                    task,
                    "Task status updated successfully"));
        }

        [HttpPatch("tasks/{taskId:guid}/assign")]
        public async Task<IActionResult>
            AssignTask(
                Guid taskId,
                [FromBody]
                AssignTaskRequestDto request)
        {
            var userId = GetCurrentUserId();

            var task =
                await _taskService
                    .AssignTaskAsync(
                        taskId,
                        userId,
                        request);

            if (task == null)
            {
                return NotFound();
            }

            _logger.LogInformation(
                "Task {TaskId} assigned to {AssigneeId}",
                taskId,
                task.AssigneeId);

            return Ok(
                ApiResponse<TaskResponseDto>.Ok(
                    task,
                    "Task assignment updated successfully"));
        }

        [HttpPatch("tasks/{taskId:guid}/sprint")]
        public async Task<IActionResult> MoveTaskToSprint(
            Guid taskId,
            [FromBody] MoveTaskToSprintRequestDto request)
        {
            var task =
                await _taskService.MoveTaskToSprintAsync(
                    taskId,
                    GetCurrentUserId(),
                    request);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<TaskResponseDto>.Ok(
                    task,
                    request.SprintId.HasValue
                        ? "Task moved to sprint successfully"
                        : "Task moved to backlog successfully"));
        }

        [HttpPatch("tasks/{taskId:guid}/epic")]
        public async Task<IActionResult> AssignEpic(
            Guid taskId,
            [FromBody] AssignEpicRequestDto request)
        {
            var task =
                await _taskService.AssignEpicAsync(
                    taskId,
                    GetCurrentUserId(),
                    request);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<TaskResponseDto>.Ok(
                    task,
                    request.EpicId.HasValue
                        ? "Task assigned to epic successfully"
                        : "Task removed from epic successfully"));
        }

        [HttpPatch("tasks/{taskId:guid}/story-points")]
        public async Task<IActionResult> UpdateStoryPoints(
            Guid taskId,
            [FromBody] UpdateStoryPointsRequestDto request)
        {
            var task =
                await _taskService.UpdateStoryPointsAsync(
                    taskId,
                    GetCurrentUserId(),
                    request);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<TaskResponseDto>.Ok(
                    task,
                    "Task story points updated successfully"));
        }

        [HttpPatch("tasks/{taskId:guid}/team")]
        public async Task<IActionResult> AssignTeam(
            Guid taskId,
            [FromBody] AssignTeamRequestDto request)
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.AssignTeamAsync(taskId, userId, request);
            if (task == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Task {TaskId} assigned to team {TeamId}", taskId, request.TeamId);
            return Ok(ApiResponse<TaskResponseDto>.Ok(task, "Task team updated successfully"));
        }

        [HttpPost("tasks/{taskId:guid}/watchers/me")]
        public async Task<IActionResult> WatchTask(
            Guid taskId)
        {
            var watcher =
                await _taskService.WatchTaskAsync(
                    taskId,
                    GetCurrentUserId());

            if (watcher == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<TaskWatcherResponseDto>.Ok(
                    watcher,
                    "Task watcher added successfully"));
        }

        [HttpGet("tasks/{taskId:guid}/watchers")]
        public async Task<IActionResult> GetWatchers(
            Guid taskId)
        {
            var watchers =
                await _taskService.GetWatchersAsync(
                    taskId,
                    GetCurrentUserId());

            if (watchers == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<IEnumerable<TaskWatcherResponseDto>>.Ok(
                    watchers));
        }

        [HttpDelete("tasks/{taskId:guid}/watchers/me")]
        public async Task<IActionResult> UnwatchTask(
            Guid taskId)
        {
            var removed =
                await _taskService.UnwatchTaskAsync(
                    taskId,
                    GetCurrentUserId());

            if (!removed)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPatch("sprints/{sprintId:guid}/start")]
        public async Task<IActionResult> StartSprint(
            Guid sprintId)
        {
            var sprint =
                await _taskService.StartSprintAsync(
                    sprintId,
                    GetCurrentUserId());

            if (sprint == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<SprintResponseDto>.Ok(
                    sprint,
                    "Sprint started successfully"));
        }

        [HttpPatch("sprints/{sprintId:guid}/complete")]
        public async Task<IActionResult> CompleteSprint(
            Guid sprintId)
        {
            var sprint =
                await _taskService.CompleteSprintAsync(
                    sprintId,
                    GetCurrentUserId());

            if (sprint == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<SprintResponseDto>.Ok(
                    sprint,
                    "Sprint completed successfully"));
        }

        [HttpDelete("tasks/{taskId:guid}")]
        public async Task<IActionResult>
            DeleteTask(Guid taskId)
        {
            var userId = GetCurrentUserId();

            var deleted =
                await _taskService
                    .DeleteTaskAsync(
                        taskId,
                        userId);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("tasks/{taskId:guid}/labels")]
        public async Task<IActionResult> AddLabel(
            Guid taskId,
            [FromBody] AddTaskLabelRequestDto request)
        {
            var label =
                await _taskService.AddLabelAsync(
                    taskId,
                    GetCurrentUserId(),
                    request);

            if (label == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<TaskLabelResponseDto>.Ok(
                    label,
                    "Task label added successfully"));
        }

        [HttpGet("tasks/{taskId:guid}/labels")]
        public async Task<IActionResult> GetLabels(
            Guid taskId)
        {
            var labels =
                await _taskService.GetLabelsAsync(
                    taskId,
                    GetCurrentUserId());

            if (labels == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<IEnumerable<TaskLabelResponseDto>>.Ok(
                    labels));
        }

        [HttpDelete("tasks/{taskId:guid}/labels/{labelId:guid}")]
        public async Task<IActionResult> RemoveLabel(
            Guid taskId,
            Guid labelId)
        {
            var removed =
                await _taskService.RemoveLabelAsync(
                    taskId,
                    labelId,
                    GetCurrentUserId());

            if (!removed)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("tasks/{taskId:guid}/subtasks")]
        public async Task<IActionResult> CreateSubTask(
            Guid taskId,
            [FromBody] CreateSubTaskRequestDto request)
        {
            var subTask =
                await _taskService.CreateSubTaskAsync(
                    taskId,
                    GetCurrentUserId(),
                    request);

            if (subTask == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<SubTaskResponseDto>.Ok(
                    subTask,
                    "Subtask created successfully"));
        }

        [HttpGet("tasks/{taskId:guid}/subtasks")]
        public async Task<IActionResult> GetSubTasks(
            Guid taskId)
        {
            var subTasks =
                await _taskService.GetSubTasksAsync(
                    taskId,
                    GetCurrentUserId());

            if (subTasks == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<IEnumerable<SubTaskResponseDto>>.Ok(
                    subTasks));
        }

        [HttpPatch("subtasks/{subTaskId:guid}/completion")]
        public async Task<IActionResult> ChangeSubTaskCompletion(
            Guid subTaskId,
            [FromBody] ChangeSubTaskCompletionRequestDto request)
        {
            var subTask =
                await _taskService.ChangeSubTaskCompletionAsync(
                    subTaskId,
                    GetCurrentUserId(),
                    request);

            if (subTask == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<SubTaskResponseDto>.Ok(
                    subTask,
                    "Subtask completion updated successfully"));
        }

        [HttpDelete("subtasks/{subTaskId:guid}")]
        public async Task<IActionResult> DeleteSubTask(
            Guid subTaskId)
        {
            var deleted =
                await _taskService.DeleteSubTaskAsync(
                    subTaskId,
                    GetCurrentUserId());

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("tasks/{taskId:guid}/child-tasks")]
        public async Task<IActionResult> GetChildTasks(Guid taskId)
        {
            var userId = GetCurrentUserId();
            var childTasks = await _taskService.GetChildTasksAsync(taskId, userId);
            return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(childTasks));
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new UnauthorizedAccessException();
            }

            return Guid.Parse(userIdClaim);
        }
    }
}
