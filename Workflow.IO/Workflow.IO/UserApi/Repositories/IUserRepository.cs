using UserApi.Model.Domian.Entities;

namespace UserApi.Repositories
{
    public interface IUserRepository
    {
        Task<User> RegisterUserAsync(User user);

        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User?> GetUserByIdAsync(Guid userId);

        Task CreateUserAsync(User user);

        Task AssignOrganizationAsync(Guid userId, Guid organizationId);

        Task<User?> UpdateUserAsync(Guid userId, string firstName, string lastName, string? avatarUrl);

        Task<User?> DeleteUserAsync(Guid userId);

        Task<IEnumerable<User>> SearchUsersByNameAsync(string nameQuery);

        //List<User> GetUsers();

        //Task<bool> ExistsByEmailAsync(string email);
        //Task<User> RegisterUserAsync(User user);
        //Task<IEnumerable<User>> GetUsersByRoleAsync(string userRole);
        //Task<User> GetUserByEmailAsync(string email);
    }
}