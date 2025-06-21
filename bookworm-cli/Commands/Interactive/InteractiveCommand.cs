using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using bookworm_cli;
using Serilog;
using Services;
using Spectre.Console;

namespace Commands.Interactive;

public class InteractiveCommand
    : Command
{
    private readonly IBookwormService _bookwormService;
    private readonly IMessageWriter _messageWriter;
    public InteractiveCommand(IBookwormService bookwormService, IMessageWriter messageWriter, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;
        _messageWriter = messageWriter;

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
                        _messageWriter.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
                        return;
                    }

                    if (title.Length > 50)
                    {
                        Log.Error("Title cannot exceed 50 characters.");
                        _messageWriter.ShowMessage(MessageType.Error, ["Title cannot exceed 50 characters."]);
                        return;
                    }

                    try
                    {

                        var result = await _bookwormService.AddBookAsync(title, category, read, cancellationToken);
                        if (result)
                        {
                            Log.Information("Book '{Title}' added successfully.", title);
                            _messageWriter.ShowMessage(MessageType.Info, ["Book added successfully."]);
                        }
                        else
                        {
                            Log.Warning("Book '{Title}' could not be added.", title);
                            _messageWriter.ShowMessage(MessageType.Warning, ["Book could not be added."]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{ex.Message}", ex);
                        return;
                    }
                    break;
                case "Remove book":
                    var removeTitle = AnsiConsole.Prompt(
                        new TextPrompt<string>("Please provide the title of book")
                    );
                    if (string.IsNullOrWhiteSpace(removeTitle))
                    {
                        Log.Error("Title cannot be null or empty.");
                        _messageWriter.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
                        return;
                    }

                    try
                    {
                        var result = await _bookwormService.RemoveBookAsync(removeTitle, cancellationToken);
                        if (result)
                        {
                            Log.Information("Book '{Title}' removed successfully.", removeTitle);
                            _messageWriter.ShowMessage(MessageType.Info, ["Book removed successfully."]);
                        }
                        else
                        {
                            Log.Warning("Book '{Title}' could not be removed.", removeTitle);
                            _messageWriter.ShowMessage(MessageType.Warning, ["Book could not be removed."]);
                        }
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
                        _messageWriter.ShowMessage(MessageType.Warning, ["No books found."]);
                    }
                    break;
                case "Export":
                    var outputFile = AnsiConsole.Prompt(
                        new TextPrompt<string>("Please provide the output file name (default:'books.json')")
                        .DefaultValue("books.json")
                    );
                    try
                    {
                        var result = await _bookwormService.ExportBooksAsync(outputFile, cancellationToken);
                        if (result > 0)
                        {
                            Log.Information("Books exported successfully to {outputFile}.", outputFile);
                            _messageWriter.ShowMessage(MessageType.Info, ["Books imported successfully."]);
                        }
                        else
                        {
                            Log.Warning("No books could be exported.");
                            _messageWriter.ShowMessage(MessageType.Warning, ["No books could be exported."]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error exporting books to {outputFile}.", outputFile);
                        _messageWriter.ShowMessage(MessageType.Error, [ex.Message]);
                    }
                    break;
                case "Import":
                    var inputFile = AnsiConsole.Prompt(
                           new TextPrompt<string>("Please provide the input file name (default:'books.json')")
                           .DefaultValue("books.json")
                       );
                    try
                    {
                        var result = await _bookwormService.ImportBooksAsync(inputFile, cancellationToken);
                        if (result > 0)
                        {
                            Log.Information($"'{result}' books imported successfully from {inputFile}.");
                            _messageWriter.ShowMessage(MessageType.Info, ["Books imported successfully."]);
                        }
                        else
                        {
                            Log.Warning("No books could be added.");
                            _messageWriter.ShowMessage(MessageType.Warning, ["No books imported successfully."]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error importing books from {inputFile}.", inputFile);
                        _messageWriter.ShowMessage(MessageType.Error, [ex.Message]);
                    }
                    break;
                case "Exit":
                    isRunning = false;
                    break;
            }
        }
    }
}
