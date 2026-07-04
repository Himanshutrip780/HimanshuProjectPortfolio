using System.Threading.Tasks;
using ATS.Application.DTOs.Auth;
using ATS.Shared.Models;

namespace ATS.Application.Common.Interfaces
{
    public interface IEmailVerificationService
    {
        Task<Result> SendOtpAsync(RegisterRequest request, string? ipAddress, string? userAgent);
        Task<Result<AuthResponse>> VerifyOtpAsync(string email, string code, string? ipAddress, string? userAgent);
        Task<Result> ResendOtpAsync(string email, string? ipAddress, string? userAgent);
        Task<Result<string>> CheckStatusAsync(string email);
    }
}
