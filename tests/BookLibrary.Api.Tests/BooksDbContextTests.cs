using BookLibrary.Api.Data;
using BookLibrary.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Api.Tests;

public sealed class BooksDbContextTests
{
    [Fact]
    public async Task BooksAndLoansPersistInTheSameStore()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<BooksDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var db = new BooksDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            var book = new Book(0, "Test Book", "Test Author", "123", "Test", 2026, true);
            db.Books.Add(book);
            await db.SaveChangesAsync();

            db.Loans.Add(new Loan(0, book.Id, "Tester", DateTime.UtcNow, null));
            await db.SaveChangesAsync();
        }

        await using var reopened = new BooksDbContext(options);
        Assert.Single(await reopened.Books.ToListAsync());
        Assert.Single(await reopened.Loans.ToListAsync());
    }
}