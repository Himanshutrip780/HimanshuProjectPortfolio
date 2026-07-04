using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskApi.Model.Dto
{
    public class CreateTeamRequestDto
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [Required]
        public Guid LeadId { get; set; }

        [MaxLength(30)]
        public string Visibility { get; set; } = "Public";

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateTeamRequestDto
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [Required]
        public Guid LeadId { get; set; }

        [MaxLength(30)]
        public string Visibility { get; set; } = "Public";

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class TeamResponseDto
    {
        public Guid TeamId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public Guid LeadId { get; set; }
        public string Visibility { get; set; } = "Public";
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IEnumerable<TeamMemberResponseDto> Members { get; set; } = new List<TeamMemberResponseDto>();
    }

    public class TeamMemberResponseDto
    {
        public Guid TeamMemberId { get; set; }
        public Guid TeamId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = "Member";
        public DateTime JoinedAt { get; set; }
    }

    public class AddTeamMemberRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [MaxLength(40)]
        public string Role { get; set; } = "Member";
    }

    public class ChangeMemberRoleRequestDto
    {
        [Required]
        [MaxLength(40)]
        public string Role { get; set; } = "Member";
    }

    public class TransferTeamOwnershipRequestDto
    {
        [Required]
        public Guid NewLeadId { get; set; }
    }

    public class TeamAnalyticsDto
    {
        public int TotalIssuesCount { get; set; }
        public int CompletedIssuesCount { get; set; }
        public double SprintCompletionRate { get; set; } // Percentage of tasks completed in active/recent sprints
        public double Velocity { get; set; } // Average completed issues or story points per sprint
        public double Throughput { get; set; } // Average completed issues per week/month
        public IEnumerable<StatusDistributionItem> StatusDistribution { get; set; } = new List<StatusDistributionItem>();
    }

    public class StatusDistributionItem
    {
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
