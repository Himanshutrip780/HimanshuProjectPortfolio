using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskApi.Clients;
using TaskApi.Services;
using Workflow.IO.Shared.Contracts;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("api/tasks/projects/{projectId:guid}/daily-update")]
    [Authorize]
    public class DailyUpdateController : ControllerBase
    {
        private readonly ITaskDailyUpdateService _dailyUpdateService;
        private readonly IProjectAccessClient _projectAccessClient;

        public DailyUpdateController(
            ITaskDailyUpdateService dailyUpdateService,
            IProjectAccessClient projectAccessClient)
        {
            _dailyUpdateService = dailyUpdateService;
            _projectAccessClient = projectAccessClient;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new UnauthorizedAccessException();
            }
            return Guid.Parse(userIdClaim);
        }

        private async Task<bool> HasAccessAsync(Guid projectId)
        {
            return await _projectAccessClient.IsProjectMemberAsync(projectId, GetUserId());
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyUpdateState(Guid projectId)
        {
            if (!await HasAccessAsync(projectId))
            {
                return Forbid();
            }

            var state = await _dailyUpdateService.GetDailyUpdateStateAsync(projectId);

            var extraRecipientsArray = !string.IsNullOrWhiteSpace(state.ExtraRecipients)
                ? state.ExtraRecipients.Split(',').Select(x => x.Trim()).ToArray()
                : Array.Empty<string>();

            // Trigger button is enabled from 11 AM onwards
            var currentHour = DateTime.Now.Hour;
            var allowedManualTrigger = currentHour >= 11;

            var responseData = new
            {
                state.ProjectId,
                state.LastSentAt,
                state.IsTriggeredToday,
                ExtraRecipients = extraRecipientsArray,
                AllowedManualTrigger = allowedManualTrigger
            };

            return Ok(ApiResponse<object>.Ok(responseData));
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendDailyUpdate(Guid projectId)
        {
            if (!await HasAccessAsync(projectId))
            {
                return Forbid();
            }

            var success = await _dailyUpdateService.TriggerDailyUpdateAsync(projectId, GetUserId());
            if (!success)
            {
                return BadRequest(new { Success = false, Message = "Failed to trigger daily sprint update. Verify if active sprint exists." });
            }

            return Ok(ApiResponse<object>.Ok(null!, "Daily sprint update email sent successfully."));
        }

        [HttpPost("recipients")]
        public async Task<IActionResult> SaveRecipients(Guid projectId, [FromBody] SaveRecipientsRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Success = false, Message = "Invalid request." });
            }

            if (!await HasAccessAsync(projectId))
            {
                return Forbid();
            }

            var state = await _dailyUpdateService.SaveDailyUpdateStateAsync(projectId, request.ExtraRecipients);

            var extraRecipientsArray = !string.IsNullOrWhiteSpace(state.ExtraRecipients)
                ? state.ExtraRecipients.Split(',').Select(x => x.Trim()).ToArray()
                : Array.Empty<string>();

            return Ok(ApiResponse<object>.Ok(extraRecipientsArray, "Recipients updated successfully."));
        }
    }

    public class SaveRecipientsRequest
    {
        public string[] ExtraRecipients { get; set; } = Array.Empty<string>();
    }
}
