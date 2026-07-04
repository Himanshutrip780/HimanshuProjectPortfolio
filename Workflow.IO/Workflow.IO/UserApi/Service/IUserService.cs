using UserApi.Model.Domian.Entities;
using UserApi.Model.Dto;

namespace UserApi.Service
{
    public interface IUserService
    {
        Task<UserDto> RegisterUserAsync(RegisterUserRequestDTO request);

        // ✅ CHANGED
        // Returning DTO instead of Entity is better for API boundary protection
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        // ✅ CHANGED
        // Returning DTO instead of Entity
        Task<UserDto?> GetUserByIdAsync(Guid userId);

        Task<UserDto?> GetUserByEmailAsync(string email);

        Task<UserProfileDto?> GetUserProfileAsync(
            Guid userId,
            string? emailFallback = null);

        Task<IEnumerable<UserLookupDto>> LookupUsersAsync(string emailQuery, Guid currentUserId);

        Task<User> CreateUserAsync(User user);

        // ✅ CHANGED
        // Update should use DTO request instead of full entity from API
        Task<UserDto?> UpdateUserAsync(Guid userId, RegisterUserRequestDTO request);

        Task<UserDto?> UpdateProfileAsync(
            Guid userId,
            UpdateProfileRequestDto request);

        Task ChangePasswordAsync(
            Guid userId,
            ChangePasswordRequestDto request);

        // ✅ CHANGED
        // Delete should return bool instead of deleted entity
        Task<bool> DeleteUserAsync(Guid userId);

        //List<User> GetUsers();
    }
}