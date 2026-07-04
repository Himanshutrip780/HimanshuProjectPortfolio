using System;

namespace Workflow.IO.Shared.Contracts
{
    public interface ITenantContext
    {
        Guid? CurrentOrganizationId { get; set; }
        Guid? CurrentWorkspaceId { get; set; }
    }
}
