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

namespace ATS.Application.Features.Offers
{
    public record GetOfferByIdQuery(Guid Id, Guid CompanyId) : IRequest<Result<OfferDto>>;

    public class GetOfferByIdQueryHandler : IRequestHandler<GetOfferByIdQuery, Result<OfferDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetOfferByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<OfferDto>> Handle(GetOfferByIdQuery request, CancellationToken cancellationToken)
        {
            var offer = await _context.Offers
                .AsNoTracking()
                .Include(o => o.Application).ThenInclude(a => a.Candidate)
                .Include(o => o.Application).ThenInclude(a => a.Job)
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (offer == null || offer.Application?.Job == null)
            {
                return Result<OfferDto>.Failure("Offer not found.");
            }

            var dto = new OfferDto
            {
                Id = offer.Id,
                ApplicationId = offer.ApplicationId,
                CandidateName = $"{offer.Application.Candidate.FirstName} {offer.Application.Candidate.LastName}",
                JobTitle = offer.Application.Job.Title,
                Salary = offer.Salary,
                StartDate = offer.StartDate,
                Status = offer.Status,
                OfferLetterPath = offer.OfferLetterPath,
                OfferLetterContent = offer.OfferLetterContent,
                ESignatureDetails = offer.ESignatureDetails,
                CreatedDate = offer.CreatedDate
            };

            return Result<OfferDto>.Success(dto);
        }
    }

    public record GetOffersQuery : IRequest<Result<List<OfferDto>>>
    {
        public Guid CompanyId { get; init; }
        public Guid? ApplicationId { get; init; }
        public OfferStatus? Status { get; init; }
    }

    public class GetOffersQueryHandler : IRequestHandler<GetOffersQuery, Result<List<OfferDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetOffersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<OfferDto>>> Handle(GetOffersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Offers
                .AsNoTracking()
                .Where(o => o.Application.Job != null);

            if (request.ApplicationId.HasValue)
            {
                query = query.Where(o => o.ApplicationId == request.ApplicationId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(o => o.Status == request.Status.Value);
            }

            var items = await query
                .Include(o => o.Application).ThenInclude(a => a.Candidate)
                .Include(o => o.Application).ThenInclude(a => a.Job)
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new OfferDto
                {
                    Id = o.Id,
                    ApplicationId = o.ApplicationId,
                    CandidateName = $"{o.Application.Candidate.FirstName} {o.Application.Candidate.LastName}",
                    JobTitle = o.Application.Job.Title,
                    Salary = o.Salary,
                    StartDate = o.StartDate,
                    Status = o.Status,
                    OfferLetterPath = o.OfferLetterPath,
                    OfferLetterContent = o.OfferLetterContent,
                    ESignatureDetails = o.ESignatureDetails,
                    CreatedDate = o.CreatedDate
                })
                .ToListAsync(cancellationToken);

            return Result<List<OfferDto>>.Success(items);
        }
    }
}
