using TaskApi.Model.Domain.Enums;

namespace TaskApi.Model.Dto
{
    public class SprintResponseDto
    {
        public Guid SprintId { get; set; }

        public Guid ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public SprintStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
