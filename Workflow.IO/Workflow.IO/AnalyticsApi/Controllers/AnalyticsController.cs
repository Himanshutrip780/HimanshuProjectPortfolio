using System.Security.Claims;
using AnalyticsApi.Clients;
using AnalyticsApi.Model.Dto;
using AnalyticsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workflow.IO.Shared.Contracts;

namespace AnalyticsApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsQueryService _analyticsQueryService;

        private readonly IProjectAccessClient _projectAccessClient;

        public AnalyticsController(
            IAnalyticsQueryService analyticsQueryService,
            IProjectAccessClient projectAccessClient)
        {
            _analyticsQueryService = analyticsQueryService;
            _projectAccessClient = projectAccessClient;
        }

        [HttpGet("projects/{projectId:guid}/summary")]
        public async Task<ActionResult<ApiResponse<ProjectAnalyticsSummaryDto>>>
            GetProjectSummary(Guid projectId)
        {
            await EnsureProjectAccessAsync(projectId);

            var summary =
                await _analyticsQueryService
                    .GetProjectSummaryAsync(projectId);

            return Ok(
                ApiResponse<ProjectAnalyticsSummaryDto>.Ok(summary));
        }

        [HttpGet("projects/{projectId:guid}/tasks-by-status")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MetricBucketDto>>>>
            GetTasksByStatus(Guid projectId)
        {
            await EnsureProjectAccessAsync(projectId);

            var buckets =
                await _analyticsQueryService
                    .GetTasksByStatusAsync(projectId);

            return Ok(
                ApiResponse<IEnumerable<MetricBucketDto>>.Ok(buckets));
        }

        [HttpGet("projects/{projectId:guid}/sprints/{sprintId:guid}/velocity")]
        public async Task<ActionResult<ApiResponse<SprintVelocityDto>>>
            GetSprintVelocity(
                Guid projectId,
                Guid sprintId)
        {
            await EnsureProjectAccessAsync(projectId);

            var velocity =
                await _analyticsQueryService
                    .GetSprintVelocityAsync(
                        projectId,
                        sprintId);

            return Ok(
                ApiResponse<SprintVelocityDto>.Ok(velocity));
        }

        [HttpGet("projects/{projectId:guid}/activity-trends")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ActivityTrendDto>>>>
            GetActivityTrends(
                Guid projectId,
                [FromQuery] int days = 14)
        {
            await EnsureProjectAccessAsync(projectId);

            var trends =
                await _analyticsQueryService
                    .GetActivityTrendsAsync(
                        projectId,
                        days);

            return Ok(
                ApiResponse<IEnumerable<ActivityTrendDto>>.Ok(trends));
        }

        [HttpGet("projects/{projectId:guid}/sprints/{sprintId:guid}/burndown")]
        public async Task<ActionResult<ApiResponse<SprintBurndownDto>>>
            GetSprintBurndown(
                Guid projectId,
                Guid sprintId,
                [FromQuery] int days = 14)
        {
            await EnsureProjectAccessAsync(projectId);

            var burndown =
                await _analyticsQueryService
                    .GetSprintBurndownAsync(
                        projectId,
                        sprintId,
                        days);

            return Ok(
                ApiResponse<SprintBurndownDto>.Ok(burndown));
        }

        private async Task EnsureProjectAccessAsync(Guid projectId)
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new UnauthorizedAccessException();
            }

            var hasAccess =
                await _projectAccessClient.IsProjectMemberAsync(
                    projectId,
                    Guid.Parse(userIdClaim));

            if (!hasAccess)
            {
                throw new Workflow.IO.Shared.Exceptions.ForbiddenException(
                    "User cannot access analytics for this project");
            }
        }
    }
}
