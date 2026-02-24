using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryManagement.Models;
using System;
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
            AuthorComboBox.SelectedIndex = authors.FindIndex(a => a.Id == book.AuthorId);
            GenreComboBox.SelectedIndex = genres.FindIndex(g => g.Id == book.GenreId);
            YearNumeric.Value = book.PublishYear;
            ISBNTextBox.Text = book.ISBN;
            QuantityNumeric.Value = book.QuantityInStock;
        }
        else
        {
            Title = "Добавление книги";
            YearNumeric.Value = DateTime.Now.Year;
            QuantityNumeric.Value = 1;
            if (authors.Count > 0) AuthorComboBox.SelectedIndex = 0;
            if (genres.Count > 0) GenreComboBox.SelectedIndex = 0;
        }
    }

    private bool ValidateYear(int year, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        if (year < 1000)
        {
            errorMessage = "Год издания не может быть меньше 1000";
            return false;
        }
        
        if (year > DateTime.Now.Year + 1)
        {
            errorMessage = $"Год издания не может быть больше {DateTime.Now.Year + 1}";
            return false;
        }
        
        return true;
    }

    private bool ValidateISBN(string isbn, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        if (string.IsNullOrWhiteSpace(isbn))
        {
            errorMessage = "ISBN не может быть пустым";
            return false;
        }

        // Убираем пробелы и дефисы для проверки
        string cleanISBN = isbn.Replace("-", "").Replace(" ", "");

        // ISBN-10 или ISBN-13
        if (cleanISBN.Length != 10 && cleanISBN.Length != 13)
        {
            errorMessage = "ISBN должен содержать 10 или 13 цифр\nПример: 978-5-17-123456-7 или 5-17-123456-7";
            return false;
        }

        // Проверяем что все символы - цифры (кроме последнего символа в ISBN-10, может быть X)
        for (int i = 0; i < cleanISBN.Length; i++)
        {
            if (i == cleanISBN.Length - 1 && cleanISBN.Length == 10 && (cleanISBN[i] == 'X' || cleanISBN[i] == 'x'))
                continue;
            
            if (!char.IsDigit(cleanISBN[i]))
            {
                errorMessage = "ISBN должен содержать только цифры и дефисы\nПример: 978-5-17-123456-7";
                return false;
            }
        }

        // Проверяем формат с дефисами
        if (isbn.Contains("-"))
        {
            var parts = isbn.Split('-');
            
            if (cleanISBN.Length == 13)
            {
                if (parts.Length < 4)
                {
                    errorMessage = "Неверный формат ISBN-13\nПример: 978-5-17-123456-7";
                    return false;
                }
            }
            else if (cleanISBN.Length == 10)
            {
                if (parts.Length < 4)
                {
                    errorMessage = "Неверный формат ISBN-10\nПример: 5-17-123456-7";
                    return false;
                }
            }
        }

        return true;
    }

    private async System.Threading.Tasks.Task ShowError(string message)
    {
        var errorWindow = new Window
        {
            Title = "Ошибка валидации",
            Width = 380,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
        panel.Children.Add(new TextBlock 
        { 
            Text = message, 
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Foreground = Avalonia.Media.Brushes.Red,
            FontSize = 14
        });

        var okButton = new Button 
        { 
            Content = "OK", 
            Width = 80,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        okButton.Click += (s, e) => errorWindow.Close();
        
        panel.Children.Add(okButton);
        errorWindow.Content = panel;

        await errorWindow.ShowDialog(this);
    }

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        // Валидация названия
        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            await ShowError("Название книги не может быть пустым");
            return;
        }

        // Валидация автора
        if (AuthorComboBox.SelectedItem is not Author selectedAuthor)
        {
            await ShowError("Необходимо выбрать автора");
            return;
        }

        // Валидация жанра
        if (GenreComboBox.SelectedItem is not Genre selectedGenre)
        {
            await ShowError("Необходимо выбрать жанр");
            return;
        }

        // Валидация года
        int year = (int)(YearNumeric.Value ?? DateTime.Now.Year);
        if (!ValidateYear(year, out string yearError))
        {
            await ShowError(yearError);
            return;
        }

        // Валидация ISBN
        string isbn = ISBNTextBox.Text ?? string.Empty;
        if (!ValidateISBN(isbn, out string isbnError))
        {
            await ShowError(isbnError);
            return;
        }

        // Проверка количества
        int quantity = (int)(QuantityNumeric.Value ?? 0);
        if (quantity < 0)
        {
            await ShowError("Количество не может быть отрицательным");
            return;
        }

        // Всё ОК - создаём/обновляем книгу
        var book = new Book
        {
            Id = _book?.Id ?? 0,
            Title = TitleTextBox.Text.Trim(),
            AuthorId = selectedAuthor.Id,
            GenreId = selectedGenre.Id,
            PublishYear = year,
            ISBN = isbn.Trim(),
            QuantityInStock = quantity
        };

        Close(book);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}