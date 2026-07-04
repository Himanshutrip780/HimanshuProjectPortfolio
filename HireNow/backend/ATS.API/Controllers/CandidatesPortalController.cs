using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Application.Features.Offers;
using ATS.Domain.Enums;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize(Roles = "Candidate")]
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesPortalController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        private readonly MediatR.ISender _mediator;

        public CandidatesPortalController(IApplicationDbContext context, MediatR.ISender mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        [HttpGet("applications")]
        public async Task<ActionResult<Result<List<object>>>> GetApplications()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var candidateId))
            {
                return BadRequest(Result<List<object>>.Failure("Invalid candidate identifier."));
            }

            var applications = await _context.Applications
                .AsNoTracking()
                .Include(a => a.Job)
                .Where(a => a.CandidateId == candidateId)
                .Select(a => new
                {
                    a.Id,
                    a.JobId,
                    JobTitle = a.Job.Title,
                    a.CurrentStage,
                    a.Status,
                    a.CreatedDate
                })
                .ToListAsync();

            return Ok(Result<List<object>>.Success(applications.Cast<object>().ToList()));
        }

        [HttpGet("interviews")]
        public async Task<ActionResult<Result<List<object>>>> GetInterviews()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var candidateId))
            {
                return BadRequest(Result<List<object>>.Failure("Invalid candidate identifier."));
            }

            var interviews = await _context.Interviews
                .AsNoTracking()
                .Include(i => i.Application).ThenInclude(a => a.Job)
                .Where(i => i.Application.CandidateId == candidateId)
                .Select(i => new
                {
                    i.Id,
                    i.ApplicationId,
                    JobTitle = i.Application.Job.Title,
                    i.Title,
                    i.Type,
                    i.ScheduledTime,
                    i.DurationMinutes,
                    i.VideoLink,
                    i.Status
                })
                .ToListAsync();

            return Ok(Result<List<object>>.Success(interviews.Cast<object>().ToList()));
        }

        [HttpGet("offers")]
        public async Task<ActionResult<Result<List<object>>>> GetOffers()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var candidateId))
            {
                return BadRequest(Result<List<object>>.Failure("Invalid candidate identifier."));
            }

            var offers = await _context.Offers
                .AsNoTracking()
                .Include(o => o.Application).ThenInclude(a => a.Job)
                .Where(o => o.Application.CandidateId == candidateId)
                .Select(o => new
                {
                    o.Id,
                    o.ApplicationId,
                    JobTitle = o.Application.Job.Title,
                    o.Salary,
                    o.StartDate,
                    o.OfferLetterPath,
                    o.OfferLetterContent,
                    o.Status,
                    o.ESignatureDetails
                })
                .ToListAsync();

            return Ok(Result<List<object>>.Success(offers.Cast<object>().ToList()));
        }

        [HttpPost("offers/{id}/accept")]
        public async Task<ActionResult<Result>> AcceptOffer(Guid id, [FromBody] AcceptOfferModel model)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var candidateId))
            {
                return BadRequest(Result.Failure("Invalid candidate identifier."));
            }

            var userName = User.FindFirstValue(ClaimTypes.Name) ?? $"{User.FindFirstValue("FirstName")} {User.FindFirstValue("LastName")}";
            var clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                ?? HttpContext.Connection.RemoteIpAddress?.ToString() 
                ?? "127.0.0.1";
            
            var result = await _mediator.Send(new UpdateOfferStatusCommand
            {
                OfferId = id,
                Status = OfferStatus.Accepted,
                ESignatureDetails = $"Digitally signed by {model.SignatureName} at {DateTime.UtcNow:g}",
                Actor = userName,
                CandidateId = candidateId,
                ClientIpAddress = clientIp
            });

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("documents")]
        public async Task<ActionResult<Result<List<object>>>> GetDocuments()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var candidateId))
            {
                return BadRequest(Result<List<object>>.Failure("Invalid candidate identifier."));
            }

            var candidate = await _context.Candidates.FirstOrDefaultAsync(x => x.Id == candidateId);
            var docs = new List<object>();

            if (candidate != null && !string.IsNullOrEmpty(candidate.ResumePath))
            {
                docs.Add(new
                {
                    Name = Path.GetFileName(candidate.ResumePath),
                    Type = "Resume File",
                    Path = candidate.ResumePath
                });
            }

            var acceptedOffers = await _context.Offers
                .AsNoTracking()
                .Include(o => o.Application)
                .Where(o => o.Application.CandidateId == candidateId && o.Status == OfferStatus.Accepted)
                .ToListAsync();

            foreach (var offer in acceptedOffers)
            {
                docs.Add(new
                {
                    Name = Path.GetFileName(offer.OfferLetterPath),
                    Type = "Signed Offer Letter",
                    Path = offer.OfferLetterPath
                });
            }

            return Ok(Result<List<object>>.Success(docs));
        }
    }

    public class AcceptOfferModel
    {
        public string SignatureName { get; set; }
    }
}
