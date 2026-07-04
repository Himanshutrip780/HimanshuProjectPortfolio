using System.Threading.Tasks;
using UserApi.Model.Dto;

namespace UserApi.Service
{
    public interface IEmailVerificationService
    {
        Task SendOtpAsync(RegisterUserRequestDTO request, string? ipAddress, string? userAgent);
        Task<UserDto> VerifyOtpAsync(string email, string code, string? ipAddress, string? userAgent);
        Task ResendOtpAsync(string email, string? ipAddress, string? userAgent);
        Task<string> CheckStatusAsync(string email);
    }
}
