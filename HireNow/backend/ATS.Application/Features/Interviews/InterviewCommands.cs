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
using ATS.Shared.Models;

namespace ATS.Application.Features.Interviews
{
    public record ScheduleInterviewCommand : IRequest<Result<Guid>>
    {
        public Guid ApplicationId { get; init; }
        public Guid InterviewerId { get; init; }
        public string Title { get; init; }
        public InterviewType Type { get; init; }
        public DateTime ScheduledTime { get; init; }
        public int DurationMinutes { get; init; }
        public string VideoLink { get; init; }
        public string Actor { get; init; }
    }

    public class ScheduleInterviewCommandHandler : IRequestHandler<ScheduleInterviewCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IMediator _mediator;

        public ScheduleInterviewCommandHandler(IApplicationDbContext context, IEmailService emailService, IMediator mediator)
        {
            _context = context;
            _emailService = emailService;
            _mediator = mediator;
        }

        public async Task<Result<Guid>> Handle(ScheduleInterviewCommand request, CancellationToken cancellationToken)
        {
            var app = await _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (app == null || app.Job == null)
            {
                return Result<Guid>.Failure("Application not found.");
            }

            var interview = new Interview
            {
                ApplicationId = request.ApplicationId,
                InterviewerId = request.InterviewerId,
                Title = request.Title,
                Type = request.Type,
                ScheduledTime = request.ScheduledTime,
                DurationMinutes = request.DurationMinutes,
                VideoLink = request.VideoLink,
                Status = InterviewStatus.Scheduled
            };

            await _context.Interviews.AddAsync(interview, cancellationToken);

            // Add activity log
            await _context.ActivityLogs.AddAsync(new ActivityLog
            {
                ApplicationId = app.Id,
                CandidateId = app.CandidateId,
                Action = "Schedule Interview",
                Details = $"{request.Type} Interview scheduled for {request.ScheduledTime:g}.",
                PerformedBy = request.Actor ?? "System"
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            try
            {
                await _mediator.Publish(new ATS.Application.Common.Events.InterviewScheduledEvent(interview.Id, request.Actor), cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Domain Event Error] Failed to publish InterviewScheduledEvent: {ex.Message}");
            }

            // Send Email Invite
            var templateVars = new Dictionary<string, string>
            {
                { "CandidateName", $"{app.Candidate.FirstName} {app.Candidate.LastName}" },
                { "JobTitle", app.Job.Title },
                { "InterviewType", request.Type.ToString() },
                { "ScheduledTime", request.ScheduledTime.ToString("F") },
                { "VideoLink", request.VideoLink }
            };

            try
            {
                await _emailService.SendEmailTemplateAsync(app.Candidate.Email, "InterviewScheduled", templateVars);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Error] Failed to send interview invitation email: {ex.Message}");
            }

            return Result<Guid>.Success(interview.Id);
        }
    }

    public record SubmitFeedbackCommand : IRequest<Result>
    {
        public Guid InterviewId { get; init; }
        public Guid InterviewerId { get; init; }
        public int CommunicationScore { get; init; }
        public int ProblemSolvingScore { get; init; }
        public int CodingScore { get; init; }
        public int SystemDesignScore { get; init; }
        public int CultureFitScore { get; init; }
        public string FeedbackText { get; init; }
        public RecommendationType Recommendation { get; init; }
        public string Actor { get; init; }
    }

    public class SubmitFeedbackCommandHandler : IRequestHandler<SubmitFeedbackCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public SubmitFeedbackCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(SubmitFeedbackCommand request, CancellationToken cancellationToken)
        {
            var interview = await _context.Interviews
                .Include(i => i.Application).ThenInclude(a => a.Job)
                .Include(i => i.Application).ThenInclude(a => a.Candidate)
                .FirstOrDefaultAsync(i => i.Id == request.InterviewId, cancellationToken);

            if (interview == null || interview.Application?.Job == null)
            {
                return Result.Failure("Interview not found.");
            }

            var feedback = new InterviewFeedback
            {
                InterviewId = request.InterviewId,
                InterviewerId = request.InterviewerId,
                CommunicationScore = request.CommunicationScore,
                ProblemSolvingScore = request.ProblemSolvingScore,
                CodingScore = request.CodingScore,
                SystemDesignScore = request.SystemDesignScore,
                CultureFitScore = request.CultureFitScore,
                FeedbackText = request.FeedbackText,
                Recommendation = request.Recommendation,
                SubmittedDate = DateTime.UtcNow
            };

            await _context.InterviewFeedbacks.AddAsync(feedback, cancellationToken);

            // Update interview status
            interview.Status = InterviewStatus.Completed;

            // Log activity
            await _context.ActivityLogs.AddAsync(new ActivityLog
            {
                ApplicationId = interview.ApplicationId,
                CandidateId = interview.Application.CandidateId,
                Action = "Submit Feedback",
                Details = $"Feedback submitted by interviewer. Recommendation: {request.Recommendation}.",
                PerformedBy = request.Actor ?? "Interviewer"
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public record CancelInterviewCommand(Guid Id, string Actor) : IRequest<Result>;

    public class CancelInterviewCommandHandler : IRequestHandler<CancelInterviewCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public CancelInterviewCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(CancelInterviewCommand request, CancellationToken cancellationToken)
        {
            var interview = await _context.Interviews
                .Include(i => i.Application).ThenInclude(a => a.Job)
                .Include(i => i.Application).ThenInclude(a => a.Candidate)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (interview == null || interview.Application?.Job == null)
            {
                return Result.Failure("Interview not found.");
            }

            interview.Status = InterviewStatus.Cancelled;

            // Log activity
            await _context.ActivityLogs.AddAsync(new ActivityLog
            {
                ApplicationId = interview.ApplicationId,
                CandidateId = interview.Application.CandidateId,
                Action = "Cancel Interview",
                Details = "Interview was cancelled.",
                PerformedBy = request.Actor ?? "System"
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public record RescheduleInterviewCommand(Guid Id, DateTime ScheduledTime, int DurationMinutes, string Actor) : IRequest<Result>;

    public class RescheduleInterviewCommandHandler : IRequestHandler<RescheduleInterviewCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public RescheduleInterviewCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(RescheduleInterviewCommand request, CancellationToken cancellationToken)
        {
            var interview = await _context.Interviews
                .Include(i => i.Application).ThenInclude(a => a.Job)
                .Include(i => i.Application).ThenInclude(a => a.Candidate)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (interview == null || interview.Application?.Job == null)
            {
                return Result.Failure("Interview not found.");
            }

            interview.Status = InterviewStatus.Rescheduled;
            interview.ScheduledTime = request.ScheduledTime;
            interview.DurationMinutes = request.DurationMinutes;

            // Log activity
            await _context.ActivityLogs.AddAsync(new ActivityLog
            {
                ApplicationId = interview.ApplicationId,
                CandidateId = interview.Application.CandidateId,
                Action = "Reschedule Interview",
                Details = $"Interview rescheduled for {request.ScheduledTime:g}.",
                PerformedBy = request.Actor ?? "System"
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
