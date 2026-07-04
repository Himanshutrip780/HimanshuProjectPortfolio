using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Shared.Models;

namespace ATS.Application.Features.CandidateNotes
{
    public record GetCandidateNotesQuery : IRequest<Result<List<CandidateNoteDto>>>
    {
        public Guid CandidateId { get; init; }
        public Guid? ApplicationId { get; init; }
        public Guid CompanyId { get; init; }
    }

    public class GetCandidateNotesQueryHandler : IRequestHandler<GetCandidateNotesQuery, Result<List<CandidateNoteDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetCandidateNotesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CandidateNoteDto>>> Handle(GetCandidateNotesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CandidateNotes
                .Where(n => n.CandidateId == request.CandidateId && n.Candidate.CompanyId == request.CompanyId);

            if (request.ApplicationId.HasValue)
            {
                // Retrieve notes that are either general (ApplicationId is null) OR match the specific ApplicationId
                query = query.Where(n => n.ApplicationId == null || n.ApplicationId == request.ApplicationId.Value);
            }

            var notes = await query
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new CandidateNoteDto
                {
                    Id = n.Id,
                    CandidateId = n.CandidateId,
                    ApplicationId = n.ApplicationId,
                    Text = n.Text,
                    AuthorName = n.AuthorName,
                    CreatedDate = n.CreatedDate
                })
                .ToListAsync(cancellationToken);

            return Result<List<CandidateNoteDto>>.Success(notes);
        }
    }
}
