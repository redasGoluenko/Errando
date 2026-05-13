# Errando

Errando is a full-stack task management app with a Vue frontend and an ASP.NET Core backend.

## Stack

- Frontend: Vue 3, TypeScript, Vite, Pinia, Vue Router, Axios, Tailwind CSS
- Backend: ASP.NET Core 9, Entity Framework Core, PostgreSQL, JWT authentication
- Tests: Vitest for the frontend, xUnit for the backend

## Project Structure

- `frontend/` - Vue application
- `backend/` - ASP.NET Core API
- `backend.Tests/` - backend unit and integration tests

## Common Commands

Frontend:

```powershell
cd frontend
npm install
npm run dev
npm run test
npm run test:coverage
```

Backend:

```powershell
dotnet restore
dotnet build
dotnet test
```

## Coverage Reports

Generated coverage reports are kept in:

- `frontend/coverage/index.html`
- `backend.Tests/coverage-report/index.html`

These reports are static snapshots from the last coverage run. Regenerate them before using the numbers for a final submission.
