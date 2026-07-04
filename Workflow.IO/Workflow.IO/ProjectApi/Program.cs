using Microsoft.EntityFrameworkCore;
using ProjectApi.Authorization;
using ProjectApi.Data;
using ProjectApi.Mappings;
using ProjectApi.Repositories;
using ProjectApi.Services;
using Workflow.IO.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.shared.json",
    optional: true,
    reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddWorkflowIOApiDefaults("ProjectApi");

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient();

builder.Services.AddStandardApiBehavior();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation("Project API");

builder.Services.AddDbContext<ProjectDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddUnitOfWork<ProjectDbContext>();

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

builder.Services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();

builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddScoped<Workflow.IO.Shared.Authorization.IRoleProvider, ProjectApi.Authorization.ProjectRoleProvider>();

builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();

builder.Services.Configure<Workflow.IO.Shared.IntegrationEvents.RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddHostedService<RabbitMqTenantProvisioningConsumer>();

builder.Services.AddAutoMapper(typeof(ProjectMappingProfile).Assembly);

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsOnStartupAsync<ProjectDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseWorkflowIOApiPipeline();

app.MapControllers();

app.MapWorkflowIOHealthChecks();

app.Run();
