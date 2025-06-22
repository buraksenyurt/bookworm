using System.Text.Json;
using bookworm_cli;
using Client;
using Serilog;

namespace Services;

public class BookwormService(IBookwormApiClient apiClient, IMessageWriter messageWriter)
    : IBookwormService
{
    public async Task<bool> AddBookAsync(string title, string category, bool read, CancellationToken cancellationToken)
    {
        var book = new Book
        {
            Title = title,
            Category = category ?? "Uncategorized",
            Read = read
        };

        return await apiClient.AddAsync(book, cancellationToken);
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken)
    {
        return await apiClient.GetAllAsync(cancellationToken);
    }

    public async Task<bool> RemoveBookAsync(string title, CancellationToken cancellationToken)
    {
        return await apiClient.RemoveAsync(title, cancellationToken);
    }

    public async Task<int> ExportBooksAsync(string fileName, CancellationToken cancellationToken)
    {
        var books = await apiClient.GetAllAsync(cancellationToken);
        if (!books.Any())
        {
            Log.Information(Messages.ExportCommandMessages.ThereIsNoBooksToExport);
            messageWriter.ShowMessage(MessageType.Warning, [Messages.ExportCommandMessages.ThereIsNoBooksToExport]);
            return byte.MinValue;
        }
        var json = JsonSerializer.Serialize(books);
        await File.WriteAllTextAsync(fileName, json, cancellationToken);
        return books.Count();
    }

    public async Task<int> ImportBooksAsync(string fileName, CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(fileName, cancellationToken);
        var importedBooks = JsonSerializer.Deserialize<List<Book>>(json);

        if (importedBooks == null || importedBooks.Count == 0)
        {
            Log.Warning("{NoBooks} '{FileName}'.", Messages.ImportCommandMessages.NoBooksFoundInFile, fileName);
            messageWriter.ShowMessage(MessageType.Warning, [Messages.ImportCommandMessages.NoBooksFoundInFile]);
            return byte.MinValue;
        }
        var importedCounter = 0;
        foreach (var book in importedBooks)
        {
            if (await AddBookAsync(book.Title, book.Category, book.Read, cancellationToken))
            {
                importedCounter++;
            }
        }
        return importedCounter;
    }
}
