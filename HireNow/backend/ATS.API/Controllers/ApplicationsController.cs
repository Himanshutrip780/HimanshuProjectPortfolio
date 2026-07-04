using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.Applications;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Recruiter,HiringManager")]
    public class ApplicationsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<Result<List<ApplicationDto>>>> GetApplications(
            [FromQuery] Guid? jobId,
            [FromQuery] Guid? candidateId,
            [FromQuery] string? stage,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm)
        {
            var result = await Mediator.Send(new GetApplicationsQuery
            {
                CompanyId = CompanyId,
                JobId = jobId,
                CandidateId = candidateId,
                Stage = stage,
                Status = status,
                SearchTerm = searchTerm
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<ApplicationDto>>> GetApplication(Guid id)
        {
            var result = await Mediator.Send(new GetApplicationByIdQuery(id, CompanyId));
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result<Guid>>> Create(SubmitApplicationCommand command)
        {
            var result = await Mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("{id}/stage")]
        public async Task<ActionResult<Result>> UpdateStage(Guid id, [FromBody] UpdateStageModel model)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("FirstName") + " " + User.FindFirstValue("LastName");
            var result = await Mediator.Send(new MoveApplicationStageCommand
            {
                ApplicationId = id,
                NewStage = model.NewStage,
                Actor = userName
            });

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("{id}/timeline")]
        public async Task<ActionResult<Result<List<ActivityLogDto>>>> GetTimeline(Guid id)
        {
            var result = await Mediator.Send(new GetApplicationTimelineQuery(id, CompanyId));
            return Ok(result);
        }
    }

    public class UpdateStageModel
    {
        public string NewStage { get; set; }
    }
}
