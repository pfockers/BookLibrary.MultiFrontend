namespace BookLibrary.Api.Models;

public sealed record Book(
    int Id,
    string Title,
    string Author,
    string Isbn,
    string Genre,
    int PublishedYear,
    bool IsAvailable);

public sealed record CreateBookRequest(
    string Title,
    string Author,
    string Isbn,
    string Genre,
    int PublishedYear,
    bool IsAvailable = true);
