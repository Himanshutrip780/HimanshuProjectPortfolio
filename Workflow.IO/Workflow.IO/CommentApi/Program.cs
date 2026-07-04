using CommentApi.Clients;
using CommentApi.Data;
using CommentApi.Mappings;
using CommentApi.Repositories;
using CommentApi.Services;
using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.shared.json",
    optional: true,
    reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddWorkflowIOApiDefaults(
    "CommentApi",
    checkSql: true,
    checkRabbitMq: true);

builder.Services.AddControllers();

builder.Services.AddStandardApiBehavior();

builder.Services.AddHttpContextAccessor();

builder.Services.AddUnitOfWork<CommentDbContext>();

builder.Services.AddOutboxRabbitMqEventPublisher<CommentDbContext>(
    builder.Configuration);

builder.Services.AddHttpClient<ITaskAccessClient, TaskAccessClient>(
        client =>
        {
            client.BaseAddress =
                new Uri(builder.Configuration["Services:TaskApi"]!);
        })
    .AddWorkflowIOResilience();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation("Comment API");

builder.Services.AddDbContext<CommentDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddScoped<ICommentRepository, CommentRepository>();

builder.Services.AddScoped<ICommentService, CommentService>();

builder.Services.AddAutoMapper(typeof(CommentMappingProfile).Assembly);

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsOnStartupAsync<CommentDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseWorkflowIOApiPipeline();

app.MapControllers();

app.MapWorkflowIOHealthChecks();

app.Run();
