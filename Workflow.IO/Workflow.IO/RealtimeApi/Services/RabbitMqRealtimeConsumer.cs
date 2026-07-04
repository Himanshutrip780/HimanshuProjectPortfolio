using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RealtimeApi.Hubs;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.IntegrationEvents;

namespace RealtimeApi.Services
{
    public class RabbitMqRealtimeConsumer : BackgroundService
    {
        private readonly IHubContext<WorkflowIOHub> _hubContext;

        private readonly RabbitMqOptions _options;

        private readonly ILogger<RabbitMqRealtimeConsumer> _logger;

        private IConnection? _connection;

        private IModel? _channel;

        public RabbitMqRealtimeConsumer(
            IHubContext<WorkflowIOHub> hubContext,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqRealtimeConsumer> logger)
        {
            _hubContext = hubContext;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
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

                    const string queueName = "workflow.io.realtime";

                    _channel.QueueDeclare(
                        queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false);

                    _channel.QueueBind(
                        queueName,
                        _options.ExchangeName,
                        "#");

                    var consumer =
                        new AsyncEventingBasicConsumer(_channel);

                    consumer.Received += async (_, args) =>
                    {
                        await ConsumeAsync(args);
                    };

                    _channel.BasicConsume(
                        queueName,
                        autoAck: false,
                        consumer);

                    _logger.LogInformation("Realtime RabbitMQ consumer started successfully.");
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
                        "Realtime RabbitMQ consumer could not start. Retrying in 5 seconds...");
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

        private async Task ConsumeAsync(
            BasicDeliverEventArgs args)
        {
            try
            {
                var json =
                    Encoding.UTF8.GetString(
                        args.Body.ToArray());

                var integrationEvent =
                    JsonSerializer.Deserialize<IntegrationEventRequest>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (integrationEvent == null)
                {
                    _channel?.BasicAck(
                        args.DeliveryTag,
                        multiple: false);

                    return;
                }

                var realtimeEvent =
                    RealtimeEventMapper.Map(integrationEvent);

                await BroadcastAsync(realtimeEvent);

                _channel?.BasicAck(
                    args.DeliveryTag,
                    multiple: false);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to consume realtime event");

                _channel?.BasicNack(
                    args.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        }

        private async Task BroadcastAsync(
            Model.Dto.RealtimeEventDto realtimeEvent)
        {
            var sendTasks =
                new List<Task>
                {
                    _hubContext.Clients.All.SendAsync(
                        "EventReceived",
                        realtimeEvent)
                };

            if (realtimeEvent.ProjectId.HasValue)
            {
                sendTasks.Add(
                    _hubContext.Clients
                        .Group(
                            WorkflowIOHub.ProjectGroup(
                                realtimeEvent.ProjectId.Value))
                        .SendAsync(
                            "ProjectEventReceived",
                            realtimeEvent));
            }

            if (realtimeEvent.TaskId.HasValue)
            {
                sendTasks.Add(
                    _hubContext.Clients
                        .Group(
                            WorkflowIOHub.TaskGroup(
                                realtimeEvent.TaskId.Value))
                        .SendAsync(
                            "TaskEventReceived",
                            realtimeEvent));
            }

            if (realtimeEvent.RecipientId.HasValue)
            {
                sendTasks.Add(
                    _hubContext.Clients
                        .Group(
                            WorkflowIOHub.UserGroup(
                                realtimeEvent.RecipientId.Value))
                        .SendAsync(
                            "NotificationReceived",
                            realtimeEvent));
            }

            await Task.WhenAll(sendTasks);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();

            base.Dispose();
        }
    }
}
