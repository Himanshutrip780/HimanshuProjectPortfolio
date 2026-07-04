namespace ATS.Application.DTOs.Auth
{
    public class MfaSetupDto
    {
        public string SharedKey { get; set; } = string.Empty;
        public string AuthenticatorUri { get; set; } = string.Empty;
    }
}
