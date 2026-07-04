using TaskApi.Model.Domain.Enums;

namespace TaskApi.Model.Domain.Entities
{
    public class Sprint
    {
        public Guid SprintId { get; private set; }

        public Guid ProjectId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public DateTime StartDate { get; private set; }

        public DateTime EndDate { get; private set; }

        public SprintStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        private Sprint()
        {
        }

        public Sprint(
            Guid projectId,
            string name,
            DateTime startDate,
            DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Sprint name is required");
            }

            if (endDate <= startDate)
            {
                throw new ArgumentException(
                    "Sprint end date must be after start date");
            }

            SprintId = Guid.NewGuid();

            ProjectId = projectId;

            Name = name.Trim();

            StartDate = startDate;

            EndDate = endDate;

            Status = SprintStatus.Planned;

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;
        }

        public void Start(bool anotherSprintIsActive)
        {
            if (anotherSprintIsActive)
            {
                throw new InvalidOperationException(
                    "Only one sprint can be active per project");
            }

            if (Status == SprintStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Completed sprints cannot be started");
            }

            Status = SprintStatus.Active;

            UpdatedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (Status != SprintStatus.Active)
            {
                throw new InvalidOperationException(
                    "Only active sprints can be completed");
            }

            Status = SprintStatus.Completed;

            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(
            string name,
            DateTime startDate,
            DateTime endDate)
        {
            if (endDate <= startDate)
            {
                throw new ArgumentException(
                    "Sprint end date must be after start date");
            }

            if (Status == SprintStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Completed sprints cannot be edited");
            }

            Name = name.Trim();

            StartDate = startDate;

            EndDate = endDate;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
