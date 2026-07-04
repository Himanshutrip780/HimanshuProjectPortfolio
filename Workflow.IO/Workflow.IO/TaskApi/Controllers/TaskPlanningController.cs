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
    public class TaskPlanningController : ControllerBase
    {
        private readonly ITaskJiraFeatureService _jiraFeatures;

        public TaskPlanningController(
            ITaskJiraFeatureService jiraFeatures)
        {
            _jiraFeatures = jiraFeatures;
        }

        [HttpGet("issues/{issueKey}")]
        public async Task<IActionResult> GetByIssueKey(string issueKey)
        {
            var task =
                await _jiraFeatures.GetTaskByIssueKeyAsync(
                    issueKey,
                    GetUserId());

            if (task == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<TaskResponseDto>.Ok(task));
        }

        [HttpPatch("tasks/{taskId:guid}/rank")]
        public async Task<IActionResult> UpdateRank(
            Guid taskId,
            [FromBody] UpdateBacklogRankRequestDto request)
        {
            var task =
                await _jiraFeatures.UpdateBacklogRankAsync(
                    taskId,
                    GetUserId(),
                    request);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<TaskResponseDto>.Ok(task));
        }

        [HttpPost("projects/{projectId:guid}/components")]
        public async Task<IActionResult> CreateComponent(
            Guid projectId,
            [FromBody] CreateComponentRequestDto request)
        {
            var component =
                await _jiraFeatures.CreateComponentAsync(
                    projectId,
                    GetUserId(),
                    request);

            return Ok(
                ApiResponse<ComponentResponseDto>.Ok(
                    component,
                    "Component created"));
        }

        [HttpGet("projects/{projectId:guid}/components")]
        public async Task<IActionResult> GetComponents(Guid projectId)
        {
            var components =
                await _jiraFeatures.GetComponentsAsync(
                    projectId,
                    GetUserId());

            return Ok(
                ApiResponse<IEnumerable<ComponentResponseDto>>.Ok(
                    components));
        }

        [HttpPost("projects/{projectId:guid}/versions")]
        public async Task<IActionResult> CreateVersion(
            Guid projectId,
            [FromBody] CreateReleaseVersionRequestDto request)
        {
            var version =
                await _jiraFeatures.CreateVersionAsync(
                    projectId,
                    GetUserId(),
                    request);

            return Ok(
                ApiResponse<ReleaseVersionResponseDto>.Ok(
                    version,
                    "Version created"));
        }

        [HttpGet("projects/{projectId:guid}/versions")]
        public async Task<IActionResult> GetVersions(Guid projectId)
        {
            var versions =
                await _jiraFeatures.GetVersionsAsync(
                    projectId,
                    GetUserId());

            return Ok(
                ApiResponse<IEnumerable<ReleaseVersionResponseDto>>.Ok(
                    versions));
        }

        [HttpPatch("versions/{versionId:guid}/release")]
        public async Task<IActionResult> ReleaseVersion(Guid versionId)
        {
            var version =
                await _jiraFeatures.ReleaseVersionAsync(
                    versionId,
                    GetUserId());

            if (version == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<ReleaseVersionResponseDto>.Ok(version));
        }

        [HttpPost("tasks/{taskId:guid}/links")]
        public async Task<IActionResult> CreateLink(
            Guid taskId,
            [FromBody] CreateTaskLinkRequestDto request)
        {
            var link =
                await _jiraFeatures.CreateLinkAsync(
                    taskId,
                    GetUserId(),
                    request);

            if (link == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<TaskLinkResponseDto>.Ok(link));
        }

        [HttpGet("tasks/{taskId:guid}/links")]
        public async Task<IActionResult> GetLinks(Guid taskId)
        {
            var links =
                await _jiraFeatures.GetLinksAsync(
                    taskId,
                    GetUserId());

            if (links == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<IEnumerable<TaskLinkResponseDto>>.Ok(links));
        }

        [HttpDelete("links/{linkId:guid}")]
        public async Task<IActionResult> DeleteLink(Guid linkId)
        {
            var deleted =
                await _jiraFeatures.DeleteLinkAsync(
                    linkId,
                    GetUserId());

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("tasks/{taskId:guid}/worklogs")]
        public async Task<IActionResult> AddWorkLog(
            Guid taskId,
            [FromBody] CreateWorkLogRequestDto request)
        {
            var log =
                await _jiraFeatures.AddWorkLogAsync(
                    taskId,
                    GetUserId(),
                    request);

            if (log == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<WorkLogResponseDto>.Ok(log));
        }

        [HttpGet("tasks/{taskId:guid}/worklogs")]
        public async Task<IActionResult> GetWorkLogs(Guid taskId)
        {
            var logs =
                await _jiraFeatures.GetWorkLogsAsync(
                    taskId,
                    GetUserId());

            if (logs == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<IEnumerable<WorkLogResponseDto>>.Ok(logs));
        }

        [HttpPost("projects/{projectId:guid}/tasks/bulk")]
        public async Task<IActionResult> BulkUpdate(
            Guid projectId,
            [FromBody] BulkTaskUpdateRequestDto request)
        {
            var result =
                await _jiraFeatures.BulkUpdateAsync(
                    projectId,
                    GetUserId(),
                    request);

            return Ok(ApiResponse<BulkTaskUpdateResponseDto>.Ok(result));
        }

        [HttpPost("projects/{projectId:guid}/filters")]
        public async Task<IActionResult> CreateFilter(
            Guid projectId,
            [FromBody] CreateSavedFilterRequestDto request)
        {
            var filter =
                await _jiraFeatures.CreateSavedFilterAsync(
                    projectId,
                    GetUserId(),
                    request);

            return Ok(ApiResponse<SavedFilterResponseDto>.Ok(filter));
        }

        [HttpGet("projects/{projectId:guid}/filters")]
        public async Task<IActionResult> GetFilters(Guid projectId)
        {
            var filters =
                await _jiraFeatures.GetSavedFiltersAsync(
                    projectId,
                    GetUserId());

            return Ok(
                ApiResponse<IEnumerable<SavedFilterResponseDto>>.Ok(
                    filters));
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetMyFilters()
        {
            var filters =
                await _jiraFeatures.GetSavedFiltersAsync(
                    null,
                    GetUserId());

            return Ok(
                ApiResponse<IEnumerable<SavedFilterResponseDto>>.Ok(
                    filters));
        }

        [HttpGet("filters/{filterId:guid}/results")]
        public async Task<IActionResult> ExecuteFilter(Guid filterId)
        {
            var tasks =
                await _jiraFeatures.ExecuteSavedFilterAsync(
                    filterId,
                    GetUserId());

            if (tasks == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks));
        }

        [HttpPut("sprints/{sprintId:guid}")]
        public async Task<IActionResult> UpdateSprint(
            Guid sprintId,
            [FromBody] UpdateSprintRequestDto request)
        {
            var sprint =
                await _jiraFeatures.UpdateSprintAsync(
                    sprintId,
                    GetUserId(),
                    request);

            if (sprint == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<SprintResponseDto>.Ok(sprint));
        }

        [HttpDelete("sprints/{sprintId:guid}")]
        public async Task<IActionResult> DeleteSprint(Guid sprintId)
        {
            var deleted =
                await _jiraFeatures.DeleteSprintAsync(
                    sprintId,
                    GetUserId());

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        private Guid GetUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new UnauthorizedAccessException();
            }

            return Guid.Parse(userIdClaim);
        }
    }
}
