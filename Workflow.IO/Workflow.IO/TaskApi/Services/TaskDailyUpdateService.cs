using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TaskApi.Data;
using TaskApi.Model.Domain.Entities;
using TaskApi.Model.Domain.Enums;

namespace TaskApi.Services
{
    public class TaskDailyUpdateService : ITaskDailyUpdateService
    {
        private readonly TaskDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TaskDailyUpdateService> _logger;

        private readonly string _projectApiUrl;
        private readonly string _userApiUrl;
        private readonly string _activityApiUrl;

        public TaskDailyUpdateService(
            TaskDbContext dbContext,
            IEmailService emailService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<TaskDailyUpdateService> logger)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            _projectApiUrl = _configuration["Services:ProjectApi"] ?? "http://localhost:5250";
            _userApiUrl = _configuration["Services:UserApi"] ?? "http://localhost:5240";
            _activityApiUrl = _configuration["Services:ActivityApi"] ?? "http://localhost:5300";
        }

        public async Task<DailyUpdateState> GetDailyUpdateStateAsync(Guid projectId)
        {
            var state = await _dbContext.DailyUpdateStates
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);

            if (state == null)
            {
                state = new DailyUpdateState
                {
                    ProjectId = projectId,
                    IsTriggeredToday = false,
                    LastSentAt = null,
                    ExtraRecipients = null
                };

                await _dbContext.DailyUpdateStates.AddAsync(state);
                await _dbContext.SaveChangesAsync();
            }

            return state;
        }

        public async Task<DailyUpdateState> SaveDailyUpdateStateAsync(Guid projectId, string[] extraRecipients)
        {
            var state = await GetDailyUpdateStateAsync(projectId);
            state.ExtraRecipients = extraRecipients != null && extraRecipients.Length > 0
                ? string.Join(",", extraRecipients.Select(r => r.Trim()))
                : null;

            _dbContext.DailyUpdateStates.Update(state);
            await _dbContext.SaveChangesAsync();
            return state;
        }

        public async Task<bool> TriggerDailyUpdateAsync(Guid projectId, Guid userId)
        {
            _logger.LogInformation("Manually triggering daily sprint update for project: {ProjectId}", projectId);

            // 1. Fetch Active Sprint
            var activeSprint = await _dbContext.Sprints
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.Status == SprintStatus.Active);

            if (activeSprint == null)
            {
                _logger.LogWarning("Daily update trigger failed: No active sprint found for project {ProjectId}.", projectId);
                return false;
            }

            // 2. Fetch Tasks in Active Sprint
            var tasks = await _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.SprintId == activeSprint.SprintId)
                .ToListAsync();

            if (tasks.Count == 0)
            {
                _logger.LogInformation("No stories in the active sprint '{SprintName}' to report.", activeSprint.Name);
            }

            // 3. Resolve Member Emails
            var recipientEmails = new List<string>();
            try
            {
                var systemToken = GenerateSystemToken();
                using var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", systemToken);

                // In the single-tenant model, we just send updates to leadership and extra recipients by default,
                // or we could fetch all organization users if needed.
                _logger.LogInformation("Skipping project member fetch; relying on leadership and extra recipients.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving project members' emails from APIs.");
            }

            // 4. Load Leadership and Extra Recipients
            var leadershipEmails = _configuration.GetSection("DailyUpdate:LeadershipEmails").Get<string[]>()
                ?? new[] { "leadership@workflow.io.com" };

            foreach (var email in leadershipEmails)
            {
                if (!string.IsNullOrWhiteSpace(email) && !recipientEmails.Contains(email))
                {
                    recipientEmails.Add(email.Trim());
                }
            }

            var state = await GetDailyUpdateStateAsync(projectId);
            if (!string.IsNullOrWhiteSpace(state.ExtraRecipients))
            {
                var extra = state.ExtraRecipients.Split(',');
                foreach (var email in extra)
                {
                    if (!string.IsNullOrWhiteSpace(email) && !recipientEmails.Contains(email))
                    {
                        recipientEmails.Add(email.Trim());
                    }
                }
            }

            // 5. Query Activity Logs and build HTML table rows
            var tableRowsHtml = new StringBuilder();
            using var clientForActivity = _httpClientFactory.CreateClient();
            clientForActivity.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateSystemToken());

            foreach (var task in tasks)
            {
                var updateLog = "—";
                try
                {
                    // Fetch activities for task
                    var activityUrl = $"{_activityApiUrl}/api/activities/Task/{task.TaskId}";
                    var activityResponse = await clientForActivity.GetAsync(activityUrl);
                    if (activityResponse.IsSuccessStatusCode)
                    {
                        var activityJson = await activityResponse.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(activityJson);
                        if (doc.RootElement.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array)
                        {
                            var logs = new List<string>();
                            // ActivityApi returns records ordered descending, reverse to get chronological since story start
                            var activityList = dataArray.EnumerateArray().Reverse().ToList();
                            foreach (var act in activityList)
                            {
                                var description = act.TryGetProperty("description", out var descProp) ? descProp.GetString() : "";
                                var createdAtStr = act.TryGetProperty("createdAt", out var createdProp) ? createdProp.GetString() : "";
                                
                                if (!string.IsNullOrWhiteSpace(description) && DateTime.TryParse(createdAtStr, out var createdAt))
                                {
                                    logs.Add($"{createdAt.ToString("dd MMM")} – {description}");
                                }
                            }

                            if (logs.Count > 0)
                            {
                                updateLog = string.Join("<br/>", logs);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching update logs for task {TaskId}", task.TaskId);
                }

                var initialEtaStr = task.InitialEta.HasValue ? task.InitialEta.Value.ToString("dd MMM") : "—";
                var latestEtaStr = task.LatestEta.HasValue ? task.LatestEta.Value.ToString("dd MMM") : "—";
                var isDelayed = task.LatestEta.HasValue && task.InitialEta.HasValue && task.LatestEta.Value.Date > task.InitialEta.Value.Date;
                var latestEtaColor = isDelayed ? "color: red;" : "";

                tableRowsHtml.AppendLine("        <tr>");
                tableRowsHtml.AppendLine($"          <td style=\"border:1px solid #ccc; padding:8px;\">{task.IssueKey}</td>");
                tableRowsHtml.AppendLine($"          <td style=\"border:1px solid #ccc; padding:8px;\">{task.Title}</td>");
                tableRowsHtml.AppendLine($"          <td style=\"border:1px solid #ccc; padding:8px;\">{task.FeDeveloper ?? "—"}</td>");
                tableRowsHtml.AppendLine($"          <td style=\"border:1px solid #ccc; padding:8px;\">{task.BeDeveloper ?? "—"}</td>");
                tableRowsHtml.AppendLine($"          <td style=\"border:1px solid #ccc; padding:8px;\">{task.QaEngineer ?? "—"}</td>");
                tableRowsHtml.AppendLine($"          <td style=\"border:1px solid #ccc; padding:8px;\">{updateLog}</td>");
                tableRowsHtml.AppendLine($"          <td style=\"border:1px solid #ccc; padding:8px;\">{initialEtaStr}</td>");
                tableRowsHtml.AppendLine($"          <td style=\"border:1px solid #ccc; padding:8px; {latestEtaColor}\">{latestEtaStr}</td>");
                tableRowsHtml.AppendLine("        </tr>");
            }

            // 6. Generate HTML Body
            var dateStr = DateTime.Today.ToString("MMMM dd, yyyy");
            var emailHtml = $@"<html>
  <body style=""font-family:Segoe UI, sans-serif; color:#333;"">
    <h2 style=""background:#003366; color:#fff; padding:10px; margin-top:0;"">
      Sprint Updates – {dateStr}
    </h2>
    <table style=""width:100%; border-collapse:collapse;"">
      <thead style=""background:#f4f4f4;"">
        <tr>
          <th style=""border:1px solid #ccc; padding:8px; text-align:left;"">Story ID</th>
          <th style=""border:1px solid #ccc; padding:8px; text-align:left;"">Summary</th>
          <th style=""border:1px solid #ccc; padding:8px; text-align:left;"">FE Dev</th>
          <th style=""border:1px solid #ccc; padding:8px; text-align:left;"">BE Dev</th>
          <th style=""border:1px solid #ccc; padding:8px; text-align:left;"">QA</th>
          <th style=""border:1px solid #ccc; padding:8px; text-align:left;"">Update Log</th>
          <th style=""border:1px solid #ccc; padding:8px; text-align:left;"">Initial ETA</th>
          <th style=""border:1px solid #ccc; padding:8px; text-align:left;"">Latest ETA</th>
        </tr>
      </thead>
      <tbody>
{(tableRowsHtml.Length > 0 ? tableRowsHtml.ToString() : "        <tr><td colspan=\"8\" style=\"border:1px solid #ccc; padding:8px; text-align:center;\">No stories found in this sprint.</td></tr>")}
      </tbody>
    </table>
    <p style=""margin-top:20px;"">
      Regards,<br>
      Sprint Automation System
    </p>
  </body>
</html>";

            // 7. Send Email
            var subject = $"Ambit Focus: {activeSprint.Name} Updates – {DateTime.Today.ToString("dd MMM yyyy")}";
            await _emailService.SendEmailAsync(recipientEmails.ToArray(), subject, emailHtml);

            // 8. Update execution states
            state.LastSentAt = DateTime.UtcNow;
            state.IsTriggeredToday = true;
            _dbContext.DailyUpdateStates.Update(state);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task ResetDailyUpdatesAsync()
        {
            _logger.LogInformation("Resetting daily update trigger flags for all projects (11 AM).");

            var states = await _dbContext.DailyUpdateStates.ToListAsync();
            foreach (var state in states)
            {
                state.IsTriggeredToday = false;
            }

            _dbContext.DailyUpdateStates.UpdateRange(states);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AutoSendDailyUpdatesAsync()
        {
            _logger.LogInformation("Auto-sending unsent daily updates for active sprints (12 PM).");

            // Fetch all projects with active sprints
            var activeSprints = await _dbContext.Sprints
                .AsNoTracking()
                .Where(s => s.Status == SprintStatus.Active)
                .ToListAsync();

            foreach (var sprint in activeSprints)
            {
                var state = await GetDailyUpdateStateAsync(sprint.ProjectId);
                if (!state.IsTriggeredToday)
                {
                    try
                    {
                        await TriggerDailyUpdateAsync(sprint.ProjectId, Guid.Empty);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to auto-send daily update for project {ProjectId}", sprint.ProjectId);
                    }
                }
            }
        }

        private string GenerateSystemToken()
        {
            var jwtSecurityKey = _configuration["Jwt:SecurityKey"];
            if (string.IsNullOrWhiteSpace(jwtSecurityKey))
            {
                throw new InvalidOperationException("Jwt:SecurityKey is not configured.");
            }

            var tokenKey = Encoding.UTF8.GetBytes(jwtSecurityKey);
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.NameIdentifier, Guid.Empty.ToString()),
                new Claim(ClaimTypes.Name, "DailyUpdateSystemBot")
            });

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature);

            var issuer = _configuration["Jwt:Issuer"] ?? "https://workflow.io.local";
            var audience = _configuration["Jwt:Audience"] ?? "workflow.io-api";

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(securityTokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }
    }
}
