using System.ComponentModel.DataAnnotations;

namespace UserApi.Model.Dto
{
    public class RegisterUserRequestDTO
    {
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? OrganizationName { get; set; }

        [MaxLength(20)]
        public string? InviteCode { get; set; }
    }
}
