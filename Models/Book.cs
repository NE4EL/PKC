using System.Collections.Generic;
using System.Linq;

namespace LibraryManagement.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PublishYear { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }

    public ICollection<Author> Authors { get; set; } = new List<Author>();
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();

    public string AuthorsDisplay =>
        Authors != null && Authors.Count > 0
            ? string.Join(", ", Authors.Select(a => $"{a.FirstName} {a.LastName}"))
            : "—";

    public string GenresDisplay =>
        Genres != null && Genres.Count > 0
            ? string.Join(", ", Genres.Select(g => g.Name))
            : "—";
}