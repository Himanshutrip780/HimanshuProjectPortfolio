using UserApi.Model.Domain.Common;
using UserApi.Model.Domian.Common;

namespace UserApi.Model.Domian.Entities
{
    public class User : BaseAuditableEntity
    {
        // ✅ SAME ID as UserAccount.Id
        public Guid UserId { get; private set; }

        // ✅ Business profile fields only

        public string FirstName { get; private set; } = string.Empty;

        public string LastName { get; private set; } = string.Empty;

        public string? AvatarUrl { get; private set; }

        // ✅ Business/account status
        public UserStatus Status { get; private set; }

        public Guid? OrganizationId { get; private set; }
        public Organization? Organization { get; private set; }

        private User() { }

        // ✅ CHANGED
        // UserAccount now owns identity
        // so User receives UserId externally

        public User(
            Guid userId,
            string firstName,
            string lastName)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId is required");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required");

            UserId = userId;

            FirstName = firstName.Trim();

            LastName = lastName.Trim();

            Status = UserStatus.Active;
        }

        public void UpdateProfile(
            string firstName,
            string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required");

            FirstName = firstName.Trim();

            LastName = lastName.Trim();

            MarkAsUpdated();
        }

        public void AssignOrganization(Guid organizationId)
        {
            OrganizationId = organizationId;
            MarkAsUpdated();
        }

        public void UpdateAvatar(string avatarUrl)
        {
            AvatarUrl = avatarUrl;

            MarkAsUpdated();
        }

        public void Deactivate()
        {
            Status = UserStatus.Inactive;

            MarkAsUpdated();
        }

        public void Activate()
        {
            Status = UserStatus.Active;

            MarkAsUpdated();
        }
    }
}
