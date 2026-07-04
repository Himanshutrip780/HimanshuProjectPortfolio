using System;
using Microsoft.AspNetCore.Identity;

namespace ATS.Domain.Entities
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() : base()
        {
            Id = Guid.NewGuid();
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
            Id = Guid.NewGuid();
            Name = roleName;
            NormalizedName = roleName.ToUpperInvariant();
        }
    }
}
