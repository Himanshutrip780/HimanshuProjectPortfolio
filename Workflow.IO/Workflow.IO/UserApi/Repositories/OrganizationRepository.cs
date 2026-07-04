using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Model.Domian.Entities;

namespace UserApi.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly UserDbContext _context;

        public OrganizationRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Organization> CreateOrganizationAsync(Organization organization)
        {
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<Organization?> GetOrganizationByInviteCodeAsync(string inviteCode)
        {
            return await _context.Organizations
                .FirstOrDefaultAsync(o => o.InviteCode == inviteCode);
        }

        public async Task<Organization?> GetUserOrganizationAsync(Guid userId)
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user?.Organization;
        }

        public async Task<Organization?> GetOrganizationByNameAsync(string name)
        {
            return await _context.Organizations
                .FirstOrDefaultAsync(o => o.Name.ToLower() == name.ToLower());
        }
    }
}
