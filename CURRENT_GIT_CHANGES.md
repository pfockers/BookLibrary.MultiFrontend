# Current Git Changes

This document describes the changes currently present in the working tree compared with the committed `master` branch.

## 1. Database and persistence

1. Added Entity Framework Core SQLite support to `src/BookLibrary.Api/BookLibrary.Api.csproj`.
2. Added EF Core design-time tooling for migrations.
3. Added PostgreSQL support through `Npgsql.EntityFrameworkCore.PostgreSQL`.
4. Extended `BooksDbContext` with:
   - `Books`
   - `Loans`
   - `Reservations`
5. Added entity configuration for primary keys, required fields, maximum borrower/requester name lengths, and cascading book relationships.
6. Added the initial EF Core migration under `src/BookLibrary.Api/Data/Migrations/`.
7. Changed startup database initialization from `EnsureCreated()` to `Database.Migrate()`.
8. Added database provider selection through configuration:
   - Development: SQLite using `src/BookLibrary.Api/books.db`
   - Production: PostgreSQL
9. Added PostgreSQL connection settings and a persistent Docker volume in `docker-compose.yml`.

## 2. API model changes

1. Added `UpdateBookRequest`.
2. Added the `Loan` model.
3. Added the `Reservation` model.
4. Added `BorrowBookRequest`.
5. Added `ReserveBookRequest`.

## 3. Book API changes

The API remains available under `/api/books` and now supports:

1. List books with optional search, genre, availability, and pagination parameters:

   ```text
   GET /api/books?search=clean&genre=Software&available=true&page=1&pageSize=50
   ```

2. Add a book:

   ```text
   POST /api/books
   ```

3. Update a book:

   ```text
   PUT /api/books/{id}
   ```

4. Delete a book:

   ```text
   DELETE /api/books/{id}
   ```

5. Validate required title and author values.
6. Return `404 Not Found` for missing books.
7. Return consistent error responses through ASP.NET Core Problem Details configuration.
8. Order book results by title and limit page sizes to a maximum of 100 records.

## 4. Lending and reservation features

1. List loans:

   ```text
   GET /api/loans
   ```

2. Borrow a book:

   ```text
   POST /api/books/{bookId}/loans
   ```

3. Return a book:

   ```text
   POST /api/loans/{loanId}/return
   ```

4. List active reservations:

   ```text
   GET /api/reservations
   ```

5. Reserve a book:

   ```text
   POST /api/books/{bookId}/reservations
   ```

6. Prevent borrowing unavailable books.
7. Mark a book unavailable while it is on loan.
8. Mark a book available after it is returned.
9. Record UTC timestamps for borrowing, returning, and reservations.

## 5. API platform features

1. Added the `/health` endpoint.
2. Added OpenAPI mapping.
3. Added optional Auth0 JWT bearer authentication configuration.
4. Authentication is enabled only when both `Auth0:Domain` and `Auth0:Audience` are configured.
5. Restricted CORS to configured frontend origins:
   - `http://localhost:5044`
   - `http://localhost:5173`
   - `http://localhost:5174`
6. Added development and production database configuration files.

## 6. Blazor frontend changes

Updated `src/BookLibrary.Blazor/Pages/Home.razor` to support:

1. Add and edit modes using the same form.
2. Search by title or author.
3. Display book availability.
4. Edit books through the `PUT` endpoint.
5. Borrow available books.
6. Delete books.
7. Loading and error states.
8. Updated Bookshelf visual presentation.

## 7. React frontend changes

Updated `src/BookLibrary.React/src/App.tsx` to support:

1. Search by title or author.
2. Add and edit modes.
3. Borrowing with a prompted borrower name.
4. Delete operations.
5. Loading and error states.
6. Updated Bookshelf visual presentation.
7. API access through `http://localhost:5211/api/books`.
8. Production container packaging through `src/BookLibrary.React/Dockerfile`.

The React development server can be started on port 5174 with:

```powershell
Set-Location src/BookLibrary.React
npm run dev -- --port 5174
```

## 8. WPF frontend changes

Updated `src/BookLibrary.Wpf/MainViewModel.cs` and `MainWindow.xaml` to support:

1. API access through `http://localhost:5211`.
2. Add and edit modes.
3. Borrowing books using the current Windows username.
4. Delete operations.
5. Updated Bookshelf branding and layout.
6. Availability display and action controls.

## 9. Docker changes

Added:

- `src/BookLibrary.Api/Dockerfile`
- `src/BookLibrary.React/Dockerfile`
- `docker-compose.yml`

The Docker Compose setup starts:

1. PostgreSQL on port `5432`.
2. The API on port `5211`.
3. The React frontend on port `5174`.
4. A persistent `postgres-data` volume.

Start the containerized application with:

```powershell
docker compose up --build
```

## 10. Automated tests

Added the `tests/BookLibrary.Api.Tests` project with an xUnit test that verifies:

1. A book can be saved with EF Core.
2. A loan can be saved in the same SQLite store.
3. The entities can be read after reopening the database context.
4. The in-memory SQLite test database retains data while its connection remains open.

Run the tests with:

```powershell
dotnet test BookLibrary.MultiFrontend.slnx
```

## 11. Continuous integration

Added `.github/workflows/ci.yml` with separate jobs for .NET and React.

The .NET job:

1. Installs .NET 9.
2. Restores the solution.
3. Builds the solution in Release mode.
4. Runs the .NET tests.

The React job:

1. Installs Node.js 22.
2. Restores npm dependencies with `npm ci`.
3. Runs the React linter.
4. Builds the React application.

## 12. Solution changes

Added the API test project to `BookLibrary.MultiFrontend.slnx`.

## 13. Recommended validation sequence

Run these commands from the repository root:

```powershell
dotnet restore BookLibrary.MultiFrontend.slnx
dotnet build BookLibrary.MultiFrontend.slnx
dotnet test BookLibrary.MultiFrontend.slnx
Set-Location src/BookLibrary.React
npm ci
npm run lint
npm run build
```

For local development, start the API before the frontends:

```powershell
dotnet run --project src/BookLibrary.Api/BookLibrary.Api.csproj --launch-profile http
dotnet run --project src/BookLibrary.Blazor/BookLibrary.Blazor.csproj --launch-profile http
dotnet run --project src/BookLibrary.Wpf/BookLibrary.Wpf.csproj
Set-Location src/BookLibrary.React
npm run dev -- --port 5174
```

## 14. Current changed-file inventory

### Modified files

- `BookLibrary.MultiFrontend.slnx`
- `src/BookLibrary.Api/BookLibrary.Api.csproj`
- `src/BookLibrary.Api/Data/BooksDbContext.cs`
- `src/BookLibrary.Api/Models/Book.cs`
- `src/BookLibrary.Api/Program.cs`
- `src/BookLibrary.Api/appsettings.Development.json`
- `src/BookLibrary.Api/appsettings.json`
- `src/BookLibrary.Blazor/Pages/Home.razor`
- `src/BookLibrary.React/src/App.tsx`
- `src/BookLibrary.Wpf/MainViewModel.cs`
- `src/BookLibrary.Wpf/MainWindow.xaml`

### Added files and directories

- `.github/workflows/ci.yml`
- `docker-compose.yml`
- `src/BookLibrary.Api/Data/Migrations/`
- `src/BookLibrary.Api/Dockerfile`
- `src/BookLibrary.React/Dockerfile`
- `tests/BookLibrary.Api.Tests/BookLibrary.Api.Tests.csproj`
- `tests/BookLibrary.Api.Tests/UnitTest1.cs`
