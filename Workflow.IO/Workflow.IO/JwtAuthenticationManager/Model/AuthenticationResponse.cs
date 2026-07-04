namespace JwtAuthenticationManager.Model
{
    public class AuthenticationResponse
    {
        public string? Email { get; set; }

        public string? JwtToken { get; set; }

        public string? RefreshToken { get; set; }

        public int ExpiresIn { get; set; }

        public string? Role { get; set; }
    }
}
