using System.ComponentModel.DataAnnotations;

namespace ProjectApi.Model.Dto
{
    public class ImportProjectRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Key { get; set; }

        public int ProjectType { get; set; } = 1; // 1 = Scrum, 2 = Kanban

        public string? Description { get; set; }

        public List<ImportTaskDto>? Tasks { get; set; }
    }

    public class ImportTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Priority { get; set; } = 2; // 1 = Low, 2 = Medium, 3 = High, 4 = Critical

        public int IssueType { get; set; } = 1; // 1 = Story, 2 = Task, 3 = Bug, 4 = SubTask

        public int? StoryPoints { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
