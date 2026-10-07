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

public sealed record UpdateBookRequest(
    string Title,
    string Author,
    string Isbn,
    string Genre,
    int PublishedYear,
    bool IsAvailable);

public sealed record Loan(
    int Id,
    int BookId,
    string BorrowerName,
    DateTime BorrowedAt,
    DateTime? ReturnedAt);

public sealed record Reservation(
    int Id,
    int BookId,
    string RequesterName,
    DateTime ReservedAt,
    bool IsActive = true);

public sealed record BorrowBookRequest(string BorrowerName);
public sealed record ReserveBookRequest(string RequesterName);
