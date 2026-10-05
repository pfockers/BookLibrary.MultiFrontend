using BookLibrary.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalFrontends", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var books = new List<Book>
{
    new(1, "Clean Code", "Robert C. Martin", "9780132350884", "Software", 2008, true),
    new(2, "The Pragmatic Programmer", "David Thomas", "9780135957059", "Software", 2019, true)
};
var nextId = 3;

var app = builder.Build();
app.UseCors("LocalFrontends");

app.MapGet("/api/books", () => Results.Ok(books));

app.MapPost("/api/books", (CreateBookRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Author))
    {
        return Results.BadRequest("Title and author are required.");
    }

    var book = new Book(
        nextId++, request.Title.Trim(), request.Author.Trim(), request.Isbn.Trim(),
        request.Genre.Trim(), request.PublishedYear, request.IsAvailable);
    books.Add(book);
    return Results.Created($"/api/books/{book.Id}", book);
});

app.MapDelete("/api/books/{id:int}", (int id) =>
{
    var removed = books.RemoveAll(book => book.Id == id);
    return removed == 0 ? Results.NotFound() : Results.NoContent();
});

app.Run();
