using System.Text.Json;

namespace bookworm;

public class BookwormService
{
    private readonly List<Book> _books = [];

    public void AddBook(string title, string category, bool read)
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
            Category = category ?? "Uncategorized",
            Read = read
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

        return _books.OrderBy(b => b.Title)
                     .ThenBy(b => b.Category)
                     .ThenBy(b => b.Read);
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

    public async Task ExportBooksAsync(string fileName)
    {
        try
        {
            var json = JsonSerializer.Serialize(_books);
            await File.WriteAllTextAsync(fileName, json);
        }
        catch (Exception ex)
        {
            Helper.ShowMessage(MessageType.Error, [ex.Message]);
        }
        
    }
}
