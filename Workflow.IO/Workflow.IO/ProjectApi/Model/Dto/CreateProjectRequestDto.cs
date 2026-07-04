using System.ComponentModel.DataAnnotations;
using ProjectApi.Model.Domain.Enums;

namespace ProjectApi.Model.Dto
{
    public class CreateProjectRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(10)]
        public string? Key { get; set; }

        public ProjectType ProjectType { get; set; } = ProjectType.Scrum;
    }
}
