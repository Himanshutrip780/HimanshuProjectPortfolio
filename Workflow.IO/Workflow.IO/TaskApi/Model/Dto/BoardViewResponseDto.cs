namespace TaskApi.Model.Dto
{
    public class BoardViewResponseDto
    {
        public BoardResponseDto Board { get; set; } = new();

        public IEnumerable<BoardColumnViewDto> Columns { get; set; } =
            new List<BoardColumnViewDto>();
    }
}
