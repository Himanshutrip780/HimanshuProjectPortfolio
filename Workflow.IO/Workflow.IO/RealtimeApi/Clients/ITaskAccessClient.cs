namespace RealtimeApi.Clients
{
    public interface ITaskAccessClient
    {
        Task<bool> CanAccessTaskAsync(
            Guid taskId,
            string? accessToken,
            string? orgId,
            CancellationToken cancellationToken = default);
    }
}
