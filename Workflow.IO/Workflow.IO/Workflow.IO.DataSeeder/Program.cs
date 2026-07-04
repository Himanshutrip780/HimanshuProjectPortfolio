using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

// Entity namespaces
using AuthEnt = JwtAuthenticationManager.Model;
using UserEnt = UserApi.Model.Domian.Entities;
using ProjEnt = ProjectApi.Model.Domain.Entities;
using TaskEnt = TaskApi.Model.Domain.Entities;
using CommEnt = CommentApi.Model.Domain.Entities;
using FileEnt = FileApi.Model.Domain.Entities;
using NotiEnt = NotificationApi.Model.Domain.Entities;
using ActiEnt = ActivityApi.Model.Domain.Entities;
using AnalEnt = AnalyticsApi.Model.Domain.Entities;

using UserApi.Model.Domian;
using ProjectApi.Model.Domain.Enums;
using TaskApi.Model.Domain.Enums;
using TaskStatus = TaskApi.Model.Domain.Enums.TaskStatus;


namespace Workflow.IO.DataSeeder
{
    class Program
    {
        static T CreateEntity<T>(Dictionary<string, object> props)
        {
            var entity = (T)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
            foreach (var p in props)
            {
                var prop = typeof(T).GetProperty(p.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (prop != null && prop.CanWrite && p.Value != null)
                {
                    prop.SetValue(entity, p.Value);
                }
            }
            return entity;
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting massive integrated database seeding for Zensar Technologies...");

            string baseConnStr = "Host=localhost;Port=5432;Username=postgres;Password=postgres;";
            
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                baseConnStr = args[0];
                Console.WriteLine("Using provided connection string for all contexts...");
            }

            string userDbStr = args.Length > 0 ? baseConnStr : baseConnStr + "Database=Workflow.IOUserDb";
            string projDbStr = args.Length > 0 ? baseConnStr : baseConnStr + "Database=Workflow.IOProjectDb";
            string taskDbStr = args.Length > 0 ? baseConnStr : baseConnStr + "Database=Workflow.IOTaskDb";
            string commDbStr = args.Length > 0 ? baseConnStr : baseConnStr + "Database=Workflow.IOCommentDb";
            string fileDbStr = args.Length > 0 ? baseConnStr : baseConnStr + "Database=Workflow.IOFileDb";
            string notiDbStr = args.Length > 0 ? baseConnStr : baseConnStr + "Database=Workflow.IONotificationDb";
            string actiDbStr = args.Length > 0 ? baseConnStr : baseConnStr + "Database=Workflow.IOActivityDb";
            string analDbStr = args.Length > 0 ? baseConnStr : baseConnStr + "Database=Workflow.IOAnalyticsDb";

            var authOptions = new DbContextOptionsBuilder<JwtAuthenticationManager.Data.AuthDbContext>().UseNpgsql(userDbStr).Options;
            var userOptions = new DbContextOptionsBuilder<UserApi.Data.UserDbContext>().UseNpgsql(userDbStr).Options;
            var projOptions = new DbContextOptionsBuilder<ProjectApi.Data.ProjectDbContext>().UseNpgsql(projDbStr).Options;
            var taskOptions = new DbContextOptionsBuilder<TaskApi.Data.TaskDbContext>().UseNpgsql(taskDbStr).Options;
            var commOptions = new DbContextOptionsBuilder<CommentApi.Data.CommentDbContext>().UseNpgsql(commDbStr).Options;
            var fileOptions = new DbContextOptionsBuilder<FileApi.Data.FileDbContext>().UseNpgsql(fileDbStr).Options;
            var notiOptions = new DbContextOptionsBuilder<NotificationApi.Data.NotificationDbContext>().UseNpgsql(notiDbStr).Options;
            var actiOptions = new DbContextOptionsBuilder<ActivityApi.Data.ActivityDbContext>().UseNpgsql(actiDbStr).Options;
            var analOptions = new DbContextOptionsBuilder<AnalyticsApi.Data.AnalyticsDbContext>().UseNpgsql(analDbStr).Options;

            var tenantContext = new Workflow.IO.Shared.Contracts.TenantContext();
            
            using var authDb = new JwtAuthenticationManager.Data.AuthDbContext(authOptions);
            using var userDb = new UserApi.Data.UserDbContext(userOptions, tenantContext);
            using var projDb = new ProjectApi.Data.ProjectDbContext(projOptions, tenantContext);
            using var taskDb = new TaskApi.Data.TaskDbContext(taskOptions, tenantContext);
            using var commDb = new CommentApi.Data.CommentDbContext(commOptions);
            using var fileDb = new FileApi.Data.FileDbContext(fileOptions);
            using var notiDb = new NotificationApi.Data.NotificationDbContext(notiOptions);
            using var actiDb = new ActivityApi.Data.ActivityDbContext(actiOptions);
            using var analDb = new AnalyticsApi.Data.AnalyticsDbContext(analOptions);

            Console.WriteLine("1. Truncating all tables in microservices databases...");
            await TruncateDatabases(authDb, userDb, projDb, taskDb, commDb, fileDb, notiDb, actiDb, analDb);

            Console.WriteLine("2. Generating Users & Zensar Organization...");
            var data = await GenerateUsersAndOrganizations(authDb, userDb);
            var users = data.Users;
            var primaryOrg = data.Organizations.First(o => o.Name == "Zensar Technologies");

            Console.WriteLine("3. Generating Projects, Clients, and Project Memberships...");
            var projects = await GenerateProjects(projDb, users, primaryOrg.OrganizationId);

            Console.WriteLine("4. Generating Teams, Sprints, Epics, and Tasks...");
            var tasks = await GenerateTeamsSprintsEpicsAndTasks(taskDb, projects, users, primaryOrg.OrganizationId);

            Console.WriteLine("5. Generating Comments & User Mentions...");
            await GenerateComments(commDb, tasks, users);

            Console.WriteLine("6. Generating File Attachments...");
            await GenerateFiles(fileDb, tasks, users);

            Console.WriteLine("7. Generating Analytics & Activities...");
            await GenerateAnalyticsAndActivities(analDb, actiDb, tasks, users);
            
            Console.WriteLine("8. Generating Notifications...");
            await GenerateNotifications(notiDb, tasks, users);

            Console.WriteLine("Seeding completed successfully! All tables are populated and logically integrated.");
        }

        static async Task TruncateDatabases(params DbContext[] contexts)
        {
            foreach (var ctx in contexts)
            {
                var conn = ctx.Database.GetDbConnection();
                Console.WriteLine($"  Connecting to {conn.Database}...");
                await conn.OpenAsync();
                Console.WriteLine($"  Connected! Truncating {conn.Database}...");
                
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT table_schema, table_name 
                    FROM information_schema.tables 
                    WHERE table_type = 'BASE TABLE' 
                      AND table_schema NOT IN ('pg_catalog', 'information_schema', 'pg_toast')
                      AND table_name != '__EFMigrationsHistory';";

                var tables = new List<(string Schema, string Name)>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tables.Add((reader.GetString(0), reader.GetString(1)));
                    }
                }

                foreach (var table in tables)
                {
                    if (table.Name.Equals("schema_migrations", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    try
                    {
                        using var execCmd = conn.CreateCommand();
                        execCmd.CommandText = $@"TRUNCATE TABLE ""{table.Schema}"".""{table.Name}"" RESTART IDENTITY CASCADE;";
                        await execCmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Skipping truncate for \"{table.Schema}\".\"{table.Name}\": {ex.Message}");
                    }
                }
                
                await conn.CloseAsync();
            }
        }

        static async Task<(List<UserEnt.User> Users, List<UserEnt.Organization> Organizations)> GenerateUsersAndOrganizations(JwtAuthenticationManager.Data.AuthDbContext authDb, UserApi.Data.UserDbContext userDb)
        {
            var generatedUsers = new List<UserEnt.User>();
            var generatedOrgs = new List<UserEnt.Organization>();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

            var zensarOrg = CreateEntity<UserEnt.Organization>(new Dictionary<string, object> {
                { "OrganizationId", Guid.Parse("a04be361-9c60-4965-b771-36b124806a6b") },
                { "Name", "Zensar Technologies" },
                { "Subdomain", "workflow" },
                { "SubscriptionTier", "Enterprise" },
                { "CreatedAt", DateTime.UtcNow.AddYears(-2) }
            });
            userDb.Organizations.Add(zensarOrg);
            generatedOrgs.Add(zensarOrg);

            var coreUsers = new List<(Guid Id, string First, string Last, UserRole Role, string Email)>
            {
                (Guid.Parse("11111111-1111-1111-1111-111111111111"), "Himanshu", "Tripathi", UserRole.Admin, "himanshu.tripathi@zensar.com"),
                (Guid.Parse("f0000000-0000-0000-0000-000000000001"), "Himanshu", "PM", UserRole.User, "himanshu.pm@zensar.com"),
                (Guid.Parse("f0000000-0000-0000-0000-000000000002"), "Himanshu", "PO", UserRole.User, "himanshu.po@zensar.com"),
                (Guid.Parse("f0000000-0000-0000-0000-000000000003"), "Himanshu", "BA", UserRole.User, "himanshu.ba@zensar.com"),
                (Guid.Parse("f0000000-0000-0000-0000-000000000004"), "Himanshu", "Dev", UserRole.User, "himanshu.dev@zensar.com"),
                (Guid.Parse("f0000000-0000-0000-0000-000000000005"), "Himanshu", "QA", UserRole.User, "himanshu.qa@zensar.com"),
                (Guid.Parse("f0000000-0000-0000-0000-000000000006"), "Himanshu", "DevOps", UserRole.User, "himanshu.devops@zensar.com"),
                (Guid.Parse("f0000000-0000-0000-0000-000000000007"), "Himanshu", "Support", UserRole.User, "himanshu.support@zensar.com"),
                (Guid.Parse("22222222-2222-2222-2222-222222222222"), "Anuja", "Kulkarni", UserRole.Admin, "anuja.kulkarni@zensar.com"),
                (Guid.Parse("33333333-3333-3333-3333-333333333333"), "Prabhat", "Ranjan", UserRole.Admin, "prabhat.ranjan@zensar.com"),
                (Guid.Parse("44444444-4444-4444-4444-444444444444"), "Deepak", "Kakkar", UserRole.User, "deepak.kakkar@zensar.com"),
                (Guid.Parse("55555555-5555-5555-5555-555555555555"), "Divya", "Raghavaraju", UserRole.User, "divya.raghavaraju@zensar.com"),
                (Guid.Parse("66666666-6666-6666-6666-666666666666"), "Nishanth", "Uppuleti", UserRole.User, "nishanth.uppuleti@zensar.com"),
                (Guid.Parse("77777777-7777-7777-7777-777777777777"), "Poonam", "Nerkar", UserRole.User, "poonam.nerkar@zensar.com"),
                (Guid.Parse("88888888-8888-8888-8888-888888888888"), "Rahul Krishnat", "Kamble", UserRole.User, "rahul.kamble@zensar.com"),
                (Guid.Parse("99999999-9999-9999-9999-999999999999"), "Sakshi", "Khandelwal", UserRole.User, "sakshi.khandelwal@zensar.com"),
                (Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Sumit", "Thote", UserRole.User, "sumit.thote@zensar.com"),
                (Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Vaishnavi", "Gadewar", UserRole.User, "vaishnavi.gadewar@zensar.com"),
                (Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Vijay", "Pawar", UserRole.User, "vijay.pawar@zensar.com"),
                (Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Vijay", "Rajagopalan", UserRole.User, "vijay.rajagopalan@zensar.com"),
                (Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Vishakha Vilas", "Khair", UserRole.User, "vishakha.khair@zensar.com"),
                (Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Vidhi S.", "Puri", UserRole.User, "vidhi.puri@zensar.com")
            };

            foreach (var u in coreUsers)
            {
                var account = CreateEntity<AuthEnt.UserAccount>(new Dictionary<string, object> {
                    { "Id", u.Id },
                    { "Email", u.Email },
                    { "PasswordHash", passwordHash },
                    { "Role", u.Role },
                    { "IsActive", true },
                    { "CreatedAt", DateTime.UtcNow.AddMonths(-12) },
                    { "UpdatedAt", DateTime.UtcNow }
                });
                authDb.UserAccounts.Add(account);

                var profile = CreateEntity<UserEnt.User>(new Dictionary<string, object> {
                    { "UserId", u.Id },
                    { "FirstName", u.First },
                    { "LastName", u.Last },
                    { "AvatarUrl", $"https://api.dicebear.com/7.x/avataaars/svg?seed={u.First}" },
                    { "Status", UserApi.Model.Domian.Common.UserStatus.Active },
                    { "CreatedAt", account.CreatedAt },
                    { "UpdatedAt", DateTime.UtcNow },
                    { "IsDeleted", false },
                    { "OrganizationId", zensarOrg.OrganizationId }
                });
                userDb.Users.Add(profile);
                generatedUsers.Add(profile);
            }

            await authDb.SaveChangesAsync();
            await userDb.SaveChangesAsync();

            return (generatedUsers, generatedOrgs);
        }

        static async Task<List<ProjEnt.Project>> GenerateProjects(ProjectApi.Data.ProjectDbContext projDb, List<UserEnt.User> users, Guid primaryOrgId)
        {
            var faker = new Faker();
            var projects = new List<ProjEnt.Project>();
            var himanshu = users.First(u => u.FirstName == "Himanshu");
            var prabhat = users.First(u => u.FirstName == "Prabhat");

            // Client Zensar
            var client = CreateEntity<ProjEnt.Client>(new Dictionary<string, object> {
                { "ClientId", Guid.Parse("c0000000-0000-0000-0000-000000000001") },
                { "Name", "Zensar Enterprises Ltd" },
                { "Industry", "Technology Services" },
                { "ContactPerson", "Sanjay Kumar" },
                { "Email", "sanjay.kumar@zensar-client.com" },
                { "Keywords", "tech,consulting,global" },
                { "OrganizationId", primaryOrgId },
                { "CreatedAt", DateTime.UtcNow.AddMonths(-12) },
                { "UpdatedAt", DateTime.UtcNow }
            });
            projDb.Clients.Add(client);

            // Generate Workspaces
            var workspaces = new List<ProjEnt.Workspace>();
            var wsDefs = new List<(Guid Id, string Name, string Desc)>
            {
                (Guid.Parse("a0000000-0000-0000-0000-000000000001"), "Internal Products", "Workspace for internal product development"),
                (Guid.Parse("a0000000-0000-0000-0000-000000000002"), "Client Delivery", "Workspace for delivering projects to Zensar clients"),
                (Guid.Parse("a0000000-0000-0000-0000-000000000003"), "Innovation Lab", "Research and innovation workspace")
            };

            foreach (var wd in wsDefs)
            {
                var ws = CreateEntity<ProjEnt.Workspace>(new Dictionary<string, object> {
                    { "WorkspaceId", wd.Id },
                    { "Name", wd.Name },
                    { "Description", wd.Desc },
                    { "OrganizationId", primaryOrgId },
                    { "CreatedAt", DateTime.UtcNow.AddMonths(-12) },
                    { "UpdatedAt", DateTime.UtcNow }
                });
                projDb.Workspaces.Add(ws);
                workspaces.Add(ws);
            }

            // Three core projects
            var projDefs = new List<(Guid Id, string Name, string Key, Guid Owner, ProjectType Type, Guid WorkspaceId)>
            {
                (Guid.Parse("10000000-0000-0000-0000-000000000001"), "Zensar Core App", "ZEN", himanshu.UserId, ProjectType.Scrum, Guid.Parse("a0000000-0000-0000-0000-000000000001")),
                (Guid.Parse("10000000-0000-0000-0000-000000000002"), "Zensar Cloud Migration", "CLD", himanshu.UserId, ProjectType.Scrum, Guid.Parse("a0000000-0000-0000-0000-000000000002")),
                (Guid.Parse("10000000-0000-0000-0000-000000000003"), "Zensar Customer Portal", "PORT", prabhat.UserId, ProjectType.Kanban, Guid.Parse("a0000000-0000-0000-0000-000000000003"))
            };

            foreach (var pd in projDefs)
            {
                var project = CreateEntity<ProjEnt.Project>(new Dictionary<string, object> {
                    { "ProjectId", pd.Id },
                    { "Name", pd.Name },
                    { "Description", $"Agile management dashboard for the {pd.Name} team." },
                    { "OwnerId", pd.Owner },
                    { "OrganizationId", primaryOrgId },
                    { "WorkspaceId", pd.WorkspaceId },
                    { "Key", pd.Key },
                    { "ProjectType", pd.Type },
                    { "Status", ProjectStatus.Active },
                    { "CreatedAt", DateTime.UtcNow.AddMonths(-6) },
                    { "UpdatedAt", DateTime.UtcNow }
                });
                projects.Add(project);
                projDb.Projects.Add(project);

                // Add all 15 users as members of every project to ensure fully integrated access
                foreach (var u in users)
                {
                    ProjectRole role = ProjectRole.Member;
                    if (u.UserId == pd.Owner)
                    {
                        role = ProjectRole.Owner;
                    }
                    else if (u.FirstName == "Himanshu" || u.FirstName == "Anuja" || u.FirstName == "Prabhat")
                    {
                        role = ProjectRole.Admin;
                    }

                    var member = CreateEntity<ProjEnt.ProjectMember>(new Dictionary<string, object> {
                        { "ProjectMemberId", Guid.NewGuid() },
                        { "ProjectId", pd.Id },
                        { "UserId", u.UserId },
                        { "Role", role },
                        { "JoinedAt", project.CreatedAt }
                    });
                    projDb.ProjectMembers.Add(member);
                }
            }

            await projDb.SaveChangesAsync();
            return projects;
        }

        static async Task<List<TaskEnt.TaskItem>> GenerateTeamsSprintsEpicsAndTasks(TaskApi.Data.TaskDbContext taskDb, List<ProjEnt.Project> projects, List<UserEnt.User> users, Guid primaryOrgId)
        {
            var faker = new Faker();
            var allTasks = new List<TaskEnt.TaskItem>();
            var himanshu = users.First(u => u.FirstName == "Himanshu");
            var anuja = users.First(u => u.FirstName == "Anuja");
            var prabhat = users.First(u => u.FirstName == "Prabhat");
            var vijayR = users.First(u => u.FirstName == "Vijay" && u.LastName == "Rajagopalan");

            // 1. Create Teams
            var teams = new List<(Guid Id, string Name, Guid Lead, string Desc)>
            {
                (Guid.Parse("90000000-0000-0000-0000-000000000001"), "Frontend Avengers", anuja.UserId, "Frontend UI specialists"),
                (Guid.Parse("90000000-0000-0000-0000-000000000002"), "Backend Wizards", himanshu.UserId, "API & DB systems engineering"),
                (Guid.Parse("90000000-0000-0000-0000-000000000003"), "QA Gladiators", prabhat.UserId, "Quality validation and testing suites"),
                (Guid.Parse("90000000-0000-0000-0000-000000000004"), "DevOps Guardians", vijayR.UserId, "CI/CD & cloud architecture management")
            };

            var generatedTeams = new List<TaskEnt.Team>();
            foreach (var t in teams)
            {
                var team = CreateEntity<TaskEnt.Team>(new Dictionary<string, object> {
                    { "TeamId", t.Id },
                    { "Name", t.Name },
                    { "AvatarUrl", $"https://api.dicebear.com/7.x/identicon/svg?seed={t.Name}" },
                    { "LeadId", t.Lead },
                    { "Visibility", "Public" },
                    { "Description", t.Desc },
                    { "IsArchived", false },
                    { "CreatedAt", DateTime.UtcNow.AddYears(-1) },
                    { "UpdatedAt", DateTime.UtcNow }
                });
                taskDb.Teams.Add(team);
                generatedTeams.Add(team);

                // Add matching team members based on roles
                foreach (var u in users)
                {
                    bool isMember = false;
                    string role = "Developer";
                    if (u.UserId == t.Lead)
                    {
                        isMember = true;
                        role = "Team Lead";
                    }
                    else if (t.Name == "Frontend Avengers" && (u.FirstName == "Deepak" || u.FirstName == "Divya" || u.FirstName == "Nishanth"))
                    {
                        isMember = true;
                    }
                    else if (t.Name == "Backend Wizards" && (u.FirstName == "Poonam" || u.FirstName == "Rahul Krishnat" || u.FirstName == "Sakshi" || u.FirstName == "Sumit" || u.FirstName == "Vaishnavi" || u.FirstName == "Vijay"))
                    {
                        isMember = true;
                    }
                    else if (t.Name == "QA Gladiators" && (u.FirstName == "Vishakha Vilas" || u.FirstName == "Vidhi S."))
                    {
                        isMember = true;
                        role = "QA Engineer";
                    }
                    else if (t.Name == "DevOps Guardians" && u.FirstName == "Vijay" && u.LastName == "Rajagopalan")
                    {
                        isMember = true;
                        role = "DevOps Engineer";
                    }

                    if (isMember)
                    {
                        var tm = CreateEntity<TaskEnt.TeamMember>(new Dictionary<string, object> {
                            { "TeamMemberId", Guid.NewGuid() },
                            { "TeamId", t.Id },
                            { "UserId", u.UserId },
                            { "Role", role },
                            { "JoinedAt", team.CreatedAt }
                        });
                        taskDb.TeamMembers.Add(tm);
                    }
                }
            }

            // Populate CalendarDays
            for (int day = -30; day <= 60; day++)
            {
                var dt = DateTime.UtcNow.Date.AddDays(day);
                var cd = CreateEntity<TaskEnt.CalendarDay>(new Dictionary<string, object> {
                    { "Date", dt },
                    { "IsWorkingDay", dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday }
                });
                taskDb.CalendarDays.Add(cd);
            }

            // Loop through projects to generate Epics, Sprints, Components, ReleaseVersions, Boards, and Tasks
            foreach (var proj in projects)
            {
                // Boards & columns
                var board = CreateEntity<TaskEnt.Board>(new Dictionary<string, object> {
                    { "BoardId", Guid.NewGuid() },
                    { "ProjectId", proj.ProjectId },
                    { "Name", proj.Name + " Sprint Board" },
                    { "CreatedAt", proj.CreatedAt },
                    { "UpdatedAt", DateTime.UtcNow }
                });
                taskDb.Boards.Add(board);

                var columns = new List<(string Name, TaskStatus Status, int Order)>
                {
                    ("Backlog", TaskStatus.Todo, 0),
                    ("In Progress", TaskStatus.InProgress, 1),
                    ("Under Review", TaskStatus.Review, 2),
                    ("Done", TaskStatus.Done, 3),
                    ("Blocked", TaskStatus.Blocked, 4)
                };

                foreach (var col in columns)
                {
                    var bc = CreateEntity<TaskEnt.BoardColumn>(new Dictionary<string, object> {
                        { "BoardColumnId", Guid.NewGuid() },
                        { "BoardId", board.BoardId },
                        { "Name", col.Name },
                        { "Status", col.Status },
                        { "SortOrder", col.Order }
                    });
                    taskDb.BoardColumns.Add(bc);
                }

                // Epics
                var epicList = new List<TaskEnt.Epic>();
                for (int e = 1; e <= 3; e++)
                {
                    var epic = CreateEntity<TaskEnt.Epic>(new Dictionary<string, object> {
                        { "EpicId", Guid.NewGuid() },
                        { "ProjectId", proj.ProjectId },
                        { "Name", $"{proj.Key} Epic {e}: " + (e == 1 ? "Core Architecture & Foundation" : e == 2 ? "Integrations & real-time Sync" : "Optimization & Cloud Migration") },
                        { "Description", $"High-level epic tracking epic requirements for {proj.Name}." },
                        { "CreatedAt", proj.CreatedAt },
                        { "UpdatedAt", DateTime.UtcNow }
                    });
                    taskDb.Epics.Add(epic);
                    epicList.Add(epic);
                }

                // Components
                var compList = new List<TaskEnt.Component>();
                var components = new[] { "UI Component Library", "API Controllers & Logic", "Data Access & Performance" };
                foreach (var cName in components)
                {
                    var component = CreateEntity<TaskEnt.Component>(new Dictionary<string, object> {
                        { "ComponentId", Guid.NewGuid() },
                        { "ProjectId", proj.ProjectId },
                        { "Name", cName },
                        { "Description", $"Files and dependencies for {cName}." },
                        { "CreatedAt", proj.CreatedAt }
                    });
                    taskDb.Components.Add(component);
                    compList.Add(component);
                }

                // ReleaseVersions
                var versionList = new List<TaskEnt.ReleaseVersion>();
                var versions = new[] { "v1.0.0-release", "v1.1.0-beta" };
                foreach (var vName in versions)
                {
                    var rv = CreateEntity<TaskEnt.ReleaseVersion>(new Dictionary<string, object> {
                        { "ReleaseVersionId", Guid.NewGuid() },
                        { "ProjectId", proj.ProjectId },
                        { "Name", vName },
                        { "Description", $"Release tracking version {vName}." },
                        { "IsReleased", vName.Contains("1.0.0") },
                        { "ReleaseDate", vName.Contains("1.0.0") ? DateTime.UtcNow.AddDays(-5) : (DateTime?)null },
                        { "CreatedAt", proj.CreatedAt }
                    });
                    taskDb.ReleaseVersions.Add(rv);
                    versionList.Add(rv);
                }

                // Active Sprint
                var activeSprint = CreateEntity<TaskEnt.Sprint>(new Dictionary<string, object> {
                    { "SprintId", Guid.NewGuid() },
                    { "ProjectId", proj.ProjectId },
                    { "Name", $"{proj.Key} Sprint 1" },
                    { "StartDate", DateTime.UtcNow.AddDays(-5) },
                    { "EndDate", DateTime.UtcNow.AddDays(9) },
                    { "Status", SprintStatus.Active },
                    { "CreatedAt", DateTime.UtcNow.AddDays(-6) },
                    { "UpdatedAt", DateTime.UtcNow }
                });
                taskDb.Sprints.Add(activeSprint);

                // Daily Update States
                var dus = CreateEntity<TaskEnt.DailyUpdateState>(new Dictionary<string, object> {
                    { "ProjectId", proj.ProjectId },
                    { "LastSentAt", DateTime.UtcNow.AddDays(-1) },
                    { "IsTriggeredToday", false },
                    { "ExtraRecipients", "management@zensar.com,lead-group@zensar.com" }
                });
                taskDb.DailyUpdateStates.Add(dus);

                // Generate Tasks for the project (around 20-30 tasks each)
                int taskCount = faker.Random.Int(20, 25);
                for (int i = 0; i < taskCount; i++)
                {
                    var taskId = Guid.NewGuid();
                    var created = faker.Date.Between(proj.CreatedAt, DateTime.UtcNow.AddDays(-5));
                    
                    var assignee = faker.PickRandom(users);
                    var reporter = faker.PickRandom(users);
                    
                    var status = faker.PickRandom<TaskStatus>();
                    var resolution = status == TaskStatus.Done ? TaskResolution.Done : (TaskResolution?)null;
                    var priority = faker.PickRandom<TaskPriority>();
                    var issueType = faker.PickRandom<IssueType>();
                    
                    var originalEst = faker.Random.Int(120, 1440);
                    var remainingEst = status == TaskStatus.Done ? 0 : status == TaskStatus.InProgress ? originalEst / 2 : originalEst;
                    
                    var dueDate = created.AddDays(faker.Random.Int(10, 45));

                    var task = CreateEntity<TaskEnt.TaskItem>(new Dictionary<string, object> {
                        { "TaskId", taskId },
                        { "OrganizationId", primaryOrgId },
                        { "ProjectId", proj.ProjectId },
                        { "IssueNumber", i + 1 },
                        { "IssueKey", $"{proj.Key}-{i+1}" },
                        { "IssueType", issueType },
                        { "Title", faker.Hacker.Phrase() },
                        { "Description", faker.Lorem.Paragraphs(faker.Random.Int(1, 2)) },
                        { "Status", status },
                        { "Resolution", resolution },
                        { "Priority", priority },
                        { "ReporterId", reporter.UserId },
                        { "AssigneeId", assignee.UserId },
                        { "SprintId", activeSprint.SprintId },
                        { "EpicId", faker.PickRandom(epicList).EpicId },
                        { "ComponentId", faker.PickRandom(compList).ComponentId },
                        { "FixVersionId", faker.PickRandom(versionList).ReleaseVersionId },
                        { "TeamId", faker.PickRandom(generatedTeams).TeamId },
                        { "StoryPoints", faker.PickRandom(new[] { 1, 2, 3, 5, 8, 13 }) },
                        { "OriginalEstimateMinutes", originalEst },
                        { "RemainingEstimateMinutes", remainingEst },
                        { "BacklogRank", (decimal)(i + 1) },
                        { "DueDate", dueDate },
                        { "CreatedAt", created },
                        { "UpdatedAt", DateTime.UtcNow },
                        { "FeDeveloper", "Anuja Kulkarni" },
                        { "BeDeveloper", "Himanshu Tripathi" },
                        { "QaEngineer", "Vishakha Vilas Khair" },
                        { "InitialEta", dueDate.AddDays(-2) },
                        { "LatestEta", dueDate },
                        { "IsOverdue", DateTime.UtcNow > dueDate && status != TaskStatus.Done }
                    });

                    taskDb.Tasks.Add(task);
                    allTasks.Add(task);

                    // Add Subtasks for ~20% of tasks
                    if (faker.Random.Bool(0.2f))
                    {
                        for (int s = 1; s <= faker.Random.Int(2, 3); s++)
                        {
                            var sub = CreateEntity<TaskEnt.SubTask>(new Dictionary<string, object> {
                                { "SubTaskId", Guid.NewGuid() },
                                { "TaskId", taskId },
                                { "Title", $"Subtask {s} for {task.IssueKey}: " + faker.Hacker.Verb() + " " + faker.Hacker.Noun() },
                                { "IsCompleted", status == TaskStatus.Done || (status == TaskStatus.Review && s == 1) },
                                { "CreatedAt", created.AddHours(2) },
                                { "UpdatedAt", DateTime.UtcNow }
                            });
                            taskDb.SubTasks.Add(sub);
                        }
                    }

                    // Add Labels
                    var label = CreateEntity<TaskEnt.TaskLabel>(new Dictionary<string, object> {
                        { "TaskLabelId", Guid.NewGuid() },
                        { "TaskId", taskId },
                        { "Name", faker.PickRandom(new[] { "backend", "frontend", "bug-fix", "api", "security" }) },
                        { "Color", faker.PickRandom(new[] { "#F44336", "#3F51B5", "#4CAF50", "#FF9800", "#E91E63" }) },
                        { "CreatedAt", created }
                    });
                    taskDb.TaskLabels.Add(label);

                    // Add Watchers
                    int watcherCount = faker.Random.Int(1, 3);
                    var shuffledUsers = users.OrderBy(x => Guid.NewGuid()).Take(watcherCount);
                    foreach (var watcherUser in shuffledUsers)
                    {
                        var watcher = CreateEntity<TaskEnt.TaskWatcher>(new Dictionary<string, object> {
                            { "TaskWatcherId", Guid.NewGuid() },
                            { "TaskId", taskId },
                            { "UserId", watcherUser.UserId },
                            { "CreatedAt", created.AddHours(1) }
                        });
                        taskDb.TaskWatchers.Add(watcher);
                    }

                    // Add WorkLogs for in progress / done tasks
                    if (status == TaskStatus.InProgress || status == TaskStatus.Done)
                    {
                        var workLog = CreateEntity<TaskEnt.WorkLog>(new Dictionary<string, object> {
                            { "WorkLogId", Guid.NewGuid() },
                            { "TaskId", taskId },
                            { "UserId", assignee.UserId },
                            { "TimeSpentMinutes", originalEst / 2 },
                            { "Comment", "Spent time analyzing requirements and writing structural code." },
                            { "StartedAt", created.AddDays(1) },
                            { "CreatedAt", created.AddDays(1).AddHours(4) }
                        });
                        taskDb.WorkLogs.Add(workLog);
                    }
                }
            }

            // Generate Task Links (relations between tasks)
            for (int k = 0; k < allTasks.Count - 1; k += 3)
            {
                var source = allTasks[k];
                var target = allTasks[k + 1];
                var link = CreateEntity<TaskEnt.TaskLink>(new Dictionary<string, object> {
                    { "TaskLinkId", Guid.NewGuid() },
                    { "SourceTaskId", source.TaskId },
                    { "TargetTaskId", target.TaskId },
                    { "LinkType", TaskLinkType.Blocks },
                    { "CreatedById", himanshu.UserId },
                    { "CreatedAt", DateTime.UtcNow.AddDays(-2) }
                });
                taskDb.TaskLinks.Add(link);
            }

            // Generate SavedFilters for Himanshu & Anuja
            var defaultFilters = new[] { "My High Priority Bugs", "Assigned to Me", "Sprint Backlog" };
            foreach (var filterName in defaultFilters)
            {
                var sf1 = CreateEntity<TaskEnt.SavedFilter>(new Dictionary<string, object> {
                    { "SavedFilterId", Guid.NewGuid() },
                    { "UserId", himanshu.UserId },
                    { "ProjectId", projects[0].ProjectId },
                    { "Name", filterName },
                    { "JqlQuery", filterName.Contains("Bug") ? "type = Bug AND priority >= High" : filterName.Contains("Me") ? "assignee = currentUser()" : "sprint in (openSprints())" },
                    { "CreatedAt", DateTime.UtcNow }
                });
                taskDb.SavedFilters.Add(sf1);

                var sf2 = CreateEntity<TaskEnt.SavedFilter>(new Dictionary<string, object> {
                    { "SavedFilterId", Guid.NewGuid() },
                    { "UserId", anuja.UserId },
                    { "ProjectId", projects[0].ProjectId },
                    { "Name", filterName },
                    { "JqlQuery", filterName.Contains("Bug") ? "type = Bug AND priority >= High" : filterName.Contains("Me") ? "assignee = currentUser()" : "sprint in (openSprints())" },
                    { "CreatedAt", DateTime.UtcNow }
                });
                taskDb.SavedFilters.Add(sf2);
            }

            await taskDb.SaveChangesAsync();
            return allTasks;
        }

        static async Task GenerateComments(CommentApi.Data.CommentDbContext commDb, List<TaskEnt.TaskItem> tasks, List<UserEnt.User> users)
        {
            var faker = new Faker();
            foreach (var task in tasks)
            {
                var commentCount = faker.Random.Int(1, 3);
                for (int i = 0; i < commentCount; i++)
                {
                    var author = faker.PickRandom(users);
                    var commentId = Guid.NewGuid();
                    var comment = CreateEntity<CommEnt.Comment>(new Dictionary<string, object> {
                        { "CommentId", commentId },
                        { "TaskId", task.TaskId },
                        { "AuthorId", author.UserId },
                        { "ParentCommentId", (Guid?)null },
                        { "Body", $"Hi @{faker.PickRandom(users).FirstName}, could you verify our updates for issue {task.IssueKey}?" },
                        { "IsDeleted", false },
                        { "CreatedAt", faker.Date.Between(task.CreatedAt, DateTime.UtcNow) },
                        { "UpdatedAt", DateTime.UtcNow }
                    });
                    commDb.Comments.Add(comment);

                    // Add Comment Mention
                    var mentioned = users.First(u => comment.Body.Contains("@" + u.FirstName));
                    if (mentioned != null)
                    {
                        var mention = CreateEntity<CommEnt.CommentMention>(new Dictionary<string, object> {
                            { "CommentMentionId", Guid.NewGuid() },
                            { "CommentId", commentId },
                            { "MentionedUserId", mentioned.UserId },
                            { "CreatedAt", comment.CreatedAt }
                        });
                        commDb.CommentMentions.Add(mention);
                    }
                }
            }
            await commDb.SaveChangesAsync();
        }

        static async Task GenerateFiles(FileApi.Data.FileDbContext fileDb, List<TaskEnt.TaskItem> tasks, List<UserEnt.User> users)
        {
            var faker = new Faker();
            foreach (var task in tasks)
            {
                if (faker.Random.Bool(0.3f))
                {
                    var fileId = Guid.NewGuid();
                    var uploader = faker.PickRandom(users);
                    var file = CreateEntity<FileEnt.FileAttachment>(new Dictionary<string, object> {
                        { "FileAttachmentId", fileId },
                        { "TaskId", task.TaskId },
                        { "UploadedById", uploader.UserId },
                        { "OriginalFileName", faker.System.FileName("pdf") },
                        { "StoredFileName", $"{fileId}.pdf" },
                        { "ContentType", "application/pdf" },
                        { "SizeInBytes", faker.Random.Long(50000, 5000000) },
                        { "StoragePath", $"/uploads/{task.OrganizationId}/" },
                        { "IsDeleted", false },
                        { "CreatedAt", faker.Date.Between(task.CreatedAt, DateTime.UtcNow) },
                        { "UpdatedAt", DateTime.UtcNow }
                    });
                    fileDb.FileAttachments.Add(file);
                }
            }
            await fileDb.SaveChangesAsync();
        }

        static async Task GenerateAnalyticsAndActivities(AnalyticsApi.Data.AnalyticsDbContext analDb, ActivityApi.Data.ActivityDbContext actiDb, List<TaskEnt.TaskItem> tasks, List<UserEnt.User> users)
        {
            var analItems = new List<AnalEnt.TaskAnalyticsItem>();
            var actItems = new List<ActiEnt.ActivityRecord>();
            var analEvents = new List<AnalEnt.AnalyticsEvent>();

            foreach (var task in tasks)
            {
                var analItem = CreateEntity<AnalEnt.TaskAnalyticsItem>(new Dictionary<string, object> {
                    { "TaskId", task.TaskId },
                    { "ProjectId", task.ProjectId },
                    { "Status", task.Status.ToString() },
                    { "Priority", task.Priority.ToString() },
                    { "AssigneeId", task.AssigneeId },
                    { "SprintId", task.SprintId },
                    { "EpicId", task.EpicId },
                    { "StoryPoints", task.StoryPoints },
                    { "DueDate", task.DueDate },
                    { "IsDeleted", false },
                    { "CreatedAt", task.CreatedAt },
                    { "UpdatedAt", task.UpdatedAt }
                });
                analItems.Add(analItem);

                // Create Task Created activity
                var actCreated = CreateEntity<ActiEnt.ActivityRecord>(new Dictionary<string, object> {
                    { "ActivityRecordId", Guid.NewGuid() },
                    { "EventType", "TaskCreated" },
                    { "EntityType", "Task" },
                    { "EntityId", task.TaskId },
                    { "ActorId", task.ReporterId },
                    { "Description", $"Created task {task.IssueKey}: {task.Title}" },
                    { "PayloadJson", "{}" },
                    { "CreatedAt", task.CreatedAt }
                });
                actItems.Add(actCreated);

                var aEvent = CreateEntity<AnalEnt.AnalyticsEvent>(new Dictionary<string, object> {
                    { "AnalyticsEventId", Guid.NewGuid() },
                    { "EventType", "TaskCreated" },
                    { "EntityType", "Task" },
                    { "EntityId", task.TaskId },
                    { "ProjectId", task.ProjectId },
                    { "ActorId", task.ReporterId },
                    { "RecipientId", (Guid?)null },
                    { "Description", $"Task {task.IssueKey} was created in project." },
                    { "PayloadJson", "{}" },
                    { "OccurredAt", task.CreatedAt }
                });
                analEvents.Add(aEvent);

                // If task is in Done status, log done activity
                if (task.Status == TaskStatus.Done)
                {
                    var actDone = CreateEntity<ActiEnt.ActivityRecord>(new Dictionary<string, object> {
                        { "ActivityRecordId", Guid.NewGuid() },
                        { "EventType", "TaskStatusChanged" },
                        { "EntityType", "Task" },
                        { "EntityId", task.TaskId },
                        { "ActorId", task.AssigneeId ?? task.ReporterId },
                        { "Description", $"Completed task {task.IssueKey}" },
                        { "PayloadJson", "{\"newStatus\":\"Done\"}" },
                        { "CreatedAt", task.UpdatedAt }
                    });
                    actItems.Add(actDone);
                }
            }

            analDb.TaskAnalyticsItems.AddRange(analItems);
            analDb.AnalyticsEvents.AddRange(analEvents);
            actiDb.Activities.AddRange(actItems);

            await analDb.SaveChangesAsync();
            await actiDb.SaveChangesAsync();
        }
        
        static async Task GenerateNotifications(NotificationApi.Data.NotificationDbContext notiDb, List<TaskEnt.TaskItem> tasks, List<UserEnt.User> users)
        {
            var faker = new Faker();
            var notifications = new List<NotiEnt.Notification>();

            foreach (var task in tasks.Take(15))
            {
                var recipient = users.First(u => u.UserId == task.AssigneeId);
                if (recipient != null)
                {
                    var noti = CreateEntity<NotiEnt.Notification>(new Dictionary<string, object> {
                        { "NotificationId", Guid.NewGuid() },
                        { "RecipientId", recipient.UserId },
                        { "EventType", "TaskAssigned" },
                        { "EntityType", "Task" },
                        { "EntityId", task.TaskId },
                        { "Message", $"You were assigned to task {task.IssueKey}: {task.Title}" },
                        { "IsRead", faker.Random.Bool(0.4f) },
                        { "CreatedAt", DateTime.UtcNow.AddHours(-faker.Random.Int(1, 10)) }
                    });
                    notifications.Add(noti);
                }
            }

            notiDb.Notifications.AddRange(notifications);
            await notiDb.SaveChangesAsync();
        }
    }
}
