using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryManagement.Models;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagement.Views;

public partial class BookEditWindow : Window
{
    private Book? _book;
    private List<Author> _authors;
    private List<Genre> _genres;

    public BookEditWindow(List<Author> authors, List<Genre> genres, Book? book = null)
    {
        InitializeComponent();

        _book = book;
        _authors = authors;
        _genres = genres;

        AuthorComboBox.ItemsSource = authors;
        GenreComboBox.ItemsSource = genres;

        if (book != null)
        {
            Title = "Редактирование книги";
            TitleTextBox.Text = book.Title;
            // Ищем по Id
            AuthorComboBox.SelectedIndex = authors.FindIndex(a => a.Id == book.AuthorId);
            GenreComboBox.SelectedIndex = genres.FindIndex(g => g.Id == book.GenreId);
            YearNumeric.Value = book.PublishYear;
            ISBNTextBox.Text = book.ISBN;
            QuantityNumeric.Value = book.QuantityInStock;
        }
        else
        {
            Title = "Добавление книги";
            YearNumeric.Value = 2024;
            QuantityNumeric.Value = 1;
            // Выбираем первый элемент по умолчанию
            if (authors.Count > 0) AuthorComboBox.SelectedIndex = 0;
            if (genres.Count > 0) GenreComboBox.SelectedIndex = 0;
        }
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            return;
        }

        if (AuthorComboBox.SelectedItem is not Author selectedAuthor)
        {
            return;
        }

        if (GenreComboBox.SelectedItem is not Genre selectedGenre)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(ISBNTextBox.Text))
        {
            return;
        }

        var book = new Book
        {
            Id = _book?.Id ?? 0,
            Title = TitleTextBox.Text,
            AuthorId = selectedAuthor.Id,
            GenreId = selectedGenre.Id,
            PublishYear = (int)(YearNumeric.Value ?? 2024),
            ISBN = ISBNTextBox.Text,
            QuantityInStock = (int)(QuantityNumeric.Value ?? 0)
        };

        Close(book);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}