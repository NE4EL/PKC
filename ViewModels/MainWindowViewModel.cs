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
        .Include(b => b.Author)
        .Include(b => b.Genre)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(SearchText))
        query = query.Where(b => b.Title.ToLower().Contains(SearchText.ToLower()));

    if (SelectedFilterAuthor != null)
        query = query.Where(b => b.AuthorId == SelectedFilterAuthor.Id);

    if (SelectedFilterGenre != null)
        query = query.Where(b => b.GenreId == SelectedFilterGenre.Id);

    Books = new ObservableCollection<Book>(query.ToList());
}

    public void AddBook(Book book)
    {
        using var db = new LibraryDbContext();
        // Создаём новый объект чтобы не было проблем с отслеживанием
        var newBook = new Book
        {
            Title = book.Title,
            AuthorId = book.AuthorId,
            GenreId = book.GenreId,
            PublishYear = book.PublishYear,
            ISBN = book.ISBN,
            QuantityInStock = book.QuantityInStock
        };
        db.Books.Add(newBook);
        db.SaveChanges();
        LoadBooks();
    }

    public void UpdateBook(Book book)
    {
        using var db = new LibraryDbContext();
        var existing = db.Books.Find(book.Id);
        if (existing != null)
        {
            existing.Title = book.Title;
            existing.AuthorId = book.AuthorId;
            existing.GenreId = book.GenreId;
            existing.PublishYear = book.PublishYear;
            existing.ISBN = book.ISBN;
            existing.QuantityInStock = book.QuantityInStock;
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
        var newAuthor = new Author
        {
            FirstName = author.FirstName,
            LastName = author.LastName,
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
            existing.FirstName = author.FirstName;
            existing.LastName = author.LastName;
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
        var newGenre = new Genre
        {
            Name = genre.Name,
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
            existing.Name = genre.Name;
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