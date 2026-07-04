namespace TaskApi.Model.Dto
{
    public class SubTaskResponseDto
    {
        public Guid SubTaskId { get; set; }

        public Guid TaskId { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
