-- ==========================================
-- Workflow.IO Comprehensive Database Seeding Script
-- Author: Senior QA Automation Engineer & Data Architect
-- Target: Microsoft SQL Server (Docker Container)
-- ==========================================

-- ==========================================
-- 1. Workflow.IOUserDb (Identity & Auth)
-- ==========================================
USE Workflow.IOUserDb;
GO

-- Cleanup existing data
DELETE FROM dbo.RefreshTokens;
DELETE FROM dbo.UserAccounts;
DELETE FROM [identity].Users;
GO

-- Seed identity.Users (12 users)
INSERT INTO [identity].Users (UserId, FirstName, LastName, AvatarUrl, Status, CreatedAt, UpdatedAt, IsDeleted) VALUES
('ECD71539-1002-480B-9892-9767D0A7B6A6', 'Himanshu', 'Tripathi', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-01 09:00:00', '2026-05-01 09:00:00', 0),
('E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'Neha', 'Sharma', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-01 09:15:00', '2026-05-01 09:15:00', 0),
('C8583B92-F19F-491A-8518-917D16A1E112', 'Rohit', 'Verma', 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-01 09:30:00', '2026-05-01 09:30:00', 0),
('9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'Karan', 'Patel', 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-01 09:45:00', '2026-05-01 09:45:00', 0),
('D18A246B-4FC2-4B6A-B68C-7023E5DA9F6A', 'Sneha', 'Rao', 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-01 10:00:00', '2026-05-01 10:00:00', 0),
('7A2218D0-449C-4E60-B149-166D44FE24B3', 'Vikram', 'Singh', 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-01 10:15:00', '2026-05-01 10:15:00', 0),
('A6520B4A-711F-4DC0-8BCE-2B1FFCE24C41', 'Aarav', 'Mehta', 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-01 10:30:00', '2026-05-01 10:30:00', 0),
('5D5E99FC-D650-4F58-9A74-1AA8DCF9E32C', 'Priya', 'Nair', 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-01 10:45:00', '2026-05-01 10:45:00', 0),
('FA1DCE4B-2B7A-4D78-8BE5-BD3DE4FA6A88', 'Emily', 'Brown', 'https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-02 08:00:00', '2026-05-02 08:00:00', 0),
('1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', 'John', 'Doe', 'https://images.unsplash.com/photo-1522075469751-3a6694fb2f61?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-02 08:15:00', '2026-05-02 08:15:00', 0),
('4B246C89-AE94-4F9A-B23E-74AE5D89A6C2', 'Alice', 'Smith', 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=150&h=150&q=80', 'Active', '2026-05-02 08:30:00', '2026-05-02 08:30:00', 0),
('07CF2D4C-9786-4F11-8BEA-A8E2CD5A6B44', 'Bob', 'Jones', 'https://images.unsplash.com/photo-1519345182560-3f2917c472ef?auto=format&fit=crop&w=150&h=150&q=80', 'Inactive', '2026-05-02 08:45:00', '2026-05-02 08:45:00', 0);
GO

-- Seed dbo.UserAccounts (12 accounts - bcrypt hash matches "Password123")
INSERT INTO dbo.UserAccounts (Id, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt) VALUES
('ECD71539-1002-480B-9892-9767D0A7B6A6', 'himanshutrip780@gmail.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'Admin', 1, '2026-05-01 09:00:00', '2026-05-01 09:00:00'),
('E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'neha.sharma@workflow.io.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'Admin', 1, '2026-05-01 09:15:00', '2026-05-01 09:15:00'),
('C8583B92-F19F-491A-8518-917D16A1E112', 'rohit.verma@workflow.io.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'User', 1, '2026-05-01 09:30:00', '2026-05-01 09:30:00'),
('9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'karan.patel@workflow.io.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'User', 1, '2026-05-01 09:45:00', '2026-05-01 09:45:00'),
('D18A246B-4FC2-4B6A-B68C-7023E5DA9F6A', 'sneha.rao@workflow.io.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'User', 1, '2026-05-01 10:00:00', '2026-05-01 10:00:00'),
('7A2218D0-449C-4E60-B149-166D44FE24B3', 'vikram.singh@workflow.io.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'User', 1, '2026-05-01 10:15:00', '2026-05-01 10:15:00'),
('A6520B4A-711F-4DC0-8BCE-2B1FFCE24C41', 'aarav.mehta@workflow.io.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'User', 1, '2026-05-01 10:30:00', '2026-05-01 10:30:00'),
('5D5E99FC-D650-4F58-9A74-1AA8DCF9E32C', 'priya.nair@workflow.io.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'User', 1, '2026-05-01 10:45:00', '2026-05-01 10:45:00'),
('FA1DCE4B-2B7A-4D78-8BE5-BD3DE4FA6A88', 'emily.brown@workflow.io.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'Guest', 1, '2026-05-02 08:00:00', '2026-05-02 08:00:00'),
('1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', 'john.doe@indusbank.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'Guest', 1, '2026-05-02 08:15:00', '2026-05-02 08:15:00'),
('4B246C89-AE94-4F9A-B23E-74AE5D89A6C2', 'alice.smith@client.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'Guest', 1, '2026-05-02 08:30:00', '2026-05-02 08:30:00'),
('07CF2D4C-9786-4F11-8BEA-A8E2CD5A6B44', 'bob.jones@contractor.com', '$2a$11$zF08774I25zinWLRl0kXv.EH8WqhAh5b6sWSTMY00k.uO1p62nYeC', 'Guest', 0, '2026-05-02 08:45:00', '2026-05-02 08:45:00');
GO

-- Seed dbo.RefreshTokens (12 tokens)
INSERT INTO dbo.RefreshTokens (RefreshTokenId, UserAccountId, TokenHash, ExpiresAt, CreatedAt, RevokedAt) VALUES
(NEWID(), 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'token_hash_value_himanshu_123456789', '2026-06-01 09:00:00', '2026-05-24 09:00:00', NULL),
(NEWID(), 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'token_hash_value_neha_123456789', '2026-06-01 09:15:00', '2026-05-24 09:15:00', NULL),
(NEWID(), 'C8583B92-F19F-491A-8518-917D16A1E112', 'token_hash_value_rohit_123456789', '2026-06-01 09:30:00', '2026-05-24 09:30:00', NULL),
(NEWID(), '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'token_hash_value_karan_123456789', '2026-06-01 09:45:00', '2026-05-24 09:45:00', NULL),
(NEWID(), 'D18A246B-4FC2-4B6A-B68C-7023E5DA9F6A', 'token_hash_value_sneha_123456789', '2026-06-01 10:00:00', '2026-05-24 10:00:00', NULL),
(NEWID(), '7A2218D0-449C-4E60-B149-166D44FE24B3', 'token_hash_value_vikram_123456789', '2026-06-01 10:15:00', '2026-05-24 10:15:00', NULL),
(NEWID(), 'A6520B4A-711F-4DC0-8BCE-2B1FFCE24C41', 'token_hash_value_aarav_123456789', '2026-06-01 10:30:00', '2026-05-24 10:30:00', NULL),
(NEWID(), '5D5E99FC-D650-4F58-9A74-1AA8DCF9E32C', 'token_hash_value_priya_123456789', '2026-06-01 10:45:00', '2026-05-24 10:45:00', NULL),
(NEWID(), 'FA1DCE4B-2B7A-4D78-8BE5-BD3DE4FA6A88', 'token_hash_value_emily_123456789', '2026-06-01 08:00:00', '2026-05-24 08:00:00', NULL),
(NEWID(), '1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', 'token_hash_value_john_123456789', '2026-06-01 08:15:00', '2026-05-24 08:15:00', NULL),
(NEWID(), '4B246C89-AE94-4F9A-B23E-74AE5D89A6C2', 'token_hash_value_alice_123456789', '2026-06-01 08:30:00', '2026-05-24 08:30:00', NULL),
(NEWID(), '07CF2D4C-9786-4F11-8BEA-A8E2CD5A6B44', 'token_hash_value_bob_123456789', '2026-06-01 08:45:00', '2026-05-24 08:45:00', '2026-05-24 12:00:00');
GO


-- ==========================================
-- 2. Workflow.IOProjectDb
-- ==========================================
USE Workflow.IOProjectDb;
GO

-- Cleanup existing data
DELETE FROM dbo.ProjectMembers;
DELETE FROM dbo.Projects;
GO

-- Seed dbo.Projects (12 projects, status: 0=Active, 2=Completed, types: Scrum/Kanban)
INSERT INTO dbo.Projects (ProjectId, Name, Description, OwnerId, [Key], ProjectType, Status, CreatedAt, UpdatedAt) VALUES
('B0010000-0000-0000-0000-000000000001', 'Workflow.IO SaaS Core Platform', 'Core SaaS project management redesign inspired by Vercel and Linear design aesthetics. Includes fully responsive dashboard, Kanban drag-and-drop workflow system, advanced Gantt chart roadmaps, task calendars, team collaboration boards, and micro-frontend components.', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'WORKFLOW.IO', 'Scrum', 0, '2026-05-01 09:00:00', '2026-05-24 09:00:00'),
('B0020000-0000-0000-0000-000000000002', 'Client Escalation Portal', 'Portal for tracking major high-importance escalation tickets filed by VIP corporate accounts. Crucial service level agreements (SLAs) apply.', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'CLIENT', 'Kanban', 0, '2026-05-02 10:00:00', '2026-05-24 10:00:00'),
('B0030000-0000-0000-0000-000000000003', 'Indus Bank Migration', 'Indus Bank legacy migration project. This database migration effort aims to convert over forty-five million legacy customer rows spanning multiple legacy databases and Oracle database systems into high-performance distributed Microsoft SQL Server clusters with zero downtime. Requires strict data compliance, end-to-end data auditing, and multi-tenant security verification at every database layer.', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'INDUS', 'Scrum', 0, '2026-05-03 11:00:00', '2026-05-24 11:00:00'),
('B0040000-0000-0000-0000-000000000004', 'Analytics Metric Engine', 'High throughput analytics metrics dashboard engine aggregating real-time operations, issue distributions, work logs, project velocity and KPI parameters.', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'ANALYTICS', 'Scrum', 0, '2026-05-04 12:00:00', '2026-05-24 12:00:00'),
('B0050000-0000-0000-0000-000000000005', 'Workflow.IO Mobile Native Apps', 'Native mobile application for Workflow.IO project workspace, developed in Flutter for iOS and Android platforms, emphasizing modern design with responsive styling.', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'MOBILE', 'Scrum', 0, '2026-05-05 13:00:00', '2026-05-24 13:00:00'),
('B0060000-0000-0000-0000-000000000006', 'SSO & MFA Security Suite', 'Implementation of Single Sign-On and Multi-Factor Authentication for enterprise partners. Features robust JWT validation and claims audits.', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'AUTH', 'Scrum', 0, '2026-05-06 14:00:00', '2026-05-24 14:00:00'),
('B0070000-0000-0000-0000-000000000007', 'API Gateway Performance Tuning', 'YARP Gateway tuning to optimize routing latency, rate limiting policies, and TLS handshakes.', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'GATEWAY', 'Kanban', 0, '2026-05-07 15:00:00', '2026-05-24 15:00:00'),
('B0080000-0000-0000-0000-000000000008', 'Subscription Billing & Stripe', 'Billing integrations supporting plans, subscriptions, Stripe checkout webhooks, discount structures.', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'BILLING', 'Scrum', 0, '2026-05-08 16:00:00', '2026-05-24 16:00:00'),
('B0090000-0000-0000-0000-000000000009', 'Real-time PubSub Notifications', 'Websockets and SignalR messaging architecture delivering instant alerts to UI users on task updates, comments, and mentions.', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'NOTIFY', 'Kanban', 2, '2026-05-09 17:00:00', '2026-05-24 17:00:00'),
('B0100000-0000-0000-0000-000000000010', 'Kubernetes Cloud Deployments', 'Setting up Azure Kubernetes Services (AKS) deployments, Helm charts, and CI/CD pipelines for zero-downtime microservice staging environments.', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'DEVOPS', 'Scrum', 2, '2026-05-10 18:00:00', '2026-05-24 18:00:00'),
('B0110000-0000-0000-0000-000000000011', 'Workflow.IO Customer Helpdesk', 'Integration of support ticketing services and Zendesk APIs into the project management workspace, tracking customer escalations directly.', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'SUPPORT', 'Kanban', 2, '2026-05-11 19:00:00', '2026-05-24 19:00:00'),
('B0120000-0000-0000-0000-000000000012', 'Growth Hacking & SEO Campaigns', 'Marketing page updates, search engine optimizations, content generation workflows, and Google Analytics integrations.', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'MARKETING', 'Kanban', 0, '2026-05-12 20:00:00', '2026-05-24 20:00:00');
GO

-- Seed dbo.ProjectMembers (12 members, roles: Owner=0, Admin=1, Member=2, Viewer=3)
INSERT INTO dbo.ProjectMembers (ProjectMemberId, ProjectId, UserId, Role, JoinedAt) VALUES
(NEWID(), 'B0010000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 0, '2026-05-01 09:00:00'),
(NEWID(), 'B0010000-0000-0000-0000-000000000001', 'C8583B92-F19F-491A-8518-917D16A1E112', 2, '2026-05-01 09:30:00'),
(NEWID(), 'B0010000-0000-0000-0000-000000000001', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 2, '2026-05-01 09:45:00'),
(NEWID(), 'B0020000-0000-0000-0000-000000000002', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 0, '2026-05-02 10:00:00'),
(NEWID(), 'B0020000-0000-0000-0000-000000000002', 'D18A246B-4FC2-4B6A-B68C-7023E5DA9F6A', 2, '2026-05-02 10:15:00'),
(NEWID(), 'B0030000-0000-0000-0000-000000000003', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 0, '2026-05-03 11:00:00'),
(NEWID(), 'B0030000-0000-0000-0000-000000000003', 'C8583B92-F19F-491A-8518-917D16A1E112', 2, '2026-05-03 11:30:00'),
(NEWID(), 'B0040000-0000-0000-0000-000000000004', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 0, '2026-05-04 12:00:00'),
(NEWID(), 'B0050000-0000-0000-0000-000000000005', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 0, '2026-05-05 13:00:00'),
(NEWID(), 'B0060000-0000-0000-0000-000000000006', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 0, '2026-05-06 14:00:00'),
(NEWID(), 'B0060000-0000-0000-0000-000000000006', '7A2218D0-449C-4E60-B149-166D44FE24B3', 2, '2026-05-06 14:15:00'),
(NEWID(), 'B0070000-0000-0000-0000-000000000007', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 0, '2026-05-07 15:00:00');
GO


-- ==========================================
-- 3. Workflow.IOTaskDb
-- ==========================================
USE Workflow.IOTaskDb;
GO

-- Cleanup existing data
DELETE FROM dbo.SubTasks;
DELETE FROM dbo.TaskLabels;
DELETE FROM dbo.TaskWatchers;
DELETE FROM dbo.TaskLinks;
DELETE FROM dbo.WorkLogs;
DELETE FROM dbo.Tasks;
DELETE FROM dbo.BoardColumns;
DELETE FROM dbo.Boards;
DELETE FROM dbo.Sprints;
DELETE FROM dbo.Epics;
DELETE FROM dbo.Components;
DELETE FROM dbo.ReleaseVersions;
DELETE FROM dbo.SavedFilters;
DELETE FROM dbo.CalendarDays;
DELETE FROM dbo.DailyUpdateStates;
DELETE FROM dbo.ProjectIssueCounters;
DELETE FROM dbo.OutboxMessages;
GO

-- Seed dbo.Boards (12 boards, one for each project)
INSERT INTO dbo.Boards (BoardId, ProjectId, Name, CreatedAt, UpdatedAt) VALUES
('A0010000-0000-0000-0000-000000000001', 'B0010000-0000-0000-0000-000000000001', 'WORKFLOW.IO Board', '2026-05-01 09:00:00', '2026-05-01 09:00:00'),
('A0020000-0000-0000-0000-000000000002', 'B0020000-0000-0000-0000-000000000002', 'CLIENT Board', '2026-05-02 10:00:00', '2026-05-02 10:00:00'),
('A0030000-0000-0000-0000-000000000003', 'B0030000-0000-0000-0000-000000000003', 'INDUS Board', '2026-05-03 11:00:00', '2026-05-03 11:00:00'),
('A0040000-0000-0000-0000-000000000004', 'B0040000-0000-0000-0000-000000000004', 'ANALYTICS Board', '2026-05-04 12:00:00', '2026-05-04 12:00:00'),
('A0050000-0000-0000-0000-000000000005', 'B0050000-0000-0000-0000-000000000005', 'MOBILE Board', '2026-05-05 13:00:00', '2026-05-05 13:00:00'),
('A0060000-0000-0000-0000-000000000006', 'B0060000-0000-0000-0000-000000000006', 'AUTH Board', '2026-05-06 14:00:00', '2026-05-06 14:00:00'),
('A0070000-0000-0000-0000-000000000007', 'B0070000-0000-0000-0000-000000000007', 'GATEWAY Board', '2026-05-07 15:00:00', '2026-05-07 15:00:00'),
('A0080000-0000-0000-0000-000000000008', 'B0080000-0000-0000-0000-000000000008', 'BILLING Board', '2026-05-08 16:00:00', '2026-05-08 16:00:00'),
('A0090000-0000-0000-0000-000000000009', 'B0090000-0000-0000-0000-000000000009', 'NOTIFY Board', '2026-05-09 17:00:00', '2026-05-09 17:00:00'),
('A0100000-0000-0000-0000-000000000010', 'B0100000-0000-0000-0000-000000000010', 'DEVOPS Board', '2026-05-10 18:00:00', '2026-05-10 18:00:00'),
('A0110000-0000-0000-0000-000000000011', 'B0110000-0000-0000-0000-000000000011', 'SUPPORT Board', '2026-05-11 19:00:00', '2026-05-11 19:00:00'),
('A0120000-0000-0000-0000-000000000012', 'B0120000-0000-0000-0000-000000000012', 'MARKETING Board', '2026-05-12 20:00:00', '2026-05-12 20:00:00');
GO

-- Seed dbo.BoardColumns (5 columns per board = 60 rows total)
-- Board 1 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0010000-0000-0000-0000-000000000001', 'To Do', 'Todo', 1),
(NEWID(), 'A0010000-0000-0000-0000-000000000001', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0010000-0000-0000-0000-000000000001', 'Review', 'Review', 3),
(NEWID(), 'A0010000-0000-0000-0000-000000000001', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0010000-0000-0000-0000-000000000001', 'Done', 'Done', 5);
-- Board 2 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0020000-0000-0000-0000-000000000002', 'To Do', 'Todo', 1),
(NEWID(), 'A0020000-0000-0000-0000-000000000002', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0020000-0000-0000-0000-000000000002', 'Review', 'Review', 3),
(NEWID(), 'A0020000-0000-0000-0000-000000000002', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0020000-0000-0000-0000-000000000002', 'Done', 'Done', 5);
-- Board 3 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0030000-0000-0000-0000-000000000003', 'To Do', 'Todo', 1),
(NEWID(), 'A0030000-0000-0000-0000-000000000003', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0030000-0000-0000-0000-000000000003', 'Review', 'Review', 3),
(NEWID(), 'A0030000-0000-0000-0000-000000000003', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0030000-0000-0000-0000-000000000003', 'Done', 'Done', 5);
-- Board 4 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0040000-0000-0000-0000-000000000004', 'To Do', 'Todo', 1),
(NEWID(), 'A0040000-0000-0000-0000-000000000004', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0040000-0000-0000-0000-000000000004', 'Review', 'Review', 3),
(NEWID(), 'A0040000-0000-0000-0000-000000000004', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0040000-0000-0000-0000-000000000004', 'Done', 'Done', 5);
-- Board 5 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0050000-0000-0000-0000-000000000005', 'To Do', 'Todo', 1),
(NEWID(), 'A0050000-0000-0000-0000-000000000005', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0050000-0000-0000-0000-000000000005', 'Review', 'Review', 3),
(NEWID(), 'A0050000-0000-0000-0000-000000000005', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0050000-0000-0000-0000-000000000005', 'Done', 'Done', 5);
-- Board 6 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0060000-0000-0000-0000-000000000006', 'To Do', 'Todo', 1),
(NEWID(), 'A0060000-0000-0000-0000-000000000006', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0060000-0000-0000-0000-000000000006', 'Review', 'Review', 3),
(NEWID(), 'A0060000-0000-0000-0000-000000000006', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0060000-0000-0000-0000-000000000006', 'Done', 'Done', 5);
-- Board 7 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0070000-0000-0000-0000-000000000007', 'To Do', 'Todo', 1),
(NEWID(), 'A0070000-0000-0000-0000-000000000007', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0070000-0000-0000-0000-000000000007', 'Review', 'Review', 3),
(NEWID(), 'A0070000-0000-0000-0000-000000000007', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0070000-0000-0000-0000-000000000007', 'Done', 'Done', 5);
-- Board 8 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0080000-0000-0000-0000-000000000008', 'To Do', 'Todo', 1),
(NEWID(), 'A0080000-0000-0000-0000-000000000008', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0080000-0000-0000-0000-000000000008', 'Review', 'Review', 3),
(NEWID(), 'A0080000-0000-0000-0000-000000000008', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0080000-0000-0000-0000-000000000008', 'Done', 'Done', 5);
-- Board 9 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0090000-0000-0000-0000-000000000009', 'To Do', 'Todo', 1),
(NEWID(), 'A0090000-0000-0000-0000-000000000009', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0090000-0000-0000-0000-000000000009', 'Review', 'Review', 3),
(NEWID(), 'A0090000-0000-0000-0000-000000000009', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0090000-0000-0000-0000-000000000009', 'Done', 'Done', 5);
-- Board 10 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0100000-0000-0000-0000-000000000010', 'To Do', 'Todo', 1),
(NEWID(), 'A0100000-0000-0000-0000-000000000010', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0100000-0000-0000-0000-000000000010', 'Review', 'Review', 3),
(NEWID(), 'A0100000-0000-0000-0000-000000000010', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0100000-0000-0000-0000-000000000010', 'Done', 'Done', 5);
-- Board 11 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0110000-0000-0000-0000-000000000011', 'To Do', 'Todo', 1),
(NEWID(), 'A0110000-0000-0000-0000-000000000011', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0110000-0000-0000-0000-000000000011', 'Review', 'Review', 3),
(NEWID(), 'A0110000-0000-0000-0000-000000000011', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0110000-0000-0000-0000-000000000011', 'Done', 'Done', 5);
-- Board 12 Columns
INSERT INTO dbo.BoardColumns (BoardColumnId, BoardId, Name, Status, SortOrder) VALUES
(NEWID(), 'A0120000-0000-0000-0000-000000000012', 'To Do', 'Todo', 1),
(NEWID(), 'A0120000-0000-0000-0000-000000000012', 'In Progress', 'InProgress', 2),
(NEWID(), 'A0120000-0000-0000-0000-000000000012', 'Review', 'Review', 3),
(NEWID(), 'A0120000-0000-0000-0000-000000000012', 'Blocked', 'Blocked', 4),
(NEWID(), 'A0120000-0000-0000-0000-000000000012', 'Done', 'Done', 5);
GO

-- Seed dbo.Sprints (12 sprints, status: Active, Planned, Completed)
INSERT INTO dbo.Sprints (SprintId, ProjectId, Name, StartDate, EndDate, Status, CreatedAt, UpdatedAt) VALUES
('55510000-0000-0000-0000-000000000001', 'B0010000-0000-0000-0000-000000000001', 'ZT Sprint 1: Foundation', '2026-05-01 09:00:00', '2026-05-14 18:00:00', 'Completed', '2026-05-01 09:00:00', '2026-05-14 18:00:00'),
('55510000-0000-0000-0000-000000000002', 'B0010000-0000-0000-0000-000000000001', 'ZT Sprint 2: Core SaaS UI', '2026-05-15 09:00:00', '2026-05-28 18:00:00', 'Active', '2026-05-01 09:00:00', '2026-05-15 09:00:00'),
('55510000-0000-0000-0000-000000000003', 'B0010000-0000-0000-0000-000000000001', 'ZT Sprint 3: Advanced Charts', '2026-05-29 09:00:00', '2026-06-11 18:00:00', 'Planned', '2026-05-01 09:00:00', '2026-05-01 09:00:00'),
('55510000-0000-0000-0000-000000000004', 'B0030000-0000-0000-0000-000000000003', 'Indus Migration Sprint 1', '2026-05-03 09:00:00', '2026-05-17 18:00:00', 'Completed', '2026-05-03 09:00:00', '2026-05-17 18:00:00'),
('55510000-0000-0000-0000-000000000005', 'B0030000-0000-0000-0000-000000000003', 'Indus Migration Sprint 2', '2026-05-18 09:00:00', '2026-06-01 18:00:00', 'Active', '2026-05-03 09:00:00', '2026-05-18 09:00:00'),
('55510000-0000-0000-0000-000000000006', 'B0040000-0000-0000-0000-000000000004', 'Analytics Metrics Sprint 1', '2026-05-04 09:00:00', '2026-05-18 18:00:00', 'Completed', '2026-05-04 09:00:00', '2026-05-18 18:00:00'),
('55510000-0000-0000-0000-000000000007', 'B0040000-0000-0000-0000-000000000004', 'Analytics Metrics Sprint 2', '2026-05-19 09:00:00', '2026-06-02 18:00:00', 'Active', '2026-05-04 09:00:00', '2026-05-19 09:00:00'),
('55510000-0000-0000-0000-000000000008', 'B0050000-0000-0000-0000-000000000005', 'Mobile Core Sprint 1', '2026-05-05 09:00:00', '2026-05-19 18:00:00', 'Completed', '2026-05-05 09:00:00', '2026-05-19 18:00:00'),
('55510000-0000-0000-0000-000000000009', 'B0050000-0000-0000-0000-000000000005', 'Mobile UI Sprint 2', '2026-05-20 09:00:00', '2026-06-03 18:00:00', 'Active', '2026-05-05 09:00:00', '2026-05-20 09:00:00'),
('55510000-0000-0000-0000-000000000010', 'B0060000-0000-0000-0000-000000000006', 'SSO Security Sprint 1', '2026-05-06 09:00:00', '2026-05-20 18:00:00', 'Completed', '2026-05-06 09:00:00', '2026-05-20 18:00:00'),
('55510000-0000-0000-0000-000000000011', 'B0080000-0000-0000-0000-000000000008', 'Billing Integration Sprint 1', '2026-05-08 09:00:00', '2026-05-22 18:00:00', 'Completed', '2026-05-08 09:00:00', '2026-05-22 18:00:00'),
('55510000-0000-0000-0000-000000000012', 'B0080000-0000-0000-0000-000000000008', 'Billing Webhooks Sprint 2', '2026-05-23 09:00:00', '2026-06-06 18:00:00', 'Active', '2026-05-08 09:00:00', '2026-05-23 09:00:00');
GO

-- Seed dbo.Epics (12 epics)
INSERT INTO dbo.Epics (EpicId, ProjectId, Name, Description, CreatedAt, UpdatedAt) VALUES
('E0010000-0000-0000-0000-000000000001', 'B0010000-0000-0000-0000-000000000001', 'Vercel-inspired Core UI Design', 'Implement a dark/light glassmorphic UI design matching Linear and Vercel standards across all user interfaces.', '2026-05-01 09:00:00', '2026-05-01 09:00:00'),
('E0010000-0000-0000-0000-000000000002', 'B0010000-0000-0000-0000-000000000001', 'Real-time WebSocket Engine', 'Websockets and SignalR endpoints mapping changes directly to user browsers instantly.', '2026-05-01 09:00:00', '2026-05-01 09:00:00'),
('E0010000-0000-0000-0000-000000000003', 'B0020000-0000-0000-0000-000000000002', 'VIP Escalation Service Desk', 'Dedicated dashboards and SLA monitors for VIP escalations.', '2026-05-02 10:00:00', '2026-05-02 10:00:00'),
('E0010000-0000-0000-0000-000000000004', 'B0030000-0000-0000-0000-000000000003', 'Indus Oracle DB Migration', 'Migration of oracle database tables containing core transaction records to SQL Server database.', '2026-05-03 11:00:00', '2026-05-03 11:00:00'),
('E0010000-0000-0000-0000-000000000005', 'B0040000-0000-0000-0000-000000000004', 'Analytics Reporting Aggregations', 'Pre-calculating analytics indicators, speeds, trends, and project KPIs.', '2026-05-04 12:00:00', '2026-05-04 12:00:00'),
('E0010000-0000-0000-0000-000000000006', 'B0050000-0000-0000-0000-000000000005', 'Mobile App Push Notifications', 'Integration of Firebase Cloud Messaging for instant mobile push alerts.', '2026-05-05 13:00:00', '2026-05-05 13:00:00'),
('E0010000-0000-0000-0000-000000000007', 'B0060000-0000-0000-0000-000000000006', 'OAuth2/SSO Enterprise Setup', 'Federated identity setup for business customers via SAML/OIDC.', '2026-05-06 14:00:00', '2026-05-06 14:00:00'),
('E0010000-0000-0000-0000-000000000008', 'B0070000-0000-0000-0000-000000000007', 'API Gateway Resilience Policy', 'Polly resilience, retry logic, timeout handling, and rate limiting in YARP gateway.', '2026-05-07 15:00:00', '2026-05-07 15:00:00'),
('E0010000-0000-0000-0000-000000000009', 'B0080000-0000-0000-0000-000000000008', 'Stripe Subscriptions Core', 'Stripe webhook mapping, billing schedules, and recurring pricing tiers.', '2026-05-08 16:00:00', '2026-05-08 16:00:00'),
('E0010000-0000-0000-0000-000000000010', 'B0100000-0000-0000-0000-000000000010', 'K8s ArgoCD GitOps Pipeline', 'Deploying ArgoCD workflows for fully automated deployments from git branches.', '2026-05-10 18:00:00', '2026-05-10 18:00:00'),
('E0010000-0000-0000-0000-000000000011', 'B0110000-0000-0000-0000-000000000011', 'Zendesk Ticket Live Sync', 'Two-way integration linking Zendesk escalations straight to Kanban bugs.', '2026-05-11 19:00:00', '2026-05-11 19:00:00'),
('E0010000-0000-0000-0000-000000000012', 'B0120000-0000-0000-0000-000000000012', 'Marketing Growth Strategy', 'Search engine optimization, landing page UI improvements, conversion dashboards.', '2026-05-12 20:00:00', '2026-05-12 20:00:00');
GO

-- Seed dbo.Components (12 components)
INSERT INTO dbo.Components (ComponentId, ProjectId, Name, Description, CreatedAt) VALUES
('C0010000-0000-0000-0000-000000000001', 'B0010000-0000-0000-0000-000000000001', 'Frontend Angular UI', 'Angular components, layouts, state management, routes.', '2026-05-01 09:00:00'),
('C0010000-0000-0000-0000-000000000002', 'B0010000-0000-0000-0000-000000000001', 'Backend Gateway Core', 'GatewayApi and Ocelot/YARP configurations.', '2026-05-01 09:00:00'),
('C0010000-0000-0000-0000-000000000003', 'B0010000-0000-0000-0000-000000000001', 'Identity API', 'User management, authorization, profiles.', '2026-05-01 09:00:00'),
('C0010000-0000-0000-0000-000000000004', 'B0030000-0000-0000-0000-000000000003', 'DB Migration Scripts', 'Data pipelines, bulk copy scripts, audit tools.', '2026-05-03 11:00:00'),
('C0010000-0000-0000-0000-000000000005', 'B0040000-0000-0000-0000-000000000004', 'Analytics Cron Jobs', 'Background jobs calculating stats daily.', '2026-05-04 12:00:00'),
('C0010000-0000-0000-0000-000000000006', 'B0050000-0000-0000-0000-000000000005', 'Flutter Navigation', 'State routing, tabs, shell pages in mobile app.', '2026-05-05 13:00:00'),
('C0010000-0000-0000-0000-000000000007', 'B0060000-0000-0000-0000-000000000006', 'OAuth Adapters', 'Bespoke adapters integration for Active Directory.', '2026-05-06 14:00:00'),
('C0010000-0000-0000-0000-000000000008', 'B0070000-0000-0000-0000-000000000007', 'Polly Resilience Logic', 'Circuit breaker configuration classes.', '2026-05-07 15:00:00'),
('C0010000-0000-0000-0000-000000000009', 'B0080000-0000-0000-0000-000000000008', 'Stripe Webhooks Handler', 'Handling event types: customer.subscription.updated.', '2026-05-08 16:00:00'),
('C0010000-0000-0000-0000-000000000010', 'B0100000-0000-0000-0000-000000000010', 'Helm Configuration Charts', 'Microservice Helm values YAML definitions.', '2026-05-10 18:00:00'),
('C0010000-0000-0000-0000-000000000011', 'B0110000-0000-0000-0000-000000000011', 'Zendesk API SDK', 'HttpClient classes querying Zendesk APIs.', '2026-05-11 19:00:00'),
('C0010000-0000-0000-0000-000000000012', 'B0120000-0000-0000-0000-000000000012', 'SEO Meta Tags Manager', 'Dynamic title/meta injection service for search indexers.', '2026-05-12 20:00:00');
GO

-- Seed dbo.ReleaseVersions (12 release versions)
INSERT INTO dbo.ReleaseVersions (ReleaseVersionId, ProjectId, Name, Description, IsReleased, ReleaseDate, CreatedAt) VALUES
('99910000-0000-0000-0000-000000000001', 'B0010000-0000-0000-0000-000000000001', 'v1.0.0-rc1', 'Release candidate 1 for core saas launch.', 1, '2026-05-10 00:00:00', '2026-05-01 09:00:00'),
('99910000-0000-0000-0000-000000000002', 'B0010000-0000-0000-0000-000000000001', 'v1.0.0', 'Production launch release.', 0, NULL, '2026-05-01 09:00:00'),
('99910000-0000-0000-0000-000000000003', 'B0020000-0000-0000-0000-000000000002', 'v1.1.0-escalate', 'Escalation portal beta version.', 1, '2026-05-15 00:00:00', '2026-05-02 10:00:00'),
('99910000-0000-0000-0000-000000000004', 'B0030000-0000-0000-0000-000000000003', 'v2.0.0-indus-alpha', 'First data migration trial phase release.', 0, NULL, '2026-05-03 11:00:00'),
('99910000-0000-0000-0000-000000000005', 'B0040000-0000-0000-0000-000000000004', 'v0.9.0-metrics', 'Analytics engine pre-release.', 1, '2026-05-20 00:00:00', '2026-05-04 12:00:00'),
('99910000-0000-0000-0000-000000000006', 'B0050000-0000-0000-0000-000000000005', 'v1.0-android', 'Android production app release.', 1, '2026-05-22 00:00:00', '2026-05-05 13:00:00'),
('99910000-0000-0000-0000-000000000007', 'B0060000-0000-0000-0000-000000000006', 'v1.5.0-mfa', 'SSO security module integration version.', 0, NULL, '2026-05-06 14:00:00'),
('99910000-0000-0000-0000-000000000008', 'B0070000-0000-0000-0000-000000000007', 'v1.2.0-gateway', 'API gateway performance updates.', 1, '2026-05-24 00:00:00', '2026-05-07 15:00:00'),
('99910000-0000-0000-0000-000000000009', 'B0080000-0000-0000-0000-000000000008', 'v2.1.0-billing', 'Stripe checkout and coupon features.', 0, NULL, '2026-05-08 16:00:00'),
('99910000-0000-0000-0000-000000000010', 'B0100000-0000-0000-0000-000000000010', 'v3.0.0-k8s', 'Kubernetes cluster deployments release.', 1, '2026-05-24 00:00:00', '2026-05-10 18:00:00'),
('99910000-0000-0000-0000-000000000011', 'B0110000-0000-0000-0000-000000000011', 'v1.4.0-helpdesk', 'Zendesk ticket syncing integration patch.', 0, NULL, '2026-05-11 19:00:00'),
('99910000-0000-0000-0000-000000000012', 'B0120000-0000-0000-0000-000000000012', 'v1.0.2-seo', 'SEO optimization campaigns version.', 1, '2026-05-24 12:00:00', '2026-05-12 20:00:00');
GO

-- Seed dbo.Tasks (12 tasks)
INSERT INTO dbo.Tasks (TaskId, ProjectId, IssueNumber, IssueKey, IssueType, Title, Description, Status, Priority, Resolution, AssigneeId, ReporterId, SprintId, EpicId, ParentTaskId, ComponentId, FixVersionId, StoryPoints, OriginalEstimateMinutes, RemainingEstimateMinutes, BacklogRank, DueDate, CreatedAt, UpdatedAt, FeDeveloper, BeDeveloper, QaEngineer, InitialEta, LatestEta) VALUES
('88810000-0000-0000-0000-000000000001', 'B0010000-0000-0000-0000-000000000001', 1, 'WORKFLOW.IO-1', 'Task', 'Design Modern Light and Dark SaaS Layout', 'Create Vercel-inspired landing page and dashboard workspace layouts. Make sure it uses vibrant gradients, micro-animations, glassmorphic card boundaries, and responsive desktop sidebar navigation. Needs to support responsive breakpoints from 1024px upwards.', 'InProgress', 'High', NULL, 'C8583B92-F19F-491A-8518-917D16A1E112', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '55510000-0000-0000-0000-000000000002', 'E0010000-0000-0000-0000-000000000001', NULL, 'C0010000-0000-0000-0000-000000000001', '99910000-0000-0000-0000-000000000002', 5, 240, 120, 1.0000, '2026-05-30 18:00:00', '2026-05-15 09:00:00', '2026-05-24 09:00:00', 'Rohit Verma', NULL, 'Sneha Rao', '2026-05-29 18:00:00', '2026-05-30 18:00:00'),
('88810000-0000-0000-0000-000000000002', 'B0010000-0000-0000-0000-000000000001', 2, 'WORKFLOW.IO-2', 'Bug', 'Gateway API Routing Failure under High Load', 'Gateway throws HTTP 502 Bad Gateway intermittently when traffic spikes above 5,000 requests per minute. Stack trace: System.Net.Http.HttpRequestException: Connection refused ---> System.Net.Sockets.SocketException: Address already in use at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.CreateException... at Microsoft.AspNetCore.Http.DefaultHttpContext.set_Response...', 'Blocked', 'Critical', NULL, '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '55510000-0000-0000-0000-000000000002', 'E0010000-0000-0000-0000-000000000002', NULL, 'C0010000-0000-0000-0000-000000000002', '99910000-0000-0000-0000-000000000002', 8, 480, 480, 2.0000, '2026-05-28 18:00:00', '2026-05-16 10:00:00', '2026-05-24 10:00:00', NULL, 'Karan Patel', 'Sneha Rao', '2026-05-27 18:00:00', '2026-05-28 18:00:00'),
('88810000-0000-0000-0000-000000000003', 'B0020000-0000-0000-0000-000000000002', 1, 'CLIENT-1', 'Bug', 'Indus Bank escalation: dashboard charts blank', 'Indus bank clients report that the metrics page does not render any graphs. Console log error shows: TypeError: Cannot read properties of undefined (reading map) at AnalyticsComponent.renderCharts (analytics.component.ts:145). This causes full blockage for VIP user audits.', 'Todo', 'Critical', NULL, 'C8583B92-F19F-491A-8518-917D16A1E112', '1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', NULL, 'E0010000-0000-0000-0000-000000000003', NULL, 'C0010000-0000-0000-0000-000000000001', '99910000-0000-0000-0000-000000000003', 3, 120, 120, 3.0000, '2026-05-26 12:00:00', '2026-05-24 08:00:00', '2026-05-24 08:00:00', 'Rohit Verma', NULL, 'Sneha Rao', '2026-05-25 18:00:00', '2026-05-26 12:00:00'),
('88810000-0000-0000-0000-000000000004', 'B0030000-0000-0000-0000-000000000003', 1, 'INDUS-1', 'Story', 'Migration of Customer Profile Records', 'Develop and verify Oracle bulk export scripts to transfer customer identity profiles (estimated 15 million rows) directly into MS SQL Server clustered databases with zero data loss or encoding formatting errors.', 'InProgress', 'Medium', NULL, '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '55510000-0000-0000-0000-000000000005', 'E0010000-0000-0000-0000-000000000004', NULL, 'C0010000-0000-0000-0000-000000000004', '99910000-0000-0000-0000-000000000004', 13, 960, 480, 4.0000, '2026-05-30 18:00:00', '2026-05-18 09:00:00', '2026-05-24 09:00:00', NULL, 'Karan Patel', 'Sneha Rao', '2026-05-29 18:00:00', '2026-05-30 18:00:00'),
('88810000-0000-0000-0000-000000000005', 'B0030000-0000-0000-0000-000000000003', 2, 'INDUS-2', 'Bug', 'Bcrypt password migration compatibility issue', 'Bcrypt password hashes migrated from original PHP codebase are failing validation in JwtAuthenticationManager due to differences in formatting ($2y$ vs $2a$). Script needs to rewrite prefixes during database imports.', 'Review', 'High', NULL, '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '55510000-0000-0000-0000-000000000005', 'E0010000-0000-0000-0000-000000000004', NULL, 'C0010000-0000-0000-0000-000000000004', '99910000-0000-0000-0000-000000000004', 5, 240, 30, 5.0000, '2026-05-29 18:00:00', '2026-05-19 10:00:00', '2026-05-24 10:00:00', NULL, 'Karan Patel', 'Sneha Rao', '2026-05-28 18:00:00', '2026-05-29 18:00:00'),
('88810000-0000-0000-0000-000000000006', 'B0040000-0000-0000-0000-000000000004', 1, 'ANALYTICS-1', 'Task', 'Implement pre-aggregation cron scheduler', 'Write a C# background worker class using Quartz.NET scheduler to compute dashboard stats hourly. Minimizes direct analytics DB query load during peak traffic times.', 'Done', 'Medium', 'Fixed', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '55510000-0000-0000-0000-000000000006', 'E0010000-0000-0000-0000-000000000005', NULL, 'C0010000-0000-0000-0000-000000000005', '99910000-0000-0000-0000-000000000005', 8, 360, 0, 6.0000, '2026-05-18 18:00:00', '2026-05-04 09:00:00', '2026-05-18 18:00:00', NULL, 'Karan Patel', 'Sneha Rao', '2026-05-18 18:00:00', '2026-05-18 18:00:00'),
('88810000-0000-0000-0000-000000000007', 'B0050000-0000-0000-0000-000000000005', 1, 'MOBILE-1', 'Task', 'Integrate FCM (Firebase) Push Notifications SDK', 'Set up Firebase SDK in Flutter app, configure certificates for iOS APNS and Android FCM services, and test background messaging handling.', 'Done', 'High', 'Fixed', 'C8583B92-F19F-491A-8518-917D16A1E112', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '55510000-0000-0000-0000-000000000008', 'E0010000-0000-0000-0000-000000000006', NULL, 'C0010000-0000-0000-0000-000000000006', '99910000-0000-0000-0000-000000000006', 5, 240, 0, 7.0000, '2026-05-19 18:00:00', '2026-05-05 09:00:00', '2026-05-19 18:00:00', 'Rohit Verma', NULL, 'Sneha Rao', '2026-05-19 18:00:00', '2026-05-19 18:00:00'),
('88810000-0000-0000-0000-000000000008', 'B0060000-0000-0000-0000-000000000006', 1, 'AUTH-1', 'Story', 'Implement SAML 2.0 Identity Provider Adapters', 'Build adapter middleware linking active directory setups to user authentication manager claims schemas for enterprise users.', 'Todo', 'High', NULL, '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', NULL, 'E0010000-0000-0000-0000-000000000007', NULL, 'C0010000-0000-0000-0000-000000000007', '99910000-0000-0000-0000-000000000007', 8, 480, 480, 8.0000, '2026-06-05 18:00:00', '2026-05-06 09:00:00', '2026-05-06 09:00:00', NULL, 'Karan Patel', 'Sneha Rao', '2026-06-04 18:00:00', '2026-06-05 18:00:00'),
('88810000-0000-0000-0000-000000000009', 'B0070000-0000-0000-0000-000000000007', 1, 'GATEWAY-1', 'Task', 'Add Circuit Breaker Policies via Polly in Gateway', 'Add Ocelot/YARP middleware extension configuring circuit breakers for ProjectApi and TaskApi endpoints. Set failure threshold at 30% error rates over 15 seconds.', 'Done', 'Medium', 'Fixed', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'ECD71539-1002-480B-9892-9767D0A7B6A6', NULL, 'E0010000-0000-0000-0000-000000000008', NULL, 'C0010000-0000-0000-0000-000000000008', '99910000-0000-0000-0000-000000000008', 5, 240, 0, 9.0000, '2026-05-24 18:00:00', '2026-05-07 09:00:00', '2026-05-24 18:00:00', NULL, 'Karan Patel', 'Sneha Rao', '2026-05-24 18:00:00', '2026-05-24 18:00:00'),
('88810000-0000-0000-0000-000000000010', 'B0080000-0000-0000-0000-000000000008', 1, 'BILLING-1', 'Story', 'Implement Stripe Webhook Handler for Subscriptions', 'Setup HTTP endpoint listening to Stripe webhooks. Write logic parsing invoice.payment_succeeded and customer.subscription.deleted events to update customer account status.', 'InProgress', 'High', NULL, '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '55510000-0000-0000-0000-000000000012', 'E0010000-0000-0000-0000-000000000009', NULL, 'C0010000-0000-0000-0000-000000000009', '99910000-0000-0000-0000-000000000009', 8, 360, 180, 10.0000, '2026-06-02 18:00:00', '2026-05-08 09:00:00', '2026-05-24 09:00:00', NULL, 'Karan Patel', 'Sneha Rao', '2026-06-01 18:00:00', '2026-06-02 18:00:00'),
('88810000-0000-0000-0000-000000000011', 'B0100000-0000-0000-0000-000000000010', 1, 'DEVOPS-1', 'Task', 'Define Helm Chart Templates for K8s Deployments', 'Write standard Helm templates mapping gateway, user, task and auth microservices config maps, service endpoints, ingress paths, and secrets.', 'Done', 'High', 'Fixed', 'A6520B4A-711F-4DC0-8BCE-2B1FFCE24C41', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', NULL, 'E0010000-0000-0000-0000-000000000010', NULL, 'C0010000-0000-0000-0000-000000000010', '99910000-0000-0000-0000-000000000010', 5, 240, 0, 11.0000, '2026-05-24 18:00:00', '2026-05-10 09:00:00', '2026-05-24 18:00:00', NULL, NULL, 'Sneha Rao', '2026-05-24 18:00:00', '2026-05-24 18:00:00'),
('88810000-0000-0000-0000-000000000012', 'B0120000-0000-0000-0000-000000000012', 1, 'MARKETING-1', 'Task', 'Optimize Marketing SEO Meta Tags Injection', 'Add server-side head parser injecting OpenGraph cards and canonical page tags into index.html response headers for search engine crawl optimizations.', 'InProgress', 'Low', NULL, 'C8583B92-F19F-491A-8518-917D16A1E112', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', NULL, 'E0010000-0000-0000-0000-000000000012', NULL, 'C0010000-0000-0000-0000-000000000012', '99910000-0000-0000-0000-000000000012', 2, 120, 60, 12.0000, '2026-05-30 18:00:00', '2026-05-12 09:00:00', '2026-05-24 09:00:00', 'Rohit Verma', NULL, 'Sneha Rao', '2026-05-29 18:00:00', '2026-05-30 18:00:00');
GO

-- Seed dbo.SubTasks (12 subtasks)
INSERT INTO dbo.SubTasks (SubTaskId, TaskId, Title, IsCompleted, CreatedAt, UpdatedAt) VALUES
(NEWID(), '88810000-0000-0000-0000-000000000001', 'Sketch mockup variants for light/dark theme', 1, '2026-05-15 09:30:00', '2026-05-18 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000001', 'Set up CSS typography and HSL variables', 1, '2026-05-15 10:00:00', '2026-05-19 12:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000001', 'Implement desktop sidebar routes', 0, '2026-05-15 10:30:00', '2026-05-15 10:30:00'),
(NEWID(), '88810000-0000-0000-0000-000000000002', 'Investigate TCP connection pooling configurations', 1, '2026-05-16 10:30:00', '2026-05-18 14:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000002', 'Increase system file descriptor limits in Dockerfile', 0, '2026-05-16 11:00:00', '2026-05-16 11:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000003', 'Verify UI rendering engine with blank dataset parameters', 0, '2026-05-24 08:30:00', '2026-05-24 08:30:00'),
(NEWID(), '88810000-0000-0000-0000-000000000004', 'Write Oracle SQL export queries for customer profiles', 1, '2026-05-18 10:00:00', '2026-05-20 16:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000004', 'Run trial migration on staging env (50k records)', 0, '2026-05-18 11:00:00', '2026-05-18 11:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000005', 'Compare PHP Bcrypt hash prefixes vs C# implementations', 1, '2026-05-19 10:30:00', '2026-05-20 11:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000005', 'Write regex string replace patch in database importer', 0, '2026-05-19 11:00:00', '2026-05-19 11:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000010', 'Configure Stripe webhook listening routes in gateway', 1, '2026-05-08 10:00:00', '2026-05-10 12:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000010', 'Create local integration test scripts with mock payloads', 0, '2026-05-08 11:00:00', '2026-05-08 11:00:00');
GO

-- Seed dbo.TaskLabels (12 labels)
INSERT INTO dbo.TaskLabels (TaskLabelId, TaskId, Name, Color, CreatedAt) VALUES
(NEWID(), '88810000-0000-0000-0000-000000000001', 'UI/UX', '#9333ea', '2026-05-15 09:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000001', 'Design', '#3b82f6', '2026-05-15 09:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000002', 'HighLoad', '#ef4444', '2026-05-16 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000002', 'Infrastructure', '#f97316', '2026-05-16 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000003', 'Client', '#eab308', '2026-05-24 08:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000004', 'Database', '#10b981', '2026-05-18 09:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000005', 'Security', '#06b6d4', '2026-05-19 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000006', 'Cron', '#64748b', '2026-05-04 09:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000007', 'Mobile', '#ec4899', '2026-05-05 09:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000008', 'SSO', '#14b8a6', '2026-05-06 09:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000009', 'Gateway', '#6366f1', '2026-05-07 09:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000010', 'Stripe', '#f43f5e', '2026-05-08 09:00:00');
GO

-- Seed dbo.TaskWatchers (12 watchers)
INSERT INTO dbo.TaskWatchers (TaskWatcherId, TaskId, UserId, CreatedAt) VALUES
(NEWID(), '88810000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-15 09:10:00'),
(NEWID(), '88810000-0000-0000-0000-000000000001', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-15 09:20:00'),
(NEWID(), '88810000-0000-0000-0000-000000000002', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-16 10:10:00'),
(NEWID(), '88810000-0000-0000-0000-000000000002', '7A2218D0-449C-4E60-B149-166D44FE24B3', '2026-05-16 10:30:00'),
(NEWID(), '88810000-0000-0000-0000-000000000003', '1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', '2026-05-24 08:05:00'),
(NEWID(), '88810000-0000-0000-0000-000000000003', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-24 08:10:00'),
(NEWID(), '88810000-0000-0000-0000-000000000004', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-18 09:10:00'),
(NEWID(), '88810000-0000-0000-0000-000000000005', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-19 10:10:00'),
(NEWID(), '88810000-0000-0000-0000-000000000006', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-04 09:10:00'),
(NEWID(), '88810000-0000-0000-0000-000000000007', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-05 09:10:00'),
(NEWID(), '88810000-0000-0000-0000-000000000008', '7A2218D0-449C-4E60-B149-166D44FE24B3', '2026-05-06 09:10:00'),
(NEWID(), '88810000-0000-0000-0000-000000000010', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-08 09:10:00');
GO

-- Seed dbo.TaskLinks (12 links, types: Blocks, IsBlockedBy, RelatesTo)
INSERT INTO dbo.TaskLinks (TaskLinkId, SourceTaskId, TargetTaskId, LinkType, CreatedById, CreatedAt) VALUES
(NEWID(), '88810000-0000-0000-0000-000000000002', '88810000-0000-0000-0000-000000000001', 'Blocks', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-16 12:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000001', '88810000-0000-0000-0000-000000000002', 'IsBlockedBy', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-16 12:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000004', '88810000-0000-0000-0000-000000000005', 'RelatesTo', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-19 12:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000003', '88810000-0000-0000-0000-000000000005', 'RelatesTo', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-24 08:30:00'),
(NEWID(), '88810000-0000-0000-0000-000000000001', '88810000-0000-0000-0000-000000000007', 'RelatesTo', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-15 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000002', '88810000-0000-0000-0000-000000000009', 'RelatesTo', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-16 11:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000006', '88810000-0000-0000-0000-000000000007', 'Blocks', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-05 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000007', '88810000-0000-0000-0000-000000000006', 'IsBlockedBy', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-05 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000008', '88810000-0000-0000-0000-000000000005', 'RelatesTo', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-06 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000010', '88810000-0000-0000-0000-000000000008', 'RelatesTo', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-08 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000009', '88810000-0000-0000-0000-000000000001', 'Blocks', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-07 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000001', '88810000-0000-0000-0000-000000000009', 'IsBlockedBy', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-07 10:00:00');
GO

-- Seed dbo.WorkLogs (12 logs)
INSERT INTO dbo.WorkLogs (WorkLogId, TaskId, UserId, TimeSpentMinutes, Comment, StartedAt, CreatedAt) VALUES
(NEWID(), '88810000-0000-0000-0000-000000000001', 'C8583B92-F19F-491A-8518-917D16A1E112', 120, 'Sketched layouts and configured preliminary Tailwind tokens.', '2026-05-18 09:00:00', '2026-05-18 11:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000001', 'C8583B92-F19F-491A-8518-917D16A1E112', 120, 'Implemented CSS Glassmorphism effects and validated on screens.', '2026-05-19 10:00:00', '2026-05-19 12:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000002', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 240, 'Analyzed packet transfers via Wireshark and located descriptor limits.', '2026-05-18 14:00:00', '2026-05-18 18:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000004', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 480, 'Wrote Oracle bulk export scripts and verified key indexes.', '2026-05-20 09:00:00', '2026-05-20 17:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000005', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 210, 'Patched Bcrypt string conversions in auth module.', '2026-05-20 10:00:00', '2026-05-20 13:30:00'),
(NEWID(), '88810000-0000-0000-0000-000000000006', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 360, 'Completed aggregates in Quartz job and pushed to staging.', '2026-05-18 09:00:00', '2026-05-18 15:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000007', 'C8583B92-F19F-491A-8518-917D16A1E112', 240, 'Wrote Flutter push services and configured APNS profile keys.', '2026-05-19 09:00:00', '2026-05-19 13:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000009', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 240, 'Completed circuit breaker configuration rules.', '2026-05-24 14:00:00', '2026-05-24 18:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000010', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 180, 'Configured endpoints mapping billing updates.', '2026-05-24 09:00:00', '2026-05-24 12:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000011', 'A6520B4A-711F-4DC0-8BCE-2B1FFCE24C41', 240, 'Completed Helm configuration templates.', '2026-05-24 14:00:00', '2026-05-24 18:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000012', 'C8583B92-F19F-491A-8518-917D16A1E112', 60, 'Added OpenGraph canonical tag fields.', '2026-05-24 09:00:00', '2026-05-24 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 30, 'Reviewed UI screens and checked dark theme ratios.', '2026-05-24 10:00:00', '2026-05-24 10:30:00');
GO

-- Seed dbo.SavedFilters (12 filters)
INSERT INTO dbo.SavedFilters (SavedFilterId, UserId, ProjectId, Name, JqlQuery, CreatedAt) VALUES
(NEWID(), 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'B0010000-0000-0000-0000-000000000001', 'My Open Tasks', 'project = WORKFLOW.IO AND assignee = currentUser() AND status != Done', '2026-05-15 09:00:00'),
(NEWID(), 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'B0010000-0000-0000-0000-000000000001', 'WORKFLOW.IO Critical Bugs', 'project = WORKFLOW.IO AND type = Bug AND priority = Critical', '2026-05-15 09:10:00'),
(NEWID(), 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'B0030000-0000-0000-0000-000000000003', 'Indus Blocked Issues', 'project = INDUS AND status = Blocked', '2026-05-18 10:00:00'),
(NEWID(), 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'B0040000-0000-0000-0000-000000000004', 'Analytics Sprint Backlog', 'project = ANALYTICS AND sprint = currentSprint()', '2026-05-19 11:00:00'),
(NEWID(), 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'B0020000-0000-0000-0000-000000000002', 'High Escalations Filter', 'project = CLIENT AND priority in (High, Critical)', '2026-05-24 08:15:00'),
(NEWID(), 'C8583B92-F19F-491A-8518-917D16A1E112', 'B0050000-0000-0000-0000-000000000005', 'Mobile UI Tickets', 'project = MOBILE AND component = "Flutter Navigation"', '2026-05-20 12:00:00'),
(NEWID(), '7A2218D0-449C-4E60-B149-166D44FE24B3', 'B0060000-0000-0000-0000-000000000006', 'Security Audit Backlog', 'project = AUTH AND type = Story', '2026-05-21 13:00:00'),
(NEWID(), '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'B0070000-0000-0000-0000-000000000007', 'Gateway Bugs', 'project = GATEWAY AND type = Bug', '2026-05-22 14:00:00'),
(NEWID(), 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'B0080000-0000-0000-0000-000000000008', 'Billing Launch Version', 'project = BILLING AND fixVersion = "99910000-0000-0000-0000-000000000009"', '2026-05-23 15:00:00'),
(NEWID(), 'A6520B4A-711F-4DC0-8BCE-2B1FFCE24C41', 'B0100000-0000-0000-0000-000000000010', 'Completed DevOps Tasks', 'project = DEVOPS AND status = Done', '2026-05-24 16:00:00'),
(NEWID(), '1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', 'B0110000-0000-0000-0000-000000000011', 'Helpdesk Customer Tickets', 'project = SUPPORT AND status != Done', '2026-05-24 17:00:00'),
(NEWID(), 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'B0120000-0000-0000-0000-000000000012', 'Growth Backlog Filter', 'project = MARKETING AND priority = Medium', '2026-05-24 18:00:00');
GO

-- Seed dbo.CalendarDays (12 working days)
INSERT INTO dbo.CalendarDays ([Date], IsWorkingDay) VALUES
('2026-05-01 00:00:00', 1),
('2026-05-02 00:00:00', 0),
('2026-05-03 00:00:00', 0),
('2026-05-04 00:00:00', 1),
('2026-05-05 00:00:00', 1),
('2026-05-06 00:00:00', 1),
('2026-05-07 00:00:00', 1),
('2026-05-08 00:00:00', 1),
('2026-05-09 00:00:00', 0),
('2026-05-10 00:00:00', 0),
('2026-05-11 00:00:00', 1),
('2026-05-12 00:00:00', 1);
GO

-- Seed dbo.DailyUpdateStates (12 states)
INSERT INTO dbo.DailyUpdateStates (ProjectId, LastSentAt, IsTriggeredToday, ExtraRecipients) VALUES
('B0010000-0000-0000-0000-000000000001', '2026-05-24 08:00:00', 1, 'admin@workflow.io.com;manager@workflow.io.com'),
('B0020000-0000-0000-0000-000000000002', '2026-05-24 08:30:00', 1, 'escalation.desk@workflow.io.com'),
('B0030000-0000-0000-0000-000000000003', '2026-05-24 09:00:00', 1, 'indus.liaison@indusbank.com'),
('B0040000-0000-0000-0000-000000000004', '2026-05-23 18:00:00', 0, NULL),
('B0050000-0000-0000-0000-000000000005', '2026-05-23 18:00:00', 0, 'mobile.leads@workflow.io.com'),
('B0060000-0000-0000-0000-000000000006', '2026-05-23 18:00:00', 0, 'sec-admin@workflow.io.com'),
('B0070000-0000-0000-0000-000000000007', '2026-05-23 18:00:00', 0, NULL),
('B0080000-0000-0000-0000-000000000008', '2026-05-23 18:00:00', 0, 'billing-audit@workflow.io.com'),
('B0090000-0000-0000-0000-000000000009', '2026-05-24 08:00:00', 1, NULL),
('B0100000-0000-0000-0000-000000000010', '2026-05-24 08:00:00', 1, 'ops-reports@workflow.io.com'),
('B0110000-0000-0000-0000-000000000011', '2026-05-24 08:00:00', 1, 'support-manager@workflow.io.com'),
('B0120000-0000-0000-0000-000000000012', '2026-05-24 08:00:00', 1, 'marketing-team@workflow.io.com');
GO

-- Seed dbo.ProjectIssueCounters (12 counters)
INSERT INTO dbo.ProjectIssueCounters (ProjectId, ProjectKey, LastIssueNumber) VALUES
('B0010000-0000-0000-0000-000000000001', 'WORKFLOW.IO', 2),
('B0020000-0000-0000-0000-000000000002', 'CLIENT', 1),
('B0030000-0000-0000-0000-000000000003', 'INDUS', 2),
('B0040000-0000-0000-0000-000000000004', 'ANALYTICS', 1),
('B0050000-0000-0000-0000-000000000005', 'MOBILE', 1),
('B0060000-0000-0000-0000-000000000006', 'AUTH', 1),
('B0070000-0000-0000-0000-000000000007', 'GATEWAY', 1),
('B0080000-0000-0000-0000-000000000008', 'BILLING', 1),
('B0090000-0000-0000-0000-000000000009', 'NOTIFY', 0),
('B0100000-0000-0000-0000-000000000010', 'DEVOPS', 1),
('B0110000-0000-0000-0000-000000000011', 'SUPPORT', 0),
('B0120000-0000-0000-0000-000000000012', 'MARKETING', 1);
GO

-- Seed dbo.OutboxMessages (12 messages)
INSERT INTO dbo.OutboxMessages (OutboxMessageId, EventId, EventType, PayloadJson, CreatedAt, PublishedAt, RetryCount, LastError) VALUES
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000001","Title":"Design Modern SaaS Layout"}', '2026-05-15 09:00:00', '2026-05-15 09:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000002","Title":"Gateway API Routing Failure"}', '2026-05-16 10:00:00', '2026-05-16 10:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000003","Title":"Indus Bank escalation: blank charts"}', '2026-05-24 08:00:00', '2026-05-24 08:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000004","Title":"Migration of Customer Profile Records"}', '2026-05-18 09:00:00', '2026-05-18 09:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000005","Title":"Bcrypt password migration compatibility issue"}', '2026-05-19 10:00:00', '2026-05-19 10:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000006","Title":"Implement pre-aggregation cron scheduler"}', '2026-05-04 09:00:00', '2026-05-04 09:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000007","Title":"Integrate FCM Push Notifications SDK"}', '2026-05-05 09:00:00', '2026-05-05 09:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000008","Title":"Implement SAML 2.0 Identity Provider Adapters"}', '2026-05-06 09:00:00', '2026-05-06 09:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000009","Title":"Add Circuit Breaker Policies via Polly"}', '2026-05-07 09:00:00', '2026-05-07 09:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000010","Title":"Implement Stripe Webhook Handler for Subscriptions"}', '2026-05-08 09:00:00', '2026-05-08 09:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000011","Title":"Define Helm Chart Templates for K8s"}', '2026-05-10 09:00:00', '2026-05-10 09:00:05', 0, NULL),
(NEWID(), NEWID(), 'TaskCreatedIntegrationEvent', '{"TaskId":"88810000-0000-0000-0000-000000000012","Title":"Optimize Marketing SEO Meta Tags Injection"}', '2026-05-12 09:00:00', '2026-05-12 09:00:05', 0, NULL);
GO


-- ==========================================
-- 4. Workflow.IOCommentDb
-- ==========================================
USE Workflow.IOCommentDb;
GO

-- Cleanup existing data
DELETE FROM dbo.CommentMentions;
DELETE FROM dbo.Comments;
DELETE FROM dbo.OutboxMessages;
GO

-- Seed dbo.Comments (12 comments)
INSERT INTO dbo.Comments (CommentId, TaskId, AuthorId, ParentCommentId, Body, IsDeleted, CreatedAt, UpdatedAt) VALUES
('77710000-0000-0000-0000-000000000001', '88810000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', NULL, 'Rohit, please verify the glassmorphism colors in dark mode. The border transparency seems too high on low-contrast screens.', 0, '2026-05-15 10:00:00', '2026-05-15 10:00:00'),
('77710000-0000-0000-0000-000000000002', '88810000-0000-0000-0000-000000000001', 'C8583B92-F19F-491A-8518-917D16A1E112', '77710000-0000-0000-0000-000000000001', 'Sure Himanshu, I will adjust the opacity value of the border border-white/10 to border-white/20 and run a visual diff.', 0, '2026-05-15 10:15:00', '2026-05-15 10:15:00'),
('77710000-0000-0000-0000-000000000003', '88810000-0000-0000-0000-000000000002', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', NULL, 'I tracked down the routing socket exhaustion. The issue is due to dotnet gateway keep-alive connections staying active indefinitely, exhausting local TCP ports under high stress loads. We need to set a ConnectionLifetime parameter to 10 seconds in Yarp cluster config. Let me test this change.', 0, '2026-05-16 11:30:00', '2026-05-16 11:30:00'),
('77710000-0000-0000-0000-000000000004', '88810000-0000-0000-0000-000000000002', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '77710000-0000-0000-0000-000000000003', 'Approved. Let me know when you push the deployment helm values so DevOps can apply the update.', 0, '2026-05-16 12:00:00', '2026-05-16 12:00:00'),
('77710000-0000-0000-0000-000000000005', '88810000-0000-0000-0000-000000000003', '1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', NULL, 'Critical! The charts are blank in the bank dashboard! Our management needs this by EOD.', 0, '2026-05-24 08:05:00', '2026-05-24 08:05:00'),
('77710000-0000-0000-0000-000000000006', '88810000-0000-0000-0000-000000000003', 'C8583B92-F19F-491A-8518-917D16A1E112', '77710000-0000-0000-0000-000000000005', 'On it John! Fixing a minor type error in parsing dates. Code will be live within the hour.', 0, '2026-05-24 08:15:00', '2026-05-24 08:15:00'),
('77710000-0000-0000-0000-000000000007', '88810000-0000-0000-0000-000000000004', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', NULL, 'Export script completed. Tested successfully on Oracle VM.', 0, '2026-05-20 16:30:00', '2026-05-20 16:30:00'),
('77710000-0000-0000-0000-000000000008', '88810000-0000-0000-0000-000000000005', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', NULL, 'Wrote SQL script regex replacement patch mapping hash prefixes.', 0, '2026-05-20 11:30:00', '2026-05-20 11:30:00'),
('77710000-0000-0000-0000-000000000009', '88810000-0000-0000-0000-000000000006', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', NULL, 'Quartz scheduler successfully configured on microservice launch.', 0, '2026-05-18 15:30:00', '2026-05-18 15:30:00'),
('77710000-0000-0000-0000-000000000010', '88810000-0000-0000-0000-000000000007', 'C8583B92-F19F-491A-8518-917D16A1E112', NULL, 'Flutter iOS APNS certificates uploaded successfully.', 0, '2026-05-19 13:30:00', '2026-05-19 13:30:00'),
('77710000-0000-0000-0000-000000000011', '88810000-0000-0000-0000-000000000009', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', NULL, 'Tested resilience with 10% injected failure rate, gateway stayed healthy.', 0, '2026-05-24 18:15:00', '2026-05-24 18:15:00'),
('77710000-0000-0000-0000-000000000012', '88810000-0000-0000-0000-000000000010', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', NULL, 'Webhook parser handles invoice payment status correctly.', 0, '2026-05-24 12:15:00', '2026-05-24 12:15:00');
GO

-- Seed dbo.CommentMentions (12 mentions)
INSERT INTO dbo.CommentMentions (CommentMentionId, CommentId, MentionedUserId, CreatedAt) VALUES
(NEWID(), '77710000-0000-0000-0000-000000000001', 'C8583B92-F19F-491A-8518-917D16A1E112', '2026-05-15 10:00:00'),
(NEWID(), '77710000-0000-0000-0000-000000000002', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-15 10:15:00'),
(NEWID(), '77710000-0000-0000-0000-000000000003', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-16 11:30:00'),
(NEWID(), '77710000-0000-0000-0000-000000000004', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', '2026-05-16 12:00:00'),
(NEWID(), '77710000-0000-0000-0000-000000000005', 'C8583B92-F19F-491A-8518-917D16A1E112', '2026-05-24 08:05:00'),
(NEWID(), '77710000-0000-0000-0000-000000000006', '1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', '2026-05-24 08:15:00'),
(NEWID(), '77710000-0000-0000-0000-000000000007', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-20 16:30:00'),
(NEWID(), '77710000-0000-0000-0000-000000000008', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-20 11:30:00'),
(NEWID(), '77710000-0000-0000-0000-000000000009', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-18 15:30:00'),
(NEWID(), '77710000-0000-0000-0000-000000000010', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-19 13:30:00'),
(NEWID(), '77710000-0000-0000-0000-000000000011', 'ECD71539-1002-480B-9892-9767D0A7B6A6', '2026-05-24 18:15:00'),
(NEWID(), '77710000-0000-0000-0000-000000000012', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', '2026-05-24 12:15:00');
GO

-- Seed dbo.OutboxMessages (12 messages)
INSERT INTO dbo.OutboxMessages (OutboxMessageId, EventId, EventType, PayloadJson, CreatedAt, PublishedAt, RetryCount, LastError) VALUES
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000001"}', '2026-05-15 10:00:00', '2026-05-15 10:00:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000002"}', '2026-05-15 10:15:00', '2026-05-15 10:15:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000003"}', '2026-05-16 11:30:00', '2026-05-16 11:30:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000004"}', '2026-05-16 12:00:00', '2026-05-16 12:00:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000005"}', '2026-05-24 08:05:00', '2026-05-24 08:05:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000006"}', '2026-05-24 08:15:00', '2026-05-24 08:15:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000007"}', '2026-05-20 16:30:00', '2026-05-20 16:30:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000008"}', '2026-05-20 11:30:00', '2026-05-20 11:30:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000009"}', '2026-05-18 15:30:00', '2026-05-18 15:30:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000010"}', '2026-05-19 13:30:00', '2026-05-19 13:30:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000011"}', '2026-05-24 18:15:00', '2026-05-24 18:15:02', 0, NULL),
(NEWID(), NEWID(), 'CommentCreatedEvent', '{"CommentId":"77710000-0000-0000-0000-000000000012"}', '2026-05-24 12:15:00', '2026-05-24 12:15:02', 0, NULL);
GO


-- ==========================================
-- 5. Workflow.IONotificationDb
-- ==========================================
USE Workflow.IONotificationDb;
GO

-- Cleanup existing data
DELETE FROM dbo.Notifications;
DELETE FROM dbo.ProcessedEvents;
GO

-- Seed dbo.Notifications (12 notifications)
INSERT INTO dbo.Notifications (NotificationId, RecipientId, EventType, EntityType, EntityId, Message, IsRead, CreatedAt) VALUES
(NEWID(), 'C8583B92-F19F-491A-8518-917D16A1E112', 'TaskAssigned', 'Task', '88810000-0000-0000-0000-000000000001', 'Himanshu Tripathi assigned you the task WORKFLOW.IO-1: Design Modern Light and Dark SaaS Layout.', 0, '2026-05-15 09:00:00'),
(NEWID(), '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'TaskAssigned', 'Task', '88810000-0000-0000-0000-000000000002', 'Himanshu Tripathi assigned you the task WORKFLOW.IO-2: Gateway API Routing Failure under High Load.', 0, '2026-05-16 10:00:00'),
(NEWID(), 'C8583B92-F19F-491A-8518-917D16A1E112', 'TaskAssigned', 'Task', '88810000-0000-0000-0000-000000000003', 'John Doe assigned you the task CLIENT-1: Indus Bank escalation: dashboard charts blank.', 0, '2026-05-24 08:00:00'),
(NEWID(), '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'TaskAssigned', 'Task', '88810000-0000-0000-0000-000000000004', 'Neha Sharma assigned you the task INDUS-1: Migration of Customer Profile Records.', 1, '2026-05-18 09:00:00'),
(NEWID(), 'C8583B92-F19F-491A-8518-917D16A1E112', 'CommentAdded', 'Task', '88810000-0000-0000-0000-000000000001', 'Himanshu Tripathi mentioned you in a comment on WORKFLOW.IO-1: Design Modern Light and Dark SaaS Layout.', 0, '2026-05-15 10:00:00'),
(NEWID(), 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'CommentAdded', 'Task', '88810000-0000-0000-0000-000000000001', 'Rohit Verma mentioned you in a comment on WORKFLOW.IO-1: Design Modern Light and Dark SaaS Layout.', 1, '2026-05-15 10:15:00'),
(NEWID(), 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'CommentAdded', 'Task', '88810000-0000-0000-0000-000000000002', 'Karan Patel mentioned you in a comment on WORKFLOW.IO-2: Gateway API Routing Failure under High Load.', 0, '2026-05-16 11:30:00'),
(NEWID(), '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'CommentAdded', 'Task', '88810000-0000-0000-0000-000000000002', 'Himanshu Tripathi mentioned you in a comment on WORKFLOW.IO-2: Gateway API Routing Failure under High Load.', 1, '2026-05-16 12:00:00'),
(NEWID(), 'C8583B92-F19F-491A-8518-917D16A1E112', 'CommentAdded', 'Task', '88810000-0000-0000-0000-000000000003', 'John Doe mentioned you in a comment on CLIENT-1: Indus Bank escalation: dashboard charts blank.', 0, '2026-05-24 08:05:00'),
(NEWID(), '1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', 'CommentAdded', 'Task', '88810000-0000-0000-0000-000000000003', 'Rohit Verma mentioned you in a comment on CLIENT-1: Indus Bank escalation: dashboard charts blank.', 0, '2026-05-24 08:15:00'),
(NEWID(), 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'CommentAdded', 'Task', '88810000-0000-0000-0000-000000000004', 'Karan Patel mentioned you in a comment on INDUS-1: Migration of Customer Profile Records.', 1, '2026-05-20 16:30:00'),
(NEWID(), 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'CommentAdded', 'Task', '88810000-0000-0000-0000-000000000005', 'Karan Patel mentioned you in a comment on INDUS-2: Bcrypt password migration compatibility issue.', 1, '2026-05-20 11:30:00');
GO

-- Seed dbo.ProcessedEvents (12 records)
INSERT INTO dbo.ProcessedEvents (EventId, EventType, ProcessedAt) VALUES
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-15 09:01:00'),
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-16 10:01:00'),
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-24 08:01:00'),
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-18 09:01:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-15 10:01:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-15 10:16:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-16 11:31:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-16 12:01:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-24 08:06:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-24 08:16:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-20 16:31:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-20 11:31:00');
GO


-- ==========================================
-- 6. Workflow.IOActivityDb
-- ==========================================
USE Workflow.IOActivityDb;
GO

-- Cleanup existing data
DELETE FROM dbo.Activities;
DELETE FROM dbo.ProcessedEvents;
GO

-- Seed dbo.Activities (12 activity records mapping historical logs)
INSERT INTO dbo.Activities (ActivityRecordId, EventType, EntityType, EntityId, ActorId, Description, PayloadJson, CreatedAt) VALUES
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'Himanshu Tripathi created task WORKFLOW.IO-1: Design Modern Light and Dark SaaS Layout.', '{"title":"Design Modern Light and Dark SaaS Layout","projectId":"B001"}', '2026-05-15 09:00:00'),
(NEWID(), 'TaskAssigned', 'Task', '88810000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'Himanshu Tripathi assigned task WORKFLOW.IO-1 to Rohit Verma.', '{"assigneeId":"C8583B92-F19F-491A-8518-917D16A1E112"}', '2026-05-15 09:10:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000002', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'Himanshu Tripathi created task WORKFLOW.IO-2: Gateway API Routing Failure under High Load.', '{"title":"Gateway API Routing Failure under High Load","projectId":"B001"}', '2026-05-16 10:00:00'),
(NEWID(), 'TaskStatusChanged', 'Task', '88810000-0000-0000-0000-000000000002', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'Karan Patel changed status of WORKFLOW.IO-2 to Blocked.', '{"oldStatus":"Todo","newStatus":"Blocked"}', '2026-05-16 10:30:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000003', '1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', 'John Doe created escalation CLIENT-1: Indus Bank escalation: dashboard charts blank.', '{"title":"Indus Bank escalation: dashboard charts blank","projectId":"B002"}', '2026-05-24 08:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000004', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'Neha Sharma created story INDUS-1: Migration of Customer Profile Records.', '{"title":"Migration of Customer Profile Records","projectId":"B003"}', '2026-05-18 09:00:00'),
(NEWID(), 'CommentAdded', 'Task', '88810000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'Himanshu Tripathi commented on WORKFLOW.IO-1.', '{"commentId":"7771"}', '2026-05-15 10:00:00'),
(NEWID(), 'CommentAdded', 'Task', '88810000-0000-0000-0000-000000000001', 'C8583B92-F19F-491A-8518-917D16A1E112', 'Rohit Verma commented on WORKFLOW.IO-1.', '{"commentId":"7772"}', '2026-05-15 10:15:00'),
(NEWID(), 'WorkLogAdded', 'Task', '88810000-0000-0000-0000-000000000001', 'C8583B92-F19F-491A-8518-917D16A1E112', 'Rohit Verma logged 120 minutes of work on WORKFLOW.IO-1.', '{"minutes":120}', '2026-05-18 11:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000006', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', 'Neha Sharma created task ANALYTICS-1: Implement pre-aggregation cron scheduler.', '{"title":"Implement pre-aggregation cron scheduler","projectId":"B004"}', '2026-05-04 09:00:00'),
(NEWID(), 'TaskStatusChanged', 'Task', '88810000-0000-0000-0000-000000000006', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'Karan Patel completed ANALYTICS-1.', '{"oldStatus":"InProgress","newStatus":"Done"}', '2026-05-18 18:00:00'),
(NEWID(), 'ProjectCreated', 'Project', 'B0010000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'Himanshu Tripathi created project Workflow.IO SaaS Core Platform.', '{"key":"WORKFLOW.IO"}', '2026-05-01 09:00:00');
GO

-- Seed dbo.ProcessedEvents (12 records)
INSERT INTO dbo.ProcessedEvents (EventId, EventType, ProcessedAt) VALUES
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-15 09:01:00'),
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-16 10:01:00'),
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-24 08:01:00'),
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-18 09:01:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-15 10:01:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-15 10:16:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-16 11:31:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-16 12:01:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-24 08:06:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-24 08:16:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-20 16:31:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-20 11:31:00');
GO


-- ==========================================
-- 7. Workflow.IOFileDb
-- ==========================================
USE Workflow.IOFileDb;
GO

-- Cleanup existing data
DELETE FROM dbo.FileAttachments;
DELETE FROM dbo.OutboxMessages;
GO

-- Seed dbo.FileAttachments (12 files)
INSERT INTO dbo.FileAttachments (FileAttachmentId, TaskId, UploadedById, OriginalFileName, StoredFileName, ContentType, SizeInBytes, StoragePath, IsDeleted, CreatedAt, UpdatedAt) VALUES
(NEWID(), '88810000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', 'dashboard_wireframe.png', 'dashboard_wireframe_123456.png', 'image/png', 245600, '/app/uploads/dashboard_wireframe_123456.png', 0, '2026-05-15 09:30:00', '2026-05-15 09:30:00'),
(NEWID(), '88810000-0000-0000-0000-000000000001', 'C8583B92-F19F-491A-8518-917D16A1E112', 'landing_page_design.sketch', 'landing_page_design_123456.sketch', 'application/octet-stream', 8901200, '/app/uploads/landing_page_design_123456.sketch', 0, '2026-05-15 10:00:00', '2026-05-15 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000002', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'gateway_stress_log.txt', 'gateway_stress_log_123456.txt', 'text/plain', 56700, '/app/uploads/gateway_stress_log_123456.txt', 0, '2026-05-16 10:30:00', '2026-05-16 10:30:00'),
(NEWID(), '88810000-0000-0000-0000-000000000003', '1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', 'blank_charts_screenshot.jpg', 'blank_charts_screenshot_123456.jpg', 'image/jpeg', 189000, '/app/uploads/blank_charts_screenshot_123456.jpg', 0, '2026-05-24 08:05:00', '2026-05-24 08:05:00'),
(NEWID(), '88810000-0000-0000-0000-000000000004', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'oracle_bulk_export.sql', 'oracle_bulk_export_123456.sql', 'text/plain', 12300, '/app/uploads/oracle_bulk_export_123456.sql', 0, '2026-05-20 09:30:00', '2026-05-20 09:30:00'),
(NEWID(), '88810000-0000-0000-0000-000000000005', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'bcrypt_remapper.cs', 'bcrypt_remapper_123456.cs', 'text/plain', 4500, '/app/uploads/bcrypt_remapper_123456.cs', 0, '2026-05-20 11:00:00', '2026-05-20 11:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000006', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'quartz_job_spec.xml', 'quartz_job_spec_123456.xml', 'text/xml', 3200, '/app/uploads/quartz_job_spec_123456.xml', 0, '2026-05-18 10:00:00', '2026-05-18 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000007', 'C8583B92-F19F-491A-8518-917D16A1E112', 'fcm_config_guide.pdf', 'fcm_config_guide_123456.pdf', 'application/pdf', 2450000, '/app/uploads/fcm_config_guide_123456.pdf', 0, '2026-05-19 12:00:00', '2026-05-19 12:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000008', '7A2218D0-449C-4E60-B149-166D44FE24B3', 'saml_metadata.xml', 'saml_metadata_123456.xml', 'text/xml', 15400, '/app/uploads/saml_metadata_123456.xml', 0, '2026-05-06 10:00:00', '2026-05-06 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000009', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'polly_resilience_rules.json', 'polly_resilience_rules_123456.json', 'application/json', 2300, '/app/uploads/polly_resilience_rules_123456.json', 0, '2026-05-07 10:00:00', '2026-05-07 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000010', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', 'stripe_invoice_template.html', 'stripe_invoice_template_123456.html', 'text/html', 14500, '/app/uploads/stripe_invoice_template_123456.html', 0, '2026-05-08 10:00:00', '2026-05-08 10:00:00'),
(NEWID(), '88810000-0000-0000-0000-000000000011', 'A6520B4A-711F-4DC0-8BCE-2B1FFCE24C41', 'aks_deployment_manifest.yaml', 'aks_deployment_manifest_123456.yaml', 'text/plain', 5400, '/app/uploads/aks_deployment_manifest_123456.yaml', 0, '2026-05-10 10:00:00', '2026-05-10 10:00:00');
GO

-- Seed dbo.OutboxMessages (12 messages)
INSERT INTO dbo.OutboxMessages (OutboxMessageId, EventId, EventType, PayloadJson, CreatedAt, PublishedAt, RetryCount, LastError) VALUES
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F001"}', '2026-05-15 09:30:00', '2026-05-15 09:30:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F002"}', '2026-05-15 10:00:00', '2026-05-15 10:00:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F003"}', '2026-05-16 10:30:00', '2026-05-16 10:30:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F004"}', '2026-05-24 08:05:00', '2026-05-24 08:05:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F005"}', '2026-05-20 09:30:00', '2026-05-20 09:30:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F006"}', '2026-05-20 11:00:00', '2026-05-20 11:00:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F007"}', '2026-05-18 10:00:00', '2026-05-18 10:00:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F008"}', '2026-05-19 12:00:00', '2026-05-19 12:00:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F009"}', '2026-05-06 10:00:00', '2026-05-06 10:00:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F010"}', '2026-05-07 10:00:00', '2026-05-07 10:00:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F011"}', '2026-05-08 10:00:00', '2026-05-08 10:00:02', 0, NULL),
(NEWID(), NEWID(), 'FileUploadedEvent', '{"FileAttachmentId":"F012"}', '2026-05-10 10:00:00', '2026-05-10 10:00:02', 0, NULL);
GO


-- ==========================================
-- 8. Workflow.IOAnalyticsDb
-- ==========================================
USE Workflow.IOAnalyticsDb;
GO

-- Cleanup existing data
DELETE FROM dbo.AnalyticsEvents;
DELETE FROM dbo.TaskAnalyticsItems;
DELETE FROM dbo.ProcessedEvents;
GO

-- Seed dbo.AnalyticsEvents (12 events)
INSERT INTO dbo.AnalyticsEvents (AnalyticsEventId, EventType, EntityType, EntityId, ProjectId, ActorId, RecipientId, Description, PayloadJson, OccurredAt) VALUES
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000001', 'B0010000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', NULL, 'Task WORKFLOW.IO-1 created.', NULL, '2026-05-15 09:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000002', 'B0010000-0000-0000-0000-000000000001', 'ECD71539-1002-480B-9892-9767D0A7B6A6', NULL, 'Task WORKFLOW.IO-2 created.', NULL, '2026-05-16 10:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000003', 'B0020000-0000-0000-0000-000000000002', '1F13606A-88D8-4DE3-A1C8-245CDE89F6B5', NULL, 'Task CLIENT-1 created.', NULL, '2026-05-24 08:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000004', 'B0030000-0000-0000-0000-000000000003', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', NULL, 'Task INDUS-1 created.', NULL, '2026-05-18 09:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000005', 'B0030000-0000-0000-0000-000000000003', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', NULL, 'Task INDUS-2 created.', NULL, '2026-05-19 10:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000006', 'B0040000-0000-0000-0000-000000000004', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', NULL, 'Task ANALYTICS-1 created.', NULL, '2026-05-04 09:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000007', 'B0050000-0000-0000-0000-000000000005', 'ECD71539-1002-480B-9892-9767D0A7B6A6', NULL, 'Task MOBILE-1 created.', NULL, '2026-05-05 09:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000008', 'B0060000-0000-0000-0000-000000000006', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', NULL, 'Task AUTH-1 created.', NULL, '2026-05-06 09:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000009', 'B0070000-0000-0000-0000-000000000007', 'ECD71539-1002-480B-9892-9767D0A7B6A6', NULL, 'Task GATEWAY-1 created.', NULL, '2026-05-07 09:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000010', 'B0080000-0000-0000-0000-000000000008', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', NULL, 'Task BILLING-1 created.', NULL, '2026-05-08 09:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000011', 'B0100000-0000-0000-0000-000000000010', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', NULL, 'Task DEVOPS-1 created.', NULL, '2026-05-10 09:00:00'),
(NEWID(), 'TaskCreated', 'Task', '88810000-0000-0000-0000-000000000012', 'B0120000-0000-0000-0000-000000000012', 'E4A4CD39-D3E4-42CE-BA7A-6D2908DF3B5E', NULL, 'Task MARKETING-1 created.', NULL, '2026-05-12 09:00:00');
GO

-- Seed dbo.TaskAnalyticsItems (12 snapshots)
INSERT INTO dbo.TaskAnalyticsItems (TaskId, ProjectId, Status, Priority, AssigneeId, SprintId, EpicId, StoryPoints, DueDate, IsDeleted, CreatedAt, UpdatedAt) VALUES
('88810000-0000-0000-0000-000000000001', 'B0010000-0000-0000-0000-000000000001', 'InProgress', 'High', 'C8583B92-F19F-491A-8518-917D16A1E112', '55510000-0000-0000-0000-000000000002', 'E0010000-0000-0000-0000-000000000001', 5, '2026-05-30 18:00:00', 0, '2026-05-15 09:00:00', '2026-05-24 09:00:00'),
('88810000-0000-0000-0000-000000000002', 'B0010000-0000-0000-0000-000000000001', 'Blocked', 'Critical', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', '55510000-0000-0000-0000-000000000002', 'E0010000-0000-0000-0000-000000000002', 8, '2026-05-28 18:00:00', 0, '2026-05-16 10:00:00', '2026-05-24 10:00:00'),
('88810000-0000-0000-0000-000000000003', 'B0020000-0000-0000-0000-000000000002', 'Todo', 'Critical', 'C8583B92-F19F-491A-8518-917D16A1E112', NULL, 'E0010000-0000-0000-0000-000000000003', 3, '2026-05-26 12:00:00', 0, '2026-05-24 08:00:00', '2026-05-24 08:00:00'),
('88810000-0000-0000-0000-000000000004', 'B0030000-0000-0000-0000-000000000003', 'InProgress', 'Medium', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', '55510000-0000-0000-0000-000000000005', 'E0010000-0000-0000-0000-000000000004', 13, '2026-05-30 18:00:00', 0, '2026-05-18 09:00:00', '2026-05-24 09:00:00'),
('88810000-0000-0000-0000-000000000005', 'B0030000-0000-0000-0000-000000000003', 'Review', 'High', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', '55510000-0000-0000-0000-000000000005', 'E0010000-0000-0000-0000-000000000004', 5, '2026-05-29 18:00:00', 0, '2026-05-19 10:00:00', '2026-05-24 10:00:00'),
('88810000-0000-0000-0000-000000000006', 'B0040000-0000-0000-0000-000000000004', 'Done', 'Medium', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', '55510000-0000-0000-0000-000000000006', 'E0010000-0000-0000-0000-000000000005', 8, '2026-05-18 18:00:00', 0, '2026-05-04 09:00:00', '2026-05-18 18:00:00'),
('88810000-0000-0000-0000-000000000007', 'B0050000-0000-0000-0000-000000000005', 'Done', 'High', 'C8583B92-F19F-491A-8518-917D16A1E112', '55510000-0000-0000-0000-000000000008', 'E0010000-0000-0000-0000-000000000006', 5, '2026-05-19 18:00:00', 0, '2026-05-05 09:00:00', '2026-05-19 18:00:00'),
('88810000-0000-0000-0000-000000000008', 'B0060000-0000-0000-0000-000000000006', 'Todo', 'High', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', NULL, 'E0010000-0000-0000-0000-000000000007', 8, '2026-06-05 18:00:00', 0, '2026-05-06 09:00:00', '2026-05-06 09:00:00'),
('88810000-0000-0000-0000-000000000009', 'B0070000-0000-0000-0000-000000000007', 'Done', 'Medium', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', NULL, 'E0010000-0000-0000-0000-000000000008', 5, '2026-05-24 18:00:00', 0, '2026-05-07 09:00:00', '2026-05-24 18:00:00'),
('88810000-0000-0000-0000-000000000010', 'B0080000-0000-0000-0000-000000000008', 'InProgress', 'High', '9E8DCE48-693D-4A2D-A3D2-7E4DE5B26E55', '55510000-0000-0000-0000-000000000012', 'E0010000-0000-0000-0000-000000000009', 8, '2026-06-02 18:00:00', 0, '2026-05-08 09:00:00', '2026-05-24 09:00:00'),
('88810000-0000-0000-0000-000000000011', 'B0100000-0000-0000-0000-000000000010', 'Done', 'High', 'A6520B4A-711F-4DC0-8BCE-2B1FFCE24C41', NULL, 'E0010000-0000-0000-0000-000000000010', 5, '2026-05-24 18:00:00', 0, '2026-05-10 09:00:00', '2026-05-24 18:00:00'),
('88810000-0000-0000-0000-000000000012', 'B0120000-0000-0000-0000-000000000012', 'InProgress', 'Low', 'C8583B92-F19F-491A-8518-917D16A1E112', NULL, 'E0010000-0000-0000-0000-000000000012', 2, '2026-05-30 18:00:00', 0, '2026-05-12 09:00:00', '2026-05-24 09:00:00');
GO

-- Seed dbo.ProcessedEvents (12 records)
INSERT INTO dbo.ProcessedEvents (EventId, EventType, ProcessedAt) VALUES
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-15 09:01:00'),
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-16 10:01:00'),
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-24 08:01:00'),
(NEWID(), 'TaskCreatedIntegrationEvent', '2026-05-18 09:01:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-15 10:01:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-15 10:16:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-16 11:31:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-16 12:01:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-24 08:06:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-24 08:16:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-20 16:31:00'),
(NEWID(), 'CommentCreatedEvent', '2026-05-20 11:31:00');
GO
