using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.Offers;
using ATS.Domain.Enums;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Recruiter")]
    public class OffersController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<Result<List<OfferDto>>>> GetOffers(
            [FromQuery] Guid? applicationId,
            [FromQuery] OfferStatus? status)
        {
            var result = await Mediator.Send(new GetOffersQuery
            {
                CompanyId = CompanyId,
                ApplicationId = applicationId,
                Status = status
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<OfferDto>>> GetOffer(Guid id)
        {
            var result = await Mediator.Send(new GetOfferByIdQuery(id, CompanyId));
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<Guid>>> Create(CreateOfferCommand command)
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

        [HttpPut("{id}/status")]
        public async Task<ActionResult<Result>> UpdateStatus(Guid id, [FromBody] UpdateOfferStatusModel model)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? $"{User.FindFirstValue("FirstName")} {User.FindFirstValue("LastName")}";
            var clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                ?? HttpContext.Connection.RemoteIpAddress?.ToString() 
                ?? "127.0.0.1";

            var result = await Mediator.Send(new UpdateOfferStatusCommand
            {
                OfferId = id,
                Status = model.Status,
                ESignatureDetails = model.ESignatureDetails,
                Actor = userName,
                ClientIpAddress = clientIp
            });

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("{id}/letter")]
        public async Task<ActionResult<Result>> UpdateLetter(Guid id, [FromBody] UpdateOfferLetterModel model)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? $"{User.FindFirstValue("FirstName")} {User.FindFirstValue("LastName")}";
            var result = await Mediator.Send(new UpdateOfferLetterCommand
            {
                OfferId = id,
                CompanyId = CompanyId,
                OfferLetterContent = model.OfferLetterContent,
                Actor = userName
            });

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }

    public class UpdateOfferStatusModel
    {
        public OfferStatus Status { get; set; }
        public string ESignatureDetails { get; set; }
    }

    public class UpdateOfferLetterModel
    {
        public string OfferLetterContent { get; set; }
    }
}
