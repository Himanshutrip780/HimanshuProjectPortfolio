using System.ComponentModel.DataAnnotations;
using TaskApi.Model.Domain.Enums;

namespace TaskApi.Model.Dto
{
    public class CreateTaskRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [EnumDataType(typeof(TaskPriority))]
        public TaskPriority Priority { get; set; }

        public Guid? AssigneeId { get; set; }

        public DateTime? DueDate { get; set; }

        public IssueType IssueType { get; set; } = IssueType.Task;

        public Guid? ParentTaskId { get; set; }

        public Guid? ComponentId { get; set; }

        public Guid? FixVersionId { get; set; }

        [Range(0, 100000)]
        public int? OriginalEstimateMinutes { get; set; }

        public string? FeDeveloper { get; set; }

        public string? BeDeveloper { get; set; }

        public string? QaEngineer { get; set; }

        public DateTime? InitialEta { get; set; }

        public DateTime? LatestEta { get; set; }

        public Guid? TeamId { get; set; }
    }
}
