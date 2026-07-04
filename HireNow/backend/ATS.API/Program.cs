using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Serilog;
using ATS.API.Middleware;
using ATS.API.Services;
using ATS.Application;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Entities;
using ATS.Infrastructure;
using ATS.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog Logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ats_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add HttpContext Accessor for resolving CurrentUser context in background requests
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Clean Architecture project layers
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HireNow API", Version = "v1" });
    
    // Add Authorization header definition for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// Configure CORS for Angular frontend Integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin)) return false;
            try
            {
                var uri = new Uri(origin);
                return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                       uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                       uri.Host.EndsWith(".himanshuprojectportfolio.xyz", StringComparison.OrdinalIgnoreCase) ||
                       uri.Host.Equals("himanshuprojectportfolio.xyz", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Run Database Seed at startup with retry
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    int maxRetryCount = 15;
    int delaySeconds = 4;
    for (int retry = 1; retry <= maxRetryCount; retry++)
    {
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            await DbSeeder.SeedAsync(context, userManager, roleManager);
            Log.Information("Database successfully seeded on startup.");
            break;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, $"Failed to run database seeder on attempt {retry}/{maxRetryCount}. Retrying in {delaySeconds} seconds...");
            if (retry == maxRetryCount)
            {
                Log.Error(ex, "Failed to run database seeder on application launch after all retries.");
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}

// Global Exception Interception Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ATS API v1"));
}

// Serve uploaded files (e.g. resumes) statically
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { } // Make accessible for integration testing
