namespace TaskApi.Model.Dto
{
    public class SprintBacklogResponseDto
    {
        public SprintResponseDto Sprint { get; set; } = new();

        public IEnumerable<TaskResponseDto> Tasks { get; set; } =
            new List<TaskResponseDto>();
    }
}
