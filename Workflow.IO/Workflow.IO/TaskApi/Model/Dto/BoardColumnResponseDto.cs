namespace TaskApi.Model.Dto
{
    public class BoardColumnResponseDto
    {
        public Guid BoardColumnId { get; set; }

        public Guid BoardId { get; set; }

        public string Name { get; set; } = string.Empty;

        public Model.Domain.Enums.TaskStatus Status { get; set; }

        public int SortOrder { get; set; }
    }
}
