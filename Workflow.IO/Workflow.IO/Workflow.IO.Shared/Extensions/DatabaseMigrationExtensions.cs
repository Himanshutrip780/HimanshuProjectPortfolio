using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Workflow.IO.Shared.Extensions
{
    public static class DatabaseMigrationExtensions
    {
        public static async Task<WebApplication> ApplyMigrationsOnStartupAsync<TContext>(
            this WebApplication app)
            where TContext : DbContext
        {
            var shouldApplyMigrations =
                app.Configuration.GetValue<bool>(
                    "Database:ApplyMigrationsOnStartup");

            if (!shouldApplyMigrations)
            {
                return app;
            }

            await using var scope =
                app.Services.CreateAsyncScope();

            var logger =
                scope.ServiceProvider
                    .GetRequiredService<ILogger<TContext>>();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<TContext>();

            logger.LogInformation(
                "Applying database migrations for {DbContext}",
                typeof(TContext).Name);

            var retryCount = 0;
            const int maxRetries = 6;
            var delay = TimeSpan.FromSeconds(5);

            while (true)
            {
                try
                {
                    await dbContext.Database.MigrateAsync();
                    break;
                }
                catch (Exception ex) when (retryCount < maxRetries)
                {
                    retryCount++;
                    logger.LogWarning(
                        ex,
                        "Failed to apply database migrations for {DbContext}. Retrying ({RetryCount}/{MaxRetries}) in {Delay}s...",
                        typeof(TContext).Name,
                        retryCount,
                        maxRetries,
                        delay.TotalSeconds);
                    await Task.Delay(delay);
                }
            }

            return app;
        }
    }
}
