using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Workflow.IO.Shared.IntegrationEvents
{
    public class RabbitMqOutboxDispatcher<TDbContext>
        : BackgroundService
        where TDbContext : DbContext, IOutboxDbContext
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly RabbitMqOptions _options;

        private readonly ILogger<RabbitMqOutboxDispatcher<TDbContext>>
            _logger;

        public RabbitMqOutboxDispatcher(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqOutboxDispatcher<TDbContext>> logger)
        {
            _scopeFactory = scopeFactory;

            _options = options.Value;

            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            // Wait a few seconds on startup to give EF Core migrations time to complete
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DispatchPendingMessagesAsync(stoppingToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Error occurred querying outbox messages. The database might still be migrating.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(
                        _options.DispatchIntervalSeconds),
                    stoppingToken);
            }
        }

        private async Task DispatchPendingMessagesAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<TDbContext>();

            var messages =
                await context.OutboxMessages
                    .Where(x =>
                        x.PublishedAt == null &&
                        x.RetryCount < _options.MaxRetries)
                    .OrderBy(x => x.CreatedAt)
                    .Take(_options.DispatchBatchSize)
                    .ToListAsync(cancellationToken);

            if (messages.Count == 0)
            {
                return;
            }

            IConnection? connection = null;

            IModel? channel = null;

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

                connection = factory.CreateConnection();

                channel = connection.CreateModel();

                channel.ExchangeDeclare(
                    _options.ExchangeName,
                    ExchangeType.Topic,
                    durable: true);

                channel.ConfirmSelect();

                foreach (var message in messages)
                {
                    await DispatchSingleMessageAsync(
                        context,
                        channel,
                        message,
                        cancellationToken);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Outbox dispatcher connection failure for {DbContext}",
                    typeof(TDbContext).Name);
            }
            finally
            {
                channel?.Dispose();

                connection?.Dispose();
            }
        }

        private async Task DispatchSingleMessageAsync(
            TDbContext context,
            IModel channel,
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            try
            {
                var body =
                    Encoding.UTF8.GetBytes(message.PayloadJson);

                var properties = channel.CreateBasicProperties();

                properties.Persistent = true;

                properties.MessageId = message.EventId.ToString();

                properties.Headers ??= new Dictionary<string, object>();

                properties.Headers["event-id"] =
                    message.EventId.ToString();

                channel.BasicPublish(
                    _options.ExchangeName,
                    message.EventType,
                    properties,
                    body);

                channel.WaitForConfirmsOrDie(
                    TimeSpan.FromSeconds(10));

                message.MarkPublished();

                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to publish outbox message {OutboxMessageId}",
                    message.OutboxMessageId);

                message.MarkFailed(exception.Message);

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
