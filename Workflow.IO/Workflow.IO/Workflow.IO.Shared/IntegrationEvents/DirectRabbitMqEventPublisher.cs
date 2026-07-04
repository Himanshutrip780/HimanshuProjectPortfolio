using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Workflow.IO.Shared.Contracts;

namespace Workflow.IO.Shared.IntegrationEvents
{
    public class DirectRabbitMqEventPublisher : IIntegrationEventPublisher
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        private readonly RabbitMqOptions _options;
        private readonly ILogger<DirectRabbitMqEventPublisher> _logger;

        public DirectRabbitMqEventPublisher(
            IOptions<RabbitMqOptions> options,
            ILogger<DirectRabbitMqEventPublisher> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task PublishAsync(
            IntegrationEventRequest integrationEvent,
            CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                if (integrationEvent.EventId == Guid.Empty)
                {
                    integrationEvent.EventId = Guid.NewGuid();
                }

                if (integrationEvent.OccurredAtUtc == default)
                {
                    integrationEvent.OccurredAtUtc = DateTime.UtcNow;
                }

                var payload = JsonSerializer.Serialize(integrationEvent, JsonOptions);
                var body = Encoding.UTF8.GetBytes(payload);

                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _options.HostName,
                        Port = _options.Port,
                        UserName = _options.UserName,
                        Password = _options.Password
                    };

                    using var connection = factory.CreateConnection();
                    using var channel = connection.CreateModel();

                    channel.ExchangeDeclare(
                        _options.ExchangeName,
                        ExchangeType.Topic,
                        durable: true);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.MessageId = integrationEvent.EventId.ToString();
                    properties.Headers = new Dictionary<string, object>
                    {
                        { "event-id", integrationEvent.EventId.ToString() }
                    };

                    channel.BasicPublish(
                        _options.ExchangeName,
                        integrationEvent.EventType,
                        properties,
                        body);

                    _logger.LogInformation("Successfully published direct RabbitMQ event {EventType} with ID {EventId}", integrationEvent.EventType, integrationEvent.EventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish direct RabbitMQ event {EventType} with ID {EventId}", integrationEvent.EventType, integrationEvent.EventId);
                    throw;
                }
            }, cancellationToken);
        }
    }
}
