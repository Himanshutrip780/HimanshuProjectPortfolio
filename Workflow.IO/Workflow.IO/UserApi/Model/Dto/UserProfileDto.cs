using UserApi.Model.Domian.Common;

namespace UserApi.Model.Dto
{
    public class UserProfileDto
    {
        public Guid UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public string Role { get; set; } = string.Empty;

        public UserStatus Status { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
