namespace JwtAuthenticationManager.Model
{
    public class RefreshToken
    {
        public Guid RefreshTokenId { get; private set; }

        public Guid UserAccountId { get; private set; }

        public string TokenHash { get; private set; } = string.Empty;

        public DateTime ExpiresAt { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? RevokedAt { get; private set; }

        public bool IsRevoked => RevokedAt.HasValue;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        private RefreshToken()
        {
        }

        public RefreshToken(
            Guid userAccountId,
            string tokenHash,
            DateTime expiresAt)
        {
            RefreshTokenId = Guid.NewGuid();

            UserAccountId = userAccountId;

            TokenHash = tokenHash;

            ExpiresAt = expiresAt;

            CreatedAt = DateTime.UtcNow;
        }

        public void Revoke()
        {
            RevokedAt = DateTime.UtcNow;
        }
    }
}
