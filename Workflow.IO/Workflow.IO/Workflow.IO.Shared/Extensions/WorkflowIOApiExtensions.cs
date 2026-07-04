using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Workflow.IO.Shared.Middleware;
using Workflow.IO.Shared.Contracts;

namespace Workflow.IO.Shared.Extensions
{
    public static class WorkflowIOApiExtensions
    {
        public static WebApplicationBuilder AddWorkflowIOApiDefaults(
            this WebApplicationBuilder builder,
            string serviceName,
            bool checkSql = true,
            bool checkRabbitMq = false)
        {
            builder.AddWorkflowIOSerilog(serviceName);

            builder.Services.AddWorkflowIOCors(builder.Configuration);

            builder.Services.AddWorkflowIOHealthChecks(
                builder.Configuration,
                checkSql,
                checkRabbitMq);

            // Register scoped Tenant Context for B2B multi-tenancy
            builder.Services.AddScoped<ITenantContext, TenantContext>();

            // Register Permission Engine
            builder.Services.AddSingleton<Workflow.IO.Shared.Authorization.IPermissionEngine, Workflow.IO.Shared.Authorization.PermissionEngine>();

            return builder;
        }

        public static IServiceCollection AddWorkflowIOCors(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var allowedOrigins =
                configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>()
                ?? new[]
                {
                    "http://localhost:4200",
                    "http://localhost:5173",
                    "http://127.0.0.1:4200"
                };

            services.AddCors(options =>
            {
                options.AddPolicy(
                    "WorkflowIOCors",
                    policy =>
                    {
                        policy
                            .WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            return services;
        }

        public static WebApplication UseWorkflowIOApiPipeline(
            this WebApplication app)
        {
            app.UseSerilogRequestLogging();

            app.UseMiddleware<CorrelationIdMiddleware>();

            app.UseMiddleware<TenantContextMiddleware>();

            app.UseGlobalExceptionHandling();

            // Only redirect to HTTPS in production – local dev runs on HTTP
            // and HTTPS redirect breaks SignalR negotiate
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("WorkflowIOCors");

            app.UseAuthentication();

            app.UseAuthorization();

            return app;
        }
    }
}
