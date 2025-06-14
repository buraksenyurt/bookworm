using System.Text.Json;
using bookworm.Client;
using Serilog;

namespace bookworm;

public class BookwormService(IBookwormApiClient apiClient)
{
    public async Task AddBookAsync(string title, string category, bool read, CancellationToken cancellationToken)
    {
        // if (_books.Any(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
        // {
        //     Log.Warning("A book with the title '{Title}' already exists.", title);
        //     Helper.ShowMessage(MessageType.Warning, ["A book with this title already exists."]);
        //     return;
        // }

        var book = new Book
        {
            Title = title,
            Category = category ?? "Uncategorized",
            Read = read
        };

        await apiClient.AddAsync(book, cancellationToken);
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken)
    {
        // if (_books.Count == 0)
        // {
        //     return [];
        // }

        // return _books.OrderBy(b => b.Title)
        //              .ThenBy(b => b.Category)
        //              .ThenBy(b => b.Read);
        return await apiClient.GetAllAsync(cancellationToken);
    }

    public async Task RemoveBookAsync(string title, CancellationToken cancellationToken)
    {
        // var bookToRemove = _books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        // if (bookToRemove == null)
        // {
        //     Log.Warning("No book found with the title '{Title}'.", title);
        //     Helper.ShowMessage(MessageType.Warning, ["No book found with the specified title."]);
        //     return;
        // }

        // _books.Remove(bookToRemove);
        // Log.Information("Book '{Title}' removed successfully.", title);
        await apiClient.RemoveAsync(title, cancellationToken);
        Helper.ShowMessage(MessageType.Info, ["Book removed successfully."]);
    }

    public async Task ExportBooksAsync(string fileName, CancellationToken cancellationToken)
    {
        var books = await apiClient.GetAllAsync(cancellationToken);
        if (!books.Any())
        {
            Log.Information("There is no books in store for exporting");
            Helper.ShowMessage(MessageType.Warning, ["There is no books in store for exporting"]);
            return;
        }
        var json = JsonSerializer.Serialize(books);
        await File.WriteAllTextAsync(fileName, json, cancellationToken);
    }

    public async Task ImportBooksAsync(string fileName, CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(fileName, cancellationToken);
        var importedBooks = JsonSerializer.Deserialize<List<Book>>(json);

        if (importedBooks == null || importedBooks.Count == 0)
        {
            Log.Warning("No books found in the file '{FileName}'.", fileName);
            Helper.ShowMessage(MessageType.Warning, ["No books found in the file."]);
            return;
        }

        foreach (var book in importedBooks)
        {
            await AddBookAsync(book.Title, book.Category, book.Read, cancellationToken);
        }
    }
}
