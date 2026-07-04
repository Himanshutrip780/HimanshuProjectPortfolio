using System.Text.Json;
using AnalyticsApi.Data;
using AnalyticsApi.Model.Dto;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsApi.Services
{
    public class AnalyticsQueryService : IAnalyticsQueryService
    {
        private readonly AnalyticsDbContext _context;

        public AnalyticsQueryService(
            AnalyticsDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectAnalyticsSummaryDto> GetProjectSummaryAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var tasks =
                await GetProjectTasks(projectId)
                    .ToListAsync(cancellationToken);

            var totalTasks =
                tasks.Count;

            var completedTasks =
                tasks.Count(x => x.Status == "Done");

            var totalStoryPoints =
                tasks.Sum(x => x.StoryPoints ?? 0);

            var completedStoryPoints =
                tasks
                    .Where(x => x.Status == "Done")
                    .Sum(x => x.StoryPoints ?? 0);

            return new ProjectAnalyticsSummaryDto
            {
                ProjectId = projectId,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                OverdueTasks =
                    tasks.Count(
                        x => x.DueDate.HasValue &&
                            x.DueDate.Value < DateTime.UtcNow &&
                            x.Status != "Done"),
                UnassignedTasks =
                    tasks.Count(x => x.AssigneeId == null),
                TotalStoryPoints = totalStoryPoints,
                CompletedStoryPoints = completedStoryPoints,
                CompletionRate =
                    CalculateRate(completedTasks, totalTasks),
                TasksByStatus =
                    GroupBy(tasks.Select(x => x.Status)),
                TasksByPriority =
                    GroupBy(tasks.Select(x => x.Priority))
            };
        }

        public async Task<IEnumerable<MetricBucketDto>> GetTasksByStatusAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var statuses =
                await GetProjectTasks(projectId)
                    .GroupBy(x => x.Status)
                    .Select(
                        group => new MetricBucketDto
                        {
                            Name = group.Key,
                            Count = group.Count()
                        })
                    .OrderBy(x => x.Name)
                    .ToListAsync(cancellationToken);

            return statuses;
        }

        public async Task<SprintVelocityDto> GetSprintVelocityAsync(
            Guid projectId,
            Guid sprintId,
            CancellationToken cancellationToken = default)
        {
            var tasks =
                await _context.TaskAnalyticsItems
                    .AsNoTracking()
                    .Where(
                        x => x.ProjectId == projectId &&
                            x.SprintId == sprintId &&
                            !x.IsDeleted)
                    .ToListAsync(cancellationToken);

            var totalTasks =
                tasks.Count;

            var completedTasks =
                tasks.Count(x => x.Status == "Done");

            return new SprintVelocityDto
            {
                SprintId = sprintId,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                TotalStoryPoints =
                    tasks.Sum(x => x.StoryPoints ?? 0),
                CompletedStoryPoints =
                    tasks
                        .Where(x => x.Status == "Done")
                        .Sum(x => x.StoryPoints ?? 0),
                CompletionRate =
                    CalculateRate(completedTasks, totalTasks)
            };
        }

        public async Task<IEnumerable<ActivityTrendDto>> GetActivityTrendsAsync(
            Guid projectId,
            int days,
            CancellationToken cancellationToken = default)
        {
            var normalizedDays =
                Math.Clamp(days, 1, 90);

            var startDate =
                DateTime.UtcNow.Date.AddDays(
                    -normalizedDays + 1);

            var trends =
                await _context.AnalyticsEvents
                    .AsNoTracking()
                    .Where(
                        x => x.ProjectId == projectId &&
                            x.OccurredAt >= startDate)
                    .GroupBy(x => x.OccurredAt.Date)
                    .Select(
                        group => new ActivityTrendDto
                        {
                            Date = group.Key,
                            Count = group.Count()
                        })
                    .OrderBy(x => x.Date)
                    .ToListAsync(cancellationToken);

            return trends;
        }

        public async Task<SprintBurndownDto> GetSprintBurndownAsync(
            Guid projectId,
            Guid sprintId,
            int days,
            CancellationToken cancellationToken = default)
        {
            var normalizedDays =
                Math.Clamp(days, 1, 90);

            var sprintTasks =
                await _context.TaskAnalyticsItems
                    .AsNoTracking()
                    .Where(
                        x => x.ProjectId == projectId &&
                            x.SprintId == sprintId &&
                            !x.IsDeleted)
                    .ToListAsync(cancellationToken);

            var totalStoryPoints =
                sprintTasks.Sum(x => x.StoryPoints ?? 0);

            var taskIds =
                sprintTasks
                    .Select(x => x.TaskId)
                    .ToHashSet();

            var startDate =
                DateTime.UtcNow.Date.AddDays(
                    -normalizedDays + 1);

            var completionEvents =
                await _context.AnalyticsEvents
                    .AsNoTracking()
                    .Where(
                        x => x.ProjectId == projectId &&
                            x.EventType == "TaskStatusChanged" &&
                            x.OccurredAt >= startDate &&
                            taskIds.Contains(x.EntityId))
                    .OrderBy(x => x.OccurredAt)
                    .ToListAsync(cancellationToken);

            var completedByDay =
                new Dictionary<DateTime, int>();

            foreach (var completionEvent in completionEvents)
            {
                if (!IsDoneTransition(completionEvent.PayloadJson))
                {
                    continue;
                }

                var storyPoints =
                    ReadStoryPoints(completionEvent.PayloadJson) ??
                    sprintTasks
                        .FirstOrDefault(x => x.TaskId == completionEvent.EntityId)?
                        .StoryPoints ??
                    0;

                var day = completionEvent.OccurredAt.Date;

                completedByDay[day] =
                    completedByDay.GetValueOrDefault(day) +
                    storyPoints;
            }

            var points = new List<SprintBurndownPointDto>();
            var cumulativeCompleted = 0;

            for (var dayOffset = 0;
                dayOffset < normalizedDays;
                dayOffset++)
            {
                var date = startDate.AddDays(dayOffset);

                cumulativeCompleted +=
                    completedByDay.GetValueOrDefault(date);

                var remaining =
                    Math.Max(0, totalStoryPoints - cumulativeCompleted);

                var idealRemaining =
                    totalStoryPoints == 0
                        ? 0
                        : (int)Math.Round(
                            totalStoryPoints *
                            (1m - (decimal)(dayOffset + 1) / normalizedDays));

                points.Add(
                    new SprintBurndownPointDto
                    {
                        Date = date,
                        RemainingStoryPoints = remaining,
                        IdealRemainingStoryPoints = idealRemaining
                    });
            }

            return new SprintBurndownDto
            {
                SprintId = sprintId,
                TotalStoryPoints = totalStoryPoints,
                Points = points
            };
        }

        private static bool IsDoneTransition(string? payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                return false;
            }

            try
            {
                using var document =
                    JsonDocument.Parse(payloadJson);

                if (!document.RootElement.TryGetProperty(
                        "status",
                        out var statusProperty))
                {
                    return false;
                }

                return string.Equals(
                    statusProperty.GetString(),
                    "Done",
                    StringComparison.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static int? ReadStoryPoints(string? payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                return null;
            }

            try
            {
                using var document =
                    JsonDocument.Parse(payloadJson);

                if (!document.RootElement.TryGetProperty(
                        "storyPoints",
                        out var storyPointsProperty))
                {
                    return null;
                }

                return storyPointsProperty.ValueKind == JsonValueKind.Number
                    ? storyPointsProperty.GetInt32()
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private IQueryable<Model.Domain.Entities.TaskAnalyticsItem>
            GetProjectTasks(Guid projectId)
        {
            return _context.TaskAnalyticsItems
                .AsNoTracking()
                .Where(
                    x => x.ProjectId == projectId &&
                        !x.IsDeleted);
        }

        private static IEnumerable<MetricBucketDto> GroupBy(
            IEnumerable<string> values)
        {
            return values
                .GroupBy(x => x)
                .Select(
                    group => new MetricBucketDto
                    {
                        Name = group.Key,
                        Count = group.Count()
                    })
                .OrderBy(x => x.Name)
                .ToList();
        }

        private static decimal CalculateRate(
            int completed,
            int total)
        {
            if (total == 0)
            {
                return 0;
            }

            return Math.Round(
                completed * 100m / total,
                2);
        }
    }
}
