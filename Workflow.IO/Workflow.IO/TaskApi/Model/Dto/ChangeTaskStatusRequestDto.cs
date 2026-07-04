using System.ComponentModel.DataAnnotations;
using TaskApi.Model.Domain.Enums;

namespace TaskApi.Model.Dto
{
    public class ChangeTaskStatusRequestDto
    {
        [EnumDataType(typeof(Model.Domain.Enums.TaskStatus))]
        public Model.Domain.Enums.TaskStatus Status { get; set; }

        public TaskResolution? Resolution { get; set; }

        [MaxLength(4000)]
        public string? TransitionComment { get; set; }
    }
}
