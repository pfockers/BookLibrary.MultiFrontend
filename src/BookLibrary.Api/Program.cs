using BookLibrary.Api.Data;
using BookLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BooksDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Books") ?? "Data Source=books.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalFrontends", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("LocalFrontends");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
    db.Database.EnsureCreated();

    if (!db.Books.Any())
    {
        db.Books.AddRange(
            new Book(1, "Clean Code", "Robert C. Martin", "9780132350884", "Software", 2008, true),
            new Book(2, "The Pragmatic Programmer", "David Thomas", "9780135957059", "Software", 2019, true));
        db.SaveChanges();
    }
}

app.MapGet("/api/books", async (BooksDbContext db) =>
    Results.Ok(await db.Books.AsNoTracking().ToListAsync()));

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

app.Run();
