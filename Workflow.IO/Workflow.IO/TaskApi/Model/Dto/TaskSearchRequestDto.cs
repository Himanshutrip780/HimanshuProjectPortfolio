using TaskApi.Model.Domain.Enums;

namespace TaskApi.Model.Dto
{
    public class TaskSearchRequestDto
    {
        public string? Query { get; set; }

        public Model.Domain.Enums.TaskStatus? Status { get; set; }

        public TaskPriority? Priority { get; set; }

        public Guid? AssigneeId { get; set; }

        public Guid? ReporterId { get; set; }

        public Guid? SprintId { get; set; }

        public Guid? EpicId { get; set; }

        public bool? IsOverdue { get; set; }

        public Guid? TeamId { get; set; }
    }
}
