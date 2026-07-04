# Workflow.IO

Workflow.IO is a Jira-like project management platform built with .NET 8 Web API, Clean Architecture-style service boundaries, Entity Framework Core, SQL Server, JWT authentication, RabbitMQ events, and Docker.

## Current Service Map

| Service | Responsibility | Local port |
| --- | --- | --- |
| GatewayApi | Single API entry point using YARP | 5270 |
| UserApi | users, authentication, JWT, refresh tokens | 5240 |
| ProjectApi | projects, members, project roles | 5250 |
| TaskApi | issues (keys/types), boards, sprints, backlog, epics, components, versions, links, worklogs, bulk, filters | 5260 |
| CommentApi | task comments and mentions | 5280 |
| FileApi | task attachments | 5310 |
| NotificationApi | notification read model from events | 5290 |
| ActivityApi | activity/audit read model from events | 5300 |
| AnalyticsApi | reporting read models and project/sprint metrics | 5320 |
| RealtimeApi | SignalR live updates from RabbitMQ events | 5330 |
| SQL Server | service databases | 1433 |
| RabbitMQ | event bus and management UI | 5672 / 15672 |

## Run Locally With Docker

From the repository root:

```powershell
docker compose up --build
```

The Docker profile sets `Database__ApplyMigrationsOnStartup=true`, so each API applies its own EF Core migrations when it starts. This is useful for local development. In production, keep database migrations as a controlled release step instead of startup behavior.

Open:

- Gateway: http://localhost:5270/health
- RabbitMQ dashboard: http://localhost:15672
- RabbitMQ credentials: `guest` / `guest`

## Health Checks

Each API now exposes:

- `/health` - ASP.NET health check payload
- `/ready` - simple readiness response

Useful local checks:

```powershell
Invoke-RestMethod http://localhost:5270/health
Invoke-RestMethod http://localhost:5240/health
Invoke-RestMethod http://localhost:5250/health
Invoke-RestMethod http://localhost:5260/health
Invoke-RestMethod http://localhost:5280/health
Invoke-RestMethod http://localhost:5290/health
Invoke-RestMethod http://localhost:5300/health
Invoke-RestMethod http://localhost:5310/health
Invoke-RestMethod http://localhost:5320/health
Invoke-RestMethod http://localhost:5330/health
```

## Jira-Parity API Surface (via Gateway)

| Area | Gateway path | Notes |
| --- | --- | --- |
| Issue keys | `GET /issues/{issueKey}` | e.g. `PROJ-12` |
| Components / versions | `/projects/{id}/components`, `/projects/{id}/versions` | CRUD + release |
| Links / worklogs | `/tasks/{id}/links`, `/tasks/{id}/worklogs` | Issue linking and time tracking |
| Bulk update | `POST /projects/{id}/tasks/bulk` | Multi-issue updates |
| Saved filters | `/projects/{id}/filters`, `GET /filters/{id}/results` | Simple JQL (`status=Done`, `assignee=me`, â€¦) |
| Sprint admin | `PUT/DELETE /sprints/{id}` | Update or remove sprint |
| Notifications | `GET /notifications/me`, `PATCH .../read` | Inbox + read state |
| Analytics | `GET /analytics/projects/{id}/sprints/{id}/burndown` | Sprint burndown chart data |
| Users | `GET /users/lookup?email=`, `POST /users/me/change-password` | Mention picker + password change |
| Activity | `GET /activities/projects/{id}` | Project-scoped feed |
| Realtime | `/hubs/workflow.io` | SignalR live updates |

## Local Smoke Flow

Use [docs/local-smoke.http](./docs/local-smoke.http) with Visual Studio, Rider, or the VS Code REST Client extension.

The flow validates the real Workflow.IO journey through the gateway:

1. Register owner and member.
2. Login as the owner.
3. Create a project.
4. Add the member.
5. Create a board, sprint, and epic.
6. Create a task.
7. Add watcher, label, subtask, story points, sprint, and epic.
8. Move task status.
9. Add a comment with a mention.
10. Read notifications and activity.

## LocalDB Alternative

If you run services directly with `dotnet run`, the current `appsettings.json` files point at LocalDB. You can apply migrations manually:

```powershell
.\scripts\update-databases.ps1
```

Then run the APIs from separate terminals or through your IDE launch profiles.

## Analytics

`AnalyticsApi` consumes RabbitMQ events from `workflow.io.events` into reporting tables. Current endpoints through the gateway:

- `GET /analytics/projects/{projectId}/summary`
- `GET /analytics/projects/{projectId}/tasks-by-status`
- `GET /analytics/projects/{projectId}/activity-trends?days=14`
- `GET /analytics/projects/{projectId}/sprints/{sprintId}/velocity`
- `GET /analytics/projects/{projectId}/sprints/{sprintId}/burndown?days=14`

## Realtime

`RealtimeApi` consumes RabbitMQ events from `workflow.io.events` and broadcasts them through SignalR.

Gateway hub URL:

- `/hubs/workflow.io`

Clients should connect with a JWT access token. Browser SignalR clients usually pass it through `accessTokenFactory`, which sends it as `access_token` during the WebSocket negotiation.

Hub methods:

- `JoinProject(projectId)`
- `LeaveProject(projectId)`
- `JoinTask(taskId)`
- `LeaveTask(taskId)`

Client events:

- `EventReceived`
- `ProjectEventReceived`
- `TaskEventReceived`
- `NotificationReceived`

The next engineering step is a frontend shell that uses the gateway, renders the board/backlog, and subscribes to this hub for live updates.
