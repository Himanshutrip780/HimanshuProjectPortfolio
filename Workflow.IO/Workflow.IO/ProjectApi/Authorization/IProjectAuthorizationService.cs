using ProjectApi.Model.Domain.Entities;
using ProjectApi.Model.Domain.Enums;

namespace ProjectApi.Authorization
{
    public interface IProjectAuthorizationService
    {
        Task RequireProjectAccessAsync(
            Guid projectId,
            Guid userId);

        Task<bool> HasAccessAsync(
            Guid projectId,
            Guid userId);
    }
}
