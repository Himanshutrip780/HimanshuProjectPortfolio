using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using TaskApi.Data;

namespace TaskApi.Services
{
    [DisallowConcurrentExecution]
    public class DailySprintUpdateJob : IJob
    {
        private readonly ITaskDailyUpdateService _dailyUpdateService;
        private readonly TaskDbContext _dbContext;
        private readonly ILogger<DailySprintUpdateJob> _logger;

        public DailySprintUpdateJob(
            ITaskDailyUpdateService dailyUpdateService,
            TaskDbContext dbContext,
            ILogger<DailySprintUpdateJob> logger)
        {
            _dailyUpdateService = dailyUpdateService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var today = DateTime.UtcNow.Date;

            // 1. Skip weekends/non-working days using SQL calendar table or fallback to DayOfWeek
            var calendarDay = await _dbContext.CalendarDays
                .FirstOrDefaultAsync(c => c.Date == today);

            bool isWorkingDay;
            if (calendarDay != null)
            {
                isWorkingDay = calendarDay.IsWorkingDay;
            }
            else
            {
                isWorkingDay = today.DayOfWeek != DayOfWeek.Saturday && today.DayOfWeek != DayOfWeek.Sunday;
            }

            if (!isWorkingDay)
            {
                _logger.LogInformation("Daily Sprint Update Job: Today ({Date}, {DayOfWeek}) is a weekend or non-working day. Skipping execution.", today.ToString("yyyy-MM-dd"), today.DayOfWeek);
                return;
            }

            // 2. Perform actions depending on trigger schedule (11 AM reset, 12 PM auto-send)
            var currentHour = DateTime.Now.Hour;
            _logger.LogInformation("Daily Sprint Update Job executing. Current hour: {Hour}", currentHour);

            if (currentHour == 11)
            {
                await _dailyUpdateService.ResetDailyUpdatesAsync();
                _logger.LogInformation("Daily Sprint Update Job: Reset completed successfully.");
            }
            else if (currentHour == 12)
            {
                await _dailyUpdateService.AutoSendDailyUpdatesAsync();
                _logger.LogInformation("Daily Sprint Update Job: Auto-send check completed successfully.");
            }
            else
            {
                // Fallback / Manual execution / test triggers: perform reset and auto-send checking
                _logger.LogInformation("Daily Sprint Update Job fired at test/unexpected hour. Resetting and auto-sending.");
                await _dailyUpdateService.ResetDailyUpdatesAsync();
                await _dailyUpdateService.AutoSendDailyUpdatesAsync();
            }
        }
    }
}
