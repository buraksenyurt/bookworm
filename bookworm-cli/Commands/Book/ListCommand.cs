using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using bookworm_cli;
using Serilog;

namespace Commands.Book;

public class ListCommand
    : Command
{
    private readonly BookwormService _bookwormService;
    public ListCommand(BookwormService bookwormService, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;

        Handler = CommandHandler.Create<InvocationContext>(
        async ctx =>
        {
            await OnHandleListCommand(ctx.GetCancellationToken());
        });
    }

    private async Task OnHandleListCommand(CancellationToken cancellationToken = default)
    {
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
    }
}
