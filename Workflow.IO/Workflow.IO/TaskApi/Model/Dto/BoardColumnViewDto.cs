namespace TaskApi.Model.Dto
{
    public class BoardColumnViewDto
    {
        public BoardColumnResponseDto Column { get; set; } = new();

        public IEnumerable<TaskResponseDto> Tasks { get; set; } =
            new List<TaskResponseDto>();
    }
}
