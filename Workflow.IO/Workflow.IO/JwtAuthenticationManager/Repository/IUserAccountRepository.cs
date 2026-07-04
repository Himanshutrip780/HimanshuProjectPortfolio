using JwtAuthenticationManager.Model;

namespace JwtAuthenticationManager.Repository
{
    public interface IUserAccountRepository
    {
        Task CreateAsync(UserAccount userAccount);

        Task<UserAccount?> GetByEmailAsync(string email);

        Task<IEnumerable<UserAccount>> SearchByEmailAsync(
            string emailQuery,
            int limit = 20);

        Task<UserAccount?> GetByIdAsync(Guid userAccountId);

        Task<bool> ExistsByEmailAsync(string email);

        Task UpdateAsync(UserAccount userAccount);

        Task CreateRefreshTokenAsync(RefreshToken refreshToken);

        Task<RefreshToken?> GetRefreshTokenByHashAsync(
            string tokenHash);

        Task RevokeRefreshTokenAsync(
            RefreshToken refreshToken);
    }
}
