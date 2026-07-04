namespace AnalyticsApi.Model.Dto
{
    public class ProjectAnalyticsSummaryDto
    {
        public Guid ProjectId { get; set; }

        public int TotalTasks { get; set; }

        public int CompletedTasks { get; set; }

        public int OverdueTasks { get; set; }

        public int UnassignedTasks { get; set; }

        public int TotalStoryPoints { get; set; }

        public int CompletedStoryPoints { get; set; }

        public decimal CompletionRate { get; set; }

        public IEnumerable<MetricBucketDto> TasksByStatus { get; set; } =
            new List<MetricBucketDto>();

        public IEnumerable<MetricBucketDto> TasksByPriority { get; set; } =
            new List<MetricBucketDto>();
    }
}
