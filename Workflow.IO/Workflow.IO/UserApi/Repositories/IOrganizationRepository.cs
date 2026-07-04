using UserApi.Model.Domian.Entities;

namespace UserApi.Repositories
{
    public interface IOrganizationRepository
    {
        Task<Organization> CreateOrganizationAsync(Organization organization);
        Task<Organization?> GetOrganizationByInviteCodeAsync(string inviteCode);
        Task<Organization?> GetUserOrganizationAsync(Guid userId);
        Task<Organization?> GetOrganizationByNameAsync(string name);
    }
}
