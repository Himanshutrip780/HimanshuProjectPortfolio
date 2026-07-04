using System.Threading.RateLimiting;
using Serilog;
using Workflow.IO.Shared.Extensions;
using Workflow.IO.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.shared.json",
    optional: true,
    reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddWorkflowIOSerilog("GatewayApi");

builder.Services.AddWorkflowIOCors(builder.Configuration);

builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient("TenantResolver");
builder.Services.AddHttpClient("TenantValidator");

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            context =>
            {
                var partitionKey =
                    context.User.Identity?.IsAuthenticated == true
                        ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "auth"
                        : context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                          context.Connection.RemoteIpAddress?.ToString() ?? "anon";

                return RateLimitPartition
                    .GetFixedWindowLimiter(
                        partitionKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 200,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        });
            });

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;
});

builder.Services.AddHostedService<GatewayApi.Services.KeepAliveService>();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(
        builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseCors("WorkflowIOCors");

// SPA HTML INTERCEPTOR: Prevent YARP from intercepting frontend routes (e.g. /projects) on page refresh
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";
    
    // If it's a GET request for HTML (browser navigation), and not an explicit API/SignalR call
    if (context.Request.Method == HttpMethods.Get && 
        !path.Contains(".") && 
        !path.StartsWith("/hubs/") &&
        context.Request.Headers.Accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase))
    {
        // Serve the SPA entry point directly to bypass YARP endpoint matching
        context.Request.Path = "/index.html";
        context.SetEndpoint(null);
    }
    
    await next();
});

var staticFileOptions = new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var contentType = ctx.Context.Response.ContentType;
        if (!string.IsNullOrEmpty(contentType) && (contentType.Contains("text/html") || contentType.Contains("application/javascript") || contentType.Contains("text/javascript") || contentType.Contains("text/css")))
        {
            if (!contentType.Contains("charset", StringComparison.OrdinalIgnoreCase))
            {
                ctx.Context.Response.ContentType = $"{contentType}; charset=utf-8";
            }
        }

        // Disable caching for HTML entry point to prevent chunk loading errors after new builds
        if (ctx.File.Name.Equals("index.html", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
    }
};

app.UseDefaultFiles();
app.UseStaticFiles(staticFileOptions);

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<GatewayApi.Middleware.GatewayTenantMiddleware>();

app.UseRateLimiter();


// HEALTH
app.MapHealthChecks("/health");

app.MapGet("/ready", () =>
    Results.Ok(new
    {
        status = "Healthy"
    }));


// ROOT SPA FALLBACK
app.MapFallbackToFile("index.html", staticFileOptions);


// DEBUG AUTH TEST
app.MapGet("/debug-auth", (HttpContext ctx) =>
{
    return Results.Ok(new
    {
        authenticated =
            ctx.User.Identity?.IsAuthenticated,

        name =
            ctx.User.Identity?.Name,

        claims =
            ctx.User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            })
    });
})
.RequireAuthorization();




// MAP ALL PROXY ROUTES
app.MapReverseProxy();

app.Run();


// PUBLIC PATHS
static bool IsPublicGatewayPath(PathString path)
{
    var value =
        path.Value?.ToLowerInvariant()
        ?? string.Empty;

    return value == "/" ||
           value.StartsWith("/health") ||
           value.StartsWith("/ready") ||
           value.Contains("/users/authenticate") ||
           value.Contains("/users/register") ||
           value.Contains("/users/refresh");
}