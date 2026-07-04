using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.Middleware;

namespace Workflow.IO.Shared.Extensions
{
    public static class ApiBehaviorExtensions
    {
        public static IServiceCollection AddStandardApiBehavior(
            this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                .Select(error => error.ErrorMessage)
                                .ToArray());

                    var correlationId =
                        context.HttpContext.Items[
                                CorrelationIdMiddleware.HeaderName]
                            ?.ToString()
                        ?? context.HttpContext.TraceIdentifier;

                    var response =
                        ErrorResponse.Create(
                            "Validation failed",
                            correlationId,
                            errors);

                    return new BadRequestObjectResult(response);
                };
            });

            return services;
        }

        public static IApplicationBuilder UseGlobalExceptionHandling(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<
                GlobalExceptionHandlingMiddleware>();
        }
    }
}
