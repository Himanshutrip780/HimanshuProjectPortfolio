using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Model.Domian;
using UserApi.Model.Domian.Entities;

namespace UserApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        public readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User> RegisterUserAsync(User user)
        {
            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            // ✅ GOOD
            // FindAsync is optimized for primary key lookup

            User? user =
                await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return null;
            }

            return user;
        }

        public async Task<User?> UpdateUserAsync(
            Guid userId,
            string firstName,
            string lastName,
            string? avatarUrl)
        {
            var existingUser =
                await _context.Users.FindAsync(userId);

            if (existingUser == null)
            {
                return null;
            }

            existingUser.UpdateProfile(
                firstName,
                lastName);

            if (avatarUrl != null)
            {
                existingUser.UpdateAvatar(avatarUrl);
            }

            await _context.SaveChangesAsync();

            return existingUser;
        }

        public async Task AssignOrganizationAsync(Guid userId, Guid organizationId)
        {
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user != null)
            {
                user.AssignOrganization(organizationId);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();
        }

        public async Task<User?> DeleteUserAsync(Guid userId)
        {
            var user =
                await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return null;
            }

            // ✅ CURRENTLY HARD DELETE
            // Removes record from database completely

            _context.Users.Remove(user);

            // 🔥 FUTURE IMPROVEMENT
            // Use SoftDelete():
            // user.SoftDelete();

            await _context.SaveChangesAsync();

            return user;
        }

        //public async Task<bool> ExistsByEmailAsync(string email)
        //{
        //    // ✅ CHANGED
        //    // AnyAsync is more optimized than FirstOrDefaultAsync
        //    // because it only checks existence

        //    return await _context.Users
        //        .AnyAsync(x => x.Email == email);
        //}

        public async Task<IEnumerable<User>> SearchUsersByNameAsync(string nameQuery)
        {
            var normalized = nameQuery.Trim().ToLower();
            return await _context.Users
                .Where(u => u.FirstName.ToLower().Contains(normalized) || u.LastName.ToLower().Contains(normalized))
                .Take(20)
                .ToListAsync();
        }
    }
}