using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using bookworm_cli;
using Serilog;
using Services;

namespace Commands.Book;

public class ListCommand
    : Command
{
    private readonly IBookwormService _bookwormService;
    private readonly IMessageWriter _messageWriter;
    public ListCommand(IBookwormService bookwormService, IMessageWriter messageWriter, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;
        _messageWriter = messageWriter;

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
            _messageWriter.ShowMessage(MessageType.Info, [books.Count().ToString(), Messages.ListCommandMessages.BooksFound]);
            foreach (var book in books)
            {
                var readStatus = book.Read ? "Read" : "Unread";
                Console.WriteLine($"Title: {book.Title}, Category: {book.Category}, Status: {readStatus}");
            }
        }
        else
        {
            Log.Information("No books found.");
            _messageWriter.ShowMessage(MessageType.Warning, [Messages.ListCommandMessages.NoBooksFound]);
        }
    }
}
