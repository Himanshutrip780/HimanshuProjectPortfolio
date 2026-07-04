using ActivityApi.Clients;
using ActivityApi.Data;
using ActivityApi.Model.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Workflow.IO.Shared.Contracts;

namespace ActivityApi.Controllers
{
    [ApiController]
    [Route("api/activities")]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly ActivityDbContext _context;
        private readonly IProjectAccessClient _projectAccessClient;
        private readonly ITaskAccessClient _taskAccessClient;

        public ActivityController(
            ActivityDbContext context,
            IProjectAccessClient projectAccessClient,
            ITaskAccessClient taskAccessClient)
        {
            _context = context;
            _projectAccessClient = projectAccessClient;
            _taskAccessClient = taskAccessClient;
        }

        [HttpPost("events")]
        public async Task<IActionResult> ConsumeEvent(
            [FromBody] IntegrationEventRequest request)
        {
            var activity = new ActivityRecord(
                request.EventType,
                request.EntityType,
                request.EntityId,
                request.ActorId,
                request.Description,
                request.PayloadJson);

            await _context.Activities.AddAsync(activity);

            await _context.SaveChangesAsync();

            return Ok(
                ApiResponse<object>.Ok(
                    new { activity.ActivityRecordId },
                    "Activity event consumed"));
        }

        [HttpGet("{entityType}/{entityId:guid}")]
        public async Task<IActionResult> GetEntityActivities(
            string entityType,
            Guid entityId)
        {
            if (string.Equals(entityType, "Project", StringComparison.OrdinalIgnoreCase))
            {
                var hasAccess = await _projectAccessClient.IsProjectAccessibleAsync(entityId);
                if (!hasAccess)
                {
                    return Forbid();
                }
            }
            else if (string.Equals(entityType, "Task", StringComparison.OrdinalIgnoreCase))
            {
                var hasAccess = await _taskAccessClient.IsTaskAccessibleAsync(entityId);
                if (!hasAccess)
                {
                    return Forbid();
                }
            }

            var activities =
                await _context.Activities
                    .Where(x =>
                        x.EntityType == entityType &&
                        x.EntityId == entityId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

            return Ok(
                ApiResponse<IEnumerable<ActivityRecord>>.Ok(
                    activities));
        }

        [HttpGet("projects/{projectId:guid}")]
        public async Task<IActionResult> GetProjectActivities(
            Guid projectId,
            [FromQuery] int limit = 50)
        {
            var hasAccess = await _projectAccessClient.IsProjectAccessibleAsync(projectId);
            if (!hasAccess)
            {
                return Forbid();
            }

            var projectFilter = projectId.ToString();

            var activities =
                await _context.Activities
                    .AsNoTracking()
                    .Where(x =>
                        x.PayloadJson != null &&
                        x.PayloadJson.Contains(projectFilter))
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

            return Ok(
                ApiResponse<IEnumerable<ActivityRecord>>.Ok(
                    activities));
        }
    }
}
