using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Entities;
using ATS.Domain.Enums;
using ATS.Shared.Constants;
using ATS.Shared.Models;

namespace ATS.Application.Features.Offers
{
    public record CreateOfferCommand : IRequest<Result<Guid>>
    {
        public Guid ApplicationId { get; init; }
        public decimal Salary { get; init; }
        public DateTime StartDate { get; init; }
        public string OfferLetterContent { get; init; }
        public string Actor { get; init; }
    }

    public class CreateOfferCommandHandler : IRequestHandler<CreateOfferCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public CreateOfferCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
        {
            var app = await _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (app == null || app.Job == null)
            {
                return Result<Guid>.Failure("Application not found.");
            }

            var offer = new Offer
            {
                ApplicationId = request.ApplicationId,
                Salary = request.Salary,
                StartDate = request.StartDate,
                Status = OfferStatus.Draft,
                OfferLetterPath = $"uploads/offers/offer_letter_{request.ApplicationId:N}.pdf",
                OfferLetterContent = request.OfferLetterContent
            };

            await _context.Offers.AddAsync(offer, cancellationToken);

            // Log activity
            await _context.ActivityLogs.AddAsync(new ActivityLog
            {
                ApplicationId = app.Id,
                CandidateId = app.CandidateId,
                Action = "Offer Created",
                Details = $"Offer letter drafted with salary {request.Salary:C} and start date {request.StartDate:d}.",
                PerformedBy = request.Actor ?? "System"
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(offer.Id);
        }
    }

    public record UpdateOfferStatusCommand : IRequest<Result>
    {
        public Guid OfferId { get; init; }
        public OfferStatus Status { get; init; }
        public string ESignatureDetails { get; init; }
        public string Actor { get; init; }
        public Guid? CandidateId { get; init; }
        public string ClientIpAddress { get; init; }
    }

    public class UpdateOfferStatusCommandHandler : IRequestHandler<UpdateOfferStatusCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IMediator _mediator;

        public UpdateOfferStatusCommandHandler(IApplicationDbContext context, IEmailService emailService, IMediator mediator)
        {
            _context = context;
            _emailService = emailService;
            _mediator = mediator;
        }

        public async Task<Result> Handle(UpdateOfferStatusCommand request, CancellationToken cancellationToken)
        {
            var offer = await _context.Offers
                .Include(o => o.Application).ThenInclude(a => a.Candidate)
                .Include(o => o.Application).ThenInclude(a => a.Job)
                .Include(o => o.Application).ThenInclude(a => a.StagesHistory)
                .FirstOrDefaultAsync(o => o.Id == request.OfferId, cancellationToken);

            if (offer == null || offer.Application?.Job == null)
            {
                return Result.Failure("Offer not found.");
            }

            if (request.CandidateId.HasValue && request.CandidateId.Value != Guid.Empty)
            {
                if (offer.Application.CandidateId != request.CandidateId.Value)
                {
                    return Result.Failure("Unauthorized access to offer.");
                }
            }

            var previousStatus = offer.Status;
            offer.Status = request.Status;

            if (!string.IsNullOrEmpty(request.ESignatureDetails))
            {
                offer.ESignatureDetails = request.ESignatureDetails;
                if (!string.IsNullOrEmpty(request.ClientIpAddress))
                {
                    offer.ESignatureDetails += $" (IP: {request.ClientIpAddress})";
                }
            }

            // Log activity
            var actionName = $"Offer {request.Status}";
            var detailsText = $"Offer status updated from '{previousStatus}' to '{request.Status}'.";

            if (request.Status == OfferStatus.Accepted)
            {
                offer.Application.Status = "Hired";
                offer.Application.CurrentStage = Stages.Hired;
                
                // Update previous stage in history
                var currentStageHistory = offer.Application.StagesHistory.FirstOrDefault(s => s.Status == "Current");
                if (currentStageHistory != null)
                {
                    currentStageHistory.Status = "Completed";
                    currentStageHistory.LeftDate = DateTime.UtcNow;
                }
                
                // Add stage history for Hired
                var maxSeq = offer.Application.StagesHistory.Any() ? offer.Application.StagesHistory.Max(s => s.SequenceNumber) : 0;
                var newStage = new ApplicationStage
                {
                    ApplicationId = offer.ApplicationId,
                    StageName = Stages.Hired,
                    Status = "Current",
                    EnteredDate = DateTime.UtcNow,
                    SequenceNumber = maxSeq + 1
                };
                offer.Application.StagesHistory.Add(newStage);
                await _context.ApplicationStages.AddAsync(newStage, cancellationToken);
            }

            await _context.ActivityLogs.AddAsync(new ActivityLog
            {
                ApplicationId = offer.ApplicationId,
                CandidateId = offer.Application.CandidateId,
                Action = actionName,
                Details = detailsText,
                PerformedBy = request.Actor ?? "System"
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            if (request.Status == OfferStatus.Accepted)
            {
                try
                {
                    await _mediator.Publish(new ATS.Application.Common.Events.OfferSignedEvent(offer.Id, request.Actor), cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Domain Event Error] Failed to publish OfferSignedEvent: {ex.Message}");
                }
            }

            // Send notification email when status moves to Sent
            if (request.Status == OfferStatus.Sent)
            {
                var templateVars = new Dictionary<string, string>
                {
                    { "CandidateName", $"{offer.Application.Candidate.FirstName} {offer.Application.Candidate.LastName}" },
                    { "JobTitle", offer.Application.Job.Title },
                    { "Salary", offer.Salary.ToString("C") },
                    { "StartDate", offer.StartDate.ToString("d") }
                };

                try
                {
                    await _emailService.SendEmailTemplateAsync(
                        offer.Application.Candidate.Email,
                        "OfferSent", // We will resolve this template trigger
                        templateVars);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Email Error] Failed to send offer letter email: {ex.Message}");
                }
            }

            return Result.Success();
        }
    }

    public record UpdateOfferLetterCommand : IRequest<Result>
    {
        public Guid OfferId { get; init; }
        public Guid CompanyId { get; init; }
        public string OfferLetterContent { get; init; }
        public string Actor { get; init; }
    }

    public class UpdateOfferLetterCommandHandler : IRequestHandler<UpdateOfferLetterCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public UpdateOfferLetterCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateOfferLetterCommand request, CancellationToken cancellationToken)
        {
            var offer = await _context.Offers
                .Include(o => o.Application).ThenInclude(a => a.Job)
                .FirstOrDefaultAsync(o => o.Id == request.OfferId, cancellationToken);

            if (offer == null || offer.Application?.Job == null || offer.Application.Job.CompanyId != request.CompanyId)
            {
                return Result.Failure("Offer not found.");
            }

            offer.OfferLetterContent = request.OfferLetterContent;

            // Log activity
            await _context.ActivityLogs.AddAsync(new ActivityLog
            {
                ApplicationId = offer.ApplicationId,
                CandidateId = offer.Application.CandidateId,
                Action = "Offer Letter Edited",
                Details = "Offer letter content has been updated.",
                PerformedBy = request.Actor ?? "System"
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
