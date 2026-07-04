using Microsoft.AspNetCore.Authentication.JwtBearer;
using RealtimeApi.Clients;
using RealtimeApi.Hubs;
using RealtimeApi.Services;
using Workflow.IO.Shared.Extensions;
using Workflow.IO.Shared.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.shared.json",
    optional: true,
    reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddWorkflowIOApiDefaults(
    "RealtimeApi",
    checkSql: false,
    checkRabbitMq: true);

builder.Services.AddControllers();

builder.Services.AddStandardApiBehavior();

builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation("Realtime API");

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

builder.Services.PostConfigure<JwtBearerOptions>(
    JwtBearerDefaults.AuthenticationScheme,
    options =>
    {
        options.Events ??= new JwtBearerEvents();

        var previousOnMessageReceived =
            options.Events.OnMessageReceived;

        options.Events.OnMessageReceived = async context =>
        {
            if (previousOnMessageReceived != null)
            {
                await previousOnMessageReceived(context);
            }

            var accessToken =
                context.Request.Query["access_token"];

            var path =
                context.HttpContext.Request.Path;

            if (string.IsNullOrWhiteSpace(context.Token) &&
                !string.IsNullOrWhiteSpace(accessToken) &&
                path.StartsWithSegments("/hubs/workflow.io"))
            {
                context.Token = accessToken;
            }
        };
    });

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddHostedService<RabbitMqRealtimeConsumer>();

builder.Services.AddHttpClient<IProjectAccessClient, ProjectAccessClient>(
        client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["Services:ProjectApi"]!);
        })
    .AddWorkflowIOResilience();

builder.Services.AddHttpClient<ITaskAccessClient, TaskAccessClient>(
        client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["Services:TaskApi"]!);
        })
    .AddWorkflowIOResilience();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseWorkflowIOApiPipeline();

app.UseWebSockets();

app.MapControllers();

app.MapHub<WorkflowIOHub>("/hubs/workflow.io");

app.MapWorkflowIOHealthChecks();

app.Run();
