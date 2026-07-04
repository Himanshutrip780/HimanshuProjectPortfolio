using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskApi.Model.Dto;
using TaskApi.Services;
using Workflow.IO.Shared.Contracts;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class AutomationRulesController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public AutomationRulesController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost("projects/{projectId:guid}/automation-rules")]
        public async Task<IActionResult> CreateAutomationRule(
            Guid projectId,
            [FromBody] CreateAutomationRuleRequestDto request)
        {
            var rule = await _taskService.CreateAutomationRuleAsync(
                projectId,
                GetCurrentUserId(),
                request);

            return Ok(ApiResponse<AutomationRuleResponseDto>.Ok(rule, "Automation rule created successfully"));
        }

        [HttpGet("projects/{projectId:guid}/automation-rules")]
        public async Task<IActionResult> GetProjectAutomationRules(Guid projectId)
        {
            var rules = await _taskService.GetProjectAutomationRulesAsync(
                projectId,
                GetCurrentUserId());

            return Ok(ApiResponse<IEnumerable<AutomationRuleResponseDto>>.Ok(rules));
        }

        [HttpDelete("automation-rules/{ruleId:guid}")]
        public async Task<IActionResult> DeleteAutomationRule(Guid ruleId)
        {
            var deleted = await _taskService.DeleteAutomationRuleAsync(
                ruleId,
                GetCurrentUserId());

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("automation-rules/{ruleId:guid}/toggle")]
        public async Task<IActionResult> ToggleAutomationRule(
            Guid ruleId,
            [FromQuery] bool isEnabled)
        {
            var rule = await _taskService.ToggleAutomationRuleAsync(
                ruleId,
                GetCurrentUserId(),
                isEnabled);

            if (rule == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<AutomationRuleResponseDto>.Ok(rule, "Automation rule status updated"));
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new UnauthorizedAccessException();
            }
            return Guid.Parse(userIdClaim);
        }
    }
}
