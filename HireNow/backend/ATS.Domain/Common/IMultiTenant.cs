using System;

namespace ATS.Domain.Common
{
    public interface IMultiTenant
    {
        Guid CompanyId { get; set; }
    }
}
