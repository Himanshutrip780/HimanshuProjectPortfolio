namespace TaskApi.Model.Dto
{
    public class EpicResponseDto
    {
        public Guid EpicId { get; set; }

        public Guid ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
