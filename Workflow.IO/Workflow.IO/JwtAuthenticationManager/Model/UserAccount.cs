using UserApi.Model.Domian;

namespace JwtAuthenticationManager.Model
{
    public class UserAccount
    {
        public Guid Id { get; private set; }

        public string Email { get; private set; } = string.Empty;

        public string PasswordHash { get; private set; } = string.Empty;

        public UserRole Role { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        private UserAccount() { }

        public UserAccount(
            string email,
            string passwordHash)
        {
            Id = Guid.NewGuid();

            Email = email.Trim().ToLower();

            PasswordHash = passwordHash;

            Role = UserRole.User;

            IsActive = true;

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException(
                    "Password hash is required");
            }

            PasswordHash = passwordHash;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
