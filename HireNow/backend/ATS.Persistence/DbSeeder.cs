using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ATS.Domain.Entities;
using ATS.Domain.Enums;
using ATS.Shared.Constants;

namespace ATS.Persistence
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            // Ensure database is created and migrated
            await context.Database.MigrateAsync();

            // Skip seeding if Zensar Technologies is already seeded and himanshu.superadmin@zensar.com exists
            if (await context.Companies.AnyAsync(c => c.Name == "Zensar Technologies") &&
                await userManager.FindByEmailAsync("himanshu.superadmin@zensar.com") != null)
            {
                // Ensure existing offers have default OfferLetterContent populated if empty or using old short template
                var emptyOffers = await context.Offers
                    .Include(o => o.Application).ThenInclude(a => a.Candidate)
                    .Include(o => o.Application).ThenInclude(a => a.Job).ThenInclude(j => j.Recruiter)
                    .Include(o => o.Application).ThenInclude(a => a.Job).ThenInclude(j => j.Department)
                    .Where(o => o.OfferLetterContent == "" || o.OfferLetterContent.Length < 450)
                    .ToListAsync();
                if (emptyOffers.Any())
                {
                    foreach (var o in emptyOffers)
                    {
                        var candidateName = o.Application?.Candidate != null ? $"{o.Application.Candidate.FirstName} {o.Application.Candidate.LastName}".Trim() : "Candidate";
                        var companyName = "Zensar Technologies";
                        var jobTitle = o.Application?.Job?.Title ?? "Software Professional";
                        var departmentName = o.Application?.Job?.Department?.Name ?? "Engineering";
                        var location = o.Application?.Job?.Location ?? "Remote";
                        var currency = o.Application?.Job?.Currency ?? "USD";
                        var recruiterName = o.Application?.Job?.Recruiter != null ? $"{o.Application.Job.Recruiter.FirstName} {o.Application.Job.Recruiter.LastName}".Trim() : "Lead Recruiter";
                        var recruiterEmail = o.Application?.Job?.Recruiter?.Email ?? "recruitment@zensar.com";
                        var sDate = o.StartDate;
                        var salaryFormatted = $"{currency} {o.Salary:N0}";
                        
                        o.OfferLetterContent = $"Dear {candidateName},\n\n" +
                            $"On behalf of {companyName}, we are pleased to offer you the position of {jobTitle} within our {departmentName} department, located at our {location} office. We are extremely excited about the prospect of you joining our team!\n\n" +
                            $"Please find the detailed terms of your employment offer below:\n\n" +
                            $"- **Position**: {jobTitle}\n" +
                            $"- **Department**: {departmentName}\n" +
                            $"- **Location**: {location}\n" +
                            $"- **Annual Base Salary**: {salaryFormatted}\n" +
                            $"- **Start Date**: {sDate:MMMM dd, yyyy}\n" +
                            $"- **Employment Status**: Full-Time\n\n" +
                            $"This offer is contingent upon the successful completion of standard background checks and reference validations.\n\n" +
                            $"To accept this offer, please review the document and type your full name in the signature field in the e-sign portal by {sDate:MMMM dd, yyyy}.\n\n" +
                            $"Best regards,\n\n" +
                            $"{recruiterName}\n" +
                            $"Lead Recruiter / Hiring Coordinator\n" +
                            $"{companyName} Talent Acquisition Team\n" +
                            $"Email: {recruiterEmail}";
                    }
                    await context.SaveChangesAsync();
                }

                Console.WriteLine("Database already seeded with Zensar Technologies mock data. Skipping seeding.");
                return;
            }

            Console.WriteLine("Starting Zensar Technologies Database Reset & Mock Data Generation...");

            // -------------------------------------------------------------
            // -------------------------------------------------------------
            // STEP 1: TRUNCATE EXISTING DATA (Except admin@acme.com)
            // -------------------------------------------------------------
            Console.WriteLine("Executing raw SQL purge...");
            await context.Database.ExecuteSqlRawAsync(@"
                -- Disable constraints
                ALTER TABLE [InterviewFeedbacks] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [Interviews] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [Offers] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [AIScores] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [ApplicationStages] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [Applications] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [JobSkills] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [Jobs] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [CandidateSkills] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [Candidates] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [Notifications] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [ActivityLogs] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [ResumeParsingResults] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [AuditLogs] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [CandidateNotes] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [Departments] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [EmailTemplates] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [Companies] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [AspNetUsers] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [AspNetUserRoles] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [AspNetUserClaims] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [AspNetUserLogins] NOCHECK CONSTRAINT ALL;
                ALTER TABLE [AspNetUserTokens] NOCHECK CONSTRAINT ALL;

                -- Delete records
                DELETE FROM [InterviewFeedbacks];
                DELETE FROM [Interviews];
                DELETE FROM [Offers];
                DELETE FROM [AIScores];
                DELETE FROM [ApplicationStages];
                DELETE FROM [Applications];
                DELETE FROM [JobSkills];
                DELETE FROM [Jobs];
                DELETE FROM [CandidateSkills];
                DELETE FROM [Candidates];
                DELETE FROM [Notifications];
                DELETE FROM [ActivityLogs];
                DELETE FROM [ResumeParsingResults];
                DELETE FROM [AuditLogs];
                DELETE FROM [CandidateNotes];
                DELETE FROM [Departments];
                DELETE FROM [EmailTemplates];
                DELETE FROM [Companies];

                -- Delete Identity data for non-admin users (handle potential NULL emails)
                DELETE FROM [AspNetUserRoles] WHERE [UserId] IN (SELECT [Id] FROM [AspNetUsers] WHERE [Email] IS NULL OR [Email] != 'admin@acme.com');
                DELETE FROM [AspNetUserClaims] WHERE [UserId] IN (SELECT [Id] FROM [AspNetUsers] WHERE [Email] IS NULL OR [Email] != 'admin@acme.com');
                DELETE FROM [AspNetUserLogins] WHERE [UserId] IN (SELECT [Id] FROM [AspNetUsers] WHERE [Email] IS NULL OR [Email] != 'admin@acme.com');
                DELETE FROM [AspNetUserTokens] WHERE [UserId] IN (SELECT [Id] FROM [AspNetUsers] WHERE [Email] IS NULL OR [Email] != 'admin@acme.com');
                DELETE FROM [AspNetUsers] WHERE [Email] IS NULL OR [Email] != 'admin@acme.com';

                -- Reinsert Zensar Technologies company (providing all non-nullable properties from BaseEntity)
                DECLARE @zensarId UNIQUEIDENTIFIER = NEWID();
                INSERT INTO [Companies] ([Id], [Name], [Domain], [SubscriptionPlan], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [IsDeleted])
                VALUES (@zensarId, 'Zensar Technologies', 'zensar.com', 'Enterprise', 'System', GETUTCDATE(), 'System', GETUTCDATE(), 0);

                -- Link admin user to Zensar company if admin user exists
                UPDATE [AspNetUsers] SET [CompanyId] = @zensarId WHERE [Email] = 'admin@acme.com';

                -- Enable constraints
                ALTER TABLE [InterviewFeedbacks] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [Interviews] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [Offers] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [AIScores] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [ApplicationStages] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [Applications] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [JobSkills] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [Jobs] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [CandidateSkills] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [Candidates] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [Notifications] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [ActivityLogs] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [ResumeParsingResults] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [AuditLogs] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [CandidateNotes] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [Departments] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [EmailTemplates] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [Companies] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [AspNetUsers] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [AspNetUserRoles] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [AspNetUserClaims] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [AspNetUserLogins] WITH CHECK CHECK CONSTRAINT ALL;
                ALTER TABLE [AspNetUserTokens] WITH CHECK CHECK CONSTRAINT ALL;
            ");

            // Fetch the Zensar company created via raw SQL
            var company = await context.Companies.FirstAsync(c => c.Name == "Zensar Technologies");

            // -------------------------------------------------------------
            // STEP 2: SEED ROLES & SYSTEM TEST ACCOUNTS
            // -------------------------------------------------------------
            const string defaultPassword = "Password123!";

            foreach (var roleName in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole(roleName));
                }
            }

            // Ensure Admin Users
            var adminUser = await userManager.FindByEmailAsync("admin@acme.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "admin@acme.com",
                    Email = "admin@acme.com",
                    EmailConfirmed = true,
                    FirstName = "Alice",
                    LastName = "Admin",
                    CompanyId = company.Id,
                    Role = Roles.SuperAdmin,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await userManager.CreateAsync(adminUser, defaultPassword);
                await userManager.AddToRoleAsync(adminUser, Roles.SuperAdmin);
            }

            // Zensar SuperAdmin
            var zensarAdmin = await userManager.FindByEmailAsync("himanshu.superadmin@zensar.com");
            if (zensarAdmin == null)
            {
                zensarAdmin = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "himanshu.superadmin@zensar.com",
                    Email = "himanshu.superadmin@zensar.com",
                    EmailConfirmed = true,
                    FirstName = "Himanshu",
                    LastName = "Admin",
                    CompanyId = company.Id,
                    Role = Roles.SuperAdmin,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await userManager.CreateAsync(zensarAdmin, defaultPassword);
                await userManager.AddToRoleAsync(zensarAdmin, Roles.SuperAdmin);
            }

            // Zensar Recruiter (Test Login)
            var zensarRecruiter = await userManager.FindByEmailAsync("himanshu.recruiter@zensar.com");
            if (zensarRecruiter == null)
            {
                zensarRecruiter = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "himanshu.recruiter@zensar.com",
                    Email = "himanshu.recruiter@zensar.com",
                    EmailConfirmed = true,
                    FirstName = "Himanshu",
                    LastName = "Recruiter",
                    CompanyId = company.Id,
                    Role = Roles.Recruiter,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await userManager.CreateAsync(zensarRecruiter, defaultPassword);
                await userManager.AddToRoleAsync(zensarRecruiter, Roles.Recruiter);
            }

            // Zensar Hiring Manager (Test Login)
            var zensarManager = await userManager.FindByEmailAsync("himanshu.hiringmanager@zensar.com");
            if (zensarManager == null)
            {
                zensarManager = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "himanshu.hiringmanager@zensar.com",
                    Email = "himanshu.hiringmanager@zensar.com",
                    EmailConfirmed = true,
                    FirstName = "Himanshu",
                    LastName = "HiringManager",
                    CompanyId = company.Id,
                    Role = Roles.HiringManager,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await userManager.CreateAsync(zensarManager, defaultPassword);
                await userManager.AddToRoleAsync(zensarManager, Roles.HiringManager);
            }

            // Zensar Interviewer (Test Login)
            var zensarInterviewer = await userManager.FindByEmailAsync("himanshu.interviewer@zensar.com");
            if (zensarInterviewer == null)
            {
                zensarInterviewer = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "himanshu.interviewer@zensar.com",
                    Email = "himanshu.interviewer@zensar.com",
                    EmailConfirmed = true,
                    FirstName = "Himanshu",
                    LastName = "Interviewer",
                    CompanyId = company.Id,
                    Role = Roles.Interviewer,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await userManager.CreateAsync(zensarInterviewer, defaultPassword);
                await userManager.AddToRoleAsync(zensarInterviewer, Roles.Interviewer);
            }

            // Zensar Candidate User
            var zensarCandidateUser = await userManager.FindByEmailAsync("himanshu.candidate@zensar.com");
            if (zensarCandidateUser == null)
            {
                zensarCandidateUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "himanshu.candidate@zensar.com",
                    Email = "himanshu.candidate@zensar.com",
                    EmailConfirmed = true,
                    FirstName = "Himanshu",
                    LastName = "Candidate",
                    CompanyId = company.Id,
                    Role = Roles.Candidate,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await userManager.CreateAsync(zensarCandidateUser, defaultPassword);
                await userManager.AddToRoleAsync(zensarCandidateUser, Roles.Candidate);
            }

            // -------------------------------------------------------------
            // STEP 3: SEED DEPARTMENTS (15)
            // -------------------------------------------------------------
            var deptNames = new[] 
            { 
                "Advanced Engineering Services", "Digital Experience", "Cloud & Infrastructure", 
                "Data Engineering", "Cyber Security", "Quality Engineering", "Enterprise Applications", 
                "SAP Services", "Microsoft Practice", "Customer Success", "Human Resources", 
                "Finance", "Corporate IT", "Sales", "Marketing" 
            };
            var departments = deptNames
                .Select(name => new Department { Id = Guid.NewGuid(), Name = name, CompanyId = company.Id, CreatedBy = "System" })
                .ToList();
            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();

            // -------------------------------------------------------------
            // STEP 4: SEED RECRUITERS, HIRING MANAGERS, & INTERVIEWERS
            // -------------------------------------------------------------
            var rand = new Random(101); // Safe seed for repeatable demo data

            // 15 Recruiters
            var recruitersList = new List<ApplicationUser> { zensarRecruiter };
            var recruiterNames = new[] 
            {
                ("Priya", "Sharma"), ("Ankit", "Verma"), ("Neha", "Gupta"), ("Rahul", "Singh"),
                ("Aditi", "Mishra"), ("Rohit", "Saxena"), ("Karan", "Mehta"), ("Pooja", "Srivastava"),
                ("Abhishek", "Tiwari"), ("Shivani", "Agarwal"), ("Vikas", "Joshi"), ("Nidhi", "Patwardhan"),
                ("Kunal", "Sen"), ("Ritu", "Nair"), ("Manish", "Goel")
            };
            for (int i = 0; i < recruiterNames.Length; i++)
            {
                var email = $"{recruiterNames[i].Item1.ToLower()}.{recruiterNames[i].Item2.ToLower()}@zensar.com";
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = recruiterNames[i].Item1,
                    LastName = recruiterNames[i].Item2,
                    CompanyId = company.Id,
                    Role = Roles.Recruiter,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await userManager.CreateAsync(user, defaultPassword);
                await userManager.AddToRoleAsync(user, Roles.Recruiter);
                recruitersList.Add(user);
            }

            // 20 Hiring Managers
            var managersList = new List<ApplicationUser> { zensarManager };
            var managerNames = new[] 
            {
                ("Vivek", "Sharma"), ("Amit", "Srivastava"), ("Rajat", "Verma"), ("Anurag", "Gupta"),
                ("Pallavi", "Sharma"), ("Nitin", "Khanna"), ("Aparna", "Mishra"), ("Ruchi", "Agarwal"),
                ("Sandeep", "Patil"), ("Shalini", "Deshmukh"), ("Sameer", "Kulkarni"), ("Devendra", "Fadnavis"),
                ("Anand", "Mahindra"), ("Kiran", "Mazumdar"), ("Roshni", "Nadar"), ("CP", "Gurnani"),
                ("Salil", "Parekh"), ("Rajesh", "Gopinathan"), ("C", "Vijayakumar"), ("Thierry", "Delaporte")
            };
            for (int i = 0; i < managerNames.Length; i++)
            {
                var email = $"{managerNames[i].Item1.ToLower()}.{managerNames[i].Item2.ToLower()}@zensar.com";
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = managerNames[i].Item1,
                    LastName = managerNames[i].Item2,
                    CompanyId = company.Id,
                    Role = Roles.HiringManager,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await userManager.CreateAsync(user, defaultPassword);
                await userManager.AddToRoleAsync(user, Roles.HiringManager);
                managersList.Add(user);
            }

            // 50 Interviewers
            var interviewersList = new List<ApplicationUser> { zensarInterviewer };
            var interviewerFirstNames = new[] { "Saurabh", "Mayank", "Shubham", "Abhinav", "Nikhil", "Varun", "Ritika", "Akash", "Deepa", "Rohan", "Ajay", "Vikram", "Suresh", "Ramesh", "Manoj", "Vinod", "Harish", "Sanjay", "Alok", "Sunil", "Anil", "Tarun", "Arun", "Neeraj", "Gaurav", "Saurav", "Pawan", "Prabhat", "Prashant", "Pradeep", "Pankaj", "Piyush", "Puneet", "Pranav", "Prakash", "Madhav", "Mohan", "Krishna", "Raghav", "Madhukar", "Gopal", "Govind", "Babu", "Kiran", "Satish", "Dilip", "Sudhir", "Ashok", "Karthik" };
            var interviewerLastNames = new[] { "Mishra", "Gupta", "Tiwari", "Verma", "Sharma", "Singh", "Jain", "Pandey", "Patel", "Reddy", "Nair", "Joshi", "Kulkarni", "Deshmukh", "Choudhury", "Bose", "Dutta", "Sen", "Ghosh", "Roy", "Banerjee", "Chatterjee", "Das", "Rao", "Shetty", "Pillai", "Iyer", "Iyengar", "Menon", "Prasad", "Sinha", "Saxena", "Bhat", "Hegde", "Shenoy", "Pai", "Naik", "Sawant", "Kadam", "Shinde", "Patil", "Pawar", "Desai", "Mehta", "Shah", "Trivedi", "Vyas" };

            for (int i = 0; i < 49; i++)
            {
                var first = interviewerFirstNames[i % interviewerFirstNames.Length];
                var last = interviewerLastNames[i % interviewerLastNames.Length];
                var email = $"{first.ToLower()}.{last.ToLower()}{i}@zensar.com";
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = first,
                    LastName = last,
                    CompanyId = company.Id,
                    Role = Roles.Interviewer,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await userManager.CreateAsync(user, defaultPassword);
                await userManager.AddToRoleAsync(user, Roles.Interviewer);
                interviewersList.Add(user);
            }

            // -------------------------------------------------------------
            // STEP 5: SEED JOBS (75: 50 Published, 15 Draft, 10 Closed)
            // -------------------------------------------------------------
            var jobsList = new List<Job>();
            var jobTemplates = new[] 
            {
                "Senior Angular Developer", "Senior .NET Developer", "Full Stack Developer (.NET + Angular)", 
                "Cloud Engineer (Azure)", "DevOps Engineer", "Site Reliability Engineer", 
                "QA Automation Engineer", "SDET", "Technical Architect", "Solution Architect", 
                "Data Engineer", "Data Analyst", "Azure Data Engineer", "Power BI Developer", 
                "Business Analyst", "Project Manager", "Scrum Master", "Engineering Manager", 
                "Cyber Security Analyst", "SAP Consultant", "Microsoft Dynamics Developer", 
                "Support Engineer", "Service Delivery Manager"
            };
            var locations = new[] { "Pune", "Bangalore", "Hyderabad", "Chennai", "Mumbai", "Noida", "Kolkata", "Ahmedabad", "Nagpur", "Lucknow" };
            var ctcRanges = new[] 
            {
                (600000m, 900000m), (800000m, 1200000m), (1200000m, 1800000m), (1800000m, 2500000m), 
                (2500000m, 3500000m), (3500000m, 4500000m), (4500000m, 6000000m)
            };

            // Seed Job 1, 2, 3 for Himanshu's portfolio link
            var angularJob = new Job
            {
                Id = Guid.NewGuid(),
                Title = "Senior Angular Developer",
                Description = "<p>Join our Digital Experience practice as a Senior Angular Developer. You will drive front-end architecture and client delivery.</p>",
                Responsibilities = "Design components;Enforce style guides;Improve performance profiles.",
                Qualifications = "4+ years Angular;Modern reactive state management;Experience with CSS/SCSS.",
                DepartmentId = departments.First(d => d.Name == "Digital Experience").Id,
                HiringManagerId = zensarManager.Id,
                RecruiterId = zensarRecruiter.Id,
                Status = JobStatus.Published,
                Location = "Pune",
                EmploymentType = EmploymentType.FullTime,
                SalaryMin = 1200000,
                SalaryMax = 1800000,
                Currency = "USD",
                ExperienceYears = 5,
                CompanyId = company.Id,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow.AddDays(-60)
            };
            jobsList.Add(angularJob);

            var fullstackJob = new Job
            {
                Id = Guid.NewGuid(),
                Title = "Full Stack Developer (.NET + Angular)",
                Description = "<p>We are seeking a Full Stack Developer (.NET + Angular) to build scalable cloud-native products for Zensar's enterprise practice.</p>",
                Responsibilities = "Develop Web APIs;Write unit tests;Develop responsive UI templates.",
                Qualifications = "C# .NET Core;Angular 16+;SQL Server experience.",
                DepartmentId = departments.First(d => d.Name == "Advanced Engineering Services").Id,
                HiringManagerId = zensarManager.Id,
                RecruiterId = zensarRecruiter.Id,
                Status = JobStatus.Published,
                Location = "Bangalore",
                EmploymentType = EmploymentType.FullTime,
                SalaryMin = 1400000,
                SalaryMax = 2200000,
                Currency = "USD",
                ExperienceYears = 6,
                CompanyId = company.Id,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow.AddDays(-45)
            };
            jobsList.Add(fullstackJob);

            var azureJob = new Job
            {
                Id = Guid.NewGuid(),
                Title = "Cloud Engineer (Azure)",
                Description = "<p>Azure Cloud Architect / Cloud Engineer position to deploy robust infrastructure and manage automated CI/CD releases.</p>",
                Responsibilities = "Provision infrastructure via Terraform;Deploy AKS clusters;Manage Azure DevOps pipelines.",
                Qualifications = "Azure certification;Kubernetes experience;CI/CD automation tooling.",
                DepartmentId = departments.First(d => d.Name == "Cloud & Infrastructure").Id,
                HiringManagerId = zensarManager.Id,
                RecruiterId = zensarRecruiter.Id,
                Status = JobStatus.Published,
                Location = "Hyderabad",
                EmploymentType = EmploymentType.FullTime,
                SalaryMin = 1800000,
                SalaryMax = 2800000,
                Currency = "USD",
                ExperienceYears = 7,
                CompanyId = company.Id,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            };
            jobsList.Add(azureJob);

            // Generate remaining 72 jobs to make total 75
            for (int i = 3; i < 75; i++)
            {
                var title = jobTemplates[i % jobTemplates.Length];
                var dept = departments[i % departments.Count];
                var loc = locations[rand.Next(locations.Length)];
                var (salMin, salMax) = ctcRanges[rand.Next(ctcRanges.Length)];
                var experience = rand.Next(2, 12);

                JobStatus status;
                if (i < 47) status = JobStatus.Published; // Total 50 active
                else if (i < 62) status = JobStatus.Draft; // Total 15 draft
                else status = JobStatus.Closed; // Total 10 closed

                // Recruiter Portfolio Constraint: Himanshu.Recruiter owns exactly 15 jobs
                var recruiter = (i >= 3 && i < 15) ? zensarRecruiter : recruitersList[rand.Next(recruitersList.Count)];
                
                // Hiring Manager Portfolio Constraint: Himanshu.HiringManager owns exactly 8 open requisitions
                var manager = (status == JobStatus.Published && i >= 15 && i < 21) ? zensarManager : managersList[rand.Next(managersList.Count)];

                var job = new Job
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Description = $"<p>We are seeking a talented {title} to join our high-performing team. You will drive enterprise consulting and product delivery.</p>",
                    Responsibilities = "Develop scalable solutions;Mentor junior staff;Maintain quality code patterns.",
                    Qualifications = $"{experience}+ years relevant experience;Strong communication;Technical degree or equivalent.",
                    DepartmentId = dept.Id,
                    HiringManagerId = manager.Id,
                    RecruiterId = recruiter.Id,
                    Status = status,
                    Location = loc,
                    EmploymentType = EmploymentType.FullTime,
                    SalaryMin = salMin,
                    SalaryMax = salMax,
                    Currency = "USD",
                    ExperienceYears = experience,
                    CompanyId = company.Id,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-rand.Next(15, 200))
                };
                jobsList.Add(job);
            }
            await context.Jobs.AddRangeAsync(jobsList);
            await context.SaveChangesAsync();

            // Seed Job Skills
            var jobSkills = new List<JobSkill>();
            foreach (var job in jobsList)
            {
                var titleLower = job.Title.ToLower();
                var selectedSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (titleLower.Contains("angular") || titleLower.Contains("frontend") || titleLower.Contains("full stack"))
                {
                    selectedSkills.UnionWith(new[] { "Angular", "TypeScript", "JavaScript", "HTML", "CSS" });
                }
                if (titleLower.Contains(".net") || titleLower.Contains("c#") || titleLower.Contains("full stack"))
                {
                    selectedSkills.UnionWith(new[] { "C#", ".NET Core", "ASP.NET", "SQL Server" });
                }
                if (titleLower.Contains("cloud") || titleLower.Contains("devops") || titleLower.Contains("sre"))
                {
                    selectedSkills.UnionWith(new[] { "Azure", "AWS", "Docker", "Kubernetes", "CI/CD" });
                }
                if (titleLower.Contains("qa") || titleLower.Contains("sdet") || titleLower.Contains("automation"))
                {
                    selectedSkills.UnionWith(new[] { "QA", "Testing", "Automation", "C#", "TypeScript" });
                }
                if (titleLower.Contains("data"))
                {
                    selectedSkills.UnionWith(new[] { "Python", "SQL Server", "Power BI", "Data Engineering" });
                }
                if (titleLower.Contains("project manager") || titleLower.Contains("scrum") || titleLower.Contains("product"))
                {
                    selectedSkills.UnionWith(new[] { "Agile", "Scrum", "SaaS" });
                }
                if (!selectedSkills.Any())
                {
                    selectedSkills.UnionWith(new[] { "Git", "GitHub", "REST APIs" });
                }
                foreach (var sk in selectedSkills)
                {
                    jobSkills.Add(new JobSkill { JobId = job.Id, Name = sk, CreatedBy = "System" });
                }
            }
            await context.JobSkills.AddRangeAsync(jobSkills);
            await context.SaveChangesAsync();

            // -------------------------------------------------------------
            // STEP 6: SEED CANDIDATES (750)
            // -------------------------------------------------------------
            var candidatesList = new List<Candidate>();
            var candFirstNames = new[] { "Amit", "Rahul", "Priya", "Neha", "Vijay", "Anjali", "Sanjay", "Sunita", "Deepak", "Aarav", "Reyansh", "Vivaan", "Aditya", "Ishaan", "Kabeer", "Arjun", "Sai", "Aadhya", "Ananya", "Diya", "Pari", "Pihu", "Riya", "Kavya", "Aarohi", "Vikram", "Rajesh", "Kiran", "Suresh", "Manoj", "Jaya", "Priyanka", "Preeti", "Divya", "Swati", "Nikhil", "Gaurav", "Saurav", "Ashish", "Harish", "Rohit", "Tarun", "Arun", "Jatin", "Kunal", "Meera", "Aditi", "Pooja", "Shweta", "Kriti" };
            var candLastNames = new[] { "Sharma", "Verma", "Gupta", "Singh", "Mishra", "Srivastava", "Agarwal", "Tiwari", "Patel", "Reddy", "Nair", "Kulkarni", "Deshmukh", "Choudhury", "Bose", "Dutta", "Sen", "Ghosh", "Roy", "Banerjee", "Chatterjee", "Das", "Rao", "Shetty", "Pillai", "Iyer", "Prasad", "Sinha", "Saxena", "Mehta", "Shah", "Trivedi", "Vyas", "Joshi", "Bhat", "Hegde", "Pai", "Naik", "Sawant", "Kadam", "Shinde", "Pawar", "Desai", "Dubey", "Pandey", "Tripathi", "Shukla", "Mishra", "Dwivedi", "Misra" };
            var indianITCompanies = new[] { "TCS", "Infosys", "Wipro", "HCLTech", "Tech Mahindra", "Accenture", "Capgemini", "IBM India", "Persistent", "LTIMindtree", "Cognizant", "Birlasoft", "Hexaware", "Mphasis", "Oracle India", "Microsoft India", "Amazon India", "Flipkart", "PhonePe", "Paytm", "Razorpay", "Freshworks", "Zoho" };
            var rolesPool = new[] { "Software Engineer", "Senior Developer", "Frontend Developer", "Backend Developer", "Product Manager", "Data Analyst", "QA Engineer", "DevOps Engineer", "Designer", "System Architect" };
            var candLocations = new[] { "Pune", "Bangalore", "Hyderabad", "Chennai", "Mumbai", "Delhi", "Kolkata", "Ahmedabad" };

            // Create Himanshu.Candidate
            var testCandidate = new Candidate
            {
                Id = Guid.NewGuid(),
                FirstName = "Himanshu",
                LastName = "Candidate",
                Email = "himanshu.candidate@zensar.com",
                Phone = "+919876543210",
                LinkedInUrl = "https://linkedin.com/in/himanshu-candidate",
                GitHubUrl = "https://github.com/himanshucandidate",
                PortfolioUrl = "https://himanshu.dev",
                CompanyId = company.Id,
                ResumePath = "uploads/resumes/himanshu_candidate_resume.pdf",
                Source = "Organic",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            };
            candidatesList.Add(testCandidate);

            for (int i = 1; i < 750; i++)
            {
                var first = candFirstNames[rand.Next(candFirstNames.Length)];
                var last = candLastNames[rand.Next(candLastNames.Length)];
                var email = $"{first.ToLower()}.{last.ToLower()}{i}@example.com";
                var phone = $"+9198765{rand.Next(10000, 99999)}";
                
                var cand = new Candidate
                {
                    Id = Guid.NewGuid(),
                    FirstName = first,
                    LastName = last,
                    Email = email,
                    Phone = phone,
                    LinkedInUrl = $"https://linkedin.com/in/{first.ToLower()}{last.ToLower()}{i}",
                    GitHubUrl = $"https://github.com/{first.ToLower()}{last.ToLower()}{i}",
                    PortfolioUrl = $"https://{first.ToLower()}{last.ToLower()}{i}.dev",
                    CompanyId = company.Id,
                    ResumePath = $"uploads/resumes/{first.ToLower()}_{last.ToLower()}_{i}_resume.pdf",
                    Source = rand.NextDouble() < 0.6 ? "LinkedIn" : (rand.NextDouble() < 0.8 ? "Referral" : "Organic"),
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-rand.Next(5, 180))
                };
                candidatesList.Add(cand);
            }
            await context.Candidates.AddRangeAsync(candidatesList);
            await context.SaveChangesAsync();

            // Candidate Skills & Parsing Results
            var candSkills = new List<CandidateSkill>();
            var parsingResults = new List<ResumeParsingResult>();
            var skillsPool = new[] { "Angular", "React", ".NET Core", "C#", "Azure", "AWS", "Docker", "Kubernetes", "SQL Server", "Oracle", "Node.js", "TypeScript", "Java", "Spring Boot", "Python", "Power BI", "Microservices", "REST APIs" };

            foreach (var cand in candidatesList)
            {
                var skillsCount = rand.Next(3, 8);
                var selectedSkills = skillsPool.OrderBy(x => rand.Next()).Take(skillsCount).ToList();
                foreach (var sk in selectedSkills)
                {
                    candSkills.Add(new CandidateSkill { CandidateId = cand.Id, Name = sk, CreatedBy = "System" });
                }

                // Parse details
                var curCompany = indianITCompanies[rand.Next(indianITCompanies.Length)];
                var curRole = rolesPool[rand.Next(rolesPool.Length)];
                var expYears = rand.Next(1, 15);
                var currentCtc = rand.Next(4, 18) * 100000;
                var expectedCtc = currentCtc + rand.Next(2, 12) * 100000;
                var noticePeriod = rand.Next(1, 4) * 30; // 30, 60, 90 days

                if (cand.Email == "himanshu.candidate@zensar.com")
                {
                    curCompany = "Cognizant";
                    curRole = "Senior Angular Developer";
                    expYears = 5;
                    currentCtc = 1000000;
                    expectedCtc = 1500000;
                    noticePeriod = 30;
                }

                cand.YearsOfExperience = expYears;
                cand.ExpectedSalary = (decimal)expectedCtc;

                var parsedDto = new ATS.Application.DTOs.AI.ResumeParsingResultDto
                {
                    Name = $"{cand.FirstName} {cand.LastName}",
                    Email = cand.Email,
                    Phone = cand.Phone,
                    Skills = selectedSkills,
                    Education = "B.Tech in Computer Science & Engineering",
                    Experience = $"Current Company: {curCompany}\nCurrent Role: {curRole}\nNotice Period: {noticePeriod} Days\nCurrent CTC: INR {currentCtc / 100000} LPA\nExpected CTC: INR {expectedCtc / 100000} LPA\nExperience: {expYears} Years",
                    ConfidenceScore = 0.95m,
                    CurrentTitle = curRole,
                    YearsOfExperience = $"{expYears} yrs"
                };

                parsingResults.Add(new ResumeParsingResult
                {
                    CandidateId = cand.Id,
                    RawText = parsedDto.Experience + "\nSkills: " + string.Join(", ", selectedSkills),
                    ConfidenceScore = 0.95m,
                    ParsedDataJson = System.Text.Json.JsonSerializer.Serialize(parsedDto)
                });
            }
            await context.CandidateSkills.AddRangeAsync(candSkills);
            await context.ResumeParsingResults.AddRangeAsync(parsingResults);
            await context.SaveChangesAsync();

            // -------------------------------------------------------------
            // STEP 7: SEED APPLICATIONS (1500)
            // -------------------------------------------------------------
            var applicationsList = new List<ATS.Domain.Entities.Application>();
            var stageHistories = new List<ApplicationStage>();
            var aiScoresList = new List<AIScore>();

            var stageNames = new[] 
            {
                Stages.Applied, Stages.Screening, Stages.RecruiterReview, Stages.HiringManagerReview, 
                Stages.TechnicalInterview, Stages.HRInterview, Stages.FinalInterview, Stages.Offer, 
                Stages.Hired, Stages.Rejected
            };

            // Seed Applications for Himanshu.Candidate@zensar.com
            // App 1: Senior Angular Developer (Stage: Technical Interview)
            var app1 = new ATS.Domain.Entities.Application
            {
                Id = Guid.NewGuid(),
                JobId = angularJob.Id,
                CandidateId = testCandidate.Id,
                CurrentStage = Stages.TechnicalInterview,
                Status = "Active",
                CreatedDate = DateTime.UtcNow.AddDays(-15)
            };
            applicationsList.Add(app1);

            // App 2: Full Stack Developer (Stage: HR Interview)
            var app2 = new ATS.Domain.Entities.Application
            {
                Id = Guid.NewGuid(),
                JobId = fullstackJob.Id,
                CandidateId = testCandidate.Id,
                CurrentStage = Stages.HRInterview,
                Status = "Active",
                CreatedDate = DateTime.UtcNow.AddDays(-20)
            };
            applicationsList.Add(app2);

            // App 3: Cloud Engineer (Azure) (Stage: Offer)
            var app3 = new ATS.Domain.Entities.Application
            {
                Id = Guid.NewGuid(),
                JobId = azureJob.Id,
                CandidateId = testCandidate.Id,
                CurrentStage = Stages.Offer,
                Status = "Active",
                CreatedDate = DateTime.UtcNow.AddDays(-25)
            };
            applicationsList.Add(app3);

            // Map stages history for test candidate
            var appStagesMap = new Dictionary<ATS.Domain.Entities.Application, string[]>
            {
                { app1, new[] { Stages.Applied, Stages.Screening, Stages.RecruiterReview, Stages.TechnicalInterview } },
                { app2, new[] { Stages.Applied, Stages.Screening, Stages.RecruiterReview, Stages.HiringManagerReview, Stages.TechnicalInterview, Stages.HRInterview } },
                { app3, new[] { Stages.Applied, Stages.Screening, Stages.RecruiterReview, Stages.HiringManagerReview, Stages.TechnicalInterview, Stages.HRInterview, Stages.FinalInterview, Stages.Offer } }
            };

            foreach (var kvp in appStagesMap)
            {
                for (int idx = 0; idx < kvp.Value.Length; idx++)
                {
                    var isLast = idx == kvp.Value.Length - 1;
                    stageHistories.Add(new ApplicationStage
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = kvp.Key.Id,
                        StageName = kvp.Value[idx],
                        Status = isLast ? "Current" : "Completed",
                        SequenceNumber = idx,
                        EnteredDate = kvp.Key.CreatedDate.AddDays(idx * 2),
                        LeftDate = isLast ? null : kvp.Key.CreatedDate.AddDays((idx + 1) * 2),
                        CreatedBy = "System"
                    });
                }

                // AI Match Scores
                aiScoresList.Add(new AIScore
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = kvp.Key.Id,
                    JobId = kvp.Key.JobId,
                    CandidateId = testCandidate.Id,
                    MatchScore = 88,
                    SkillMatchPercentage = 90,
                    ExperienceMatchPercentage = 85,
                    EducationMatchPercentage = 90,
                    MissingSkillsJson = "[\"Kubernetes\"]",
                    StrengthsJson = "[\"Expert frontend capability\", \"Experienced in .NET and Angular architecture\"]",
                    WeaknessesJson = "[\"Minor experience gaps in containerization\"]",
                    AIQuestionsJson = "[\"How do you handle state management in large Angular apps?\", \"Explain MediatR pipeline behaviors.\"]",
                    Recommendation = "Strong Fit",
                    AISummary = "Candidate aligns very well with required core tech stacks.",
                    CreatedBy = "System",
                    CreatedDate = kvp.Key.CreatedDate.AddHours(2)
                });
            }

            // Mapped counts for Recruiters & Managers portfolios
            int recOwnedJobsCount = 0;
            int mgrReviewsCount = 0;

            int candidateIndex = 1; // skip Himanshu.Candidate
            for (int i = 3; i < 1500; i++)
            {
                var candidate = candidatesList[candidateIndex];
                var job = jobsList[rand.Next(jobsList.Count)];
                candidateIndex = (candidateIndex + 1) % candidatesList.Count;

                // Unique candidate-job pair constraint
                if (applicationsList.Any(a => a.CandidateId == candidate.Id && a.JobId == job.Id))
                {
                    job = jobsList[(jobsList.IndexOf(job) + 1) % jobsList.Count];
                }

                string currentStage;
                string status = "Active";

                // Ensure Himanshu.Recruiter owns exactly 250 candidates' applications
                if (recOwnedJobsCount < 250 && job.RecruiterId != zensarRecruiter.Id)
                {
                    // override recruiter of the job to Himanshu
                    job.RecruiterId = zensarRecruiter.Id;
                    recOwnedJobsCount++;
                }

                // Stage distribution based on loop counter
                if (i < 400) currentStage = Stages.Applied;
                else if (i < 700) currentStage = Stages.Screening;
                else if (i < 800) currentStage = Stages.RecruiterReview;
                else if (i < 900) currentStage = Stages.HiringManagerReview;
                else if (i < 1100) currentStage = Stages.TechnicalInterview;
                else if (i < 1200) currentStage = Stages.HRInterview;
                else if (i < 1300) currentStage = Stages.FinalInterview;
                else if (i < 1380) currentStage = Stages.Offer;
                else if (i < 1440)
                {
                    currentStage = Stages.Hired;
                    status = "Hired";
                }
                else
                {
                    currentStage = Stages.Rejected;
                    status = "Rejected";
                }

                // Ensure Himanshu.HiringManager has exactly 40 pending reviews on his jobs
                if (job.HiringManagerId == zensarManager.Id && mgrReviewsCount < 40)
                {
                    if (currentStage != Stages.Screening && currentStage != Stages.RecruiterReview && currentStage != Stages.HiringManagerReview)
                    {
                        currentStage = Stages.HiringManagerReview;
                        status = "Active";
                    }
                    mgrReviewsCount++;
                }

                var app = new ATS.Domain.Entities.Application
                {
                    Id = Guid.NewGuid(),
                    JobId = job.Id,
                    CandidateId = candidate.Id,
                    CurrentStage = currentStage,
                    Status = status,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-rand.Next(5, 180))
                };
                applicationsList.Add(app);

                // Stage history setup
                int currentStageIdx = Array.IndexOf(stageNames, currentStage);
                if (currentStage == Stages.Rejected)
                {
                    currentStageIdx = rand.Next(1, 7);
                }

                for (int j = 0; j <= currentStageIdx; j++)
                {
                    var isLast = j == currentStageIdx;
                    stageHistories.Add(new ApplicationStage
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = app.Id,
                        StageName = stageNames[j],
                        Status = isLast ? "Current" : "Completed",
                        SequenceNumber = j,
                        EnteredDate = app.CreatedDate.AddDays(j * 3),
                        LeftDate = isLast ? null : app.CreatedDate.AddDays((j + 1) * 3),
                        CreatedBy = "System"
                    });
                }

                if (currentStage == Stages.Rejected)
                {
                    stageHistories.Add(new ApplicationStage
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = app.Id,
                        StageName = Stages.Rejected,
                        Status = "Current",
                        SequenceNumber = currentStageIdx + 1,
                        EnteredDate = app.CreatedDate.AddDays((currentStageIdx + 1) * 3),
                        LeftDate = null,
                        CreatedBy = "System"
                    });
                }

                // AI Match Scores
                if (rand.NextDouble() < 0.85)
                {
                    var score = rand.Next(60, 99);
                    aiScoresList.Add(new AIScore
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = app.Id,
                        JobId = job.Id,
                        CandidateId = candidate.Id,
                        MatchScore = score,
                        SkillMatchPercentage = score + rand.Next(-3, 3),
                        ExperienceMatchPercentage = score + rand.Next(-5, 2),
                        EducationMatchPercentage = score + rand.Next(-3, 3),
                        MissingSkillsJson = "[\"Azure Integration Services\", \"Docker\"]",
                        StrengthsJson = "[\"Expert technology stack overlap\", \"Good delivery portfolio description\"]",
                        WeaknessesJson = "[\"Notice period is relatively long\"]",
                        AIQuestionsJson = "[\"Explain your experience with microservices deployment on AKS.\", \"How do you approach API gateway configurations?\"]",
                        Recommendation = score > 85 ? "Strong Fit" : (score > 70 ? "Moderate Fit" : "Potential Fit"),
                        AISummary = "Candidate aligns strongly with Zensar engineering capability framework benchmarks.",
                        CreatedBy = "System",
                        CreatedDate = app.CreatedDate.AddHours(2)
                    });
                }
            }
            await context.Applications.AddRangeAsync(applicationsList);
            await context.ApplicationStages.AddRangeAsync(stageHistories);
            await context.AIScores.AddRangeAsync(aiScoresList);
            await context.SaveChangesAsync();

            // -------------------------------------------------------------
            // STEP 8: SEED INTERVIEWS (500) & FEEDBACK
            // -------------------------------------------------------------
            var interviewsList = new List<Interview>();
            var feedbacksList = new List<InterviewFeedback>();
            
            var roundNames = new[] { "Technical Screening", "Architect Interview", "Manager Fitment", "HR Discussion" };
            var advancedApps = applicationsList
                .Where(a => a.CurrentStage != Stages.Applied && a.CurrentStage != Stages.Rejected && a.CandidateId != testCandidate.Id)
                .ToList();

            // Seed Interview for Himanshu.Candidate@zensar.com
            var candidateInterview = new Interview
            {
                Id = Guid.NewGuid(),
                ApplicationId = app1.Id,
                InterviewerId = zensarInterviewer.Id,
                Title = "Technical Interview Round 1",
                Type = InterviewType.Technical,
                ScheduledTime = DateTime.UtcNow.AddDays(2),
                DurationMinutes = 60,
                VideoLink = "https://meet.google.com/himanshu-interview-link",
                Status = InterviewStatus.Scheduled,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            };
            interviewsList.Add(candidateInterview);

            // Counters for Himanshu's specific portfolios
            int recInterviewsCount = 0;
            int interviewerUpcomingCount = 0;
            int interviewerCompletedCount = 0;
            int interviewerPendingFeedbackCount = 0;

            for (int i = 1; i < 500; i++)
            {
                var app = advancedApps[i % advancedApps.Count];
                var job = jobsList.FirstOrDefault(j => j.Id == app.JobId);
                var interviewer = interviewersList[rand.Next(interviewersList.Count)];
                
                InterviewStatus status = InterviewStatus.Completed;
                DateTime schedTime = DateTime.UtcNow.AddDays(-rand.Next(1, 100));

                // Recruiter Portfolio Constraint: Himanshu.Recruiter owns exactly 80 interviews on his jobs
                if (job != null && job.RecruiterId == zensarRecruiter.Id && recInterviewsCount < 80)
                {
                    recInterviewsCount++;
                }

                // Interviewer Portfolio Constraints for Himanshu.Interviewer
                if (interviewerUpcomingCount < 25) // 25 upcoming scheduled
                {
                    interviewer = zensarInterviewer;
                    status = InterviewStatus.Scheduled;
                    schedTime = DateTime.UtcNow.AddDays(rand.Next(1, 10));
                    interviewerUpcomingCount++;
                }
                else if (interviewerCompletedCount < 40) // 40 completed
                {
                    interviewer = zensarInterviewer;
                    status = InterviewStatus.Completed;
                    schedTime = DateTime.UtcNow.AddDays(-rand.Next(1, 40));
                    interviewerCompletedCount++;
                }
                else if (interviewerPendingFeedbackCount < 10) // 10 pending feedback (Completed with NO feedback record)
                {
                    interviewer = zensarInterviewer;
                    status = InterviewStatus.Completed;
                    schedTime = DateTime.UtcNow.AddDays(-rand.Next(1, 15));
                    interviewerPendingFeedbackCount++;
                }
                else if (i % 20 == 0)
                {
                    status = InterviewStatus.Cancelled;
                }
                else if (i % 25 == 0)
                {
                    status = InterviewStatus.Rescheduled;
                }

                var roundTitle = roundNames[i % roundNames.Length];
                var type = InterviewType.Technical;
                if (roundTitle.Contains("Screening")) type = InterviewType.PhoneScreen;
                else if (roundTitle.Contains("HR") || roundTitle.Contains("Manager")) type = InterviewType.HR;

                var interview = new Interview
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = app.Id,
                    InterviewerId = interviewer.Id,
                    Title = roundTitle,
                    Type = type,
                    ScheduledTime = schedTime,
                    DurationMinutes = 45,
                    VideoLink = $"https://meet.google.com/zensar-mock-{i}",
                    Status = status,
                    CreatedBy = "System",
                    CreatedDate = app.CreatedDate.AddDays(1)
                };
                interviewsList.Add(interview);

                // Add Feedback if Completed, unless it is one of the 10 pending feedbacks
                if (status == InterviewStatus.Completed && (interviewer.Id != zensarInterviewer.Id || interviewerCompletedCount <= 40))
                {
                    feedbacksList.Add(new InterviewFeedback
                    {
                        Id = Guid.NewGuid(),
                        InterviewId = interview.Id,
                        InterviewerId = interviewer.Id,
                        CommunicationScore = rand.Next(3, 6),
                        ProblemSolvingScore = rand.Next(3, 6),
                        CodingScore = rand.Next(2, 6),
                        SystemDesignScore = rand.Next(2, 6),
                        CultureFitScore = rand.Next(3, 6),
                        FeedbackText = "Strong alignment with engineering capabilities. Handles system design tradeoffs well and communicates solutions clearly.",
                        Recommendation = (RecommendationType)rand.Next(0, 3), // Strong Hire, Hire, Neutral
                        SubmittedDate = schedTime.AddHours(1),
                        CreatedBy = "System",
                        CreatedDate = schedTime.AddHours(1)
                    });
                }
            }
            await context.Interviews.AddRangeAsync(interviewsList);
            await context.InterviewFeedbacks.AddRangeAsync(feedbacksList);
            await context.SaveChangesAsync();

            // -------------------------------------------------------------
            // STEP 9: SEED OFFERS (75)
            // -------------------------------------------------------------
            var offersList = new List<Offer>();

            // Seed Offer Pending for Himanshu.Candidate
            var candidateOffer = new Offer
            {
                Id = Guid.NewGuid(),
                ApplicationId = app3.Id,
                Salary = 2400000,
                StartDate = DateTime.UtcNow.AddDays(30),
                Status = OfferStatus.Sent,
                OfferLetterPath = "uploads/offers/himanshu_candidate_offer.pdf",
                OfferLetterContent = $"Dear Himanshu Candidate,\n\n" +
                    $"On behalf of Zensar Technologies, we are pleased to offer you the position of Cloud Engineer (Azure) within our Engineering department, located at our Pune, India office. We are extremely excited about the prospect of you joining our team!\n\n" +
                    $"Please find the detailed terms of your employment offer below:\n\n" +
                    $"- **Position**: Cloud Engineer (Azure)\n" +
                    $"- **Department**: Engineering\n" +
                    $"- **Location**: Pune, India\n" +
                    $"- **Annual Base Salary**: INR 2,400,000\n" +
                    $"- **Start Date**: {DateTime.UtcNow.AddDays(30):MMMM dd, yyyy}\n" +
                    $"- **Employment Status**: Full-Time\n\n" +
                    $"This offer is contingent upon the successful completion of standard background checks and reference validations.\n\n" +
                    $"To accept this offer, please review the document and type your full name in the signature field in the e-sign portal by {DateTime.UtcNow.AddDays(30):MMMM dd, yyyy}.\n\n" +
                    $"Best regards,\n\n" +
                    $"Himanshu Recruiter\n" +
                    $"Lead Recruiter / Hiring Coordinator\n" +
                    $"Zensar Technologies Talent Acquisition Team\n" +
                    $"Email: recruiter@zensar.com",
                ESignatureDetails = "",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            };
            offersList.Add(candidateOffer);

            var offerApps = applicationsList
                .Where(a => a.CurrentStage == Stages.Offer || a.CurrentStage == Stages.Hired || a.CurrentStage == Stages.Rejected)
                .Where(a => a.CandidateId != testCandidate.Id)
                .Take(74)
                .ToList();

            // Portfolio limits
            int recOffersCount = 0;
            int mgrOfferApprovalsCount = 0;

            for (int i = 0; i < offerApps.Count; i++)
            {
                var app = offerApps[i];
                var job = jobsList.FirstOrDefault(j => j.Id == app.JobId);
                var offeredCtc = rand.Next(8, 45) * 100000;
                
                OfferStatus status = OfferStatus.Sent;
                if (i < 15) status = OfferStatus.Accepted;
                else if (i < 30) status = OfferStatus.Approved;
                else if (i < 45) status = OfferStatus.Rejected;
                else if (i < 55) status = OfferStatus.Negotiating;
                else if (i < 65) status = OfferStatus.Draft;
                else status = OfferStatus.Withdrawn;

                // Recruiter Portfolio Constraint: Himanshu.Recruiter owns 20 offers
                if (job != null && job.RecruiterId == zensarRecruiter.Id && recOffersCount < 20)
                {
                    recOffersCount++;
                }

                // Hiring Manager Portfolio Constraint: Himanshu.HiringManager has 10 pending offer approvals
                if (job != null && job.HiringManagerId == zensarManager.Id && mgrOfferApprovalsCount < 10)
                {
                    status = OfferStatus.Draft;
                    mgrOfferApprovalsCount++;
                }

                var candidate = candidatesList.FirstOrDefault(c => c.Id == app.CandidateId);
                var candidateName = candidate != null ? $"{candidate.FirstName} {candidate.LastName}".Trim() : "Candidate";
                var companyName = company.Name;
                var jobTitle = job?.Title ?? "Software Professional";
                var sDate = DateTime.UtcNow.AddDays(rand.Next(15, 45));

                var recruiterUser = recruitersList.FirstOrDefault(r => r.Id == (job?.RecruiterId ?? zensarRecruiter.Id)) ?? zensarRecruiter;
                var recruiterName = $"{recruiterUser.FirstName} {recruiterUser.LastName}".Trim();
                var recruiterEmail = recruiterUser.Email ?? "recruitment@zensar.com";
                var department = departments.FirstOrDefault(d => d.Id == job?.DepartmentId);
                var departmentName = department?.Name ?? "Engineering";
                var location = job?.Location ?? "Remote";
                var currency = job?.Currency ?? "USD";
                var salaryFormatted = $"{currency} {offeredCtc:N0}";

                offersList.Add(new Offer
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = app.Id,
                    Salary = offeredCtc,
                    StartDate = sDate,
                    Status = status,
                    OfferLetterPath = $"uploads/offers/offer_{app.Id}.pdf",
                    OfferLetterContent = $"Dear {candidateName},\n\n" +
                        $"On behalf of {companyName}, we are pleased to offer you the position of {jobTitle} within our {departmentName} department, located at our {location} office. We are extremely excited about the prospect of you joining our team!\n\n" +
                        $"Please find the detailed terms of your employment offer below:\n\n" +
                        $"- **Position**: {jobTitle}\n" +
                        $"- **Department**: {departmentName}\n" +
                        $"- **Location**: {location}\n" +
                        $"- **Annual Base Salary**: {salaryFormatted}\n" +
                        $"- **Start Date**: {sDate:MMMM dd, yyyy}\n" +
                        $"- **Employment Status**: Full-Time\n\n" +
                        $"This offer is contingent upon the successful completion of standard background checks and reference validations.\n\n" +
                        $"To accept this offer, please review the document and type your full name in the signature field in the e-sign portal by {sDate:MMMM dd, yyyy}.\n\n" +
                        $"Best regards,\n\n" +
                        $"{recruiterName}\n" +
                        $"Lead Recruiter / Hiring Coordinator\n" +
                        $"{companyName} Talent Acquisition Team\n" +
                        $"Email: {recruiterEmail}",
                    ESignatureDetails = status == OfferStatus.Accepted ? "Signed via DocuSign" : "",
                    CreatedBy = "System",
                    CreatedDate = app.CreatedDate.AddDays(14)
                });
            }
            await context.Offers.AddRangeAsync(offersList);
            await context.SaveChangesAsync();

            // -------------------------------------------------------------
            // STEP 10: SEED ACTIVITY LOGS (10,000) & BATCH THEM
            // -------------------------------------------------------------
            Console.WriteLine("Generating 10,000 activity logs (this may take a few seconds)...");
            var activityLogsList = new List<ActivityLog>();
            var actionsPool = new[] { "Apply", "Stage Change", "Schedule Interview", "Interview Completed", "Generate Offer", "System Audit" };
            
            // Loop through all stage histories to create realistic chronological activities
            foreach (var hist in stageHistories)
            {
                var app = applicationsList.FirstOrDefault(a => a.Id == hist.ApplicationId);
                if (app == null) continue;

                string action = "Stage Change";
                string details = $"Moved candidate to {hist.StageName}.";
                if (hist.StageName == Stages.Applied)
                {
                    action = "Apply";
                    details = "Candidate applied online.";
                }
                else if (hist.StageName == Stages.Hired)
                {
                    action = "Stage Change";
                    details = "Candidate application completed. Marked as Hired.";
                }

                activityLogsList.Add(new ActivityLog
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = hist.ApplicationId,
                    CandidateId = app.CandidateId,
                    Action = action,
                    Details = details,
                    PerformedBy = recruitersList[rand.Next(recruitersList.Count)].FirstName + " " + recruitersList[rand.Next(recruitersList.Count)].LastName,
                    CreatedBy = "System",
                    CreatedDate = hist.EnteredDate
                });
            }

            // Fill up remaining log items to reach exactly 10,000 items
            var remainingLogs = 10000 - activityLogsList.Count;
            for (int i = 0; i < remainingLogs; i++)
            {
                var app = applicationsList[rand.Next(applicationsList.Count)];
                var action = actionsPool[rand.Next(actionsPool.Length)];
                
                activityLogsList.Add(new ActivityLog
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = app.Id,
                    CandidateId = app.CandidateId,
                    Action = action,
                    Details = $"Standard activity log '{action}' executed in workspace background.",
                    PerformedBy = "System",
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow.AddDays(-rand.Next(1, 350))
                });
            }

            // Batch Save in chunks of 2,000 for high performance
            const int batchSize = 2000;
            for (int i = 0; i < activityLogsList.Count; i += batchSize)
            {
                var batch = activityLogsList.Skip(i).Take(batchSize).ToList();
                await context.ActivityLogs.AddRangeAsync(batch);
                await context.SaveChangesAsync();
            }

            // -------------------------------------------------------------
            // STEP 11: SEED EMAIL TEMPLATES & NOTIFICATIONS
            // -------------------------------------------------------------
            var template1 = new EmailTemplate
            {
                CompanyId = company.Id,
                Name = "Application Received",
                Subject = "Zensar Technologies - Application Received",
                Body = "<p>Hi {{CandidateName}},</p><p>We have successfully received your application for the <strong>{{JobTitle}}</strong> role.</p><p>Regards,<br/>Talent Acquisition Team</p>",
                TriggerEvent = "ApplicationReceived",
                CreatedBy = "System"
            };
            var template2 = new EmailTemplate
            {
                CompanyId = company.Id,
                Name = "Interview Invitation",
                Subject = "Zensar Technologies - Interview Scheduled",
                Body = "<p>Hi {{CandidateName}},</p><p>Your interview round has been scheduled for the position of <strong>{{JobTitle}}</strong>.</p><p>Regards,<br/>Recruiting Team</p>",
                TriggerEvent = "InterviewScheduled",
                CreatedBy = "System"
            };
            await context.EmailTemplates.AddRangeAsync(template1, template2);
            await context.SaveChangesAsync();

            // Seed notifications for test users
            var notificationsList = new List<Notification>();
            var targetUsers = new[] { zensarAdmin, zensarRecruiter, zensarManager, zensarInterviewer };
            foreach (var user in targetUsers)
            {
                for (int i = 0; i < 5; i++)
                {
                    notificationsList.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Title = i switch 
                        {
                            0 => "Interview Reminder",
                            1 => "Feedback Pending",
                            2 => "Offer Approval Needed",
                            3 => "New Candidate Application",
                            _ => "System Status Alert"
                        },
                        Message = i switch 
                        {
                            0 => "Upcoming technical panel interview scheduled in 30 minutes.",
                            1 => "Candidate interview completed. Please submit your rating scorecard.",
                            2 => "Salary offer package is pending your review and authorization.",
                            3 => "A new candidate applied for Senior Angular Developer.",
                            _ => "System optimization task completed. All services fully operational."
                        },
                        IsRead = i >= 3,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow.AddMinutes(-15 * i)
                    });
                }
            }
            await context.Notifications.AddRangeAsync(notificationsList);
            await context.SaveChangesAsync();

            // Seed InterviewQuestionTemplates
            var questionTemplates = new List<InterviewQuestionTemplate>
            {
                new InterviewQuestionTemplate { SkillName = "Angular", Category = "Technical", Question = "Can you describe the differences between Angular Signals and RxJS Observables in Angular 19+?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = "Angular", Category = "Technical", Question = "How do you manage state and optimize rendering performance in complex Angular dashboards?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = "Angular", Category = "Behavioral", Question = "Describe a time when you had to optimize a slow Angular app. What profiling tools did you use?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = ".NET Core", Category = "Technical", Question = "How does EF Core handle tracking query lifecycle, and when would you use AsNoTracking?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = ".NET Core", Category = "Technical", Question = "Can you explain how MediatR pipeline behaviors work for global cross-cutting concerns like validation?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = "C#", Category = "Technical", Question = "What are the differences between Task.Run and async-await in CPU-bound vs IO-bound tasks?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = "C#", Category = "Technical", Question = "How do memory management and garbage collection work in .NET? What is LOH?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = "SQL Server", Category = "Technical", Question = "What are the key differences between SQL Server indexing types (clustered vs non-clustered)?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = "SQL Server", Category = "Technical", Question = "How do you identify and resolve deadlock issues in high-concurrency SQL Server workloads?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = "Docker", Category = "FollowUp", Question = "In your resume, you mentioned building cloud-native enterprise products. What containerization security practices did you enforce?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = "Kubernetes", Category = "FollowUp", Question = "How do you set resources requests and limits in Kubernetes pods, and what happens when limits are exceeded?", CreatedBy = "System" },
                new InterviewQuestionTemplate { SkillName = "Redis", Category = "FollowUp", Question = "You listed experience with Redis caching. How do you handle cache invalidation and race conditions in concurrent user scenarios?", CreatedBy = "System" }
            };
            await context.InterviewQuestionTemplates.AddRangeAsync(questionTemplates);
            await context.SaveChangesAsync();

            // -------------------------------------------------------------
            // STEP 12: VALIDATION REPORT
            // -------------------------------------------------------------
            var finalJobsCount = await context.Jobs.CountAsync();
            var finalCandidatesCount = await context.Candidates.CountAsync();
            var finalAppsCount = await context.Applications.CountAsync();
            var finalInterviewsCount = await context.Interviews.CountAsync();
            var finalFeedbacksCount = await context.InterviewFeedbacks.CountAsync();
            var finalOffersCount = await context.Offers.CountAsync();
            var finalLogsCount = await context.ActivityLogs.CountAsync();

            var report = $@"
======================================================================
               ZENSAR TECHNOLOGIES ATS DATABASE SEED REPORT
======================================================================
- Primary Tenant:    Zensar Technologies
- Seeded Departments: {departments.Count}
- Seeded Recruiters:  {recruitersList.Count}
- Seeded Managers:    {managersList.Count}
- Seeded Interviewers:{interviewersList.Count}
- Total Jobs:         {finalJobsCount} (50 Published, 15 Draft, 10 Closed)
- Total Candidates:   {finalCandidatesCount}
- Total Applications: {finalAppsCount}
- Total Interviews:   {finalInterviewsCount}
- Scorecard Feedbacks:{finalFeedbacksCount}
- Total Offers:       {finalOffersCount}
- Activity Logs:      {finalLogsCount}
======================================================================
";
            Console.WriteLine(report);
        }
    }
}
