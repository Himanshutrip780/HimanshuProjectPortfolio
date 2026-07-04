using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.CandidateNotes;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Recruiter,HiringManager")]
    public class CandidateNotesController : ApiControllerBase
    {
        [HttpGet("candidate/{candidateId}")]
        public async Task<ActionResult<Result<List<CandidateNoteDto>>>> GetNotes(Guid candidateId, [FromQuery] Guid? applicationId)
        {
            var result = await Mediator.Send(new GetCandidateNotesQuery
            {
                CandidateId = candidateId,
                ApplicationId = applicationId,
                CompanyId = CompanyId
            });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<Guid>>> Create(CreateCandidateNoteCommand command)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? $"{User.FindFirstValue("FirstName")} {User.FindFirstValue("LastName")}";
            var commandWithAuthor = command with { AuthorName = userName };
            var result = await Mediator.Send(commandWithAuthor);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result>> Delete(Guid id)
        {
            var result = await Mediator.Send(new DeleteCandidateNoteCommand(id));
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
