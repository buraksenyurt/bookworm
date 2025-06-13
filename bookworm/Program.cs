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
        addCommand.AddOption(titleOption);
        var libraryOption = new Option<uint>(
            ["--library", "-l"],
            "The library ID where the book is located"
        );
        addCommand.AddOption(libraryOption);
        var shelfOption = new Option<uint>(
            ["--shelf", "-s"],
            "The shelf ID where the book is located"
        );
        addCommand.AddOption(shelfOption);
        var orderOption = new Option<uint>(
            ["--order", "-o"],
            "The order ID of the book on the shelf"
        );
        addCommand.AddOption(orderOption);
        addCommand.SetHandler(_bookwormService.AddBook, titleOption, libraryOption, shelfOption, orderOption);

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
                    Console.WriteLine($"Title: {book.Title}, Library: {book.Library}, Shelf: {book.Shelf}, Order: {book.Order}");
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
