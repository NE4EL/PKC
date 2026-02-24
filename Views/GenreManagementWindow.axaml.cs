using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using System.Linq;

namespace LibraryManagement.Views;

public partial class GenreManagementWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private DataGrid? _genresDataGrid;
    private Genre? _selectedGenre;

    public GenreManagementWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        
        // Находим DataGrid
        _genresDataGrid = this.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();
        
        if (_genresDataGrid != null)
        {
            _genresDataGrid.ItemsSource = _viewModel.Genres;
        }
    }

    private void GenresDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid grid)
        {
            _genresDataGrid = grid;
            _selectedGenre = grid.SelectedItem as Genre;
        }
    }

    private async void Add_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new GenreEditWindow();
        var result = await dialog.ShowDialog<Genre?>(this);
        if (result != null)
        {
            _viewModel.AddGenre(result);
            if (_genresDataGrid != null)
                _genresDataGrid.ItemsSource = _viewModel.Genres;
        }
    }

    private async void Edit_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedGenre == null) return;

        var dialog = new GenreEditWindow(_selectedGenre);
        var result = await dialog.ShowDialog<Genre?>(this);
        if (result != null)
        {
            _viewModel.UpdateGenre(result);
            if (_genresDataGrid != null)
                _genresDataGrid.ItemsSource = _viewModel.Genres;
        }
    }

    private void Delete_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedGenre != null)
        {
            _viewModel.DeleteGenre(_selectedGenre);
            if (_genresDataGrid != null)
                _genresDataGrid.ItemsSource = _viewModel.Genres;
        }
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}