namespace ATS.Infrastructure.Identity
{
    public class JwtSettings
    {
        public string Secret { get; set; } = "SuperSecretKeyForJWTSigningThatIsAtLeast32BytesLong!";
        public int ExpiryMinutes { get; set; } = 120;
        public string Issuer { get; set; } = "ATS_Backend";
        public string Audience { get; set; } = "ATS_Frontend";
    }
}
