using BookLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Api.Data;

public sealed class BooksDbContext(DbContextOptions<BooksDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(book => book.Id);
            entity.Property(book => book.Title).IsRequired();
            entity.Property(book => book.Author).IsRequired();
            entity.Property(book => book.Isbn).IsRequired();
            entity.Property(book => book.Genre).IsRequired();
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(loan => loan.Id);
            entity.Property(loan => loan.BorrowerName).IsRequired().HasMaxLength(200);
            entity.HasOne<Book>().WithMany().HasForeignKey(loan => loan.BookId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(reservation => reservation.Id);
            entity.Property(reservation => reservation.RequesterName).IsRequired().HasMaxLength(200);
            entity.HasOne<Book>().WithMany().HasForeignKey(reservation => reservation.BookId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
