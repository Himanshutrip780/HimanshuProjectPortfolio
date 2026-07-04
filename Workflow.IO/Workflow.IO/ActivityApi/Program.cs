using ActivityApi.Clients;
using ActivityApi.Data;
using ActivityApi.Services;
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
    "ActivityApi",
    checkSql: true,
    checkRabbitMq: true);

builder.Services.AddControllers();

builder.Services.AddStandardApiBehavior();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<ITaskAccessClient, TaskAccessClient>(
        client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["Services:TaskApi"]!);
        })
    .AddWorkflowIOResilience();

builder.Services.AddHttpClient<IProjectAccessClient, ProjectAccessClient>(
        client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["Services:ProjectApi"]!);
        })
    .AddWorkflowIOResilience();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation("Activity API");

builder.Services.AddDbContext<ActivityDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddHostedService<RabbitMqActivityConsumer>();

var app = builder.Build();

await app.ApplyMigrationsOnStartupAsync<ActivityDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseWorkflowIOApiPipeline();

app.MapControllers();

app.MapWorkflowIOHealthChecks();

app.Run();
