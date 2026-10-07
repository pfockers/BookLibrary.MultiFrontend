# BookLibrary.MultiFrontend

A sample Book Library with one ASP.NET Core API and three clients:

- `src/BookLibrary.Api` - ASP.NET Core minimal API with EF Core persistence
- `src/BookLibrary.React` - React + TypeScript + Vite
- `src/BookLibrary.Wpf` - WPF using MVVM and `HttpClient`
- `src/BookLibrary.Blazor` - Blazor WebAssembly

## Run the complete stack with Docker

Docker Compose runs PostgreSQL, the API, and both the React and Blazor frontends. The database uses a named volume so its data survives container restarts.

1. Copy `.env.example` to `.env` and set `POSTGRES_PASSWORD` to a strong local value. `.env` is ignored by Git.
2. From the repository root, run:

	```powershell
	docker compose up --build
	```

The services are available at:

- React: <http://localhost:5174>
- Blazor: <http://localhost:5044>
- API: <http://localhost:5211/api/books>
- API health: <http://localhost:5211/health>
- PostgreSQL: `localhost:5432` (database `booklibrary`, user `booklibrary`)

Compose waits for PostgreSQL's health check before starting the API. The API applies EF Core migrations at startup. To stop the services while keeping database data, press Ctrl+C and run `docker compose down`. To also delete the persisted database volume, run `docker compose down -v`.

The supplied `.env.example` password is for local setup only; use managed secrets and a unique credential for shared or production deployments.
Compose publishes services on localhost only. The sample API endpoints do not currently require authorization, so do not expose this stack to a network or deploy it as-is.

## Run locally without Docker

The Development environment defaults to SQLite, storing the database in `src/BookLibrary.Api/books.db`.

```powershell
dotnet run --project src/BookLibrary.Api --urls http://localhost:5211
```

The API is available at <http://localhost:5211/api/books>. To select another database, configure `Database:Provider` and `ConnectionStrings:Books`; the supported providers are `Sqlite` and `Postgres`. PostgreSQL requires a configured `Books` connection string.

### Run React

With the API running:

```powershell
Set-Location src/BookLibrary.React
npm install
npm run dev -- --port 5174
```

### Run WPF

With the API running:

```powershell
dotnet run --project src/BookLibrary.Wpf
```

### Run Blazor

With the API running:

```powershell
dotnet run --project src/BookLibrary.Blazor
```

All clients share the same API and persisted library data. Local SQLite and Docker PostgreSQL are separate stores.
