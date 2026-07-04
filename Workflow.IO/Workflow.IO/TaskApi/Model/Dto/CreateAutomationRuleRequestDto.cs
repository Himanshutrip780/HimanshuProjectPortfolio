using System;
using System.ComponentModel.DataAnnotations;

namespace TaskApi.Model.Dto
{
    public class CreateAutomationRuleRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TriggerType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string TriggerValue { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ActionValue { get; set; } = string.Empty;
    }
}
