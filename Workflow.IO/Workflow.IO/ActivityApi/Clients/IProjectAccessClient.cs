using System;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityApi.Clients
{
    public interface IProjectAccessClient
    {
        Task<bool> IsProjectAccessibleAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);
    }
}
