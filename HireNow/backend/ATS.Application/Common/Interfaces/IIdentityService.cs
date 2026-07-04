using System.Collections.Generic;
using System.Threading.Tasks;
using ATS.Application.DTOs.Auth;
using ATS.Shared.Models;

namespace ATS.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
        Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
        Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken);
        Task<Result> ForgotPasswordAsync(string email);
        Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
        Task<Result<List<CompanyDto>>> GetCompaniesAsync();
        Task<Result> RequestCandidateLoginCodeAsync(string email);
        Task<Result<AuthResponse>> VerifyCandidateLoginCodeAsync(string email, string code);
        Task<Result<bool>> VerifyDomainOwnershipAsync(string token);
        Task<Result<SsoResolutionDto>> ResolveSsoAsync(string email);
        Task<Result<AuthResponse>> HandleSsoCallbackAsync(string code, string provider);
        Task<Result<MfaSetupDto>> GetMfaSetupAsync(string email);
        Task<Result<bool>> VerifyAndEnableMfaAsync(string email, string code);
        Task<Result<bool>> DisableMfaAsync(string email);
    }
}
