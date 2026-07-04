using Microsoft.EntityFrameworkCore;

namespace Workflow.IO.Shared.Persistence
{
    public sealed class EfUnitOfWork<TContext>
        : IUnitOfWork
        where TContext : DbContext
    {
        private readonly TContext _context;

        public EfUnitOfWork(TContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}
