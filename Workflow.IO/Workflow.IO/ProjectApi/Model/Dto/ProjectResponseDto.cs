using ProjectApi.Model.Domain.Enums;

namespace ProjectApi.Model.Dto
{
    public class ProjectResponseDto
    {
        public Guid ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid OwnerId { get; set; }

        public string Key { get; set; } = string.Empty;

        public ProjectType ProjectType { get; set; }

        public ProjectStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
