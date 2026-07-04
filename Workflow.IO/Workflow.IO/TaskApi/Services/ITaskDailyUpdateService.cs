using System;
using System.Threading.Tasks;
using TaskApi.Model.Domain.Entities;

namespace TaskApi.Services
{
    public interface ITaskDailyUpdateService
    {
        Task<DailyUpdateState> GetDailyUpdateStateAsync(Guid projectId);

        Task<DailyUpdateState> SaveDailyUpdateStateAsync(Guid projectId, string[] extraRecipients);

        Task<bool> TriggerDailyUpdateAsync(Guid projectId, Guid userId);

        Task ResetDailyUpdatesAsync();

        Task AutoSendDailyUpdatesAsync();
    }
}
