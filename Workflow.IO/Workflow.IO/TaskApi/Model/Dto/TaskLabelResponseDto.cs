namespace TaskApi.Model.Dto
{
    public class TaskLabelResponseDto
    {
        public Guid TaskLabelId { get; set; }

        public Guid TaskId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
