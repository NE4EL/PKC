using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using System.Linq;

namespace LibraryManagement.Views;

public partial class AuthorManagementWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private DataGrid? _authorsDataGrid;
    private Author? _selectedAuthor;

    public AuthorManagementWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        
        // Находим DataGrid
        _authorsDataGrid = this.GetVisualDescendants().OfType<DataGrid>().FirstOrDefault();
        
        if (_authorsDataGrid != null)
        {
            _authorsDataGrid.ItemsSource = _viewModel.Authors;
        }
    }

    private void AuthorsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid grid)
        {
            _authorsDataGrid = grid;
            _selectedAuthor = grid.SelectedItem as Author;
        }
    }

    private async void Add_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new AuthorEditWindow();
        var result = await dialog.ShowDialog<Author?>(this);
        if (result != null)
        {
            _viewModel.AddAuthor(result);
            if (_authorsDataGrid != null)
                _authorsDataGrid.ItemsSource = _viewModel.Authors;
        }
    }

    private async void Edit_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedAuthor == null) return;

        var dialog = new AuthorEditWindow(_selectedAuthor);
        var result = await dialog.ShowDialog<Author?>(this);
        if (result != null)
        {
            _viewModel.UpdateAuthor(result);
            if (_authorsDataGrid != null)
                _authorsDataGrid.ItemsSource = _viewModel.Authors;
        }
    }

    private void Delete_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedAuthor != null)
        {
            _viewModel.DeleteAuthor(_selectedAuthor);
            if (_authorsDataGrid != null)
                _authorsDataGrid.ItemsSource = _viewModel.Authors;
        }
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}