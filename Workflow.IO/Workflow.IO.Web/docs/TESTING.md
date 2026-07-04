# Workflow.IO.Web â€” testing guide

## Prerequisites

1. Start backend: from `Workflow.IO` repo root run `docker compose up --build`.
2. Register demo user once via `Workflow.IO/docs/local-smoke.http` (Register owner), or use the UI register page.

## Run unit tests

```powershell
cd Workflow.IO.Web
npm test -- --no-watch --browsers=ChromeHeadless
```

Covered specs:

- `api.util.spec.ts` â€” response unwrap and query params
- `api-http.service.spec.ts` â€” HTTP + `ApiResponse` mapping
- `auth.guard.spec.ts` â€” redirect when unauthenticated
- `user.service.spec.ts` â€” `/users/me` and `/users/lookup`

## Manual E2E (browser)

```powershell
cd Workflow.IO.Web
npm start
```

Open `http://localhost:4200`.

| Step | Action | Expected |
|------|--------|----------|
| 1 | `/auth/register` | Account created; auto-login â†’ projects |
| 2 | Sign out â†’ `/auth/login` | Login with demo credentials |
| 3 | `/projects` | Create project; appears in list |
| 4 | Project â†’ **Tasks** | Create task; open task detail |
| 5 | Task detail | Rich-text comment; attachment upload |
| 6 | **Board** | Drag card to another column â†’ status updates |
| 7 | **Backlog** | Backlog and sprint tasks listed |
| 8 | **Members** | Search user by email; add without pasting GUID |
| 9 | **Analytics** | Metrics or helpful empty state (needs AnalyticsApi + events) |
| 10 | **Profile** | Loads email + name from `/users/me`; save works |
| 11 | **Notifications** | Lists notifications for signed-in user |
| 12 | **Admin** (Admin role) | Lists all users |

## Build verification

```powershell
npm run build -- --configuration=development
dotnet build UserApi/UserApi.csproj
```
