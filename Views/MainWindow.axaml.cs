using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;
    private Book? _selectedBook;
    private bool _ready = false;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;

        Dispatcher.UIThread.Post(Initialize, DispatcherPriority.Loaded);
    }

    private void Initialize()
    {
        using var db = new LibraryDbContext();
        var authors = db.Authors.ToList();
        var genres = db.Genres.ToList();

        AuthorFilterComboBox.SelectionChanged -= AuthorFilter_Changed;
        GenreFilterComboBox.SelectionChanged -= GenreFilter_Changed;

        AuthorFilterComboBox.ItemsSource = authors;
        GenreFilterComboBox.ItemsSource = genres;

        AuthorFilterComboBox.SelectionChanged += AuthorFilter_Changed;
        GenreFilterComboBox.SelectionChanged += GenreFilter_Changed;

        _ready = true;
        RefreshList();
    }

    private void RefreshList()
    {
        if (!_ready) return;

        try
        {
            using var db = new LibraryDbContext();
            var query = db.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .AsQueryable();

            var searchText = SearchTextBox?.Text ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(searchText))
                query = query.Where(b => b.Title.ToLower().Contains(searchText.ToLower()));

            if (AuthorFilterComboBox?.SelectedItem is Author selectedAuthor)
                query = query.Where(b => b.AuthorId == selectedAuthor.Id);

            if (GenreFilterComboBox?.SelectedItem is Genre selectedGenre)
                query = query.Where(b => b.GenreId == selectedGenre.Id);

            var books = query.ToList();

            BooksListBox.ItemsSource = null;
            BooksListBox.ItemsSource = books;

            // Обновляем статус
            StatusText.Text = $"Всего книг: {books.Count}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private void BooksListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedBook = BooksListBox.SelectedItem as Book;
        var hasSelection = _selectedBook != null;
        EditButton.IsEnabled = hasSelection;
        DeleteButton.IsEnabled = hasSelection;
    }

    private void SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e) => RefreshList();
    private void AuthorFilter_Changed(object? sender, SelectionChangedEventArgs e) => RefreshList();
    private void GenreFilter_Changed(object? sender, SelectionChangedEventArgs e) => RefreshList();

    private void ResetFilters_Click(object? sender, RoutedEventArgs e)
    {
        AuthorFilterComboBox.SelectionChanged -= AuthorFilter_Changed;
        GenreFilterComboBox.SelectionChanged -= GenreFilter_Changed;
        SearchTextBox.Text = string.Empty;
        AuthorFilterComboBox.SelectedItem = null;
        GenreFilterComboBox.SelectedItem = null;
        AuthorFilterComboBox.SelectionChanged += AuthorFilter_Changed;
        GenreFilterComboBox.SelectionChanged += GenreFilter_Changed;
        RefreshList();
    }

    private async void AddBook_Click(object? sender, RoutedEventArgs e)
    {
        using var db = new LibraryDbContext();
        var authors = db.Authors.ToList();
        var genres = db.Genres.ToList();
        if (authors.Count == 0 || genres.Count == 0) return;

        var dialog = new BookEditWindow(authors, genres);
        var result = await dialog.ShowDialog<Book?>(this);

        if (result != null)
        {
            using var db2 = new LibraryDbContext();
            db2.Books.Add(new Book
            {
                Title = result.Title,
                AuthorId = result.AuthorId,
                GenreId = result.GenreId,
                PublishYear = result.PublishYear,
                ISBN = result.ISBN,
                QuantityInStock = result.QuantityInStock
            });
            db2.SaveChanges();
            RefreshList();
        }
    }

    private async void EditBook_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedBook == null) return;

        using var db = new LibraryDbContext();
        var authors = db.Authors.ToList();
        var genres = db.Genres.ToList();

        var dialog = new BookEditWindow(authors, genres, _selectedBook);
        var result = await dialog.ShowDialog<Book?>(this);

        if (result != null)
        {
            using var db2 = new LibraryDbContext();
            var existing = db2.Books.Find(result.Id);
            if (existing != null)
            {
                existing.Title = result.Title;
                existing.AuthorId = result.AuthorId;
                existing.GenreId = result.GenreId;
                existing.PublishYear = result.PublishYear;
                existing.ISBN = result.ISBN;
                existing.QuantityInStock = result.QuantityInStock;
                db2.SaveChanges();
            }
            _selectedBook = null;
            EditButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            RefreshList();
        }
    }

    private async void DeleteBook_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedBook == null) return;

        var confirmed = false;
        var win = new Window
        {
            Title = "Подтверждение удаления",
            Width = 320,
            Height = 140,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
        panel.Children.Add(new TextBlock { Text = $"Удалить книгу \"{_selectedBook.Title}\"?", TextWrapping = Avalonia.Media.TextWrapping.Wrap });

        var btnPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10
        };

        var yesBtn = new Button { Content = "Удалить", Width = 90 };
        var noBtn = new Button { Content = "Отмена", Width = 90 };
        yesBtn.Click += (s, a) => { confirmed = true; win.Close(); };
        noBtn.Click += (s, a) => win.Close();
        btnPanel.Children.Add(yesBtn);
        btnPanel.Children.Add(noBtn);
        panel.Children.Add(btnPanel);
        win.Content = panel;

        await win.ShowDialog(this);

        if (confirmed)
        {
            using var db = new LibraryDbContext();
            var existing = db.Books.Find(_selectedBook.Id);
            if (existing != null)
            {
                db.Books.Remove(existing);
                db.SaveChanges();
            }
            _selectedBook = null;
            EditButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            RefreshList();
        }
    }

    private async void ManageAuthors_Click(object? sender, RoutedEventArgs e)
    {
        await new AuthorManagementWindow(_viewModel).ShowDialog(this);
        Initialize();
    }

    private async void ManageGenres_Click(object? sender, RoutedEventArgs e)
    {
        await new GenreManagementWindow(_viewModel).ShowDialog(this);
        Initialize();
    }
}