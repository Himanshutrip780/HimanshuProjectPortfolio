using Microsoft.EntityFrameworkCore;
using TaskApi.Clients;
using TaskApi.Data;
using TaskApi.Mappings;
using TaskApi.Repositories;
using TaskApi.Services;
using Workflow.IO.Shared.Extensions;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.shared.json",
    optional: true,
    reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddWorkflowIOApiDefaults(
    "TaskApi",
    checkSql: true,
    checkRabbitMq: true);

builder.Services.AddControllers();

builder.Services.AddStandardApiBehavior();

builder.Services.AddHttpContextAccessor();

builder.Services.AddUnitOfWork<TaskDbContext>();

builder.Services.AddOutboxRabbitMqEventPublisher<TaskDbContext>(
    builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation("Task API");

builder.Services.AddDbContext<TaskDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddScoped<ITaskRepository, TaskRepository>();

builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddScoped<ITaskJiraFeatureService, TaskJiraFeatureService>();

builder.Services.AddHttpClient<IProjectAccessClient, ProjectAccessClient>(
        client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["Services:ProjectApi"]!);
        })
    .AddWorkflowIOResilience();

builder.Services.AddHttpClient<IProjectMetadataClient, ProjectMetadataClient>(
        client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["Services:ProjectApi"]!);
        })
    .AddWorkflowIOResilience();

builder.Services.AddAutoMapper(typeof(TaskMappingProfile).Assembly);

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailService, MicrosoftGraphEmailService>();
builder.Services.AddScoped<ITaskDailyUpdateService, TaskDailyUpdateService>();

builder.Services.Configure<Workflow.IO.Shared.IntegrationEvents.RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddHostedService<RabbitMqTenantProvisioningConsumer>();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("DailySprintUpdateJob");
    q.AddJob<DailySprintUpdateJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DailySprintUpdateJob-ResetTrigger")
        .WithCronSchedule("0 0 11 * * ?"));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DailySprintUpdateJob-AutoSendTrigger")
        .WithCronSchedule("0 0 12 * * ?"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

await app.ApplyMigrationsOnStartupAsync<TaskDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseWorkflowIOApiPipeline();

app.MapControllers();

app.MapWorkflowIOHealthChecks();

app.Run();
