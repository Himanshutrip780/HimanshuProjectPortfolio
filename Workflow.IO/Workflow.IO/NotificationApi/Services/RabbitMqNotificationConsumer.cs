using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationApi.Data;
using NotificationApi.Model.Domain.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Workflow.IO.Shared.IntegrationEvents;

namespace NotificationApi.Services
{
    public class RabbitMqNotificationConsumer : BackgroundService
    {
        private const string QueueName = "workflow.io.notifications";

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly RabbitMqOptions _options;

        private readonly ILogger<RabbitMqNotificationConsumer> _logger;

        private IConnection? _connection;

        private IModel? _channel;

        public RabbitMqNotificationConsumer(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqNotificationConsumer> logger)
        {
            _scopeFactory = scopeFactory;

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

                    _channel.QueueDeclare(
                        QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false);

                    _channel.QueueBind(
                        QueueName,
                        _options.ExchangeName,
                        "#");

                    var consumer =
                        new AsyncEventingBasicConsumer(_channel);

                    consumer.Received += async (_, args) =>
                    {
                        await ConsumeAsync(args);
                    };

                    _channel.BasicConsume(
                        QueueName,
                        autoAck: false,
                        consumer);

                    _logger.LogInformation("Notification RabbitMQ consumer started successfully.");
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
                        "Notification RabbitMQ consumer could not start. Retrying in 5 seconds...");
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
                var integrationEvent =
                    IdempotentEventConsumerHelper.Deserialize(args);

                if (integrationEvent?.RecipientId == null)
                {
                    _channel?.BasicAck(args.DeliveryTag, false);

                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                var context =
                    scope.ServiceProvider
                        .GetRequiredService<NotificationDbContext>();

                if (await IdempotentEventConsumerHelper
                        .IsAlreadyProcessedAsync(
                            context,
                            integrationEvent.EventId))
                {
                    _channel?.BasicAck(args.DeliveryTag, false);

                    return;
                }

                await context.Notifications.AddAsync(
                    new Notification(
                        integrationEvent.RecipientId,
                        integrationEvent.EventType,
                        integrationEvent.EntityType,
                        integrationEvent.EntityId,
                        integrationEvent.Description ??
                            $"{integrationEvent.EventType} occurred"));

                await IdempotentEventConsumerHelper.MarkProcessedAsync(
                    context,
                    integrationEvent);

                await context.SaveChangesAsync();

                _channel?.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to consume notification event");

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
    }
}
