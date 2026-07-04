using FileApi.Clients;
using FileApi.Data;
using FileApi.Repositories;
using FileApi.Services;
using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.shared.json",
    optional: true,
    reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddWorkflowIOApiDefaults(
    "FileApi",
    checkSql: true,
    checkRabbitMq: true);

builder.Services.AddControllers();

builder.Services.AddStandardApiBehavior();

builder.Services.AddHttpContextAccessor();

builder.Services.AddUnitOfWork<FileDbContext>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation("File API");

builder.Services.AddDbContext<FileDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();

builder.Services.AddHttpClient<ITaskAccessClient, TaskAccessClient>(
        client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["Services:TaskApi"]!);
        })
    .AddWorkflowIOResilience();

builder.Services.AddOutboxRabbitMqEventPublisher<FileDbContext>(
    builder.Configuration);

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsOnStartupAsync<FileDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseWorkflowIOApiPipeline();

app.MapControllers();

app.MapWorkflowIOHealthChecks();

app.Run();
