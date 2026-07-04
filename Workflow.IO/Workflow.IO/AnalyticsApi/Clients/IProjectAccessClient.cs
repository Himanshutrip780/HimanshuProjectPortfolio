namespace AnalyticsApi.Clients
{
    public interface IProjectAccessClient
    {
        Task<bool> IsProjectMemberAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
