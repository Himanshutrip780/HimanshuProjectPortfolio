namespace TaskApi.Model.Domain.Entities
{
    public class SavedFilter
    {
        public Guid SavedFilterId { get; private set; }

        public Guid UserId { get; private set; }

        public Guid? ProjectId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string JqlQuery { get; private set; } = string.Empty;

        public DateTime CreatedAt { get; private set; }

        private SavedFilter()
        {
        }

        public SavedFilter(
            Guid userId,
            Guid? projectId,
            string name,
            string jqlQuery)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Filter name is required");
            }

            SavedFilterId = Guid.NewGuid();

            UserId = userId;

            ProjectId = projectId;

            Name = name.Trim();

            JqlQuery = jqlQuery.Trim();

            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string jqlQuery)
        {
            Name = name.Trim();

            JqlQuery = jqlQuery.Trim();
        }
    }
}
