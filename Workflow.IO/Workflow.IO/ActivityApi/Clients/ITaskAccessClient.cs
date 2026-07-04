using System;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityApi.Clients
{
    public interface ITaskAccessClient
    {
        Task<bool> IsTaskAccessibleAsync(
            Guid taskId,
            CancellationToken cancellationToken = default);
    }
}
