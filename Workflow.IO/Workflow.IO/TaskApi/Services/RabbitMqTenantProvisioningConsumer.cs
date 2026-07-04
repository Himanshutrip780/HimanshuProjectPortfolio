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
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskApi.Data;
using TaskApi.Model.Domain.Entities;
using TaskApi.Model.Domain.Enums;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.IntegrationEvents;

namespace TaskApi.Services
{
    public class RabbitMqTenantProvisioningConsumer : BackgroundService
    {
        private const string QueueName = "workflow.io.task.tenant-provisioning";

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

                    _logger.LogInformation("Task Tenant Provisioning RabbitMQ consumer started successfully.");
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
                        "Task Tenant Provisioning RabbitMQ consumer could not start. Retrying in 5 seconds...");
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
                var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();

                // Idempotency check: verify if the team already exists
                var teamExists = await dbContext.Teams
                    .IgnoreQueryFilters()
                    .AnyAsync(t => t.TeamId == payload.TeamId);

                if (!teamExists)
                {
                    _logger.LogInformation("Provisioning TaskApi assets for organization {OrgName} ({OrgId})", payload.OrganizationName, payload.OrganizationId);

                    // 1. Create default team
                    var team = new Team(
                        "Engineering Team",
                        null,
                        payload.AdminUserId,
                        "Public",
                        "Default engineering team",
                        payload.OrganizationId
                    );
                    team.TeamId = payload.TeamId;
                    dbContext.Teams.Add(team);

                    // 2. Create default team member (Admin user as Lead)
                    var teamMember = new TeamMember(
                        team,
                        payload.AdminUserId,
                        "Lead"
                    );
                    dbContext.TeamMembers.Add(teamMember);

                    // 3. Create default board for Project
                    var board = new Board(
                        payload.ProjectId,
                        "PROJ Board"
                    );
                    dbContext.Boards.Add(board);

                    // 4. Create default board columns
                    var colTodo = new BoardColumn(board.BoardId, "To Do", Model.Domain.Enums.TaskStatus.Todo, 1);
                    var colInProgress = new BoardColumn(board.BoardId, "In Progress", Model.Domain.Enums.TaskStatus.InProgress, 2);
                    var colReview = new BoardColumn(board.BoardId, "In Review", Model.Domain.Enums.TaskStatus.Review, 3);
                    var colDone = new BoardColumn(board.BoardId, "Done", Model.Domain.Enums.TaskStatus.Done, 4);
                    dbContext.BoardColumns.AddRange(colTodo, colInProgress, colReview, colDone);

                    // 5. Create default active sprint
                    var sprint = new Sprint(
                        payload.ProjectId,
                        "Sprint 1",
                        DateTime.UtcNow,
                        DateTime.UtcNow.AddDays(14)
                    );
                    sprint.Start(false);
                    dbContext.Sprints.Add(sprint);

                    // 6. Create default ProjectIssueCounter
                    var issueCounter = new ProjectIssueCounter(
                        payload.ProjectId,
                        "PROJ"
                    );
                    dbContext.ProjectIssueCounters.Add(issueCounter);

                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Successfully provisioned TaskApi assets for organization {OrgName}", payload.OrganizationName);
                }

                _channel?.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to consume tenant provisioning event in TaskApi");
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
