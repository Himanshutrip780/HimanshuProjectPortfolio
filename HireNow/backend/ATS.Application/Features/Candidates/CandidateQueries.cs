using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Application.DTOs.AI;
using ATS.Shared.Models;

namespace ATS.Application.Features.Candidates
{
    public record GetCandidateByIdQuery(Guid Id, Guid CompanyId) : IRequest<Result<CandidateDto>>;

    public class GetCandidateByIdQueryHandler : IRequestHandler<GetCandidateByIdQuery, Result<CandidateDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCandidateByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CandidateDto>> Handle(GetCandidateByIdQuery request, CancellationToken cancellationToken)
        {
            var candidate = await _context.Candidates
                .AsNoTracking()
                .Include(c => c.Skills)
                .Include(c => c.ParsingResult)
                .Include(c => c.Applications).ThenInclude(a => a.Job)
                .Include(c => c.Applications).ThenInclude(a => a.AIScores)
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.CompanyId == request.CompanyId, cancellationToken);

            if (candidate == null)
            {
                return Result<CandidateDto>.Failure("Candidate not found.");
            }

            var latestApp = candidate.Applications.OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            var latestScore = latestApp?.AIScores.OrderByDescending(s => s.CreatedDate).FirstOrDefault();

            var currentTitle = "Software Professional";
            var yearsOfExperience = "3+ yrs";
            var education = "No Education details extracted";
            if (candidate.ParsingResult != null && !string.IsNullOrEmpty(candidate.ParsingResult.ParsedDataJson))
            {
                try
                {
                    var parsed = System.Text.Json.JsonSerializer.Deserialize<ResumeParsingResultDto>(candidate.ParsingResult.ParsedDataJson);
                    if (parsed != null)
                    {
                        currentTitle = parsed.CurrentTitle ?? currentTitle;
                        yearsOfExperience = parsed.YearsOfExperience ?? yearsOfExperience;
                        education = parsed.Education ?? education;
                    }
                }
                catch {}
            }

            var dto = new CandidateDto
            {
                Id = candidate.Id,
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Email = candidate.Email,
                Phone = candidate.Phone,
                LinkedInUrl = candidate.LinkedInUrl,
                GitHubUrl = candidate.GitHubUrl,
                PortfolioUrl = candidate.PortfolioUrl,
                ResumePath = candidate.ResumePath,
                Skills = candidate.Skills.Select(s => s.Name).ToList(),
                CreatedDate = candidate.CreatedDate,
                ParsedDataJson = candidate.ParsingResult?.ParsedDataJson,
                ParsingConfidenceScore = candidate.ParsingResult?.ConfidenceScore,
                CurrentTitle = currentTitle,
                YearsOfExperience = candidate.YearsOfExperience.HasValue ? $"{candidate.YearsOfExperience} yrs" : yearsOfExperience,
                Education = education,
                ExpectedSalary = candidate.ExpectedSalary,
                LatestApplicationJobTitle = latestApp?.Job?.Title ?? "None",
                LatestApplicationStage = latestApp?.CurrentStage ?? "Talent Pool",
                LatestApplicationMatchScore = latestScore?.MatchScore ?? 0
            };

            return Result<CandidateDto>.Success(dto);
        }
    }

    public record GetCandidatesQuery : IRequest<Result<PaginatedList<CandidateDto>>>
    {
        public Guid CompanyId { get; init; }
        public string SearchTerm { get; init; }
        public int PageIndex { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetCandidatesQueryHandler : IRequestHandler<GetCandidatesQuery, Result<PaginatedList<CandidateDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICandidateSearchService _searchService;

        public GetCandidatesQueryHandler(IApplicationDbContext context, ICandidateSearchService searchService)
        {
            _context = context;
            _searchService = searchService;
        }

        public async Task<Result<PaginatedList<CandidateDto>>> Handle(GetCandidatesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Candidates
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var matchingIds = await _searchService.SearchCandidatesAsync(request.CompanyId, request.SearchTerm, cancellationToken);
                query = query.Where(c => matchingIds.Contains(c.Id));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var candidates = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(c => c.Skills)
                .Include(c => c.ParsingResult)
                .Include(c => c.Applications).ThenInclude(a => a.Job)
                .Include(c => c.Applications).ThenInclude(a => a.AIScores)
                .ToListAsync(cancellationToken);

            var items = candidates.Select(c => {
                var latestApp = c.Applications.OrderByDescending(a => a.CreatedDate).FirstOrDefault();
                var latestScore = latestApp?.AIScores.OrderByDescending(s => s.CreatedDate).FirstOrDefault();

                var currentTitle = "Software Professional";
                var yearsOfExperience = "3+ yrs";
                var education = "No Education details extracted";
                if (c.ParsingResult != null && !string.IsNullOrEmpty(c.ParsingResult.ParsedDataJson))
                {
                    try
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<ResumeParsingResultDto>(c.ParsingResult.ParsedDataJson);
                        if (parsed != null)
                        {
                            currentTitle = parsed.CurrentTitle ?? currentTitle;
                            yearsOfExperience = parsed.YearsOfExperience ?? yearsOfExperience;
                            education = parsed.Education ?? education;
                        }
                    }
                    catch {}
                }

                return new CandidateDto
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    LinkedInUrl = c.LinkedInUrl,
                    GitHubUrl = c.GitHubUrl,
                    PortfolioUrl = c.PortfolioUrl,
                    ResumePath = c.ResumePath,
                    Skills = c.Skills.Select(s => s.Name).ToList(),
                    CreatedDate = c.CreatedDate,
                    ParsedDataJson = c.ParsingResult?.ParsedDataJson,
                    ParsingConfidenceScore = c.ParsingResult != null ? (decimal?)c.ParsingResult.ConfidenceScore : null,
                    CurrentTitle = currentTitle,
                    YearsOfExperience = c.YearsOfExperience.HasValue ? $"{c.YearsOfExperience} yrs" : yearsOfExperience,
                    Education = education,
                    ExpectedSalary = c.ExpectedSalary,
                    LatestApplicationJobTitle = latestApp?.Job?.Title ?? "None",
                    LatestApplicationStage = latestApp?.CurrentStage ?? "Talent Pool",
                    LatestApplicationMatchScore = latestScore?.MatchScore ?? 0
                };
            }).ToList();

            var paginated = new PaginatedList<CandidateDto>(items, totalCount, request.PageIndex, request.PageSize);
            return Result<PaginatedList<CandidateDto>>.Success(paginated);
        }
    }

    public record GetCandidateDuplicatesQuery(Guid CandidateId, Guid CompanyId) : IRequest<Result<List<CandidateDto>>>;

    public class GetCandidateDuplicatesQueryHandler : IRequestHandler<GetCandidateDuplicatesQuery, Result<List<CandidateDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetCandidateDuplicatesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CandidateDto>>> Handle(GetCandidateDuplicatesQuery request, CancellationToken cancellationToken)
        {
            var candidate = await _context.Candidates
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CandidateId && c.CompanyId == request.CompanyId, cancellationToken);

            if (candidate == null)
            {
                return Result<List<CandidateDto>>.Failure("Candidate not found.");
            }

            var query = _context.Candidates
                .AsNoTracking()
                .Where(c => c.Id != request.CandidateId && c.CompanyId == request.CompanyId);

            var matchEmail = !string.IsNullOrWhiteSpace(candidate.Email);
            var matchPhone = !string.IsNullOrWhiteSpace(candidate.Phone);

            if (!matchEmail && !matchPhone)
            {
                return Result<List<CandidateDto>>.Success(new List<CandidateDto>());
            }

            query = query.Where(c => 
                (matchEmail && c.Email == candidate.Email) || 
                (matchPhone && c.Phone == candidate.Phone)
            );

            var duplicates = await query
                .Include(c => c.Skills)
                .Include(c => c.ParsingResult)
                .Include(c => c.Applications).ThenInclude(a => a.Job)
                .Include(c => c.Applications).ThenInclude(a => a.AIScores)
                .ToListAsync(cancellationToken);

            var items = duplicates.Select(c => {
                var latestApp = c.Applications.OrderByDescending(a => a.CreatedDate).FirstOrDefault();
                var latestScore = latestApp?.AIScores.OrderByDescending(s => s.CreatedDate).FirstOrDefault();

                var currentTitle = "Software Professional";
                var yearsOfExperience = "3+ yrs";
                var education = "No Education details extracted";
                if (c.ParsingResult != null && !string.IsNullOrEmpty(c.ParsingResult.ParsedDataJson))
                {
                    try
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<ResumeParsingResultDto>(c.ParsingResult.ParsedDataJson);
                        if (parsed != null)
                        {
                            currentTitle = parsed.CurrentTitle ?? currentTitle;
                            yearsOfExperience = parsed.YearsOfExperience ?? yearsOfExperience;
                            education = parsed.Education ?? education;
                        }
                    }
                    catch {}
                }

                return new CandidateDto
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    LinkedInUrl = c.LinkedInUrl,
                    GitHubUrl = c.GitHubUrl,
                    PortfolioUrl = c.PortfolioUrl,
                    ResumePath = c.ResumePath,
                    Skills = c.Skills.Select(s => s.Name).ToList(),
                    CreatedDate = c.CreatedDate,
                    ParsedDataJson = c.ParsingResult?.ParsedDataJson,
                    ParsingConfidenceScore = c.ParsingResult != null ? (decimal?)c.ParsingResult.ConfidenceScore : null,
                    CurrentTitle = currentTitle,
                    YearsOfExperience = c.YearsOfExperience.HasValue ? $"{c.YearsOfExperience} yrs" : yearsOfExperience,
                    Education = education,
                    ExpectedSalary = c.ExpectedSalary,
                    LatestApplicationJobTitle = latestApp?.Job?.Title ?? "None",
                    LatestApplicationStage = latestApp?.CurrentStage ?? "Talent Pool",
                    LatestApplicationMatchScore = latestScore?.MatchScore ?? 0
                };
            }).ToList();

            return Result<List<CandidateDto>>.Success(items);
        }
    }
}
