using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Entities;
using ATS.Shared.Constants;
using ATS.Shared.Models;
using ATS.Application.Common.Exceptions;
using FluentValidation.Results;

namespace ATS.Application.Features.Candidates
{
    public record CreateCandidateCommand : IRequest<Result<Guid>>
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public string Phone { get; init; }
        public string LinkedInUrl { get; init; }
        public string GitHubUrl { get; init; }
        public string PortfolioUrl { get; init; }
        public List<string> Skills { get; init; } = new List<string>();
        public Guid CompanyId { get; init; }
    }

    public class CreateCandidateCommandHandler : IRequestHandler<CreateCandidateCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICandidateSearchService _searchService;

        public CreateCandidateCommandHandler(IApplicationDbContext context, ICandidateSearchService searchService)
        {
            _context = context;
            _searchService = searchService;
        }

        public async Task<Result<Guid>> Handle(CreateCandidateCommand request, CancellationToken cancellationToken)
        {
            var existing = await _context.Candidates.FirstOrDefaultAsync(c => c.Email == request.Email && c.CompanyId == request.CompanyId, cancellationToken);
            if (existing != null)
            {
                return Result<Guid>.Failure("Candidate with this email already exists.");
            }

            var candidate = new Candidate
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                LinkedInUrl = request.LinkedInUrl,
                GitHubUrl = request.GitHubUrl,
                PortfolioUrl = request.PortfolioUrl,
                CompanyId = request.CompanyId
            };

            foreach (var skillName in request.Skills)
            {
                candidate.Skills.Add(new CandidateSkill { Name = skillName });
            }

            await _context.Candidates.AddAsync(candidate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var indexContent = $"{candidate.FirstName} {candidate.LastName} {candidate.Email} {candidate.Phone} {string.Join(" ", candidate.Skills.Select(s => s.Name))}";
            try
            {
                await _searchService.IndexCandidateAsync(candidate.Id, indexContent, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Search Indexing Error] Failed to index candidate {candidate.Id}: {ex.Message}");
            }

            return Result<Guid>.Success(candidate.Id);
        }
    }

    public record UploadResumeCommand : IRequest<Result<Guid>>
    {
        public string FileName { get; init; }
        public byte[] FileData { get; init; }
        public Guid CompanyId { get; init; }
        public Guid? JobId { get; init; } // Optional: If applying to a specific job
        public string? CandidateFirstName { get; init; }
        public string? CandidateLastName { get; init; }
        public string? CandidateEmail { get; init; }
        public string? CandidatePhone { get; init; }
        public string? Source { get; init; }
    }

    public class UploadResumeCommandHandler : IRequestHandler<UploadResumeCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IStorageService _storageService;
        private readonly IAIEngineService _aiEngine;
        private readonly IEmailService _emailService;
        private readonly ICandidateSearchService _searchService;

        public UploadResumeCommandHandler(
            IApplicationDbContext context,
            IStorageService storageService,
            IAIEngineService aiEngine,
            IEmailService emailService,
            ICandidateSearchService searchService)
        {
            _context = context;
            _storageService = storageService;
            _aiEngine = aiEngine;
            _emailService = emailService;
            _searchService = searchService;
        }

        public async Task<Result<Guid>> Handle(UploadResumeCommand request, CancellationToken cancellationToken)
        {
            // Sanitize filename
            var sanitizedFileName = Path.GetFileName(request.FileName);

            // Validate extension whitelist
            var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".txt", ".rtf" };
            var extension = Path.GetExtension(sanitizedFileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new ValidationException(new[] { new ValidationFailure("FileName", "Invalid file extension. Only .pdf, .docx, .doc, .txt, and .rtf are allowed.") });
            }

            // 1. Upload resume to storage
            string folderName = "resumes";
            string relativePath = await _storageService.UploadFileAsync(request.FileData, sanitizedFileName, folderName);

            // 2. Parse resume using AI Engine
            var parsedResult = await _aiEngine.ParseResumeAsync(request.FileData, extension, sanitizedFileName);

            // 3. Split Name into First and Last
            string firstName = "Unknown";
            string lastName = "Candidate";
            if (!string.IsNullOrWhiteSpace(parsedResult.Name))
            {
                var nameParts = parsedResult.Name.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                firstName = nameParts[0];
                if (nameParts.Length > 1)
                {
                    lastName = nameParts[1];
                }
            }

            // Override with manual inputs if provided in the request
            string email = !string.IsNullOrWhiteSpace(request.CandidateEmail) ? request.CandidateEmail : (string.IsNullOrEmpty(parsedResult.Email) ? $"parsed_{Guid.NewGuid():N}@example.com" : parsedResult.Email);
            string firstNameOverride = !string.IsNullOrWhiteSpace(request.CandidateFirstName) ? request.CandidateFirstName : firstName;
            string lastNameOverride = !string.IsNullOrWhiteSpace(request.CandidateLastName) ? request.CandidateLastName : lastName;
            string phoneOverride = !string.IsNullOrWhiteSpace(request.CandidatePhone) ? request.CandidatePhone : parsedResult.Phone;

            // 4. Check for duplicate candidate (by Email, Phone, or LinkedIn)
            bool checkEmail = !string.IsNullOrWhiteSpace(email) && !email.StartsWith("parsed_");
            bool checkPhone = !string.IsNullOrWhiteSpace(phoneOverride);
            bool checkLinkedIn = !string.IsNullOrWhiteSpace(parsedResult.LinkedInUrl);

            Candidate candidate = null;
            if (checkEmail || checkPhone || checkLinkedIn)
            {
                candidate = await _context.Candidates
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c =>
                        c.CompanyId == request.CompanyId &&
                        (
                            (checkEmail && c.Email == email) ||
                            (checkPhone && c.Phone == phoneOverride) ||
                            (checkLinkedIn && c.LinkedInUrl == parsedResult.LinkedInUrl)
                        ), cancellationToken);
            }

            bool isNewCandidate = candidate == null;
            if (isNewCandidate)
            {
                candidate = new Candidate
                {
                    FirstName = firstNameOverride,
                    LastName = lastNameOverride,
                    Email = email,
                    Phone = phoneOverride,
                    LinkedInUrl = parsedResult.LinkedInUrl,
                    GitHubUrl = parsedResult.GitHubUrl,
                    PortfolioUrl = parsedResult.PortfolioUrl,
                    CompanyId = request.CompanyId,
                    ResumePath = relativePath,
                    Source = !string.IsNullOrWhiteSpace(request.Source) ? request.Source : "Careers Portal"
                };
                await _context.Candidates.AddAsync(candidate, cancellationToken);
            }
            else
            {
                // Update existing candidate details to prevent duplicate profiles
                candidate.ResumePath = relativePath;
                if (!string.IsNullOrWhiteSpace(firstNameOverride) && candidate.FirstName == "Unknown") candidate.FirstName = firstNameOverride;
                if (!string.IsNullOrWhiteSpace(lastNameOverride) && candidate.LastName == "Candidate") candidate.LastName = lastNameOverride;
                if (!string.IsNullOrWhiteSpace(phoneOverride)) candidate.Phone = phoneOverride;
                if (!string.IsNullOrWhiteSpace(parsedResult.LinkedInUrl)) candidate.LinkedInUrl = parsedResult.LinkedInUrl;
                if (!string.IsNullOrWhiteSpace(parsedResult.GitHubUrl)) candidate.GitHubUrl = parsedResult.GitHubUrl;
                if (!string.IsNullOrWhiteSpace(parsedResult.PortfolioUrl)) candidate.PortfolioUrl = parsedResult.PortfolioUrl;
                _context.Candidates.Update(candidate);
            }

            // Update candidate skills
            if (parsedResult.Skills != null && parsedResult.Skills.Any())
            {
                if (!isNewCandidate)
                {
                    var existingSkills = await _context.CandidateSkills.Where(s => s.CandidateId == candidate.Id).ToListAsync(cancellationToken);
                    _context.CandidateSkills.RemoveRange(existingSkills);
                }

                candidate.Skills.Clear();

                foreach (var skill in parsedResult.Skills)
                {
                    var candidateSkill = new CandidateSkill { Name = skill, CandidateId = candidate.Id };
                    candidate.Skills.Add(candidateSkill);
                    await _context.CandidateSkills.AddAsync(candidateSkill, cancellationToken);
                }
            }

            // Save parsing results
            var existingParsing = await _context.ResumeParsingResults
                .FirstOrDefaultAsync(r => r.CandidateId == candidate.Id, cancellationToken);

            if (existingParsing == null)
            {
                var parsingEntity = new ResumeParsingResult
                {
                    CandidateId = candidate.Id,
                    RawText = !string.IsNullOrEmpty(parsedResult.RawText) ? parsedResult.RawText : (parsedResult.Experience + "\n" + parsedResult.Education + "\n" + string.Join(", ", parsedResult.Skills)),
                    ConfidenceScore = parsedResult.ConfidenceScore,
                    ParsedDataJson = System.Text.Json.JsonSerializer.Serialize(parsedResult)
                };
                await _context.ResumeParsingResults.AddAsync(parsingEntity, cancellationToken);
            }
            else
            {
                existingParsing.RawText = !string.IsNullOrEmpty(parsedResult.RawText) ? parsedResult.RawText : (parsedResult.Experience + "\n" + parsedResult.Education + "\n" + string.Join(", ", parsedResult.Skills));
                existingParsing.ConfidenceScore = parsedResult.ConfidenceScore;
                existingParsing.ParsedDataJson = System.Text.Json.JsonSerializer.Serialize(parsedResult);
                _context.ResumeParsingResults.Update(existingParsing);
            }
            await _context.SaveChangesAsync(cancellationToken);

            // Index for full-text search
            var searchIndexSkillsText = string.Join(" ", candidate.Skills.Select(s => s.Name));
            var searchIndexFullText = $"{parsedResult.Experience} {parsedResult.Education} {searchIndexSkillsText}";
            var indexContent = $"{candidate.FirstName} {candidate.LastName} {candidate.Email} {candidate.Phone} {searchIndexFullText} {parsedResult.RawText}";
            try
            {
                await _searchService.IndexCandidateAsync(candidate.Id, indexContent, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Search Indexing Error] Failed to index candidate {candidate.Id}: {ex.Message}");
            }

            // 5. If applying to a specific Job, create Application and score it!
            if (request.JobId.HasValue)
            {
                var job = await _context.Jobs
                    .Include(j => j.Skills)
                    .FirstOrDefaultAsync(j => j.Id == request.JobId.Value, cancellationToken);

                if (job != null)
                {
                    // Check if already applied
                    var existingApp = await _context.Applications
                        .FirstOrDefaultAsync(a => a.JobId == job.Id && a.CandidateId == candidate.Id, cancellationToken);

                    if (existingApp == null)
                    {
                        var app = new ATS.Domain.Entities.Application
                        {
                            JobId = job.Id,
                            CandidateId = candidate.Id,
                            CurrentStage = Stages.Applied,
                            Status = "Active"
                        };

                        // Add stage history
                        app.StagesHistory.Add(new ApplicationStage
                        {
                            StageName = Stages.Applied,
                            Status = "Completed",
                            EnteredDate = DateTime.UtcNow,
                            LeftDate = DateTime.UtcNow,
                            SequenceNumber = 0
                        });
                        app.StagesHistory.Add(new ApplicationStage
                        {
                            StageName = Stages.Screening,
                            Status = "Current",
                            EnteredDate = DateTime.UtcNow,
                            SequenceNumber = 1
                        });
                        app.CurrentStage = Stages.Screening;

                        await _context.Applications.AddAsync(app, cancellationToken);
                        await _context.SaveChangesAsync(cancellationToken);

                        // Trigger AI Fit Scoring
                        var resumeSkillsText = string.Join(", ", candidate.Skills.Select(s => s.Name));
                        var resumeFullText = $"{parsedResult.Experience} {parsedResult.Education} Skills: {resumeSkillsText}";
                        var jobSkillsText = string.Join(", ", job.Skills.Select(s => s.Name));
                        var jobFullText = $"{job.Title} {job.Description} {job.Responsibilities} {job.Qualifications} Required Skills: {jobSkillsText}";

                        var scoringResult = await _aiEngine.ScoreCandidateAsync(resumeFullText, jobFullText, candidate.Id, job.Id);
                        var questionsResult = await _aiEngine.SuggestInterviewQuestionsAsync(resumeFullText, jobFullText, candidate.Id, job.Id);

                        var allQuestions = new List<string>();
                        if (questionsResult.TechnicalQuestions != null) allQuestions.AddRange(questionsResult.TechnicalQuestions);
                        if (questionsResult.BehavioralQuestions != null) allQuestions.AddRange(questionsResult.BehavioralQuestions);
                        if (questionsResult.FollowUpQuestions != null) allQuestions.AddRange(questionsResult.FollowUpQuestions);

                        var aiScore = new AIScore
                        {
                            ApplicationId = app.Id,
                            JobId = job.Id,
                            CandidateId = candidate.Id,
                            MatchScore = scoringResult.MatchScore,
                            SkillMatchPercentage = scoringResult.SkillMatchPercentage,
                            ExperienceMatchPercentage = scoringResult.ExperienceMatchPercentage,
                            EducationMatchPercentage = scoringResult.EducationMatchPercentage,
                            MissingSkillsJson = System.Text.Json.JsonSerializer.Serialize(scoringResult.MissingSkills),
                            StrengthsJson = System.Text.Json.JsonSerializer.Serialize(scoringResult.Strengths),
                            WeaknessesJson = System.Text.Json.JsonSerializer.Serialize(scoringResult.Weaknesses),
                            AIQuestionsJson = System.Text.Json.JsonSerializer.Serialize(allQuestions),
                            Recommendation = scoringResult.Recommendation,
                            AISummary = scoringResult.AISummary
                        };

                        await _context.AIScores.AddAsync(aiScore, cancellationToken);
                        
                        // Add activity log
                        await _context.ActivityLogs.AddAsync(new ActivityLog
                        {
                            ApplicationId = app.Id,
                            CandidateId = candidate.Id,
                            Action = "Apply",
                            Details = $"Applied online for job '{job.Title}'. Resume uploaded.",
                            PerformedBy = $"{candidate.FirstName} {candidate.LastName}"
                        }, cancellationToken);

                        await _context.ActivityLogs.AddAsync(new ActivityLog
                        {
                            ApplicationId = app.Id,
                            CandidateId = candidate.Id,
                            Action = "AI Analysis",
                            Details = $"AI Evaluation completed. Recommendation: {scoringResult.Recommendation}. Fit Score: {scoringResult.MatchScore}%.",
                            PerformedBy = "AI Engine"
                        }, cancellationToken);

                        await _context.SaveChangesAsync(cancellationToken);

                        // Dispatch email notification template
                        var templateVars = new Dictionary<string, string>
                        {
                            { "CandidateName", $"{candidate.FirstName} {candidate.LastName}" },
                            { "JobTitle", job.Title }
                        };

                        try
                        {
                            await _emailService.SendEmailTemplateAsync(candidate.Email, "ApplicationReceived", templateVars);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Email Error] Failed to send template email: {ex.Message}");
                        }
                    }
                }
            }

            return Result<Guid>.Success(candidate.Id);
        }
    }

    public record AssignTalentPoolCommand : IRequest<Result>
    {
        public Guid CandidateId { get; init; }
        public Guid CompanyId { get; init; }
        public string PoolName { get; init; }
        public string Actor { get; init; }
    }

    public class AssignTalentPoolCommandHandler : IRequestHandler<AssignTalentPoolCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public AssignTalentPoolCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(AssignTalentPoolCommand request, CancellationToken cancellationToken)
        {
            var candidate = await _context.Candidates
                .FirstOrDefaultAsync(c => c.Id == request.CandidateId && c.CompanyId == request.CompanyId, cancellationToken);

            if (candidate == null)
            {
                return Result.Failure("Candidate not found.");
            }

            candidate.Source = request.PoolName;
            _context.Candidates.Update(candidate);

            // Log activity
            await _context.ActivityLogs.AddAsync(new ActivityLog
            {
                CandidateId = candidate.Id,
                Action = "Assign Talent Pool",
                Details = $"Candidate assigned to talent pool: {request.PoolName}.",
                PerformedBy = request.Actor ?? "Recruiter"
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
