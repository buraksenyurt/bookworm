namespace bookworm;

public class BookwormService
{
    private readonly List<Book> _books = [];

    public void AddBook(string title, string library, string shelf, string order)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Helper.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
            return;
        }

        if (!int.TryParse(library, out var libraryId))
        {
            Helper.ShowMessage(MessageType.Error, ["Invalid library ID."]);
            return;
        }

        if (!int.TryParse(shelf, out var shelfId))
        {
            Helper.ShowMessage(MessageType.Error, ["Invalid shelf ID."]);
            return;
        }

        if (!int.TryParse(order, out var orderId))
        {
            Helper.ShowMessage(MessageType.Error, ["Invalid order ID."]);
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
            Library = libraryId,
            Shelf = shelfId,
            Order = orderId
        };

        _books.Add(book);
        Helper.ShowMessage(MessageType.Info, ["Book added successfully."]);
    }

    public IEnumerable<Book> ListBooks()
    {
        throw new NotImplementedException();
    }

    public void RemoveBook(string title)
    {
        throw new NotImplementedException();
    }
}
