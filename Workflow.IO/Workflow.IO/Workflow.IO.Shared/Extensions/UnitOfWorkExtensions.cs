using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Workflow.IO.Shared.Persistence;

namespace Workflow.IO.Shared.Extensions
{
    public static class UnitOfWorkExtensions
    {
        public static IServiceCollection AddUnitOfWork<TContext>(
            this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddScoped<IUnitOfWork, EfUnitOfWork<TContext>>();

            return services;
        }
    }
}
