using Microsoft.EntityFrameworkCore;

namespace Workflow.IO.Shared.IntegrationEvents
{
    public interface IOutboxDbContext
    {
        DbSet<OutboxMessage> OutboxMessages { get; set; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
