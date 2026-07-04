using System.ComponentModel.DataAnnotations;

namespace JwtAuthenticationManager.Model
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
