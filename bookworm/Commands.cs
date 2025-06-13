namespace bookworm;

public static class Commands
{
    private static readonly BookwormService _bookwormService = new();
    public static void OnHandleAddCommand(string title, string category, bool read)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Helper.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
            return;
        }

        if (title.Length > 50)
        {
            Helper.ShowMessage(MessageType.Error, ["Title cannot exceed 50 characters."]);
            return;
        }

        _bookwormService.AddBook(title, category, read);

        Helper.ShowMessage(MessageType.Info, ["Book added successfully."]);
    }
    public static void OnHandleRemoveCommand(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
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
            Helper.ShowMessage(MessageType.Info, ["No books found."]);
        }
    }

    public static async Task OnHandleExportCommand(string filePath)
    {
        try
        {
            await _bookwormService.ExportBooksAsync(filePath);
            Helper.ShowMessage(MessageType.Info, ["Books exported successfully."]);
        }
        catch (Exception ex)
        {
            Helper.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }

    public static async Task OnHandleImportCommand(string filePath)
    {
        try
        {
            await _bookwormService.ImportBooksAsync(filePath);
            Helper.ShowMessage(MessageType.Info, ["Books imported successfully."]);
        }
        catch (Exception ex)
        {
            Helper.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }
}
