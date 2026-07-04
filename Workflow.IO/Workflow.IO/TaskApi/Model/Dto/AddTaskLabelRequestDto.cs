using System.ComponentModel.DataAnnotations;

namespace TaskApi.Model.Dto
{
    public class AddTaskLabelRequestDto
    {
        [Required]
        [MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Color { get; set; }
    }
}
