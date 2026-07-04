namespace RealtimeApi.Clients
{
    public interface IProjectAccessClient
    {
        Task<bool> IsProjectMemberAsync(
            Guid projectId,
            Guid userId,
            string? accessToken,
            string? orgId,
            CancellationToken cancellationToken = default);
    }
}
