using JwtAuthenticationManager.Data;
using JwtAuthenticationManager.Model;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthenticationManager.Repository
{
    public class UserAccountRepository: IUserAccountRepository
    {
        private readonly AuthDbContext _context;

        public UserAccountRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(UserAccount userAccount)
        {
            await _context.UserAccounts.AddAsync(userAccount);

            await _context.SaveChangesAsync();
        }

        public async Task<UserAccount?> GetByEmailAsync(string email)
        {
            return await _context.UserAccounts.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<IEnumerable<UserAccount>> SearchByEmailAsync(
            string emailQuery,
            int limit = 20)
        {
            var normalized = emailQuery.Trim().ToLower();

            return await _context.UserAccounts
                .Where(x => x.Email.Contains(normalized))
                .OrderBy(x => x.Email)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<UserAccount?> GetByIdAsync(Guid userAccountId)
        {
            return await _context.UserAccounts
                .FirstOrDefaultAsync(x => x.Id == userAccountId);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.UserAccounts.AnyAsync(x => x.Email == email);
        }

        public async Task UpdateAsync(UserAccount userAccount)
        {
            _context.UserAccounts.Update(userAccount);

            await _context.SaveChangesAsync();
        }

        public async Task CreateRefreshTokenAsync(
            RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);

            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenByHashAsync(
            string tokenHash)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(
                    x => x.TokenHash == tokenHash);
        }

        public async Task RevokeRefreshTokenAsync(
            RefreshToken refreshToken)
        {
            refreshToken.Revoke();

            await _context.SaveChangesAsync();
        }
    }
}
