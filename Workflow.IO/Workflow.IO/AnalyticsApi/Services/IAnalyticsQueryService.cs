using AnalyticsApi.Model.Dto;

namespace AnalyticsApi.Services
{
    public interface IAnalyticsQueryService
    {
        Task<ProjectAnalyticsSummaryDto> GetProjectSummaryAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<MetricBucketDto>> GetTasksByStatusAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<SprintVelocityDto> GetSprintVelocityAsync(
            Guid projectId,
            Guid sprintId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ActivityTrendDto>> GetActivityTrendsAsync(
            Guid projectId,
            int days,
            CancellationToken cancellationToken = default);

        Task<SprintBurndownDto> GetSprintBurndownAsync(
            Guid projectId,
            Guid sprintId,
            int days,
            CancellationToken cancellationToken = default);
    }
}
