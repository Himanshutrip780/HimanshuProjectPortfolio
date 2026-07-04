using UserApi.Model.Domian;
using UserApi.Model.Domian.Common;

namespace UserApi.Model.Dto
{
    public class UserDto
    {
        public Guid UserId { get; private set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string? AvatarUrl { get; private set; }
        public string Role { get; set; } = string.Empty;
        public UserStatus Status { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
    }
}
