using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectApi.Model.Domain.Entities;
using ProjectApi.Model.Dto;
using ProjectApi.Services;
using System.Security.Claims;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Workflow.IO.Shared.Authorization;
using Workflow.IO.Shared.Contracts;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ProjectController(IProjectService projectService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _projectService = projectService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost]
        [RequirePermission(Permissions.ProjectsCreate)]
        public async Task<IActionResult> CreateProject(
            [FromBody] CreateProjectRequestDto request)
        {
            var ownerId = GetCurrentUserId();

            var project =
                await _projectService.CreateProjectAsync(
                    ownerId,
                    request);

            return Ok(
                ApiResponse<ProjectResponseDto>.Ok(
                    project,
                    "Project created successfully"));
        }

        [HttpGet]
        [RequirePermission(Permissions.ProjectsView)]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = GetCurrentUserId();

            var projects =
                await _projectService.GetUserProjectsAsync(userId);

            return Ok(
                ApiResponse<IEnumerable<ProjectResponseDto>>.Ok(
                    projects));
        }


        [HttpGet("key/{projectKey}")]
        [RequirePermission(Permissions.ProjectsView)]
        public async Task<IActionResult> GetProjectByKey(string projectKey)
        {
            var userId = GetCurrentUserId();

            var project =
                await _projectService.GetProjectByKeyAsync(
                    projectKey,
                    userId);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<ProjectResponseDto>.Ok(project));
        }

        [HttpGet("{projectId:guid}")]
        [RequirePermission(Permissions.ProjectsView)]
        public async Task<IActionResult> GetProjectById(Guid projectId)
        {
            var userId = GetCurrentUserId();

            var project =
                await _projectService.GetProjectByIdAsync(
                    projectId,
                    userId);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<ProjectResponseDto>.Ok(project));
        }

        [HttpPut("{projectId:guid}")]
        [RequirePermission(Permissions.ProjectsEdit)]
        public async Task<IActionResult> UpdateProject(
            Guid projectId,
            [FromBody] UpdateProjectRequestDto request)
        {
            var userId = GetCurrentUserId();

            var project =
                await _projectService.UpdateProjectAsync(
                    projectId,
                    userId,
                    request);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<ProjectResponseDto>.Ok(
                    project,
                    "Project updated successfully"));
        }

        [HttpDelete("{projectId:guid}")]
        [RequirePermission(Permissions.ProjectsDelete)]
        public async Task<IActionResult> DeleteProject(Guid projectId)
        {
            var userId = GetCurrentUserId();

            var deleted =
                await _projectService.DeleteProjectAsync(
                    projectId,
                    userId);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPatch("{projectId:guid}/archive")]
        [RequirePermission(Permissions.ProjectsDelete)]
        public async Task<IActionResult> ArchiveProject(Guid projectId)
        {
            var userId = GetCurrentUserId();

            var archived =
                await _projectService.ArchiveProjectAsync(
                    projectId,
                    userId);

            if (!archived)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<object>.Ok(
                    new { projectId },
                    "Project archived successfully"));
        }


        [HttpPost("import")]
        [RequirePermission(Permissions.ProjectsCreate)]
        public async Task<IActionResult> ImportProject([FromBody] ImportProjectRequestDto request)
        {
            var ownerId = GetCurrentUserId();

            var createRequest = new CreateProjectRequestDto
            {
                Name = request.Name,
                Description = request.Description,
                Key = request.Key,
                ProjectType = (ProjectApi.Model.Domain.Enums.ProjectType)request.ProjectType
            };

            var project = await _projectService.CreateProjectAsync(ownerId, createRequest);

            if (request.Tasks != null && request.Tasks.Any())
            {
                var authorizationHeader = Request.Headers["Authorization"].ToString();
                var client = _httpClientFactory.CreateClient();
                if (!string.IsNullOrWhiteSpace(authorizationHeader))
                {
                    client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
                }

                var orgIdHeader = Request.Headers["X-Organization-ID"].ToString();
                if (!string.IsNullOrWhiteSpace(orgIdHeader))
                {
                    client.DefaultRequestHeaders.Add("X-Organization-ID", orgIdHeader);
                }

                var workspaceIdHeader = Request.Headers["X-Workspace-ID"].ToString();
                if (!string.IsNullOrWhiteSpace(workspaceIdHeader))
                {
                    client.DefaultRequestHeaders.Add("X-Workspace-ID", workspaceIdHeader);
                }

                var taskApiBaseUrl = _configuration["Services:TaskApi"] ?? "http://taskapi:8080/";
                var taskApiUrl = $"{taskApiBaseUrl.TrimEnd('/')}/api/projects/{project.ProjectId}/tasks";

                foreach (var taskDto in request.Tasks)
                {
                    var payload = new
                    {
                        title = taskDto.Title,
                        description = taskDto.Description,
                        priority = taskDto.Priority,
                        issueType = taskDto.IssueType,
                        dueDate = taskDto.DueDate,
                        storyPoints = taskDto.StoryPoints
                    };

                    try
                    {
                        var response = await client.PostAsJsonAsync(taskApiUrl, payload);
                        if (!response.IsSuccessStatusCode)
                        {
                            var errContent = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Failed to import task {taskDto.Title}: {errContent}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception importing task {taskDto.Title}: {ex.Message}");
                    }
                }
            }

            return Ok(ApiResponse<ProjectResponseDto>.Ok(project, "Project imported successfully with tasks."));
        }

        private Guid GetCurrentUserId()
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
