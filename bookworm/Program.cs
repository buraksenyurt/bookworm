namespace bookworm;

class Program
{
    static void Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Helper.ShowMessage(MessageType.Error,
            ["No arguments provided. Please provide the path to the bookworm service.",
            "Available commands: add, remove, list.",
            "Usage: bookworm <command-name> <parameters>",
                "Example: bookworm add 'Book Title' 1 2 5",
                "Example: bookworm remove 'Book Title'",
                "Example: bookworm list"]);
            return;
        }

        var service = new BookwormService();
        ManageCommand(args, service);
    }

    static void ManageCommand(string[] args, BookwormService service)
    {
        var command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "add":
                if (args.Length < 5)
                {
                    Helper.ShowMessage(MessageType.Error
                    , ["Insufficient parameters for 'add' command."]);
                    break;
                }
                service.AddBook(args[1],
                    args[2],
                    args[3],
                    args[4]
                );
                break;
            case "remove":
                if (args.Length < 2)
                {
                    Helper.ShowMessage(MessageType.Error
                    , ["Insufficient parameters for 'remove' command."]);
                    break;
                }
                service.RemoveBook(args[1]);
                break;
            case "list":
                var books = service.ListBooks();
                if (!books.Any())
                {
                    Helper.ShowMessage(MessageType.Info, ["No books found."]);
                }
                else
                {
                    foreach (var book in books)
                    {
                        Console.WriteLine($"Title: {book.Title}, Library: {book.Library}, Shelf: {book.Shelf}, Order: {book.Order}");
                    }
                }
                break;
            default:
                Helper.ShowMessage(MessageType.Error
                , ["Unknown command. Available commands: add, remove, list."]);
                break;
        }
    }
}
