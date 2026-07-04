using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Workflow.IO.Shared.Extensions
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddCustomJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSecurityKey =
                configuration["Jwt:SecurityKey"];

            if (string.IsNullOrWhiteSpace(jwtSecurityKey))
            {
                throw new InvalidOperationException(
                    "Jwt:SecurityKey must be configured.");
            }

            var issuer =
                configuration["Jwt:Issuer"] ?? "https://workflow.io.local";

            var audience =
                configuration["Jwt:Audience"] ?? "workflow.io-api";

            var key =
                Encoding.UTF8.GetBytes(jwtSecurityKey);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata =
                        configuration.GetValue<bool>(
                            "Jwt:RequireHttpsMetadata");

                    options.SaveToken = true;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(key),

                            ValidateIssuer = true,

                            ValidIssuer = issuer,

                            ValidateAudience = true,

                            ValidAudience = audience,

                            ValidateLifetime = true,

                            ClockSkew = TimeSpan.FromMinutes(1)
                        };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
