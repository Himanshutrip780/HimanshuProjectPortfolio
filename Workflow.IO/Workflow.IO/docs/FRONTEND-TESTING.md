# Frontend integration testing (Angular services)

## Prerequisites

1. Copy `.env.example` to `.env` and set `JWT_SECURITY_KEY`.
2. Start backend: `docker compose --env-file .env up --build`
3. Wire these services into your Angular `app.config.ts`:

```typescript
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
  ],
};
```

## Smoke flow

1. `AuthService.register({ email, password, firstName, lastName })`
2. `AuthService.login({ email, password })`
3. `ProjectService.create('Demo', 'Jira-like board')`
4. `TaskService.create(projectId, { title: 'Story 1', priority: 'Medium' })`
5. `TaskService.getBoard(projectId)` — Kanban columns
6. `RealtimeService.connect()` + `joinProject(projectId)`

Gateway base URL: `http://localhost:5270` (see `src/environments/environment.development.ts`).
