using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ATS.Application.Common.Interfaces;
using ATS.Application.DTOs.Auth;
using ATS.Domain.Entities;
using ATS.Shared.Constants;
using ATS.Shared.Models;

namespace ATS.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IApplicationDbContext context,
            IOptions<JwtSettings> jwtSettings,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
        }

        private string GetEmailDomain(string email)
        {
            if (string.IsNullOrEmpty(email)) return string.Empty;
            var atIndex = email.IndexOf('@');
            if (atIndex == -1 || atIndex >= email.Length - 1) return string.Empty;
            return email.Substring(atIndex + 1).ToLower().Trim();
        }

        private bool IsGenericDomain(string domain)
        {
            var genericDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "aol.com", "icloud.com", "mail.com"
            };
            return genericDomains.Contains(domain);
        }

        public async Task<Result<bool>> VerifyDomainOwnershipAsync(string token)
        {
            var request = await _context.TenantVerificationRequests
                .FirstOrDefaultAsync(r => r.Token == token && !r.IsUsed);

            if (request == null)
            {
                return Result<bool>.Failure("Invalid or already used verification token.");
            }

            if (request.ExpiresAt < DateTime.UtcNow)
            {
                return Result<bool>.Failure("Verification token has expired.");
            }

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result<bool>.Failure("User not found.");
            }

            // Map user to the correct company and clear pending flag
            user.CompanyId = request.CompanyId;
            user.IsPendingVerification = false;
            
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result<bool>.Failure(result.Errors.Select(e => e.Description));
            }

            request.IsUsed = true;
            await _context.SaveChangesAsync(default);

            return Result<bool>.Success(true);
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<AuthResponse>.Failure("Email is already registered.");
            }

            Company company;
            bool isPendingVerification = false;

            if (request.CompanyId.HasValue && request.CompanyId.Value != Guid.Empty)
            {
                company = await _context.Companies.FindAsync(request.CompanyId.Value);
                if (company == null)
                {
                    return Result<AuthResponse>.Failure("Selected organization does not exist.");
                }

                // Selected existing company -> if email matches company domain, require verification
                var emailDomain = GetEmailDomain(request.Email);
                if (!string.IsNullOrEmpty(emailDomain) && !IsGenericDomain(emailDomain) &&
                    string.Equals(company.Domain, emailDomain, StringComparison.OrdinalIgnoreCase))
                {
                    isPendingVerification = true;
                }
            }
            else
            {
                var domain = request.Domain;
                if (string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(request.Email))
                {
                    domain = GetEmailDomain(request.Email);
                }

                var existingCompany = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == request.CompanyName.ToLower() ||
                                              (!string.IsNullOrEmpty(domain) && c.Domain.ToLower() == domain.ToLower()));

                if (existingCompany != null)
                {
                    company = existingCompany;
                    // Corporate domain matches an existing tenant -> put user in pending verification
                    var emailDomain = GetEmailDomain(request.Email);
                    if (!string.IsNullOrEmpty(emailDomain) && !IsGenericDomain(emailDomain) &&
                        string.Equals(company.Domain, emailDomain, StringComparison.OrdinalIgnoreCase))
                    {
                        isPendingVerification = true;
                    }
                }
                else
                {
                    company = new Company
                    {
                        Name = request.CompanyName,
                        Domain = domain ?? string.Empty,
                        SubscriptionPlan = "Free Trial",
                        CreatedBy = request.Email,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _context.Companies.AddAsync(company);
                    await _context.SaveChangesAsync(default);
                }
            }

            var assignedRole = request.Role ?? Roles.SuperAdmin;
            if (!await _roleManager.RoleExistsAsync(assignedRole))
            {
                await _roleManager.CreateAsync(new ApplicationRole(assignedRole));
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = !isPendingVerification,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CompanyId = company.Id,
                Role = assignedRole,
                IsPendingVerification = isPendingVerification,
                CreatedBy = request.Email,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return Result<AuthResponse>.Failure(result.Errors.Select(e => e.Description));
            }

            await _userManager.AddToRoleAsync(user, assignedRole);

            if (isPendingVerification)
            {
                var code = new Random().Next(100000, 999999).ToString();
                var token = Guid.NewGuid().ToString("N");

                var verificationRequest = new TenantVerificationRequest
                {
                    UserId = user.Id,
                    CompanyId = company.Id,
                    Email = user.Email,
                    VerificationCode = code,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    IsUsed = false
                };

                await _context.TenantVerificationRequests.AddAsync(verificationRequest);
                await _context.SaveChangesAsync(default);

                // Send verification code to email
                var confirmationLink = $"http://localhost:5000/api/auth/verify-domain?token={token}";
                var body = $"Please verify corporate domain ownership for company {company.Name} by clicking this link: <a href='{confirmationLink}'>{confirmationLink}</a><br/>Your verification code is: <strong>{code}</strong>";
                
                try
                {
                    await _emailService.SendEmailAsync(user.Email, "Verify Corporate Domain Ownership", body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Email Error] Failed to send domain verification: {ex.Message}");
                }

                return Result<AuthResponse>.Success(new AuthResponse
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    CompanyId = company.Id,
                    Id = user.Id.ToString(),
                    IsPendingVerification = true
                });
            }

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<AuthResponse>.Failure("Invalid credentials.");
            }

            if (user.IsPendingVerification)
            {
                return Result<AuthResponse>.Failure("Domain ownership verification is pending. Please verify your email first.");
            }

            var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isValidPassword)
            {
                return Result<AuthResponse>.Failure("Invalid credentials.");
            }

            if (user.TwoFactorEnabled)
            {
                if (string.IsNullOrEmpty(request.TwoFactorCode))
                {
                    return Result<AuthResponse>.Success(new AuthResponse
                    {
                        Email = user.Email!,
                        RequiresTwoFactor = true,
                        Id = user.Id.ToString()
                    });
                }

                var isValidMfa = await _userManager.VerifyTwoFactorTokenAsync(
                    user, TokenOptions.DefaultAuthenticatorProvider, request.TwoFactorCode);

                if (!isValidMfa)
                {
                    return Result<AuthResponse>.Failure("Invalid two-factor authentication code.");
                }
            }

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
            {
                return Result<AuthResponse>.Failure("Invalid access token or refresh token.");
            }

            var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue(ClaimTypes.Name);
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                if (user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return Result<AuthResponse>.Failure("Invalid access token or refresh token.");
                }

                return await GenerateAuthResponseAsync(user);
            }

            // If user is not found in AspNetUsers, check Candidates table
            var candidate = await _context.Candidates
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());

            if (candidate == null || candidate.RefreshToken != refreshToken || candidate.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Result<AuthResponse>.Failure("Invalid access token or refresh token.");
            }

            // Generate token for candidate
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, candidate.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, candidate.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, Roles.Candidate),
                new Claim("FirstName", candidate.FirstName),
                new Claim("LastName", candidate.LastName),
                new Claim("CompanyId", candidate.CompanyId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var newToken = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(newToken);
            var newRefreshToken = GenerateRefreshToken();

            candidate.RefreshToken = newRefreshToken;
            candidate.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _context.Candidates.Update(candidate);
            await _context.SaveChangesAsync(default);

            return Result<AuthResponse>.Success(new AuthResponse
            {
                Token = jwtToken,
                RefreshToken = newRefreshToken,
                Email = candidate.Email,
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Role = Roles.Candidate,
                CompanyId = candidate.CompanyId,
                Id = candidate.Id.ToString()
            });
        }

        public async Task<Result> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Return success for security to avoid email enumeration
                return Result.Success();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // In production, send email with reset link including token. 
            // For now, we will log it.
            Console.WriteLine($"Password reset token for {email}: {token}");
            return Result.Success();
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(e => e.Description));
            }

            return Result.Success();
        }

        private async Task<Result<AuthResponse>> GenerateAuthResponseAsync(ApplicationUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("CompanyId", user.CompanyId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            return Result<AuthResponse>.Success(new AuthResponse
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                CompanyId = user.CompanyId,
                Id = user.Id.ToString()
            });
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateLifetime = false // We validate lifetime manually or allow reading expired token to refresh it
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Result<List<CompanyDto>>> GetCompaniesAsync()
        {
            var companies = await _context.Companies
                .Select(c => new CompanyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Domain = c.Domain
                })
                .ToListAsync();

            return Result<List<CompanyDto>>.Success(companies);
        }

        public async Task<Result> RequestCandidateLoginCodeAsync(string email)
        {
            var candidate = await _context.Candidates
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());

            if (candidate == null)
            {
                return Result.Failure("No candidate profile found with this email.");
            }

            var code = new Random().Next(100000, 999999).ToString();
            candidate.LoginToken = code;
            candidate.LoginTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            _context.Candidates.Update(candidate);
            await _context.SaveChangesAsync(default);

            Console.WriteLine($"[Candidate Login Code] Sent login code to {email}: {code}");

            return Result.Success();
        }

        public async Task<Result<AuthResponse>> VerifyCandidateLoginCodeAsync(string email, string code)
        {
            var candidate = await _context.Candidates
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());

            if (candidate == null)
            {
                return Result<AuthResponse>.Failure("Invalid email or code.");
            }

            if (candidate.LoginToken != code || !candidate.LoginTokenExpiry.HasValue || candidate.LoginTokenExpiry.Value < DateTime.UtcNow)
            {
                return Result<AuthResponse>.Failure("Invalid or expired login code.");
            }

            candidate.LoginToken = null;
            candidate.LoginTokenExpiry = null;

            _context.Candidates.Update(candidate);
            await _context.SaveChangesAsync(default);

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, candidate.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, candidate.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, Roles.Candidate),
                new Claim("FirstName", candidate.FirstName),
                new Claim("LastName", candidate.LastName),
                new Claim("CompanyId", candidate.CompanyId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();
 
            candidate.RefreshToken = refreshToken;
            candidate.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _context.Candidates.Update(candidate);
            await _context.SaveChangesAsync(default);
 
            return Result<AuthResponse>.Success(new AuthResponse
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                Email = candidate.Email,
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Role = Roles.Candidate,
                CompanyId = candidate.CompanyId,
                Id = candidate.Id.ToString()
            });
        }

        public async Task<Result<SsoResolutionDto>> ResolveSsoAsync(string email)
        {
            var emailDomain = GetEmailDomain(email);
            if (string.IsNullOrEmpty(emailDomain) || IsGenericDomain(emailDomain))
            {
                return Result<SsoResolutionDto>.Success(new SsoResolutionDto { SsoEnabled = false });
            }

            var company = await _context.Companies
                .IgnoreQueryFilters() // Bypass tenant isolation to check domain
                .FirstOrDefaultAsync(c => c.Domain.ToLower() == emailDomain.ToLower());

            if (company == null || !company.SsoEnabled)
            {
                return Result<SsoResolutionDto>.Success(new SsoResolutionDto { SsoEnabled = false });
            }

            var redirectUrl = $"{company.SsoRedirectUrl}?state={company.Id}&client_id={company.SsoClientId}&redirect_uri=http://localhost:5000/api/auth/sso/callback";

            return Result<SsoResolutionDto>.Success(new SsoResolutionDto
            {
                SsoEnabled = true,
                SsoProvider = company.SsoProvider ?? "OIDC",
                RedirectUrl = redirectUrl
            });
        }

        public async Task<Result<AuthResponse>> HandleSsoCallbackAsync(string code, string provider)
        {
            string email;
            if (code.StartsWith("mock_sso_code_for_", StringComparison.OrdinalIgnoreCase))
            {
                email = code.Substring("mock_sso_code_for_".Length);
            }
            else
            {
                email = code; // Fallback
            }

            if (string.IsNullOrEmpty(email))
            {
                return Result<AuthResponse>.Failure("SSO callback code is invalid.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Create user automatically for matched domain company
                var emailDomain = GetEmailDomain(email);
                var company = await _context.Companies
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Domain.ToLower() == emailDomain.ToLower());
                
                if (company == null)
                {
                    return Result<AuthResponse>.Failure($"No registered company matches email domain '{emailDomain}'.");
                }

                if (!company.SsoEnabled)
                {
                    return Result<AuthResponse>.Failure($"SSO is not enabled for company domain '{emailDomain}'.");
                }

                var assignedRole = Roles.Recruiter; // Default role
                if (!await _roleManager.RoleExistsAsync(assignedRole))
                {
                    await _roleManager.CreateAsync(new ApplicationRole(assignedRole));
                }

                var parts = email.Split('@')[0].Split('.');
                var firstName = parts.Length > 0 ? parts[0] : "SSO";
                var lastName = parts.Length > 1 ? parts[1] : "User";

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(firstName),
                    LastName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lastName),
                    CompanyId = company.Id,
                    Role = assignedRole,
                    IsPendingVerification = false,
                    CreatedBy = "SSO",
                    CreatedDate = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, Guid.NewGuid().ToString() + "1aA!");
                if (!createResult.Succeeded)
                {
                    return Result<AuthResponse>.Failure(createResult.Errors.Select(e => e.Description));
                }

                await _userManager.AddToRoleAsync(user, assignedRole);
            }
            else
            {
                // User exists, make sure they are verified
                user.IsPendingVerification = false;
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<Result<MfaSetupDto>> GetMfaSetupAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<MfaSetupDto>.Failure("User not found.");
            }

            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var emailEncoder = System.Text.Encodings.Web.UrlEncoder.Default;
            var authenticatorUri = string.Format(
                "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                emailEncoder.Encode("HireNow"),
                emailEncoder.Encode(user.Email!),
                unformattedKey);

            return Result<MfaSetupDto>.Success(new MfaSetupDto
            {
                SharedKey = unformattedKey!,
                AuthenticatorUri = authenticatorUri
            });
        }

        public async Task<Result<bool>> VerifyAndEnableMfaAsync(string email, string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<bool>.Failure("User not found.");
            }

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, TokenOptions.DefaultAuthenticatorProvider, code);

            if (!isValid)
            {
                return Result<bool>.Failure("Invalid verification code.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DisableMfaAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<bool>.Failure("User not found.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            return Result<bool>.Success(true);
        }
    }
}
