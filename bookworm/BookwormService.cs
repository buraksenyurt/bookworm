namespace bookworm;

public class BookwormService
{
    private readonly List<Book> _books = [];

    public void AddBook(string title, uint library, uint shelf, uint order)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Helper.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
            return;
        }

        if (_books.Any(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
        {
            Helper.ShowMessage(MessageType.Warning, ["A book with this title already exists."]);
            return;
        }

        var book = new Book
        {
            Title = title,
            Library = library,
            Shelf = shelf,
            Order = order
        };

        _books.Add(book);
        Helper.ShowMessage(MessageType.Info, ["Book added successfully."]);
    }

    public IEnumerable<Book> ListBooks()
    {
        if (_books.Count == 0)
        {
            return [];
        }

        return _books.OrderBy(b => b.Library).ThenBy(b => b.Shelf).ThenBy(b => b.Order);

    }

    public void RemoveBook(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Helper.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
            return;
        }

        var bookToRemove = _books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        if (bookToRemove == null)
        {
            Helper.ShowMessage(MessageType.Warning, ["No book found with the specified title."]);
            return;
        }

        _books.Remove(bookToRemove);
        Helper.ShowMessage(MessageType.Info, ["Book removed successfully."]);
    }
}
