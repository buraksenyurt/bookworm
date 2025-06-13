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
        var command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "add":
                if (args.Length < 5)
                {
                    Helper.ShowMessage(MessageType.Error
                    , ["Insufficient parameters for 'add' command."]);
                    return;
                }
                service.AddBook(new Book
                {
                    Title = args[1],
                    Library = int.Parse(args[2]),
                    Shelf = int.Parse(args[3]),
                    Order = int.Parse(args[4])
                });
                break;
            case "remove":
                if (args.Length < 2)
                {
                    Helper.ShowMessage(MessageType.Error
                    , ["Insufficient parameters for 'remove' command."]);
                    return;
                }
                service.RemoveBook(args[1]);
                break;
            case "list":
                var books = service.ListBooks();
                if (books.Count() == 0)
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
