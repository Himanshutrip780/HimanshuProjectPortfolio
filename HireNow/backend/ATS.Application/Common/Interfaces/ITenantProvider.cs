using System;

namespace ATS.Application.Common.Interfaces
{
    public interface ITenantProvider
    {
        Guid? GetCompanyId();
    }
}
