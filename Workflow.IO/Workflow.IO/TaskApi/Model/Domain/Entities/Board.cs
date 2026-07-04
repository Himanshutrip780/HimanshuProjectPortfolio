namespace TaskApi.Model.Domain.Entities
{
    public class Board
    {
        public Guid BoardId { get; private set; }

        public Guid ProjectId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        private Board()
        {
        }

        public Board(
            Guid projectId,
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Board name is required");
            }

            BoardId = Guid.NewGuid();

            ProjectId = projectId;

            Name = name.Trim();

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Board name is required");
            }

            Name = name.Trim();

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
