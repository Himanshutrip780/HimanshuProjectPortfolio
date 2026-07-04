namespace AnalyticsApi.Model.Dto
{
    public class SprintBurndownDto
    {
        public Guid SprintId { get; set; }

        public int TotalStoryPoints { get; set; }

        public IEnumerable<SprintBurndownPointDto> Points { get; set; } =
            Enumerable.Empty<SprintBurndownPointDto>();
    }

    public class SprintBurndownPointDto
    {
        public DateTime Date { get; set; }

        public int RemainingStoryPoints { get; set; }

        public int IdealRemainingStoryPoints { get; set; }
    }
}
