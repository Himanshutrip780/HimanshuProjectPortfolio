# Workflow.IO â€” Run locally (FE + BE)

## Prerequisites

- Docker Desktop (recommended) **or** .NET 9 SDK + SQL Server + RabbitMQ
- Node.js 20+ for the Angular app

## Backend (Docker â€” recommended)

```powershell
cd Workflow.IO
copy .env.example .env
# Edit .env: set JWT_SECURITY_KEY (32+ chars) and MSSQL_SA_PASSWORD

docker compose up --build -d
```

Wait until all services are healthy. API gateway: **http://localhost:5270**

| Service | Port |
|---------|------|
| Gateway | 5270 |
| User API | 5240 |
| Project API | 5250 |
| Task API | 5260 |
| Comment API | 5280 |
| Notification API | 5290 |
| Activity API | 5300 |
| File API | 5310 |
| Analytics API | 5320 |
| Realtime API | 5330 |

Health: `GET http://localhost:5270/ready`

## Frontend

```powershell
cd Workflow.IO.Web
npm install
npm start
```

Open **http://localhost:4200**. The app calls the gateway at `http://localhost:5270` (see `src/environments/environment.ts`).

## First-time usage

1. Register at `/auth/register`
2. Create a project (optional key, e.g. `DEMO`)
3. Create tasks, use Board / Backlog / Sprints / Planning / Analytics

## All integrated features

- **Auth:** login, register, refresh, profile, change password
- **Users (admin):** list, edit, delete
- **Projects:** CRUD, archive, open by key, members & roles
- **Tasks:** CRUD, search, assign, board, backlog, sprints, epics, labels, subtasks, watchers
- **Planning:** components, versions, links, worklogs, bulk update, saved filters
- **Comments & files** on tasks
- **Notifications:** inbox, unread badge, mark read
- **Activity:** per task and per project
- **Analytics:** summary, trends, burndown, velocity
- **Realtime:** SignalR via gateway `/hubs/workflow.io`

## Troubleshooting

- **CORS:** Gateway allows `http://localhost:4200` by default.
- **Analytics empty:** Create tasks and change status so RabbitMQ events populate reporting tables.
- **Realtime:** Ensure `realtimeapi` and `rabbitmq` containers are running; check browser console for WebSocket errors.
- **Board empty:** Use **Create default board** on the Board tab.
