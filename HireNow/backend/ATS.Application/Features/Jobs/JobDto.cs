using System;
using System.Collections.Generic;
using ATS.Domain.Enums;

namespace ATS.Application.Features.Jobs
{
    public class JobDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Responsibilities { get; set; }
        public string Qualifications { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public Guid? HiringManagerId { get; set; }
        public string HiringManagerName { get; set; }
        public Guid? RecruiterId { get; set; }
        public string RecruiterName { get; set; }
        public JobStatus Status { get; set; }
        public string Location { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string Currency { get; set; }
        public int? ExperienceYears { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public int ApplicantCount { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
