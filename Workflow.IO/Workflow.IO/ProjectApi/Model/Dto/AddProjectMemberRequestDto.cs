using ProjectApi.Model.Domain.Enums;
using System;

namespace ProjectApi.Model.Dto
{
    public class AddProjectMemberRequestDto
    {
        public Guid UserId { get; set; }
        public ProjectRole Role { get; set; }
    }
}
