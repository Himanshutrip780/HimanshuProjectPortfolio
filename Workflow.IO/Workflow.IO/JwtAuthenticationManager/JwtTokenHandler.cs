using JwtAuthenticationManager.Model;
using JwtAuthenticationManager.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtAuthenticationManager
{
    public class JwtTokenHandler
    {
        private const int JWT_TOKEN_VALIDITY_MINS = 60;

        private const int REFRESH_TOKEN_VALIDITY_DAYS = 7;

        private readonly IUserAccountRepository _userAccountRepository;

        private readonly IConfiguration _configuration;

        public JwtTokenHandler(
            IUserAccountRepository userAccountRepository,
            IConfiguration configuration)
        {
            _userAccountRepository = userAccountRepository;

            _configuration = configuration;
        }

        public async Task<AuthenticationResponse?> GenerateToken(
            AuthenticationRequest authenticationRequest)
        {
            if (string.IsNullOrWhiteSpace(
                    authenticationRequest.Email) ||
                string.IsNullOrWhiteSpace(
                    authenticationRequest.Password))
            {
                return null;
            }

            var user =
                await _userAccountRepository.GetByEmailAsync(
                    authenticationRequest.Email.Trim().ToLower());

            if (user == null || !user.IsActive)
            {
                return null;
            }

            var isPasswordValid =
                BCrypt.Net.BCrypt.Verify(
                    authenticationRequest.Password,
                    user.PasswordHash);

            if (!isPasswordValid)
            {
                return null;
            }

            return await IssueTokenPairAsync(user);
        }

        public async Task<AuthenticationResponse?> RefreshTokenAsync(
            RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.RefreshToken))
            {
                return null;
            }

            var refreshTokenHash =
                HashRefreshToken(request.RefreshToken);

            var storedToken =
                await _userAccountRepository
                    .GetRefreshTokenByHashAsync(
                        refreshTokenHash);

            if (storedToken == null ||
                storedToken.IsExpired ||
                storedToken.IsRevoked)
            {
                return null;
            }

            var user =
                await _userAccountRepository
                    .GetByIdAsync(storedToken.UserAccountId);

            if (user == null || !user.IsActive)
            {
                return null;
            }

            await _userAccountRepository
                .RevokeRefreshTokenAsync(storedToken);

            return await IssueTokenPairAsync(user);
        }

        private async Task<AuthenticationResponse> IssueTokenPairAsync(
            UserAccount user)
        {
            var tokenExpiryTimeStamp =
                DateTime.UtcNow.AddMinutes(
                    JWT_TOKEN_VALIDITY_MINS);

            var token = CreateJwtToken(
                user,
                tokenExpiryTimeStamp);

            var refreshToken =
                Convert.ToBase64String(
                    RandomNumberGenerator.GetBytes(64));

            await _userAccountRepository
                .CreateRefreshTokenAsync(
                    new RefreshToken(
                        user.Id,
                        HashRefreshToken(refreshToken),
                        DateTime.UtcNow.AddDays(
                            REFRESH_TOKEN_VALIDITY_DAYS)));

            return new AuthenticationResponse
            {
                Email = user.Email,
                JwtToken = token,
                RefreshToken = refreshToken,
                ExpiresIn =
                    (int)tokenExpiryTimeStamp
                        .Subtract(DateTime.UtcNow)
                        .TotalSeconds,
                Role = user.Role.ToString()
            };
        }

        private string CreateJwtToken(
            UserAccount user,
            DateTime tokenExpiryTimeStamp)
        {
            var jwtSecurityKey =
                _configuration["Jwt:SecurityKey"];

            if (string.IsNullOrWhiteSpace(jwtSecurityKey))
            {
                throw new InvalidOperationException(
                    "JWT security key is not configured");
            }

            var tokenKey =
                Encoding.ASCII.GetBytes(jwtSecurityKey);

            var claimsIdentity =
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim(
                        JwtRegisteredClaimNames.Email,
                        user.Email),

                    new Claim(
                        ClaimTypes.Role,
                        user.Role.ToString()),

                    new Claim(
                        ClaimTypes.NameIdentifier,
                        user.Id.ToString())
                });

            var signingCredentials =
                new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature);

            var issuer =
                _configuration["Jwt:Issuer"] ?? "https://workflow.io.local";

            var audience =
                _configuration["Jwt:Audience"] ?? "workflow.io-api";

            var securityTokenDescriptor =
                new SecurityTokenDescriptor
                {
                    Subject = claimsIdentity,
                    Expires = tokenExpiryTimeStamp,
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = signingCredentials
                };

            var jwtSecurityTokenHandler =
                new JwtSecurityTokenHandler();

            var securityToken =
                jwtSecurityTokenHandler.CreateToken(
                    securityTokenDescriptor);

            return jwtSecurityTokenHandler.WriteToken(
                securityToken);
        }

        private static string HashRefreshToken(
            string refreshToken)
        {
            var bytes =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(refreshToken));

            return Convert.ToHexString(bytes);
        }
    }
}
