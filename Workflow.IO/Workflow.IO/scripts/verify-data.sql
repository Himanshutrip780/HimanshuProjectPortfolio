-- SQL Verification Script for Workflow.IO Databases
SET NOCOUNT ON;

PRINT '------------------------------------------------';
PRINT '1. Workflow.IOUserDb';
PRINT '------------------------------------------------';
USE Workflow.IOUserDb;
SELECT '[identity].Users' AS [Table], COUNT(*) AS [Count] FROM [identity].Users;
SELECT 'dbo.UserAccounts' AS [Table], COUNT(*) AS [Count] FROM dbo.UserAccounts;
SELECT 'dbo.RefreshTokens' AS [Table], COUNT(*) AS [Count] FROM dbo.RefreshTokens;
GO

PRINT '------------------------------------------------';
PRINT '2. Workflow.IOProjectDb';
PRINT '------------------------------------------------';
USE Workflow.IOProjectDb;
SELECT 'dbo.Projects' AS [Table], COUNT(*) AS [Count] FROM dbo.Projects;
SELECT 'dbo.ProjectMembers' AS [Table], COUNT(*) AS [Count] FROM dbo.ProjectMembers;
GO

PRINT '------------------------------------------------';
PRINT '3. Workflow.IOTaskDb';
PRINT '------------------------------------------------';
USE Workflow.IOTaskDb;
SELECT 'dbo.Boards' AS [Table], COUNT(*) AS [Count] FROM dbo.Boards;
SELECT 'dbo.BoardColumns' AS [Table], COUNT(*) AS [Count] FROM dbo.BoardColumns;
SELECT 'dbo.Sprints' AS [Table], COUNT(*) AS [Count] FROM dbo.Sprints;
SELECT 'dbo.Epics' AS [Table], COUNT(*) AS [Count] FROM dbo.Epics;
SELECT 'dbo.Components' AS [Table], COUNT(*) AS [Count] FROM dbo.Components;
SELECT 'dbo.ReleaseVersions' AS [Table], COUNT(*) AS [Count] FROM dbo.ReleaseVersions;
SELECT 'dbo.Tasks' AS [Table], COUNT(*) AS [Count] FROM dbo.Tasks;
SELECT 'dbo.SubTasks' AS [Table], COUNT(*) AS [Count] FROM dbo.SubTasks;
SELECT 'dbo.TaskLabels' AS [Table], COUNT(*) AS [Count] FROM dbo.TaskLabels;
SELECT 'dbo.TaskWatchers' AS [Table], COUNT(*) AS [Count] FROM dbo.TaskWatchers;
SELECT 'dbo.TaskLinks' AS [Table], COUNT(*) AS [Count] FROM dbo.TaskLinks;
SELECT 'dbo.WorkLogs' AS [Table], COUNT(*) AS [Count] FROM dbo.WorkLogs;
SELECT 'dbo.SavedFilters' AS [Table], COUNT(*) AS [Count] FROM dbo.SavedFilters;
SELECT 'dbo.CalendarDays' AS [Table], COUNT(*) AS [Count] FROM dbo.CalendarDays;
SELECT 'dbo.DailyUpdateStates' AS [Table], COUNT(*) AS [Count] FROM dbo.DailyUpdateStates;
SELECT 'dbo.ProjectIssueCounters' AS [Table], COUNT(*) AS [Count] FROM dbo.ProjectIssueCounters;
SELECT 'dbo.OutboxMessages' AS [Table], COUNT(*) AS [Count] FROM dbo.OutboxMessages;
GO

PRINT '------------------------------------------------';
PRINT '4. Workflow.IOCommentDb';
PRINT '------------------------------------------------';
USE Workflow.IOCommentDb;
SELECT 'dbo.Comments' AS [Table], COUNT(*) AS [Count] FROM dbo.Comments;
SELECT 'dbo.CommentMentions' AS [Table], COUNT(*) AS [Count] FROM dbo.CommentMentions;
SELECT 'dbo.OutboxMessages' AS [Table], COUNT(*) AS [Count] FROM dbo.OutboxMessages;
GO

PRINT '------------------------------------------------';
PRINT '5. Workflow.IONotificationDb';
PRINT '------------------------------------------------';
USE Workflow.IONotificationDb;
SELECT 'dbo.Notifications' AS [Table], COUNT(*) AS [Count] FROM dbo.Notifications;
SELECT 'dbo.ProcessedEvents' AS [Table], COUNT(*) AS [Count] FROM dbo.ProcessedEvents;
GO

PRINT '------------------------------------------------';
PRINT '6. Workflow.IOActivityDb';
PRINT '------------------------------------------------';
USE Workflow.IOActivityDb;
SELECT 'dbo.Activities' AS [Table], COUNT(*) AS [Count] FROM dbo.Activities;
SELECT 'dbo.ProcessedEvents' AS [Table], COUNT(*) AS [Count] FROM dbo.ProcessedEvents;
GO

PRINT '------------------------------------------------';
PRINT '7. Workflow.IOFileDb';
PRINT '------------------------------------------------';
USE Workflow.IOFileDb;
SELECT 'dbo.FileAttachments' AS [Table], COUNT(*) AS [Count] FROM dbo.FileAttachments;
SELECT 'dbo.OutboxMessages' AS [Table], COUNT(*) AS [Count] FROM dbo.OutboxMessages;
GO

PRINT '------------------------------------------------';
PRINT '8. Workflow.IOAnalyticsDb';
PRINT '------------------------------------------------';
USE Workflow.IOAnalyticsDb;
SELECT 'dbo.AnalyticsEvents' AS [Table], COUNT(*) AS [Count] FROM dbo.AnalyticsEvents;
SELECT 'dbo.TaskAnalyticsItems' AS [Table], COUNT(*) AS [Count] FROM dbo.TaskAnalyticsItems;
SELECT 'dbo.ProcessedEvents' AS [Table], COUNT(*) AS [Count] FROM dbo.ProcessedEvents;
GO
