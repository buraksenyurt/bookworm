using System.Text.Json;

namespace bookworm;

public class BookwormService
{
    private readonly List<Book> _books = [];

    public void AddBook(string title, string category, bool read)
    {
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
    }

    public IEnumerable<Book> GetAllBooks()
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
        var json = JsonSerializer.Serialize(_books);
        await File.WriteAllTextAsync(fileName, json);
    }

    public async Task ImportBooksAsync(string fileName)
    {
        var json = await File.ReadAllTextAsync(fileName);
        var importedBooks = JsonSerializer.Deserialize<List<Book>>(json);

        if (importedBooks == null || importedBooks.Count == 0)
        {
            Helper.ShowMessage(MessageType.Warning, ["No books found in the file."]);
            return;
        }

        foreach (var book in importedBooks)
        {
            AddBook(book.Title, book.Category, book.Read);
        }
    }    
}
