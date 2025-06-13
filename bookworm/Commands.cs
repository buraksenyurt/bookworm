using Serilog;

namespace bookworm;

public static class Commands
{
    private static readonly BookwormService _bookwormService = new();
    public static void OnHandleAddCommand(string title, string category, bool read)
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

        _bookwormService.AddBook(title, category, read);

        Log.Information("Book '{Title}' added successfully.", title);
        Helper.ShowMessage(MessageType.Info, ["Book added successfully."]);
    }
    public static void OnHandleRemoveCommand(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Log.Error("Title cannot be null or empty.");
            Helper.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
            return;
        }

        _bookwormService.RemoveBook(title);
    }

    public static void OnHandleListCommand()
    {
        var books = _bookwormService.GetAllBooks();
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

    public static async Task OnHandleExportCommand(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await _bookwormService.ExportBooksAsync(filePath, cancellationToken);
            Log.Information("Books exported successfully to {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Info, ["Books exported successfully."]);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error exporting books to {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }

    public static async Task OnHandleImportCommand(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await _bookwormService.ImportBooksAsync(filePath, cancellationToken);
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
