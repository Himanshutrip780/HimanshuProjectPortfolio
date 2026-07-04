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

namespace ATS.Application.Features.Jobs
{
    public record GetJobByIdQuery(Guid Id, Guid? CompanyId = null) : IRequest<Result<JobDto>>;

    public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, Result<JobDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetJobByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<JobDto>> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
        {
            Job job;
            if (request.CompanyId == null || request.CompanyId == Guid.Empty)
            {
                // Anonymous guest request: bypass global query filter but restrict to published & not deleted
                job = await _context.Jobs
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(j => j.Department)
                    .Include(j => j.HiringManager)
                    .Include(j => j.Recruiter)
                    .Include(j => j.Skills)
                    .Include(j => j.Applications)
                    .FirstOrDefaultAsync(j => j.Id == request.Id && !j.IsDeleted && j.Status == JobStatus.Published, cancellationToken);
            }
            else
            {
                // Internal recruiter request: enforce company ID boundaries
                job = await _context.Jobs
                    .AsNoTracking()
                    .Include(j => j.Department)
                    .Include(j => j.HiringManager)
                    .Include(j => j.Recruiter)
                    .Include(j => j.Skills)
                    .Include(j => j.Applications)
                    .FirstOrDefaultAsync(j => j.Id == request.Id && j.CompanyId == request.CompanyId.Value, cancellationToken);
            }

            if (job == null)
            {
                return Result<JobDto>.Failure("Job not found.");
            }

            var dto = new JobDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Responsibilities = job.Responsibilities,
                Qualifications = job.Qualifications,
                DepartmentId = job.DepartmentId,
                DepartmentName = job.Department?.Name,
                HiringManagerId = job.HiringManagerId,
                HiringManagerName = job.HiringManager != null ? $"{job.HiringManager.FirstName} {job.HiringManager.LastName}" : null,
                RecruiterId = job.RecruiterId,
                RecruiterName = job.Recruiter != null ? $"{job.Recruiter.FirstName} {job.Recruiter.LastName}" : null,
                Status = job.Status,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                Currency = job.Currency,
                ExperienceYears = job.ExperienceYears,
                Skills = job.Skills.Select(s => s.Name).ToList(),
                ApplicantCount = job.Applications.Count(a => !a.IsDeleted),
                CompanyId = job.CompanyId,
                CreatedDate = job.CreatedDate
            };

            return Result<JobDto>.Success(dto);
        }
    }

    public record GetJobsQuery : IRequest<Result<PaginatedList<JobDto>>>
    {
        public Guid CompanyId { get; init; }
        public Guid? DepartmentId { get; init; }
        public JobStatus? Status { get; init; }
        public string SearchTerm { get; init; }
        public int PageIndex { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, Result<PaginatedList<JobDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetJobsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PaginatedList<JobDto>>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Jobs
                .AsNoTracking();

            if (request.DepartmentId.HasValue)
            {
                query = query.Where(j => j.DepartmentId == request.DepartmentId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(j => j.Status == request.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(search) || j.Location.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(j => j.CreatedDate)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(j => j.Department)
                .Include(j => j.HiringManager)
                .Include(j => j.Recruiter)
                .Include(j => j.Skills)
                .Include(j => j.Applications)
                .Select(job => new JobDto
                {
                    Id = job.Id,
                    Title = job.Title,
                    Description = job.Description,
                    Responsibilities = job.Responsibilities,
                    Qualifications = job.Qualifications,
                    DepartmentId = job.DepartmentId,
                    DepartmentName = job.Department != null ? job.Department.Name : null,
                    HiringManagerId = job.HiringManagerId,
                    HiringManagerName = job.HiringManager != null ? $"{job.HiringManager.FirstName} {job.HiringManager.LastName}" : null,
                    RecruiterId = job.RecruiterId,
                    RecruiterName = job.Recruiter != null ? $"{job.Recruiter.FirstName} {job.Recruiter.LastName}" : null,
                    Status = job.Status,
                    Location = job.Location,
                    EmploymentType = job.EmploymentType,
                    SalaryMin = job.SalaryMin,
                    SalaryMax = job.SalaryMax,
                    Currency = job.Currency,
                    ExperienceYears = job.ExperienceYears,
                    Skills = job.Skills.Select(s => s.Name).ToList(),
                    ApplicantCount = job.Applications.Count(a => !a.IsDeleted),
                    CompanyId = job.CompanyId,
                    CreatedDate = job.CreatedDate
                })
                .ToListAsync(cancellationToken);

            var paginated = new PaginatedList<JobDto>(items, totalCount, request.PageIndex, request.PageSize);
            return Result<PaginatedList<JobDto>>.Success(paginated);
        }
    }

    public record GetDepartmentsQuery(Guid CompanyId) : IRequest<Result<List<DepartmentDto>>>;

    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, Result<List<DepartmentDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetDepartmentsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<DepartmentDto>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var depts = await _context.Departments
                .AsNoTracking()
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .OrderBy(d => d.Name)
                .ToListAsync(cancellationToken);

            return Result<List<DepartmentDto>>.Success(depts);
        }
    }

    public record GetUsersQuery : IRequest<Result<List<UserDto>>>
    {
        public Guid CompanyId { get; init; }
        public string? Role { get; init; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<List<UserDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetUsersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Users
                .AsNoTracking();

            if (!string.IsNullOrEmpty(request.Role))
            {
                query = query.Where(u => u.Role == request.Role);
            }

            var users = await query
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    Role = u.Role
                })
                .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                .ToListAsync(cancellationToken);

            return Result<List<UserDto>>.Success(users);
        }
    }

    public record GetJobFeedQuery(Guid CompanyId) : IRequest<Result<List<JobFeedDto>>>;

    public class JobFeedDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Responsibilities { get; set; }
        public string Qualifications { get; set; }
        public string Location { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string DepartmentName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLogoUrl { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class GetJobFeedQueryHandler : IRequestHandler<GetJobFeedQuery, Result<List<JobFeedDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetJobFeedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<JobFeedDto>>> Handle(GetJobFeedQuery request, CancellationToken cancellationToken)
        {
            var company = await _context.Companies
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

            if (company == null)
            {
                return Result<List<JobFeedDto>>.Failure("Company not found.");
            }

            var jobs = await _context.Jobs
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(j => j.Department)
                .Where(j => j.CompanyId == request.CompanyId && !j.IsDeleted && j.Status == JobStatus.Published)
                .OrderByDescending(j => j.CreatedDate)
                .Select(j => new JobFeedDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    Responsibilities = j.Responsibilities,
                    Qualifications = j.Qualifications,
                    Location = j.Location,
                    EmploymentType = j.EmploymentType,
                    SalaryMin = j.SalaryMin,
                    SalaryMax = j.SalaryMax,
                    DepartmentName = j.Department != null ? j.Department.Name : "General",
                    CompanyName = company.Name,
                    CompanyLogoUrl = company.LogoUrl ?? "",
                    CreatedDate = j.CreatedDate
                })
                .ToListAsync(cancellationToken);

            return Result<List<JobFeedDto>>.Success(jobs);
        }
    }
}
