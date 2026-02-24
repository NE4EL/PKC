using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryManagement.Models;
using System;

namespace LibraryManagement.Views;

public partial class AuthorEditWindow : Window
{
    private Author? _author;

    public AuthorEditWindow(Author? author = null)
    {
        InitializeComponent();
        _author = author;

        if (author != null)
        {
            FirstNameTextBox.Text = author.FirstName;
            LastNameTextBox.Text = author.LastName;
            BirthDatePicker.SelectedDate = author.BirthDate;
            CountryTextBox.Text = author.Country;
        }
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
            string.IsNullOrWhiteSpace(LastNameTextBox.Text))
        {
            return;
        }

        var author = _author ?? new Author();
        author.FirstName = FirstNameTextBox.Text;
        author.LastName = LastNameTextBox.Text;
        author.BirthDate = BirthDatePicker.SelectedDate;
        author.Country = CountryTextBox.Text ?? string.Empty;

        Close(author);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}