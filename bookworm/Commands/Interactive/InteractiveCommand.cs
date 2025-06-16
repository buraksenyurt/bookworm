using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using Serilog;
using Spectre.Console;

namespace bookworm.Commands.Interactive;

public class InteractiveCommand
    : Command
{
    private readonly BookwormService _bookwormService;
    public InteractiveCommand(BookwormService bookwormService, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;

        Handler = CommandHandler.Create<InvocationContext>(
        async (ctx) =>
        {
            await OnHandleInteractiveMode(ctx.GetCancellationToken());
        });
    }

    private async Task OnHandleInteractiveMode(CancellationToken cancellationToken = default)
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
                        new SelectionPrompt<string>()
                        .Title("Please provide the category of book")
                        .AddChoices(Constants.Categories)
                    );
                    var asnwer = AnsiConsole.Prompt(
                        new SelectionPrompt<string>().Title("Did you read this book?")
                        .AddChoices("Yes", "No")
                    );
                    bool read = asnwer == "Yes";
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

                        await _bookwormService.AddBookAsync(title, category, read, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{ex.Message}", ex);
                        return;
                    }

                    Log.Information("Book '{Title}' added successfully.", title);
                    Helper.ShowMessage(MessageType.Info, ["Book added successfully."]);
                    break;
                case "Remove book":
                    var removeTitle = AnsiConsole.Prompt(
                        new TextPrompt<string>("Please provide the title of book")
                    );
                    if (string.IsNullOrWhiteSpace(removeTitle))
                    {
                        Log.Error("Title cannot be null or empty.");
                        Helper.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
                        return;
                    }

                    try
                    {
                        await _bookwormService.RemoveBookAsync(removeTitle, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{ex.Message}", ex);
                        return;
                    }
                    break;
                case "List":
                    var books = await _bookwormService.GetAllBooksAsync(cancellationToken);
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
                    break;
                case "Export":
                    var outputFile = AnsiConsole.Prompt(
                        new TextPrompt<string>("Please provide the output file name (default:'books.json')")
                        .DefaultValue("books.json")
                    );
                    try
                    {
                        await _bookwormService.ExportBooksAsync(outputFile, cancellationToken);
                        Log.Information("Books exported successfully to {outputFile}.", outputFile);
                        Helper.ShowMessage(MessageType.Info, ["Books exported successfully."]);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error exporting books to {outputFile}.", outputFile);
                        Helper.ShowMessage(MessageType.Error, [ex.Message]);
                    }
                    break;
                case "Import":
                    var inputFile = AnsiConsole.Prompt(
                           new TextPrompt<string>("Please provide the input file name (default:'books.json')")
                           .DefaultValue("books.json")
                       );
                    try
                    {
                        await _bookwormService.ImportBooksAsync(inputFile, cancellationToken);
                        Log.Information("Books imported successfully from {inputFile}.", inputFile);
                        Helper.ShowMessage(MessageType.Info, ["Books imported successfully."]);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error importing books from {inputFile}.", inputFile);
                        Helper.ShowMessage(MessageType.Error, [ex.Message]);
                    }
                    break;
                case "Exit":
                    isRunning = false;
                    break;
            }
        }
    }
}
