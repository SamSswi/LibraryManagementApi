using System.ComponentModel.DataAnnotations;
using LibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Book => Set<Book>();
    public DbSet<Author> Author => Set<Author>();
    public DbSet<AuthorBook> AuthorBooks => Set<AuthorBook>();
    public DbSet<Loan> Loan => Set<Loan>();
    public DbSet<LoanItem> LoanItem => Set<LoanItem>();
    public DbSet<User> User=> Set<User>();


    // Author <-> Book :: Many to Many 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<AuthorBook>()
            .HasKey(sc => new {sc.AuthorId, sc.BookId});

        modelBuilder.Entity<AuthorBook>()
            .HasOne(sc => sc.Author)
            .WithMany(sc => sc.AuthorBooks)
            .HasForeignKey(sc => sc.AuthorId);

        modelBuilder.Entity<AuthorBook>()
            .HasOne(sc => sc.Book)
            .WithMany(sc => sc.AuthorBooks)
            .HasForeignKey(sc => sc.BookId);
    }
    

}