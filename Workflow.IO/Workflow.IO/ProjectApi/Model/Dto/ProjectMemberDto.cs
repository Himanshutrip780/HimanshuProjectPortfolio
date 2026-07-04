using ProjectApi.Model.Domain.Enums;
using System;

namespace ProjectApi.Model.Dto
{
    public class ProjectMemberDto
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public ProjectRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
