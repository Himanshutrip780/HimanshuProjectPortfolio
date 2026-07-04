using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using JwtAuthenticationManager;
using JwtAuthenticationManager.Model;
using JwtAuthenticationManager.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApi.Model.Domian.Entities;
using UserApi.Model.Dto;
using UserApi.Repositories;
using UserApi.Service;
using Workflow.IO.Shared.Contracts;
using UserApi.Data;

namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ ADDED → Entire controller protected by JWT
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly JwtTokenHandler _jwtAuthenticationManager;
        private readonly IEmailVerificationService _emailVerificationService;

        public UserController(
            IUserService userService,
            IMapper mapper,
            JwtTokenHandler jwtAuthenticationManager,
            IEmailVerificationService emailVerificationService)
        {
            _userService = userService;
            _mapper = mapper;
            _jwtAuthenticationManager = jwtAuthenticationManager;
            _emailVerificationService = emailVerificationService;
        }

        [AllowAnonymous] // ✅ ADDED → Login accessible without token
        [HttpPost("authenticate")]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> LoginUser(
            [FromBody] AuthenticationRequest user,
            [FromServices] IUserAccountRepository userAccountRepository,
            [FromServices] IUserRepository userRepository,
            [FromServices] ITenantContext tenantContext)
        {
            var authenticationResponse = await _jwtAuthenticationManager.GenerateToken(user);

            if (authenticationResponse == null)
            {
                return Unauthorized();
            }

            // Valid credentials. Verify organization membership if multi-tenancy is active
            if (tenantContext.CurrentOrganizationId.HasValue)
            {
                var account = await userAccountRepository.GetByEmailAsync(user.Email.Trim().ToLower());
                if (account != null)
                {
                    var userProfile = await userRepository.GetUserByIdAsync(account.Id);
                    if (userProfile == null || userProfile.OrganizationId != tenantContext.CurrentOrganizationId.Value)
                    {
                        return BadRequest(ErrorResponse.Create("You do not belong to this workspace.", HttpContext.TraceIdentifier));
                    }
                }
            }

            return Ok(
                ApiResponse<AuthenticationResponse>.Ok(
                    authenticationResponse,
                    "Login successful"));
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>>
            RefreshToken(
                [FromBody]
                RefreshTokenRequest request)
        {
            var authenticationResponse =
                await _jwtAuthenticationManager
                    .RefreshTokenAsync(request);

            if (authenticationResponse == null)
            {
                return Unauthorized();
            }

            return Ok(
                ApiResponse<AuthenticationResponse>.Ok(
                    authenticationResponse,
                    "Token refreshed successfully"));
        }

        [AllowAnonymous] // ✅ ADDED → Register accessible without token
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserRequestDTO request)
        {
            var response =
                await _userService.RegisterUserAsync(request);

            return Ok(
                ApiResponse<UserDto>.Ok(
                    response,
                    "User registered successfully"));
        }

        [AllowAnonymous]
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtpAsync([FromBody] RegisterUserRequestDTO request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers["User-Agent"].ToString();

            await _emailVerificationService.SendOtpAsync(request, ip, ua);

            return Ok(ApiResponse<object>.Ok(null!, "Verification code sent successfully."));
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtpAsync([FromBody] VerifyOtpRequestDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers["User-Agent"].ToString();

            var userDto = await _emailVerificationService.VerifyOtpAsync(request.Email, request.Code, ip, ua);

            return Ok(ApiResponse<UserDto>.Ok(userDto, "Email verified and registration completed."));
        }

        [AllowAnonymous]
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtpAsync([FromBody] ResendOtpRequestDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers["User-Agent"].ToString();

            await _emailVerificationService.ResendOtpAsync(request.Email, ip, ua);

            return Ok(ApiResponse<object>.Ok(null!, "Verification code resent successfully."));
        }

        [AllowAnonymous]
        [HttpGet("check-status")]
        public async Task<IActionResult> CheckStatusAsync([FromQuery] string email)
        {
            var status = await _emailVerificationService.CheckStatusAsync(email);
            return Ok(ApiResponse<string>.Ok(status, "Status retrieved successfully."));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var email =
                User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            var profile =
                await _userService.GetUserProfileAsync(
                    userId.Value,
                    email);

            if (profile == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<UserProfileDto>.Ok(profile));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("me/organization")]
        public async Task<IActionResult> GetMyOrganizationAsync(
            [FromServices] IOrganizationRepository organizationRepository)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var organization = await organizationRepository.GetUserOrganizationAsync(userId.Value);
            
            if (organization == null)
                return NotFound();

            return Ok(ApiResponse<Organization>.Ok(organization));
        }

        [AllowAnonymous]
        [HttpGet("organizations/by-subdomain/{subdomain}")]
        public async Task<IActionResult> GetOrganizationBySubdomainAsync(
            string subdomain,
            [FromServices] UserDbContext dbContext)
        {
            var org = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                dbContext.Organizations, 
                o => o.Subdomain == subdomain);

            if (org == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<Organization>.Ok(org));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUserAsync(
            [FromBody] UpdateProfileRequestDto request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var updatedUser =
                await _userService.UpdateProfileAsync(
                    userId.Value,
                    request);

            if (updatedUser == null)
            {
                return NotFound();
            }

            var email =
                User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            var profile =
                await _userService.GetUserProfileAsync(
                    userId.Value,
                    email);

            if (profile == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<UserProfileDto>.Ok(
                    profile,
                    "Profile updated successfully"));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("me/change-password")]
        public async Task<IActionResult> ChangePasswordAsync(
            [FromBody] ChangePasswordRequestDto request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            await _userService.ChangePasswordAsync(
                userId.Value,
                request);

            return Ok(
                ApiResponse<object>.Ok(
                    null!,
                    "Password changed successfully"));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("lookup")]
        public async Task<IActionResult> LookupUsersAsync(
            [FromQuery] string email)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var users =
                await _userService.LookupUsersAsync(email, userId.Value);

            return Ok(
                ApiResponse<IEnumerable<UserLookupDto>>.Ok(users));
        }

        [Authorize(Roles = "User,Admin")] // ✅ CHANGED → Allow normal users to lookup team members
        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await _userService.GetAllUsersAsync();

            if (users == null || !users.Any())
            {
                return NoContent();
            }

            return Ok(
                ApiResponse<IEnumerable<UserDto>>.Ok(users));
        }

        [Authorize(Roles = "User,Admin")] // ✅ ADDED
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid userId)
        {
            var user =
                await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<UserDto>.Ok(user));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUserAsync(Guid userId, [FromBody] RegisterUserRequestDTO request)
        {
            var updatedUser =
                await _userService.UpdateUserAsync(userId, request);

            if (updatedUser == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<UserDto>.Ok(
                    updatedUser,
                    "User updated successfully"));
        }

        [Authorize(Roles = "Admin")] // ✅ CHANGED → Only Admin can delete
        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> DeleteUserAsync(Guid userId)
        {
            var deleted = await _userService.DeleteUserAsync(userId);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return userId;
        }

        private async Task<UserDto?> ResolveCurrentUserAsync()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return null;
            }

            var user =
                await _userService.GetUserByIdAsync(userId.Value);

            if (user != null)
            {
                return user;
            }

            var email =
                User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return await _userService.GetUserByEmailAsync(email);
        }

        //[HttpGet("validate")]
        //public ActionResult<List<User>> GetAllUsersForValidation()
        //{
        //    var users = _userService.GetUsers();

        //    return Ok(users);
        //}
    }

    public class VerifyOtpRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class ResendOtpRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
