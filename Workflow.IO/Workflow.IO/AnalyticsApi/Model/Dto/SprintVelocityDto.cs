namespace AnalyticsApi.Model.Dto
{
    public class SprintVelocityDto
    {
        public Guid SprintId { get; set; }

        public int TotalTasks { get; set; }

        public int CompletedTasks { get; set; }

        public int TotalStoryPoints { get; set; }

        public int CompletedStoryPoints { get; set; }

        public decimal CompletionRate { get; set; }
    }
}
