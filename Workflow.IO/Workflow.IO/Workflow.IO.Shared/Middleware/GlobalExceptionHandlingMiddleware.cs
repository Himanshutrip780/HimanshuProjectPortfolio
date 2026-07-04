using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.Exceptions;
using Workflow.IO.Shared.Middleware;

namespace Workflow.IO.Shared.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;

            _logger = logger;

            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(
                    context,
                    exception);
            }
        }

        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            var statusCode = exception switch
            {
                ValidationException => HttpStatusCode.BadRequest,
                NotFoundException => HttpStatusCode.NotFound,
                ConflictException => HttpStatusCode.Conflict,
                ForbiddenException => HttpStatusCode.Forbidden,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                QuotaExceededException => HttpStatusCode.PaymentRequired,
                ArgumentException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "Handled exception while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }

            var message =
                statusCode == HttpStatusCode.InternalServerError &&
                !_environment.IsDevelopment()
                    ? "An unexpected error occurred"
                    : exception.Message;

            var errors =
                exception is ValidationException validationException
                    ? validationException.Errors
                    : null;

            var correlationId =
                context.Items[CorrelationIdMiddleware.HeaderName]
                    ?.ToString()
                ?? context.TraceIdentifier;

            var response =
                ErrorResponse.Create(
                    message,
                    correlationId,
                    errors);

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    response,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    }));
        }
    }
}
