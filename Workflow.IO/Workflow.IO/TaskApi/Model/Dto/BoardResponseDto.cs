namespace TaskApi.Model.Dto
{
    public class BoardResponseDto
    {
        public Guid BoardId { get; set; }

        public Guid ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
