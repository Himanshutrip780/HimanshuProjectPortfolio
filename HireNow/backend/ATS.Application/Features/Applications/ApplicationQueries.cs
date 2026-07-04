using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Application.Features.Candidates;
using ATS.Application.DTOs.AI;
using ATS.Shared.Models;

namespace ATS.Application.Features.Applications
{
    public record GetApplicationByIdQuery(Guid Id, Guid CompanyId) : IRequest<Result<ApplicationDto>>;

    public class GetApplicationByIdQueryHandler : IRequestHandler<GetApplicationByIdQuery, Result<ApplicationDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetApplicationByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ApplicationDto>> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
        {
            var app = await _context.Applications
                .AsNoTracking()
                .Include(a => a.Candidate).ThenInclude(c => c.Skills)
                .Include(a => a.Candidate).ThenInclude(c => c.ParsingResult)
                .Include(a => a.Job).ThenInclude(j => j.Skills)
                .Include(a => a.StagesHistory)
                .Include(a => a.AIScores)
                .FirstOrDefaultAsync(a => a.Id == request.Id && a.Job.CompanyId == request.CompanyId, cancellationToken);

            if (app == null)
            {
                return Result<ApplicationDto>.Failure("Application not found.");
            }

            var latestScore = app.AIScores?.OrderByDescending(s => s.CreatedDate).FirstOrDefault();

            var currentTitle = "Software Professional";
            var yearsOfExperience = "3+ yrs";
            if (app.Candidate?.ParsingResult != null && !string.IsNullOrEmpty(app.Candidate.ParsingResult.ParsedDataJson))
            {
                try
                {
                    var parsed = System.Text.Json.JsonSerializer.Deserialize<ResumeParsingResultDto>(app.Candidate.ParsingResult.ParsedDataJson);
                    if (parsed != null)
                    {
                        currentTitle = parsed.CurrentTitle ?? currentTitle;
                        yearsOfExperience = parsed.YearsOfExperience ?? yearsOfExperience;
                    }
                }
                catch {}
            }

            var dto = new ApplicationDto
            {
                Id = app.Id,
                JobId = app.JobId,
                JobTitle = app.Job?.Title,
                CandidateId = app.CandidateId,
                CandidateName = app.Candidate != null ? $"{app.Candidate.FirstName} {app.Candidate.LastName}" : null,
                CandidateEmail = app.Candidate?.Email,
                CurrentStage = app.CurrentStage,
                Status = app.Status,
                CreatedDate = app.CreatedDate,
                TimeInStageDays = app.StagesHistory != null && app.StagesHistory.Any(s => s.Status == "Current")
                    ? Math.Max(1, (int)(DateTime.UtcNow - app.StagesHistory.First(s => s.Status == "Current").EnteredDate).TotalDays)
                    : 1,
                Candidate = app.Candidate != null ? new CandidateDto
                {
                    Id = app.Candidate.Id,
                    FirstName = app.Candidate.FirstName,
                    LastName = app.Candidate.LastName,
                    Email = app.Candidate.Email,
                    Phone = app.Candidate.Phone,
                    LinkedInUrl = app.Candidate.LinkedInUrl,
                    GitHubUrl = app.Candidate.GitHubUrl,
                    PortfolioUrl = app.Candidate.PortfolioUrl,
                    ResumePath = app.Candidate.ResumePath,
                    Skills = app.Candidate.Skills?.Select(s => s.Name).ToList() ?? new List<string>(),
                    CreatedDate = app.Candidate.CreatedDate,
                    ParsedDataJson = app.Candidate.ParsingResult?.ParsedDataJson,
                    ParsingConfidenceScore = app.Candidate.ParsingResult?.ConfidenceScore,
                    CurrentTitle = currentTitle,
                    YearsOfExperience = app.Candidate.YearsOfExperience.HasValue ? $"{app.Candidate.YearsOfExperience} yrs" : yearsOfExperience,
                    ExpectedSalary = app.Candidate.ExpectedSalary,
                    LatestApplicationJobTitle = app.Job?.Title ?? "None",
                    LatestApplicationStage = app.CurrentStage,
                    LatestApplicationMatchScore = latestScore?.MatchScore ?? 0
                } : null,
                AIMatchScore = latestScore?.MatchScore,
                AIRecommendation = latestScore?.Recommendation,
                AISummary = latestScore?.AISummary,
                Strengths = !string.IsNullOrEmpty(latestScore?.StrengthsJson)
                    ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(latestScore.StrengthsJson)
                    : new List<string>(),
                Weaknesses = !string.IsNullOrEmpty(latestScore?.WeaknessesJson)
                    ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(latestScore.WeaknessesJson)
                    : new List<string>(),
                MissingSkills = !string.IsNullOrEmpty(latestScore?.MissingSkillsJson)
                    ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(latestScore.MissingSkillsJson)
                    : new List<string>(),
                AIQuestions = !string.IsNullOrEmpty(latestScore?.AIQuestionsJson)
                    ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(latestScore.AIQuestionsJson)
                    : new List<string>(),
                SkillsMatch = app.Candidate?.Skills != null && app.Job?.Skills != null
                    ? app.Candidate.Skills.Select(s => s.Name).Intersect(app.Job.Skills.Select(s => s.Name), StringComparer.OrdinalIgnoreCase).ToList()
                    : new List<string>(),
                StageHistory = app.StagesHistory == null ? new List<StageHistoryDto>() : app.StagesHistory
                    .OrderBy(h => h.SequenceNumber)
                    .Select(h => new StageHistoryDto
                    {
                        StageName = h.StageName,
                        Status = h.Status,
                        EnteredDate = h.EnteredDate,
                        LeftDate = h.LeftDate,
                        SequenceNumber = h.SequenceNumber
                    }).ToList()
            };

            return Result<ApplicationDto>.Success(dto);
        }
    }

    public record GetApplicationsQuery : IRequest<Result<List<ApplicationDto>>>
    {
        public Guid CompanyId { get; init; }
        public Guid? JobId { get; init; }
        public Guid? CandidateId { get; init; }
        public string Stage { get; init; }
        public string Status { get; init; } // Active, Hired, Rejected
        public string SearchTerm { get; init; }
    }

    public class GetApplicationsQueryHandler : IRequestHandler<GetApplicationsQuery, Result<List<ApplicationDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetApplicationsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ApplicationDto>>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Applications
                .AsNoTracking()
                .Where(a => a.Job.CompanyId == request.CompanyId);

            if (request.JobId.HasValue)
            {
                query = query.Where(a => a.JobId == request.JobId.Value);
            }

            if (request.CandidateId.HasValue)
            {
                query = query.Where(a => a.CandidateId == request.CandidateId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Stage))
            {
                query = query.Where(a => a.CurrentStage == request.Stage);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(a => a.Status == request.Status);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(a => 
                    a.Candidate.FirstName.ToLower().Contains(search) || 
                    a.Candidate.LastName.ToLower().Contains(search) ||
                    a.Job.Title.ToLower().Contains(search));
            }

            var apps = await query
                .Include(a => a.Candidate).ThenInclude(c => c.Skills)
                .Include(a => a.Candidate).ThenInclude(c => c.ParsingResult)
                .Include(a => a.Job).ThenInclude(j => j.Skills)
                .Include(a => a.StagesHistory)
                .Include(a => a.AIScores)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync(cancellationToken);

             var items = apps.Select(app => {
                var latestScore = app.AIScores?.OrderByDescending(s => s.CreatedDate).FirstOrDefault();

                var currentTitle = "Software Professional";
                var yearsOfExperience = "3+ yrs";
                if (app.Candidate?.ParsingResult != null && !string.IsNullOrEmpty(app.Candidate.ParsingResult.ParsedDataJson))
                {
                    try
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<ResumeParsingResultDto>(app.Candidate.ParsingResult.ParsedDataJson);
                        if (parsed != null)
                        {
                            currentTitle = parsed.CurrentTitle ?? currentTitle;
                            yearsOfExperience = parsed.YearsOfExperience ?? yearsOfExperience;
                        }
                    }
                    catch {}
                }

                return new ApplicationDto
                {
                    Id = app.Id,
                    JobId = app.JobId,
                    JobTitle = app.Job?.Title,
                    CandidateId = app.CandidateId,
                    CandidateName = app.Candidate != null ? $"{app.Candidate.FirstName} {app.Candidate.LastName}" : null,
                    CandidateEmail = app.Candidate?.Email,
                    CurrentStage = app.CurrentStage,
                    Status = app.Status,
                    CreatedDate = app.CreatedDate,
                    TimeInStageDays = app.StagesHistory != null && app.StagesHistory.Any(s => s.Status == "Current")
                        ? Math.Max(1, (int)(DateTime.UtcNow - app.StagesHistory.First(s => s.Status == "Current").EnteredDate).TotalDays)
                        : 1,
                    AIMatchScore = latestScore?.MatchScore,
                    AIRecommendation = latestScore?.Recommendation,
                    AISummary = latestScore?.AISummary,
                    Strengths = !string.IsNullOrEmpty(latestScore?.StrengthsJson)
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(latestScore.StrengthsJson)
                        : new List<string>(),
                    Weaknesses = !string.IsNullOrEmpty(latestScore?.WeaknessesJson)
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(latestScore.WeaknessesJson)
                        : new List<string>(),
                    MissingSkills = !string.IsNullOrEmpty(latestScore?.MissingSkillsJson)
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(latestScore.MissingSkillsJson)
                        : new List<string>(),
                    AIQuestions = !string.IsNullOrEmpty(latestScore?.AIQuestionsJson)
                        ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(latestScore.AIQuestionsJson)
                        : new List<string>(),
                    SkillsMatch = app.Candidate?.Skills != null && app.Job?.Skills != null
                        ? app.Candidate.Skills.Select(s => s.Name).Intersect(app.Job.Skills.Select(s => s.Name), StringComparer.OrdinalIgnoreCase).ToList()
                        : new List<string>(),
                    Candidate = app.Candidate != null ? new CandidateDto
                    {
                        Id = app.Candidate.Id,
                        FirstName = app.Candidate.FirstName,
                        LastName = app.Candidate.LastName,
                        Email = app.Candidate.Email,
                        Phone = app.Candidate.Phone,
                        LinkedInUrl = app.Candidate.LinkedInUrl,
                        GitHubUrl = app.Candidate.GitHubUrl,
                        PortfolioUrl = app.Candidate.PortfolioUrl,
                        ResumePath = app.Candidate.ResumePath,
                        Skills = app.Candidate.Skills?.Select(s => s.Name).ToList() ?? new List<string>(),
                        CreatedDate = app.Candidate.CreatedDate,
                        ParsedDataJson = app.Candidate.ParsingResult?.ParsedDataJson,
                        ParsingConfidenceScore = app.Candidate.ParsingResult?.ConfidenceScore,
                        CurrentTitle = currentTitle,
                        YearsOfExperience = app.Candidate.YearsOfExperience.HasValue ? $"{app.Candidate.YearsOfExperience} yrs" : yearsOfExperience,
                        ExpectedSalary = app.Candidate.ExpectedSalary,
                        LatestApplicationJobTitle = app.Job?.Title ?? "None",
                        LatestApplicationStage = app.CurrentStage,
                        LatestApplicationMatchScore = latestScore?.MatchScore ?? 0
                    } : null
                };
            }).ToList();

            return Result<List<ApplicationDto>>.Success(items);
        }
    }

    public record GetApplicationTimelineQuery(Guid ApplicationId, Guid CompanyId) : IRequest<Result<List<ActivityLogDto>>>;

    public class ActivityLogDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string PerformedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class GetApplicationTimelineQueryHandler : IRequestHandler<GetApplicationTimelineQuery, Result<List<ActivityLogDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetApplicationTimelineQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ActivityLogDto>>> Handle(GetApplicationTimelineQuery request, CancellationToken cancellationToken)
        {
            var appExists = await _context.Applications.AnyAsync(a => a.Id == request.ApplicationId && a.Job.CompanyId == request.CompanyId, cancellationToken);
            if (!appExists) return Result<List<ActivityLogDto>>.Failure("Application not found.");

            var logs = await _context.ActivityLogs
                .AsNoTracking()
                .Where(l => l.ApplicationId == request.ApplicationId)
                .OrderByDescending(l => l.CreatedDate)
                .Select(l => new ActivityLogDto
                {
                    Id = l.Id,
                    Action = l.Action,
                    Details = l.Details,
                    PerformedBy = l.PerformedBy,
                    CreatedDate = l.CreatedDate
                })
                .ToListAsync(cancellationToken);

            return Result<List<ActivityLogDto>>.Success(logs);
        }
    }
}
