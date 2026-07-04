using JwtAuthenticationManager;
using JwtAuthenticationManager.Data;
using JwtAuthenticationManager.Repository;
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Mappings;
using UserApi.Repositories;
using UserApi.Service;
using Workflow.IO.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.shared.json",
    optional: true,
    reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddWorkflowIOApiDefaults("UserApi");

builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddStandardApiBehavior();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();

builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();

builder.Services.AddDbContext<UserDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("UserApi");
            sqlOptions.EnableRetryOnFailure();
        });
});

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

builder.Services.AddScoped<JwtTokenHandler>();

builder.Services.Configure<Workflow.IO.Shared.IntegrationEvents.RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddScoped<Workflow.IO.Shared.IntegrationEvents.IIntegrationEventPublisher, Workflow.IO.Shared.IntegrationEvents.DirectRabbitMqEventPublisher>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation("User API");

builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

var app = builder.Build();

await app.ApplyMigrationsOnStartupAsync<UserDbContext>();

await app.ApplyMigrationsOnStartupAsync<AuthDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseWorkflowIOApiPipeline();

app.MapControllers();

app.MapWorkflowIOHealthChecks();

app.Run();
