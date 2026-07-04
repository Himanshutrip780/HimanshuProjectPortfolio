using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.Interviews;
using ATS.Domain.Enums;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Recruiter,HiringManager,Interviewer")]
    public class InterviewsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<Result<List<InterviewDto>>>> GetInterviews(
            [FromQuery] Guid? applicationId,
            [FromQuery] Guid? interviewerId,
            [FromQuery] InterviewStatus? status)
        {
            var result = await Mediator.Send(new GetInterviewsQuery
            {
                CompanyId = CompanyId,
                ApplicationId = applicationId,
                InterviewerId = interviewerId,
                Status = status
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<InterviewDto>>> GetInterview(Guid id)
        {
            var result = await Mediator.Send(new GetInterviewByIdQuery(id, CompanyId));
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result<Guid>>> Schedule(ScheduleInterviewCommand command)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? $"{User.FindFirstValue("FirstName")} {User.FindFirstValue("LastName")}";
            var commandWithActor = command with { Actor = userName };
            var result = await Mediator.Send(commandWithActor);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/feedback")]
        public async Task<ActionResult<Result>> SubmitFeedback(Guid id, SubmitFeedbackCommand command)
        {
            if (id != command.InterviewId)
            {
                return BadRequest(Result.Failure("Mismatched interview identifier."));
            }

            var userName = User.FindFirstValue(ClaimTypes.Name) ?? $"{User.FindFirstValue("FirstName")} {User.FindFirstValue("LastName")}";
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdStr, out var userId);

            var commandWithActor = command with { InterviewerId = userId, Actor = userName };
            var result = await Mediator.Send(commandWithActor);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result>> Cancel(Guid id)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? $"{User.FindFirstValue("FirstName")} {User.FindFirstValue("LastName")}";
            var result = await Mediator.Send(new CancelInterviewCommand(id, userName));
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/reschedule")]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result>> Reschedule(Guid id, [FromBody] RescheduleModel model)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? $"{User.FindFirstValue("FirstName")} {User.FindFirstValue("LastName")}";
            var result = await Mediator.Send(new RescheduleInterviewCommand(id, model.ScheduledTime, model.DurationMinutes, userName));
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }

    public class RescheduleModel
    {
        public DateTime ScheduledTime { get; set; }
        public int DurationMinutes { get; set; }
    }
}
