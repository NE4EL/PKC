using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using System;

namespace LibraryManagement.Data;

public class LibraryDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Genre> Genres { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=library_db;Username=egor;Password=");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка Author
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.LastName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Country).HasMaxLength(100);
        });

        // Настройка Genre
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
            entity.Property(g => g.Description).HasMaxLength(500);
        });

        // Настройка Book
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
            entity.Property(b => b.ISBN).IsRequired().HasMaxLength(20);
            entity.Property(b => b.PublishYear).IsRequired();
            entity.Property(b => b.QuantityInStock).IsRequired();

            // Связь с Author (один-ко-многим)
            entity.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Genre (один-ко-многим)
            entity.HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Начальные данные
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FirstName = "Лев", LastName = "Толстой", BirthDate = DateTime.SpecifyKind(new DateTime(1828, 9, 9), DateTimeKind.Utc), Country = "Россия" },
            new Author { Id = 2, FirstName = "Фёдор", LastName = "Достоевский", BirthDate = DateTime.SpecifyKind(new DateTime(1821, 11, 11), DateTimeKind.Utc), Country = "Россия" }
        );

        modelBuilder.Entity<Genre>().HasData(
            new Genre { Id = 1, Name = "Роман", Description = "Эпическое произведение" },
            new Genre { Id = 2, Name = "Фантастика", Description = "Научная фантастика" }
        );

        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Война и мир", AuthorId = 1, GenreId = 1, PublishYear = 1869, ISBN = "978-5-17-123456-7", QuantityInStock = 5 },
            new Book { Id = 2, Title = "Преступление и наказание", AuthorId = 2, GenreId = 1, PublishYear = 1866, ISBN = "978-5-17-234567-8", QuantityInStock = 3 }
        );
    }
}