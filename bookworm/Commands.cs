using Serilog;

namespace bookworm;

public class Commands(BookwormService bookwormService)
{
    public async Task OnHandleAddCommand(string title, string category, bool read, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Log.Error("Title cannot be null or empty.");
            Helper.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
            return;
        }

        if (title.Length > 50)
        {
            Log.Error("Title cannot exceed 50 characters.");
            Helper.ShowMessage(MessageType.Error, ["Title cannot exceed 50 characters."]);
            return;
        }

        await bookwormService.AddBookAsync(title, category, read, cancellationToken);

        Log.Information("Book '{Title}' added successfully.", title);
        Helper.ShowMessage(MessageType.Info, ["Book added successfully."]);
    }
    public async Task OnHandleRemoveCommand(string title, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Log.Error("Title cannot be null or empty.");
            Helper.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
            return;
        }

        await bookwormService.RemoveBookAsync(title, cancellationToken);
    }

    public async Task OnHandleListCommand(CancellationToken cancellationToken = default)
    {
        var books = await bookwormService.GetAllBooksAsync(cancellationToken);
        if (books.Any())
        {
            foreach (var book in books)
            {
                var readStatus = book.Read ? "Read" : "Unread";
                Console.WriteLine($"Title: {book.Title}, Category: {book.Category}, Status: {readStatus}");
            }
        }
        else
        {
            Log.Information("No books found.");
            Helper.ShowMessage(MessageType.Info, ["No books found."]);
        }
    }

    public async Task OnHandleExportCommand(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await bookwormService.ExportBooksAsync(filePath, cancellationToken);
            Log.Information("Books exported successfully to {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Info, ["Books exported successfully."]);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error exporting books to {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }

    public async Task OnHandleImportCommand(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await bookwormService.ImportBooksAsync(filePath, cancellationToken);
            Log.Information("Books imported successfully from {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Info, ["Books imported successfully."]);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error importing books from {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }
}
