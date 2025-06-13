using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace bookworm;

class Program
{
    private static readonly BookwormService _bookwormService = new();
    static async Task<int> Main(string[] args)
    {
        var rootCmd = new RootCommand("Bookworm CLI - Manage your book collection")
        {
        };

        var addCommand = new Command("add", "Add a new book")
        {
        };
        rootCmd.AddCommand(addCommand);
        var titleOption = new Option<string>(
            ["--title", "-t"],
            "The title of the book to add"
        );
        titleOption.IsRequired = true;
        addCommand.AddOption(titleOption);
        var categoryOption = new Option<string>(
            ["--category", "-c"],
            "The category of the book (optional)"
        )
        {
            IsRequired = false
        };
        addCommand.AddOption(categoryOption);
        var readOption = new Option<bool>(
            ["--read", "-r"],
            "Indicates if the book has been read (default is false)"
        )
        {
            IsRequired = false,
        };
        addCommand.AddOption(readOption);
        readOption.SetDefaultValue(false);

        addCommand.SetHandler(_bookwormService.AddBook, titleOption, categoryOption, readOption);

        var listCommand = new Command("list", "List all books")
        {
        };
        rootCmd.AddCommand(listCommand);
        listCommand.SetHandler(() =>
        {
            var books = _bookwormService.ListBooks();
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
        });
        var removeCommand = new Command("remove", "Remove a book")
        {
        };
        rootCmd.AddCommand(removeCommand);
        var removeTitleOption = new Option<string>(
            ["--title", "-t"],
            "The title of the book to remove"
        );
        removeCommand.AddOption(removeTitleOption);
        removeCommand.SetHandler(_bookwormService.RemoveBook, removeTitleOption);

        var parser = new CommandLineBuilder(rootCmd)
            .UseDefaults()
            .Build();

        return await parser.InvokeAsync(args);
    }
}
