using Microsoft.EntityFrameworkCore;
using NotificationApi.Data;
using NotificationApi.Services;
using Workflow.IO.Shared.Extensions;
using Workflow.IO.Shared.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.shared.json",
    optional: true,
    reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddWorkflowIOApiDefaults(
    "NotificationApi",
    checkSql: true,
    checkRabbitMq: true);

builder.Services.AddControllers();

builder.Services.AddStandardApiBehavior();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation("Notification API");

builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddHostedService<RabbitMqNotificationConsumer>();

var app = builder.Build();

await app.ApplyMigrationsOnStartupAsync<NotificationDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseWorkflowIOApiPipeline();

app.MapControllers();

app.MapWorkflowIOHealthChecks();

app.Run();
