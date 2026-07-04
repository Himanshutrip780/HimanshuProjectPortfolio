namespace TaskApi.Model.Dto
{
    public class TaskWatcherResponseDto
    {
        public Guid TaskWatcherId { get; set; }

        public Guid TaskId { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
