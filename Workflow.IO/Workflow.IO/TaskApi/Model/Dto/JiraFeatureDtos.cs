using System.ComponentModel.DataAnnotations;
using TaskApi.Model.Domain.Enums;

namespace TaskApi.Model.Dto
{
    public class CreateComponentRequestDto
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class ComponentResponseDto
    {
        public Guid ComponentId { get; set; }

        public Guid ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class CreateReleaseVersionRequestDto
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? ReleaseDate { get; set; }
    }

    public class ReleaseVersionResponseDto
    {
        public Guid ReleaseVersionId { get; set; }

        public Guid ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsReleased { get; set; }

        public DateTime? ReleaseDate { get; set; }
    }

    public class CreateTaskLinkRequestDto
    {
        [Required]
        public Guid TargetTaskId { get; set; }

        [Required]
        public TaskLinkType LinkType { get; set; }
    }

    public class TaskLinkResponseDto
    {
        public Guid TaskLinkId { get; set; }

        public Guid SourceTaskId { get; set; }

        public Guid TargetTaskId { get; set; }

        public TaskLinkType LinkType { get; set; }

        public string? TargetIssueKey { get; set; }
    }

    public class CreateWorkLogRequestDto
    {
        [Range(1, 1440 * 30)]
        public int TimeSpentMinutes { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }

        public DateTime? StartedAt { get; set; }
    }

    public class WorkLogResponseDto
    {
        public Guid WorkLogId { get; set; }

        public Guid TaskId { get; set; }

        public Guid UserId { get; set; }

        public int TimeSpentMinutes { get; set; }

        public string? Comment { get; set; }

        public DateTime StartedAt { get; set; }
    }

    public class UpdateBacklogRankRequestDto
    {
        public decimal BacklogRank { get; set; }
    }

    public class BulkTaskUpdateRequestDto
    {
        [Required]
        [MinLength(1)]
        public List<Guid> TaskIds { get; set; } = new();

        public Model.Domain.Enums.TaskStatus? Status { get; set; }

        public TaskResolution? Resolution { get; set; }

        public Guid? AssigneeId { get; set; }

        public Guid? SprintId { get; set; }

        public bool MoveToBacklog { get; set; }
    }

    public class BulkTaskUpdateResponseDto
    {
        public int UpdatedCount { get; set; }
    }

    public class CreateSavedFilterRequestDto
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string JqlQuery { get; set; } = string.Empty;
    }

    public class SavedFilterResponseDto
    {
        public Guid SavedFilterId { get; set; }

        public Guid? ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string JqlQuery { get; set; } = string.Empty;
    }

    public class UpdateSprintRequestDto
    {
        [MaxLength(120)]
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
