using System.ComponentModel.DataAnnotations;

namespace ProjectApi.Model.Dto
{
    public class UpdateProjectRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
