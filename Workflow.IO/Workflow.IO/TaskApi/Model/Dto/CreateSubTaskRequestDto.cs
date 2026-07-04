using System.ComponentModel.DataAnnotations;

namespace TaskApi.Model.Dto
{
    public class CreateSubTaskRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
    }
}
