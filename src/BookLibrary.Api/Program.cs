using BookLibrary.Api.Data;
using BookLibrary.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
var connectionString = builder.Configuration.GetConnectionString("Books");
if (databaseProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "A connection string named 'Books' is required when Database:Provider is Postgres.");
    }
}
else if (databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
{
    connectionString ??= "Data Source=books.db";
}
else
{
    throw new InvalidOperationException(
        $"Unsupported database provider '{databaseProvider}'. Supported providers are Sqlite and Postgres.");
}

builder.Services.AddDbContext<BooksDbContext>(options =>
{
    if (databaseProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
    {
        // The initial migration is intentionally shared with SQLite and uses
        // provider-specific SQL, so EF Core's model-snapshot comparison is not
        // meaningful for PostgreSQL's different relational type mappings.
        options.UseNpgsql(connectionString!)
            .ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
    else
    {
        options.UseSqlite(connectionString!);
    }
});

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var auth0Domain = builder.Configuration["Auth0:Domain"];
var auth0Audience = builder.Configuration["Auth0:Audience"];
var authConfigured = !string.IsNullOrWhiteSpace(auth0Domain) && !string.IsNullOrWhiteSpace(auth0Audience);
if (authConfigured)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://{auth0Domain}/";
            options.Audience = auth0Audience;
        });
    builder.Services.AddAuthorization();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalFrontends", policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [
            "http://localhost:5044", "http://localhost:5173", "http://localhost:5174"
        ])
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors("LocalFrontends");
if (authConfigured)
{
    app.UseAuthentication();
    app.UseAuthorization();
}
app.MapOpenApi();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
    db.Database.Migrate();

    if (!db.Books.Any())
    {
        db.Books.AddRange(
            new Book(0, "Clean Code", "Robert C. Martin", "9780132350884", "Software", 2008, true),
            new Book(0, "The Pragmatic Programmer", "David Thomas", "9780135957059", "Software", 2019, true));
        db.SaveChanges();
    }
}

app.MapGet("/api/books", async (
    string? search,
    string? genre,
    bool? available,
    int? page,
    int? pageSize,
    BooksDbContext db) =>
{
    var currentPage = Math.Max(page ?? 1, 1);
    var currentPageSize = Math.Clamp(pageSize ?? 50, 1, 100);
    var query = db.Books.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var value = search.Trim();
        query = query.Where(book => book.Title.Contains(value) || book.Author.Contains(value));
    }

    if (!string.IsNullOrWhiteSpace(genre))
    {
        query = query.Where(book => book.Genre == genre);
    }

    if (available.HasValue)
    {
        query = query.Where(book => book.IsAvailable == available.Value);
    }

    return Results.Ok(await query
        .OrderBy(book => book.Title)
        .Skip((currentPage - 1) * currentPageSize)
        .Take(currentPageSize)
        .ToListAsync());
});

app.MapPost("/api/books", async (CreateBookRequest request, BooksDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Author))
    {
        return Results.BadRequest("Title and author are required.");
    }

    var book = new Book(
        0, request.Title.Trim(), request.Author.Trim(), request.Isbn.Trim(),
        request.Genre.Trim(), request.PublishedYear, request.IsAvailable);
    db.Books.Add(book);
    await db.SaveChangesAsync();
    return Results.Created($"/api/books/{book.Id}", book);
});

app.MapPut("/api/books/{id:int}", async (int id, UpdateBookRequest request, BooksDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Author))
    {
        return Results.BadRequest("Title and author are required.");
    }

    var book = await db.Books.FindAsync(id);
    if (book is null)
    {
        return Results.NotFound();
    }

    db.Entry(book).CurrentValues.SetValues(new Book(
        id, request.Title.Trim(), request.Author.Trim(), request.Isbn.Trim(),
        request.Genre.Trim(), request.PublishedYear, request.IsAvailable));
    await db.SaveChangesAsync();
    return Results.Ok(await db.Books.AsNoTracking().SingleAsync(item => item.Id == id));
});

app.MapDelete("/api/books/{id:int}", async (int id, BooksDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book is null)
    {
        return Results.NotFound();
    }

    db.Books.Remove(book);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/api/loans", async (BooksDbContext db) =>
    Results.Ok(await db.Loans.AsNoTracking().OrderByDescending(loan => loan.BorrowedAt).ToListAsync()));

app.MapPost("/api/books/{id:int}/loans", async (int id, BorrowBookRequest request, BooksDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.BorrowerName))
    {
        return Results.BadRequest("Borrower name is required.");
    }

    var book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound();
    if (!book.IsAvailable) return Results.Conflict("The book is not available.");

    var loan = new Loan(0, id, request.BorrowerName.Trim(), DateTime.UtcNow, null);
    db.Loans.Add(loan);
    db.Entry(book).CurrentValues.SetValues(book with { IsAvailable = false });
    await db.SaveChangesAsync();
    return Results.Created($"/api/loans/{loan.Id}", loan);
});

app.MapPost("/api/loans/{id:int}/return", async (int id, BooksDbContext db) =>
{
    var loan = await db.Loans.FindAsync(id);
    if (loan is null) return Results.NotFound();
    if (loan.ReturnedAt is not null) return Results.Conflict("The book has already been returned.");

    db.Entry(loan).CurrentValues.SetValues(loan with { ReturnedAt = DateTime.UtcNow });
    var book = await db.Books.FindAsync(loan.BookId);
    if (book is not null) db.Entry(book).CurrentValues.SetValues(book with { IsAvailable = true });
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/api/reservations", async (BooksDbContext db) =>
    Results.Ok(await db.Reservations.AsNoTracking().Where(item => item.IsActive).ToListAsync()));

app.MapPost("/api/books/{id:int}/reservations", async (int id, ReserveBookRequest request, BooksDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.RequesterName))
    {
        return Results.BadRequest("Requester name is required.");
    }

    if (!await db.Books.AnyAsync(book => book.Id == id)) return Results.NotFound();
    var reservation = new Reservation(0, id, request.RequesterName.Trim(), DateTime.UtcNow);
    db.Reservations.Add(reservation);
    await db.SaveChangesAsync();
    return Results.Created($"/api/reservations/{reservation.Id}", reservation);
});

app.Run();
