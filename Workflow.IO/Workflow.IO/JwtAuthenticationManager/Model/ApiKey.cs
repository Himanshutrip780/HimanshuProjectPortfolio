namespace JwtAuthenticationManager.Model
{
    public class ApiKey
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// e.g. zt_live_xxx...
        /// We only store a hash in the DB for security, but we might store a prefix for identification
        /// </summary>
        public string KeyHash { get; set; } = string.Empty;
        
        public string Prefix { get; set; } = string.Empty;

        public Guid UserAccountId { get; set; }
        public UserAccount? UserAccount { get; set; }

        public Guid OrganizationId { get; set; }
        public Guid WorkspaceId { get; set; }
        
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        
        public bool IsRevoked { get; set; }
    }
}
