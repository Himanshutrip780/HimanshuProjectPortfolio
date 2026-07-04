namespace TaskApi.Model.Dto
{
    public class BacklogResponseDto
    {
        public Guid ProjectId { get; set; }

        public IEnumerable<TaskResponseDto> BacklogTasks { get; set; } =
            new List<TaskResponseDto>();

        public IEnumerable<SprintBacklogResponseDto> Sprints { get; set; } =
            new List<SprintBacklogResponseDto>();
    }
}
