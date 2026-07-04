using System;

namespace Workflow.IO.Shared.Contracts
{
    public class TenantContext : ITenantContext
    {
        public Guid? CurrentOrganizationId { get; set; }
        public Guid? CurrentWorkspaceId { get; set; }
    }
}
