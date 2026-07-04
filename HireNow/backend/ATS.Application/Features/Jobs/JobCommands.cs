using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Entities;
using ATS.Domain.Enums;
using ATS.Shared.Models;

namespace ATS.Application.Features.Jobs
{
    public record CreateJobCommand : IRequest<Result<Guid>>
    {
        public string Title { get; init; }
        public string Description { get; init; }
        public string Responsibilities { get; init; }
        public string Qualifications { get; init; }
        public Guid DepartmentId { get; init; }
        public Guid? HiringManagerId { get; init; }
        public Guid? RecruiterId { get; init; }
        public string Location { get; init; }
        public EmploymentType EmploymentType { get; init; }
        public decimal? SalaryMin { get; init; }
        public decimal? SalaryMax { get; init; }
        public string Currency { get; init; }
        public int? ExperienceYears { get; init; }
        public List<string> Skills { get; init; } = new List<string>();
        public Guid CompanyId { get; init; }
    }

    public class CreateJobValidator : AbstractValidator<CreateJobCommand>
    {
        public CreateJobValidator()
        {
            RuleFor(v => v.Title).NotEmpty().MaximumLength(200);
            RuleFor(v => v.Description).NotEmpty();
            RuleFor(v => v.DepartmentId).NotEmpty();
            RuleFor(v => v.CompanyId).NotEmpty();
        }
    }

    public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public CreateJobCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
        {
            var company = await _context.Companies.FindAsync(new object[] { request.CompanyId }, cancellationToken);
            if (company != null && (string.IsNullOrEmpty(company.SubscriptionPlan) || company.SubscriptionPlan.Equals("Free Trial", StringComparison.OrdinalIgnoreCase)))
            {
                var activeJobsCount = await _context.Jobs
                    .CountAsync(j => j.CompanyId == request.CompanyId && j.Status != JobStatus.Archived, cancellationToken);
                
                if (activeJobsCount >= 5)
                {
                    return Result<Guid>.Failure("Subscription Limit Exceeded: Free Trial is limited to 5 active jobs. Please upgrade to create more.");
                }
            }

            var job = new Job
            {
                Title = request.Title,
                Description = request.Description,
                Responsibilities = request.Responsibilities,
                Qualifications = request.Qualifications,
                DepartmentId = request.DepartmentId,
                HiringManagerId = request.HiringManagerId,
                RecruiterId = request.RecruiterId,
                Location = request.Location,
                EmploymentType = request.EmploymentType,
                SalaryMin = request.SalaryMin,
                SalaryMax = request.SalaryMax,
                Currency = request.Currency ?? "USD",
                ExperienceYears = request.ExperienceYears,
                CompanyId = request.CompanyId,
                Status = JobStatus.Draft
            };

            if (request.Skills != null)
            {
                foreach (var skillName in request.Skills)
                {
                    job.Skills.Add(new JobSkill { Name = skillName });
                }
            }

            await _context.Jobs.AddAsync(job, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(job.Id);
        }
    }

    public record UpdateJobCommand : IRequest<Result>
    {
        public Guid Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public string Responsibilities { get; init; }
        public string Qualifications { get; init; }
        public Guid DepartmentId { get; init; }
        public Guid? HiringManagerId { get; init; }
        public Guid? RecruiterId { get; init; }
        public string Location { get; init; }
        public EmploymentType EmploymentType { get; init; }
        public decimal? SalaryMin { get; init; }
        public decimal? SalaryMax { get; init; }
        public string Currency { get; init; }
        public int? ExperienceYears { get; init; }
        public List<string> Skills { get; init; }
    }

    public class UpdateJobValidator : AbstractValidator<UpdateJobCommand>
    {
        public UpdateJobValidator()
        {
            RuleFor(v => v.Id).NotEmpty();
            RuleFor(v => v.Title).NotEmpty().MaximumLength(200);
            RuleFor(v => v.Description).NotEmpty();
            RuleFor(v => v.DepartmentId).NotEmpty();
        }
    }

    public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public UpdateJobCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _context.Jobs
                .Include(j => j.Skills)
                .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

            if (job == null)
            {
                return Result.Failure("Job not found.");
            }

            job.Title = request.Title;
            job.Description = request.Description;
            job.Responsibilities = request.Responsibilities;
            job.Qualifications = request.Qualifications;
            job.DepartmentId = request.DepartmentId;
            job.HiringManagerId = request.HiringManagerId;
            job.RecruiterId = request.RecruiterId;
            job.Location = request.Location;
            job.EmploymentType = request.EmploymentType;
            job.SalaryMin = request.SalaryMin;
            job.SalaryMax = request.SalaryMax;
            job.Currency = request.Currency ?? "USD";
            job.ExperienceYears = request.ExperienceYears;

            // Update Skills
            if (request.Skills != null)
            {
                // Remove deleted skills
                var skillsToRemove = job.Skills.Where(s => !request.Skills.Contains(s.Name)).ToList();
                foreach (var skill in skillsToRemove)
                {
                    job.Skills.Remove(skill);
                }

                // Add new skills
                var existingSkillNames = job.Skills.Select(s => s.Name).ToList();
                var skillsToAdd = request.Skills.Where(s => !existingSkillNames.Contains(s)).ToList();
                foreach (var skillName in skillsToAdd)
                {
                    var newSkill = new JobSkill { Name = skillName, JobId = job.Id };
                    job.Skills.Add(newSkill);
                    await _context.JobSkills.AddAsync(newSkill, cancellationToken);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public record DeleteJobCommand(Guid Id) : IRequest<Result>;

    public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public DeleteJobCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (job == null)
            {
                return Result.Failure("Job not found.");
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public record UpdateJobStatusCommand(Guid Id, JobStatus Status) : IRequest<Result>;

    public class UpdateJobStatusCommandHandler : IRequestHandler<UpdateJobStatusCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public UpdateJobStatusCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateJobStatusCommand request, CancellationToken cancellationToken)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (job == null)
            {
                return Result.Failure("Job not found.");
            }

            job.Status = request.Status;
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
