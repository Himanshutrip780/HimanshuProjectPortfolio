using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Workflow.IO.Shared.Contracts;

namespace Workflow.IO.Shared.IntegrationEvents
{
    public class HttpIntegrationEventPublisher
        : IIntegrationEventPublisher
    {
        private readonly HttpClient _httpClient;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IntegrationEventPublisherOptions _options;

        private readonly ILogger<HttpIntegrationEventPublisher> _logger;

        public HttpIntegrationEventPublisher(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<IntegrationEventPublisherOptions> options,
            ILogger<HttpIntegrationEventPublisher> logger)
        {
            _httpClient = httpClient;

            _httpContextAccessor = httpContextAccessor;

            _options = options.Value;

            _logger = logger;
        }

        public async Task PublishAsync(
            IntegrationEventRequest integrationEvent,
            CancellationToken cancellationToken = default)
        {
            await PublishToEndpointAsync(
                _options.ActivityEndpoint,
                integrationEvent,
                cancellationToken);

            if (integrationEvent.RecipientId.HasValue)
            {
                await PublishToEndpointAsync(
                    _options.NotificationEndpoint,
                    integrationEvent,
                    cancellationToken);
            }
        }

        private async Task PublishToEndpointAsync(
            string? endpoint,
            IntegrationEventRequest integrationEvent,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return;
            }

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    endpoint)
                {
                    Content = JsonContent.Create(
                        integrationEvent)
                };

            var authorizationHeader =
                _httpContextAccessor
                    .HttpContext?
                    .Request
                    .Headers
                    .Authorization
                    .ToString();

            if (!string.IsNullOrWhiteSpace(authorizationHeader))
            {
                request.Headers.Authorization =
                    AuthenticationHeaderValue.Parse(
                        authorizationHeader);
            }

            try
            {
                using var response =
                    await _httpClient.SendAsync(
                        request,
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Integration event {EventType} failed at {Endpoint} with status {StatusCode}",
                        integrationEvent.EventType,
                        endpoint,
                        response.StatusCode);
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Integration event {EventType} failed at {Endpoint}",
                    integrationEvent.EventType,
                    endpoint);
            }
        }
    }
}
