using System.ComponentModel.DataAnnotations;

namespace TaskApi.Model.Dto
{
    public class CreateEpicRequestDto
    {
        [Required]
        [MaxLength(160)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }
    }
}
