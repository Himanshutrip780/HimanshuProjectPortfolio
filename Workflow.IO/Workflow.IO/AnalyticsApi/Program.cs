using AnalyticsApi.Clients;
using AnalyticsApi.Data;
using AnalyticsApi.Services;
using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.Extensions;
using Workflow.IO.Shared.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.shared.json",
    optional: true,
    reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddWorkflowIOApiDefaults(
    "AnalyticsApi",
    checkSql: true,
    checkRabbitMq: true);

builder.Services.AddControllers();

builder.Services.AddStandardApiBehavior();

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation("Analytics API");

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddScoped<IAnalyticsProjectionService, AnalyticsProjectionService>();

builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();

builder.Services.AddHttpClient<IProjectAccessClient, ProjectAccessClient>(
        client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["Services:ProjectApi"]!);
        })
    .AddWorkflowIOResilience();

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddHostedService<RabbitMqAnalyticsConsumer>();

var app = builder.Build();

await app.ApplyMigrationsOnStartupAsync<AnalyticsDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseWorkflowIOApiPipeline();

app.MapControllers();

app.MapWorkflowIOHealthChecks();

app.Run();
