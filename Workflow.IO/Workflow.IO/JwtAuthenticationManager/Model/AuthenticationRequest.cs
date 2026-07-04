using System.ComponentModel.DataAnnotations;

namespace JwtAuthenticationManager.Model
{
    public class AuthenticationRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
