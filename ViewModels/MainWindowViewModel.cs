using System.Collections.ObjectModel;
using System.Linq;
using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibraryManagement.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ObservableCollection<Book> _books = new();
    private ObservableCollection<Author> _authors = new();
    private ObservableCollection<Genre> _genres = new();
    private Book? _selectedBook;
    private Author? _selectedFilterAuthor;
    private Genre? _selectedFilterGenre;
    private string _searchText = string.Empty;

    public ObservableCollection<Book> Books
    {
        get => _books;
        set { _books = value; OnPropertyChanged(); }
    }

    public ObservableCollection<Author> Authors
    {
        get => _authors;
        set { _authors = value; OnPropertyChanged(); }
    }

    public ObservableCollection<Genre> Genres
    {
        get => _genres;
        set { _genres = value; OnPropertyChanged(); }
    }

    public Book? SelectedBook
    {
        get => _selectedBook;
        set { _selectedBook = value; OnPropertyChanged(); }
    }

    public Author? SelectedFilterAuthor
    {
        get => _selectedFilterAuthor;
        set
        {
            _selectedFilterAuthor = value;
            OnPropertyChanged();
            LoadBooks();
        }
    }

    public Genre? SelectedFilterGenre
    {
        get => _selectedFilterGenre;
        set
        {
            _selectedFilterGenre = value;
            OnPropertyChanged();
            LoadBooks();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            LoadBooks();
        }
    }

    public MainWindowViewModel()
    {
        LoadData();
    }

    public void LoadData()
    {
        LoadAuthors();
        LoadGenres();
        LoadBooks();
    }

    public void LoadAuthors()
    {
        using var db = new LibraryDbContext();
        Authors = new ObservableCollection<Author>(db.Authors.ToList());
    }

    public void LoadGenres()
    {
        using var db = new LibraryDbContext();
        Genres = new ObservableCollection<Genre>(db.Genres.ToList());
    }

    public void LoadBooks()
    {
        using var db = new LibraryDbContext();
        var query = db.Books
            .Include(b => b.Authors)
            .Include(b => b.Genres)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchText))
            query = query.Where(b => b.Title.ToLower().Contains(SearchText.ToLower()));

        if (SelectedFilterAuthor != null)
            query = query.Where(b => b.Authors.Any(a => a.Id == SelectedFilterAuthor.Id));

        if (SelectedFilterGenre != null)
            query = query.Where(b => b.Genres.Any(g => g.Id == SelectedFilterGenre.Id));

        Books = new ObservableCollection<Book>(query.ToList());
    }

    public void AddBook(Book book)
    {
        using var db = new LibraryDbContext();
        var authorIds = book.Authors.Select(a => a.Id).ToList();
        var genreIds = book.Genres.Select(g => g.Id).ToList();

        var authors = db.Authors.Where(a => authorIds.Contains(a.Id)).ToList();
        var genres = db.Genres.Where(g => genreIds.Contains(g.Id)).ToList();

        var newBook = new Book
        {
            Title = book.Title,
            PublishYear = book.PublishYear,
            ISBN = book.ISBN,
            QuantityInStock = book.QuantityInStock,
            Authors = authors,
            Genres = genres
        };

        db.Books.Add(newBook);
        db.SaveChanges();
        LoadBooks();
    }

    public void UpdateBook(Book book)
    {
        using var db = new LibraryDbContext();
        var existing = db.Books
            .Include(b => b.Authors)
            .Include(b => b.Genres)
            .FirstOrDefault(b => b.Id == book.Id);
        if (existing != null)
        {
            existing.Title = book.Title;
            existing.PublishYear = book.PublishYear;
            existing.ISBN = book.ISBN;
            existing.QuantityInStock = book.QuantityInStock;

            existing.Authors.Clear();
            existing.Genres.Clear();

            var authorIds = book.Authors.Select(a => a.Id).ToList();
            var genreIds = book.Genres.Select(g => g.Id).ToList();

            var authors = db.Authors.Where(a => authorIds.Contains(a.Id)).ToList();
            var genres = db.Genres.Where(g => genreIds.Contains(g.Id)).ToList();

            foreach (var a in authors)
                existing.Authors.Add(a);
            foreach (var g in genres)
                existing.Genres.Add(g);

            db.SaveChanges();
        }
        LoadBooks();
    }

    public void DeleteBook(Book book)
    {
        using var db = new LibraryDbContext();
        var existing = db.Books.Find(book.Id);
        if (existing != null)
        {
            db.Books.Remove(existing);
            db.SaveChanges();
        }
        LoadBooks();
    }

    public void AddAuthor(Author author)
    {
        using var db = new LibraryDbContext();
        var firstName = (author.FirstName ?? string.Empty).Trim();
        var lastName = (author.LastName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return;

        var exists = db.Authors.Any(a =>
            a.FirstName.ToLower() == firstName.ToLower() &&
            a.LastName.ToLower() == lastName.ToLower());
        if (exists)
            return;

        var newAuthor = new Author
        {
            FirstName = firstName,
            LastName = lastName,
            BirthDate = author.BirthDate,
            Country = author.Country
        };
        db.Authors.Add(newAuthor);
        db.SaveChanges();
        LoadAuthors();
    }

    public void UpdateAuthor(Author author)
    {
        using var db = new LibraryDbContext();
        var existing = db.Authors.Find(author.Id);
        if (existing != null)
        {
            var firstName = (author.FirstName ?? string.Empty).Trim();
            var lastName = (author.LastName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return;

            var exists = db.Authors.Any(a =>
                a.Id != author.Id &&
                a.FirstName.ToLower() == firstName.ToLower() &&
                a.LastName.ToLower() == lastName.ToLower());
            if (exists)
                return;

            existing.FirstName = firstName;
            existing.LastName = lastName;
            existing.BirthDate = author.BirthDate;
            existing.Country = author.Country;
            db.SaveChanges();
        }
        LoadAuthors();
    }

    public void DeleteAuthor(Author author)
    {
        using var db = new LibraryDbContext();
        var existing = db.Authors.Find(author.Id);
        if (existing != null)
        {
            db.Authors.Remove(existing);
            db.SaveChanges();
        }
        LoadAuthors();
        LoadBooks();
    }

    public void AddGenre(Genre genre)
    {
        using var db = new LibraryDbContext();
        var name = (genre.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return;

        var exists = db.Genres.Any(g => g.Name.ToLower() == name.ToLower());
        if (exists)
            return;

        var newGenre = new Genre
        {
            Name = name,
            Description = genre.Description
        };
        db.Genres.Add(newGenre);
        db.SaveChanges();
        LoadGenres();
    }

    public void UpdateGenre(Genre genre)
    {
        using var db = new LibraryDbContext();
        var existing = db.Genres.Find(genre.Id);
        if (existing != null)
        {
            var name = (genre.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                return;

            var exists = db.Genres.Any(g =>
                g.Id != genre.Id &&
                g.Name.ToLower() == name.ToLower());
            if (exists)
                return;

            existing.Name = name;
            existing.Description = genre.Description;
            db.SaveChanges();
        }
        LoadGenres();
    }

    public void DeleteGenre(Genre genre)
    {
        using var db = new LibraryDbContext();
        var existing = db.Genres.Find(genre.Id);
        if (existing != null)
        {
            db.Genres.Remove(existing);
            db.SaveChanges();
        }
        LoadGenres();
        LoadBooks();
    }
}