using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Entities;
using ATS.Shared.Constants;
using ATS.Shared.Models;

namespace ATS.Application.Features.Applications
{
    public record MoveApplicationStageCommand : IRequest<Result>
    {
        public Guid ApplicationId { get; init; }
        public string NewStage { get; init; }
        public string Actor { get; init; }
    }

    public class MoveApplicationStageCommandHandler : IRequestHandler<MoveApplicationStageCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IMediator _mediator;

        public MoveApplicationStageCommandHandler(IApplicationDbContext context, IEmailService emailService, IMediator mediator)
        {
            _context = context;
            _emailService = emailService;
            _mediator = mediator;
        }

        public async Task<Result> Handle(MoveApplicationStageCommand request, CancellationToken cancellationToken)
        {
            var app = await _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .Include(a => a.StagesHistory)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (app == null || app.Job == null || app.Candidate == null)
            {
                return Result.Failure("Application not found.");
            }

            var previousStage = app.CurrentStage;
            if (previousStage == request.NewStage)
            {
                return Result.Success(); // No changes needed
            }

            // Update application stage details
            app.CurrentStage = request.NewStage;

            // Handle terminal states
            if (request.NewStage == Stages.Rejected)
            {
                app.Status = "Rejected";
            }
            else if (request.NewStage == Stages.Hired)
            {
                app.Status = "Hired";
            }
            else
            {
                app.Status = "Active";
            }

            // Update previous stage in history
            var currentStageHistory = app.StagesHistory.FirstOrDefault(s => s.Status == "Current");
            if (currentStageHistory != null)
            {
                currentStageHistory.Status = "Completed";
                currentStageHistory.LeftDate = DateTime.UtcNow;
            }

            // Add new stage history
            var maxSeq = app.StagesHistory.Any() ? app.StagesHistory.Max(s => s.SequenceNumber) : 0;
            var newStage = new ApplicationStage
            {
                ApplicationId = app.Id,
                StageName = request.NewStage,
                Status = "Current",
                EnteredDate = DateTime.UtcNow,
                SequenceNumber = maxSeq + 1
            };
            app.StagesHistory.Add(newStage);
            await _context.ApplicationStages.AddAsync(newStage, cancellationToken);

            // Log activity
            await _context.ActivityLogs.AddAsync(new ActivityLog
            {
                ApplicationId = app.Id,
                CandidateId = app.CandidateId,
                Action = "Stage Change",
                Details = $"Moved from '{previousStage}' to '{request.NewStage}'. Status: {app.Status}.",
                PerformedBy = request.Actor ?? "System"
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            try
            {
                await _mediator.Publish(new ATS.Application.Common.Events.CandidateMovedEvent(app.Id, previousStage, request.NewStage, request.Actor), cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Domain Event Error] Failed to publish CandidateMovedEvent: {ex.Message}");
            }

            // Automatic emails based on transitions
            var templateVars = new Dictionary<string, string>
            {
                { "CandidateName", $"{app.Candidate.FirstName} {app.Candidate.LastName}" },
                { "JobTitle", app.Job.Title }
            };

            if (request.NewStage == Stages.Rejected)
            {
                try
                {
                    await _emailService.SendEmailTemplateAsync(app.Candidate.Email, "Rejection", templateVars);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Email Error] Failed to send rejection email: {ex.Message}");
                }
            }

            return Result.Success();
        }
    }

    public record SubmitApplicationCommand : IRequest<Result<Guid>>
    {
        public Guid JobId { get; init; }
        public Guid CandidateId { get; init; }
    }

    public class SubmitApplicationCommandHandler : IRequestHandler<SubmitApplicationCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public SubmitApplicationCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(SubmitApplicationCommand request, CancellationToken cancellationToken)
        {
            var jobExists = await _context.Jobs.AnyAsync(j => j.Id == request.JobId, cancellationToken);
            if (!jobExists)
            {
                return Result<Guid>.Failure("Job not found.");
            }

            var candidateExists = await _context.Candidates.AnyAsync(c => c.Id == request.CandidateId, cancellationToken);
            if (!candidateExists)
            {
                return Result<Guid>.Failure("Candidate not found.");
            }

            var existing = await _context.Applications
                .FirstOrDefaultAsync(a => a.JobId == request.JobId && a.CandidateId == request.CandidateId, cancellationToken);

            if (existing != null)
            {
                return Result<Guid>.Failure("Candidate has already applied for this job.");
            }

            var app = new ATS.Domain.Entities.Application
            {
                JobId = request.JobId,
                CandidateId = request.CandidateId,
                CurrentStage = Stages.Applied,
                Status = "Active"
            };

            app.StagesHistory.Add(new ApplicationStage
            {
                StageName = Stages.Applied,
                Status = "Current",
                EnteredDate = DateTime.UtcNow,
                SequenceNumber = 0
            });

            app.ActivityLogs.Add(new ActivityLog
            {
                CandidateId = request.CandidateId,
                Action = "Apply",
                Details = "Applied online.",
                PerformedBy = "Recruiter (Manual)"
            });

            await _context.Applications.AddAsync(app, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(app.Id);
        }
    }
}
