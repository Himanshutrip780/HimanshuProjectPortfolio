using System.ComponentModel.DataAnnotations;

namespace TaskApi.Model.Dto
{
    public class CreateBoardRequestDto
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;
    }
}
