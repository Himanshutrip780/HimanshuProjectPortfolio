using System;

namespace ATS.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string Role { get; }
        Guid? CompanyId { get; }
    }
}
