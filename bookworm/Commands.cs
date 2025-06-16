using System.Threading.Tasks;
using Serilog;
using Spectre.Console;

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

        try
        {

            await bookwormService.AddBookAsync(title, category, read, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error($"{ex.Message}", ex);
            return;
        }

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

        try
        {
            await bookwormService.RemoveBookAsync(title, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error($"{ex.Message}", ex);
            return;
        }
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
            Helper.ShowMessage(MessageType.Warning, ["No books found."]);
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

    public async Task OnHandleInteractiveMode(CancellationToken cancellationToken = default)
    {
        bool isRunning = true;

        while (isRunning)
        {
            AnsiConsole.Write(new FigletText("Book Worms").Centered().Color(Color.Gold1));

            var selectedMenu = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[blue]Your choise?[/]")
                .AddChoices([
                    "Add a new book",
                    "Remove book",
                    "List",
                    "Export",
                    "Import",
                    "Exit"
                ])
            );

            switch (selectedMenu)
            {
                case "Add a new book":
                    var title = AnsiConsole.Prompt(
                        new TextPrompt<string>("Please provide the title of book")
                    );
                    var category = AnsiConsole.Prompt(
                        new TextPrompt<string>("Please provide the category of book")
                        .AddChoices(Constants.Categories)
                    );
                    var read = AnsiConsole.Prompt(new TextPrompt<bool>("did you read?"));
                    await OnHandleAddCommand(title, category, read, cancellationToken);
                    break;
                case "Remove book":
                    var removeTitle = AnsiConsole.Prompt(
                        new TextPrompt<string>("Please provide the title of book")
                    );
                    await OnHandleRemoveCommand(removeTitle, cancellationToken);
                    break;
                case "List":
                    await OnHandleListCommand(cancellationToken);
                    break;
                case "Export":
                    var outputFile = AnsiConsole.Prompt(
                        new TextPrompt<string>("Please provide the output file name (default:'books.json')")
                        .DefaultValue("books.json")
                    );
                    await OnHandleExportCommand(outputFile, cancellationToken);
                    break;
                case "Import":
                    var inputFile = AnsiConsole.Prompt(
                           new TextPrompt<string>("Please provide the input file name (default:'books.json')")
                           .DefaultValue("books.json")
                       );
                    await OnHandleImportCommand(inputFile, cancellationToken);
                    break;
                case "Exit":
                    isRunning = false;
                    break;
            }
        }
    }
}
