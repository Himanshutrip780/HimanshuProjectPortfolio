using System.ComponentModel.DataAnnotations;

namespace UserApi.Model.Dto
{
    public class UpdateProfileRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }
    }
}
