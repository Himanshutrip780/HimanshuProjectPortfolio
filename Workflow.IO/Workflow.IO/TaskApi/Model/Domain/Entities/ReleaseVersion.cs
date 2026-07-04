namespace TaskApi.Model.Domain.Entities
{
    public class ReleaseVersion
    {
        public Guid ReleaseVersionId { get; private set; }

        public Guid ProjectId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public bool IsReleased { get; private set; }

        public DateTime? ReleaseDate { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private ReleaseVersion()
        {
        }

        public ReleaseVersion(
            Guid projectId,
            string name,
            string? description,
            DateTime? releaseDate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Version name is required");
            }

            ReleaseVersionId = Guid.NewGuid();

            ProjectId = projectId;

            Name = name.Trim();

            Description = description?.Trim();

            ReleaseDate = releaseDate;

            IsReleased = releaseDate.HasValue;

            CreatedAt = DateTime.UtcNow;
        }

        public void Release(DateTime releaseDate)
        {
            IsReleased = true;

            ReleaseDate = releaseDate;
        }
    }
}
