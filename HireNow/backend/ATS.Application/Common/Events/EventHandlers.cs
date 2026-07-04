using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Entities;

namespace ATS.Application.Common.Events
{
    public class CandidateMovedEventHandler : INotificationHandler<CandidateMovedEvent>
    {
        private readonly IApplicationDbContext _context;

        public CandidateMovedEventHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(CandidateMovedEvent notification, CancellationToken cancellationToken)
        {
            var app = await _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == notification.ApplicationId, cancellationToken);

            if (app == null) return;

            string msg = $"{app.Candidate.FirstName} {app.Candidate.LastName} was moved from '{notification.OldStage}' to '{notification.NewStage}' for '{app.Job.Title}' by {notification.Actor}.";

            if (app.Job.RecruiterId.HasValue)
            {
                await _context.Notifications.AddAsync(new Notification
                {
                    UserId = app.Job.RecruiterId.Value,
                    Title = "Candidate Moved Stage",
                    Message = msg,
                    IsRead = false
                }, cancellationToken);
            }

            if (app.Job.HiringManagerId.HasValue && app.Job.HiringManagerId != app.Job.RecruiterId)
            {
                await _context.Notifications.AddAsync(new Notification
                {
                    UserId = app.Job.HiringManagerId.Value,
                    Title = "Candidate Moved Stage",
                    Message = msg,
                    IsRead = false
                }, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public class InterviewScheduledEventHandler : INotificationHandler<InterviewScheduledEvent>
    {
        private readonly IApplicationDbContext _context;

        public InterviewScheduledEventHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(InterviewScheduledEvent notification, CancellationToken cancellationToken)
        {
            var interview = await _context.Interviews
                .Include(i => i.Application).ThenInclude(a => a.Candidate)
                .Include(i => i.Application).ThenInclude(a => a.Job)
                .FirstOrDefaultAsync(i => i.Id == notification.InterviewId, cancellationToken);

            if (interview == null) return;

            string msg = $"You have been scheduled to interview {interview.Application.Candidate.FirstName} {interview.Application.Candidate.LastName} for '{interview.Application.Job.Title}' on {interview.ScheduledTime:f}.";

            await _context.Notifications.AddAsync(new Notification
            {
                UserId = interview.InterviewerId,
                Title = "New Interview Scheduled",
                Message = msg,
                IsRead = false
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public class OfferSignedEventHandler : INotificationHandler<OfferSignedEvent>
    {
        private readonly IApplicationDbContext _context;

        public OfferSignedEventHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(OfferSignedEvent notification, CancellationToken cancellationToken)
        {
            var offer = await _context.Offers
                .Include(o => o.Application).ThenInclude(a => a.Candidate)
                .Include(o => o.Application).ThenInclude(a => a.Job)
                .FirstOrDefaultAsync(o => o.Id == notification.OfferId, cancellationToken);

            if (offer == null) return;

            string msg = $"{offer.Application.Candidate.FirstName} {offer.Application.Candidate.LastName} accepted and signed the offer for '{offer.Application.Job.Title}' with salary {offer.Salary:C}.";

            if (offer.Application.Job.RecruiterId.HasValue)
            {
                await _context.Notifications.AddAsync(new Notification
                {
                    UserId = offer.Application.Job.RecruiterId.Value,
                    Title = "Offer Accepted & Signed",
                    Message = msg,
                    IsRead = false
                }, cancellationToken);
            }

            if (offer.Application.Job.HiringManagerId.HasValue && offer.Application.Job.HiringManagerId != offer.Application.Job.RecruiterId)
            {
                await _context.Notifications.AddAsync(new Notification
                {
                    UserId = offer.Application.Job.HiringManagerId.Value,
                    Title = "Offer Accepted & Signed",
                    Message = msg,
                    IsRead = false
                }, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
