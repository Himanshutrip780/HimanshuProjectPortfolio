using System.ComponentModel.DataAnnotations;

namespace TaskApi.Model.Dto
{
    public class CreateSprintRequestDto
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
