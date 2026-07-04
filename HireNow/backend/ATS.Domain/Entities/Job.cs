using System;
using System.Collections.Generic;
using ATS.Domain.Common;
using ATS.Domain.Enums;

namespace ATS.Domain.Entities
{
    public class Job : BaseEntity, IMultiTenant
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Responsibilities { get; set; }
        public string Qualifications { get; set; }

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }

        public Guid? HiringManagerId { get; set; }
        public ApplicationUser HiringManager { get; set; }

        public Guid? RecruiterId { get; set; }
        public ApplicationUser Recruiter { get; set; }

        public JobStatus Status { get; set; } = JobStatus.Draft;
        public string Location { get; set; }
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string Currency { get; set; } = "USD";
        public int? ExperienceYears { get; set; }

        public Guid CompanyId { get; set; }
        public Company Company { get; set; }

        public ICollection<JobSkill> Skills { get; set; } = new List<JobSkill>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
