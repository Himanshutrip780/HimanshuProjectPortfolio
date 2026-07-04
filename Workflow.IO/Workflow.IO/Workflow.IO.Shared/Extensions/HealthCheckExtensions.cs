using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Workflow.IO.Shared.IntegrationEvents;

namespace Workflow.IO.Shared.Extensions
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddWorkflowIOHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration,
            bool includeSql = true,
            bool includeRabbitMq = false)
        {
            var healthChecks = services.AddHealthChecks();

            if (includeSql)
            {
                var connectionString =
                    configuration.GetConnectionString(
                        "DefaultConnection");

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    healthChecks.AddNpgSql(
                        connectionString,
                        name: "postgres",
                        tags: new[] { "ready", "db" });
                }
            }

            if (includeRabbitMq)
            {
                var rabbitOptions =
                    configuration
                        .GetSection("RabbitMq")
                        .Get<RabbitMqOptions>()
                    ?? new RabbitMqOptions();

                var rabbitConnection =
                    $"amqp://{rabbitOptions.UserName}:{rabbitOptions.Password}@{rabbitOptions.HostName}:{rabbitOptions.Port}";

                healthChecks.AddRabbitMQ(
                    rabbitConnectionString: rabbitConnection,
                    name: "rabbitmq",
                    tags: new[] { "ready", "messaging" });
            }

            return services;
        }

        public static IEndpointRouteBuilder MapWorkflowIOHealthChecks(
            this IEndpointRouteBuilder endpoints,
            string path = "/health")
        {
            endpoints.MapHealthChecks(
                path,
                new HealthCheckOptions
                {
                    ResponseWriter = WriteHealthResponseAsync
                });

            endpoints.MapHealthChecks(
                "/ready",
                new HealthCheckOptions
                {
                    Predicate = check =>
                        check.Tags.Contains("ready"),
                    ResponseWriter = WriteHealthResponseAsync
                });

            return endpoints;
        }

        private static async Task WriteHealthResponseAsync(
            HttpContext context,
            HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(
                    entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        duration = entry.Value.Duration.TotalMilliseconds,
                        error = entry.Value.Exception?.Message
                    }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    response,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
        }
    }
}
