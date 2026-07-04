using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workflow.IO.Shared.IntegrationEvents;

namespace Workflow.IO.Shared.Extensions
{
    public static class IntegrationEventExtensions
    {
        public static IServiceCollection AddOutboxRabbitMqEventPublisher<TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TDbContext : DbContext, IOutboxDbContext
        {
            services.Configure<RabbitMqOptions>(
                configuration.GetSection("RabbitMq"));

            services.AddScoped<
                IIntegrationEventPublisher,
                OutboxIntegrationEventPublisher<TDbContext>>();

            services.AddHostedService<
                RabbitMqOutboxDispatcher<TDbContext>>();

            return services;
        }
    }
}
