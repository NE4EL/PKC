using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryManagement.Models;

namespace LibraryManagement.Views;

public partial class GenreEditWindow : Window
{
    private Genre? _genre;

    public GenreEditWindow(Genre? genre = null)
    {
        InitializeComponent();
        _genre = genre;

        if (genre != null)
        {
            NameTextBox.Text = genre.Name;
            DescriptionTextBox.Text = genre.Description;
        }
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            return;
        }

        var genre = _genre ?? new Genre();
        genre.Name = NameTextBox.Text;
        genre.Description = DescriptionTextBox.Text ?? string.Empty;

        Close(genre);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}