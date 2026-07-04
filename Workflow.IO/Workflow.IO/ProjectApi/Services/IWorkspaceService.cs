using ProjectApi.Model.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectApi.Services
{
    public interface IWorkspaceService
    {
        Task<IEnumerable<Workspace>> GetWorkspacesAsync();
        Task<Workspace?> CreateWorkspaceAsync(string name, string? description);
    }
}
