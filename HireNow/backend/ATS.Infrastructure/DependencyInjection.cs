using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ATS.Application.Common.Interfaces;
using ATS.Infrastructure.Identity;
using ATS.Infrastructure.Storage;
using ATS.Infrastructure.Email;
using ATS.Infrastructure.AI;
using System;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace ATS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
            if (string.IsNullOrEmpty(jwtSecret))
            {
                jwtSecret = configuration.GetValue<string>("JwtSettings:Secret");
            }
            if (string.IsNullOrEmpty(jwtSecret))
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                if (env.Equals("Development", StringComparison.OrdinalIgnoreCase))
                {
                    jwtSecret = "super_secret_key_for_development_mode_only_needs_to_be_long_enough";
                }
                else
                {
                    throw new InvalidOperationException("JWT Secret not found. Please set JWT_SECRET environment variable.");
                }
            }

            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            jwtSettings.Secret = jwtSecret;

            services.Configure<JwtSettings>(options =>
            {
                options.Secret = jwtSettings.Secret;
                options.Issuer = jwtSettings.Issuer;
                options.Audience = jwtSettings.Audience;
                options.ExpiryMinutes = jwtSettings.ExpiryMinutes;
            });

            // Configure JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal?.Identity as ClaimsIdentity;
                        if (identity != null)
                        {
                            // Map 'sub' to NameIdentifier if NameIdentifier is not present
                            if (!identity.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
                            {
                                var subClaim = identity.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == "sub");
                                if (subClaim != null)
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
                                }
                            }
                            
                            // Map 'role' to ClaimTypes.Role if Role is not present
                            if (!identity.Claims.Any(c => c.Type == ClaimTypes.Role))
                            {
                                var roleClaims = identity.Claims.Where(c => c.Type == "role").ToList();
                                foreach (var roleClaim in roleClaims)
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                                }
                            }

                            // Map 'email' to ClaimTypes.Email if Email is not present
                            if (!identity.Claims.Any(c => c.Type == ClaimTypes.Email))
                            {
                                var emailClaim = identity.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == "email");
                                if (emailClaim != null)
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim.Value));
                                }
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Register Services
            services.AddScoped<ITenantProvider, ATS.Infrastructure.Services.TenantProvider>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IEmailVerificationService, EmailVerificationService>();
            services.AddSingleton<IStorageService, LocalFileStorageService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddHttpClient<IAIEngineService, AIEngineService>();
            services.AddScoped<ICandidateSearchService, ATS.Infrastructure.Services.CandidateSearchService>();
            services.AddHostedService<ATS.Infrastructure.BackgroundJobs.EmailOutboxProcessor>();

            return services;
        }
    }
}
