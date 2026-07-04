using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Enums;
using ATS.Shared.Models;

namespace ATS.Application.Features.Interviews
{
    public record GetInterviewByIdQuery(Guid Id, Guid CompanyId) : IRequest<Result<InterviewDto>>;

    public class GetInterviewByIdQueryHandler : IRequestHandler<GetInterviewByIdQuery, Result<InterviewDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetInterviewByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<InterviewDto>> Handle(GetInterviewByIdQuery request, CancellationToken cancellationToken)
        {
            var interview = await _context.Interviews
                .AsNoTracking()
                .Include(i => i.Application).ThenInclude(a => a.Candidate)
                .Include(i => i.Application).ThenInclude(a => a.Job)
                .Include(i => i.Interviewer)
                .Include(i => i.Feedbacks).ThenInclude(f => f.Interviewer)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (interview == null || interview.Application?.Job == null)
            {
                return Result<InterviewDto>.Failure("Interview not found.");
            }

            var dto = new InterviewDto
            {
                Id = interview.Id,
                ApplicationId = interview.ApplicationId,
                CandidateName = interview.Application != null ? $"{interview.Application.Candidate.FirstName} {interview.Application.Candidate.LastName}" : null,
                JobTitle = interview.Application?.Job?.Title,
                InterviewerId = interview.InterviewerId,
                InterviewerName = interview.Interviewer != null ? $"{interview.Interviewer.FirstName} {interview.Interviewer.LastName}" : null,
                Title = interview.Title,
                Type = interview.Type,
                ScheduledTime = interview.ScheduledTime,
                DurationMinutes = interview.DurationMinutes,
                VideoLink = interview.VideoLink,
                Status = interview.Status,
                Feedbacks = interview.Feedbacks.Select(f => new FeedbackDto
                {
                    Id = f.Id,
                    InterviewerId = f.InterviewerId,
                    InterviewerName = f.Interviewer != null ? $"{f.Interviewer.FirstName} {f.Interviewer.LastName}" : null,
                    CommunicationScore = f.CommunicationScore,
                    ProblemSolvingScore = f.ProblemSolvingScore,
                    CodingScore = f.CodingScore,
                    SystemDesignScore = f.SystemDesignScore,
                    CultureFitScore = f.CultureFitScore,
                    FeedbackText = f.FeedbackText,
                    Recommendation = f.Recommendation,
                    SubmittedDate = f.SubmittedDate
                }).ToList()
            };

            return Result<InterviewDto>.Success(dto);
        }
    }

    public record GetInterviewsQuery : IRequest<Result<List<InterviewDto>>>
    {
        public Guid CompanyId { get; init; }
        public Guid? ApplicationId { get; init; }
        public Guid? InterviewerId { get; init; }
        public InterviewStatus? Status { get; init; }
    }

    public class GetInterviewsQueryHandler : IRequestHandler<GetInterviewsQuery, Result<List<InterviewDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetInterviewsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<InterviewDto>>> Handle(GetInterviewsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Interviews
                .AsNoTracking()
                .Where(i => i.Application.Job != null);

            if (request.ApplicationId.HasValue)
            {
                query = query.Where(i => i.ApplicationId == request.ApplicationId.Value);
            }

            if (request.InterviewerId.HasValue)
            {
                query = query.Where(i => i.InterviewerId == request.InterviewerId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(i => i.Status == request.Status.Value);
            }

            var items = await query
                .Include(i => i.Application).ThenInclude(a => a.Candidate)
                .Include(i => i.Application).ThenInclude(a => a.Job)
                .Include(i => i.Interviewer)
                .OrderBy(i => i.ScheduledTime)
                .Select(i => new InterviewDto
                {
                    Id = i.Id,
                    ApplicationId = i.ApplicationId,
                    CandidateName = $"{i.Application.Candidate.FirstName} {i.Application.Candidate.LastName}",
                    JobTitle = i.Application.Job.Title,
                    InterviewerId = i.InterviewerId,
                    InterviewerName = $"{i.Interviewer.FirstName} {i.Interviewer.LastName}",
                    Title = i.Title,
                    Type = i.Type,
                    ScheduledTime = i.ScheduledTime,
                    DurationMinutes = i.DurationMinutes,
                    VideoLink = i.VideoLink,
                    Status = i.Status
                })
                .ToListAsync(cancellationToken);

            return Result<List<InterviewDto>>.Success(items);
        }
    }
}
