using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Common.Interfaces;
using ATS.Application.DTOs.Auth;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly IEmailVerificationService _emailVerificationService;

        public AuthController(IIdentityService identityService, IEmailVerificationService emailVerificationService)
        {
            _identityService = identityService;
            _emailVerificationService = emailVerificationService;
        }

        [HttpGet("companies")]
        public async Task<ActionResult<Result<List<CompanyDto>>>> GetCompanies()
        {
            var result = await _identityService.GetCompaniesAsync();
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<Result<AuthResponse>>> Register(RegisterRequest request)
        {
            var result = await _identityService.RegisterAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("send-otp")]
        public async Task<ActionResult<Result>> SendOtp(RegisterRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers["User-Agent"].ToString();
            var result = await _emailVerificationService.SendOtpAsync(request, ip, ua);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<Result<AuthResponse>>> VerifyOtp([FromBody] VerifyOtpModel model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers["User-Agent"].ToString();
            var result = await _emailVerificationService.VerifyOtpAsync(model.Email, model.Code, ip, ua);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("resend-otp")]
        public async Task<ActionResult<Result>> ResendOtp([FromBody] ResendOtpModel model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers["User-Agent"].ToString();
            var result = await _emailVerificationService.ResendOtpAsync(model.Email, ip, ua);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("check-status")]
        public async Task<ActionResult<Result<string>>> CheckStatus([FromQuery] string email)
        {
            var result = await _emailVerificationService.CheckStatusAsync(email);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<Result<AuthResponse>>> Login(LoginRequest request)
        {
            var result = await _identityService.LoginAsync(request);
            if (!result.IsSuccess)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<Result<AuthResponse>>> RefreshToken([FromBody] TokenModel model)
        {
            var result = await _identityService.RefreshTokenAsync(model.Token, model.RefreshToken);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<Result>> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var result = await _identityService.ForgotPasswordAsync(model.Email);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<Result>> ResetPassword(ResetPasswordRequest request)
        {
            var result = await _identityService.ResetPasswordAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("candidate/login-request")]
        public async Task<ActionResult<Result>> CandidateLoginRequest([FromBody] CandidateLoginRequestModel model)
        {
            var result = await _identityService.RequestCandidateLoginCodeAsync(model.Email);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("candidate/login-verify")]
        public async Task<ActionResult<Result<AuthResponse>>> CandidateLoginVerify([FromBody] CandidateLoginVerifyModel model)
        {
            var result = await _identityService.VerifyCandidateLoginCodeAsync(model.Email, model.Code);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("verify-domain")]
        public async Task<IActionResult> VerifyDomain([FromQuery] string token)
        {
            var result = await _identityService.VerifyDomainOwnershipAsync(token);
            if (!result.IsSuccess)
            {
                return Redirect($"http://localhost:4200/auth/login?error={System.Net.WebUtility.UrlEncode(result.Error)}");
            }
            return Redirect("http://localhost:4200/auth/login?verified=true");
        }

        [HttpPost("sso/resolve")]
        public async Task<ActionResult<Result<SsoResolutionDto>>> ResolveSso([FromBody] SsoResolveModel model)
        {
            var result = await _identityService.ResolveSsoAsync(model.Email);
            return Ok(result);
        }

        [HttpGet("sso/callback")]
        [HttpPost("sso/callback")]
        public async Task<IActionResult> SsoCallback([FromQuery] string? code, [FromForm] string? SAMLResponse)
        {
            var authorizationCode = code ?? SAMLResponse;
            if (string.IsNullOrEmpty(authorizationCode))
            {
                return Redirect("http://localhost:4200/auth/login?error=No+SSO+assertion+received");
            }

            var result = await _identityService.HandleSsoCallbackAsync(authorizationCode, code != null ? "OIDC" : "SAML");
            if (!result.IsSuccess)
            {
                return Redirect($"http://localhost:4200/auth/login?error={System.Net.WebUtility.UrlEncode(result.Error)}");
            }

            return Redirect($"http://localhost:4200/auth/login?token={result.Value.Token}&refreshToken={result.Value.RefreshToken}");
        }

        [HttpPost("mfa/setup")]
        public async Task<ActionResult<Result<MfaSetupDto>>> SetupMfa([FromBody] MfaSetupRequest model)
        {
            var result = await _identityService.GetMfaSetupAsync(model.Email);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("mfa/verify")]
        public async Task<ActionResult<Result<bool>>> VerifyMfa([FromBody] MfaVerifyRequest model)
        {
            var result = await _identityService.VerifyAndEnableMfaAsync(model.Email, model.Code);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("mfa/disable")]
        public async Task<ActionResult<Result<bool>>> DisableMfa([FromBody] MfaDisableRequest model)
        {
            var result = await _identityService.DisableMfaAsync(model.Email);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }

    public class SsoResolveModel
    {
        public string Email { get; set; }
    }

    public class TokenModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class ForgotPasswordModel
    {
        public string Email { get; set; }
    }

    public class CandidateLoginRequestModel
    {
        public string Email { get; set; }
    }

    public class CandidateLoginVerifyModel
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class MfaSetupRequest
    {
        public string Email { get; set; }
    }

    public class MfaVerifyRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class MfaDisableRequest
    {
        public string Email { get; set; }
    }

    public class VerifyOtpModel
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class ResendOtpModel
    {
        public string Email { get; set; }
    }
}
