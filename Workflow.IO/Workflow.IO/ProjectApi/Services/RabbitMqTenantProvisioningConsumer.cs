using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectApi.Data;
using ProjectApi.Model.Domain.Entities;
using ProjectApi.Model.Domain.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.IntegrationEvents;

namespace ProjectApi.Services
{
    public class RabbitMqTenantProvisioningConsumer : BackgroundService
    {
        private const string QueueName = "workflow.io.project.tenant-provisioning";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqTenantProvisioningConsumer> _logger;

        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMqTenantProvisioningConsumer(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqTenantProvisioningConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _options.HostName,
                        Port = _options.Port,
                        UserName = _options.UserName,
                        Password = _options.Password,
                        DispatchConsumersAsync = true
                    };

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _channel.ExchangeDeclare(
                        _options.ExchangeName,
                        ExchangeType.Topic,
                        durable: true);

                    _channel.QueueDeclare(
                        QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false);

                    _channel.QueueBind(
                        QueueName,
                        _options.ExchangeName,
                        "TenantRegistered");

                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.Received += async (_, args) =>
                    {
                        await ConsumeAsync(args);
                    };

                    _channel.BasicConsume(
                        QueueName,
                        autoAck: false,
                        consumer);

                    _logger.LogInformation("Project Tenant Provisioning RabbitMQ consumer started successfully.");
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "Project Tenant Provisioning RabbitMQ consumer could not start. Retrying in 5 seconds...");
                    try
                    {
                        await Task.Delay(5000, stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        private async Task ConsumeAsync(BasicDeliverEventArgs args)
        {
            try
            {
                var body = Encoding.UTF8.GetString(args.Body.ToArray());
                var integrationEvent = JsonSerializer.Deserialize<IntegrationEventRequest>(body, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (integrationEvent == null || integrationEvent.EventType != "TenantRegistered" || string.IsNullOrEmpty(integrationEvent.PayloadJson))
                {
                    _channel?.BasicAck(args.DeliveryTag, false);
                    return;
                }

                var payload = JsonSerializer.Deserialize<TenantRegisteredPayload>(integrationEvent.PayloadJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (payload == null)
                {
                    _channel?.BasicAck(args.DeliveryTag, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

                // Idempotency check: verify if the workspace already exists
                var workspaceExists = await dbContext.Workspaces
                    .IgnoreQueryFilters()
                    .AnyAsync(w => w.WorkspaceId == payload.WorkspaceId);

                if (!workspaceExists)
                {
                    _logger.LogInformation("Provisioning assets for organization {OrgName} ({OrgId})", payload.OrganizationName, payload.OrganizationId);

                    // 1. Create default workspace
                    var workspace = new Workspace(
                        payload.OrganizationName + " Workspace",
                        "Primary workspace for " + payload.OrganizationName,
                        payload.OrganizationId
                    );
                    workspace.WorkspaceId = payload.WorkspaceId;
                    dbContext.Workspaces.Add(workspace);

                    // 2. Create default client
                    var client = new Client(
                        "Internal Projects",
                        "Technology",
                        "Contact Person",
                        "info@company.com",
                        "internal",
                        payload.OrganizationId
                    );
                    client.ClientId = payload.ClientId;
                    dbContext.Clients.Add(client);

                    // 3. Create default project
                    var project = new Project(
                        "My First Project",
                        "My first agile project",
                        payload.AdminUserId,
                        "PROJ",
                        payload.OrganizationId,
                        payload.WorkspaceId,
                        ProjectType.Scrum
                    );
                    project.ProjectId = payload.ProjectId;
                    dbContext.Projects.Add(project);

                    // 4. Create default project member (Admin user as Owner)
                    var projectMember = new ProjectMember(
                        project,
                        payload.AdminUserId,
                        ProjectRole.Owner
                    );
                    dbContext.ProjectMembers.Add(projectMember);

                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Successfully provisioned ProjectApi assets for organization {OrgName}", payload.OrganizationName);
                }

                _channel?.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to consume tenant provisioning event in ProjectApi");
                _channel?.BasicNack(
                    args.DeliveryTag,
                    multiple: false,
                    requeue: !args.Redelivered);
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }

        private class TenantRegisteredPayload
        {
            public Guid OrganizationId { get; set; }
            public string OrganizationName { get; set; } = string.Empty;
            public Guid AdminUserId { get; set; }
            public string AdminEmail { get; set; } = string.Empty;
            public Guid WorkspaceId { get; set; }
            public Guid ClientId { get; set; }
            public Guid ProjectId { get; set; }
            public Guid TeamId { get; set; }
        }
    }
}
