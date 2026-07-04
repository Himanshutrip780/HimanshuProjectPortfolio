using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Workflow.IO.Shared.Extensions
{
    public static class HttpClientResilienceExtensions
    {
        public static IHttpClientBuilder AddWorkflowIOResilience(
            this IHttpClientBuilder builder)
        {
            return builder
                .AddPolicyHandler(
                    HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(
                            3,
                            retryAttempt =>
                                TimeSpan.FromMilliseconds(
                                    200 * retryAttempt)))
                .AddPolicyHandler(
                    HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .CircuitBreakerAsync(
                            5,
                            TimeSpan.FromSeconds(30)));
        }
    }
}
