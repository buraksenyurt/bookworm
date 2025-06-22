using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using bookworm_cli;
using Serilog;
using Services;

namespace Commands.Book;

public class RemoveCommand
    : Command
{
    private readonly IBookwormService _bookwormService;
    private readonly IMessageWriter _messageWriter;

    private readonly Option<string> titleOption = new(
            ["--title", "-t"],
            "The title of the book to remove"
        );
    public RemoveCommand(IBookwormService bookwormService, IMessageWriter messageWriter, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;
        _messageWriter = messageWriter;

        AddOption(titleOption);

        Handler = CommandHandler.Create<string, InvocationContext>(
        async (title, ctx) =>
        {
            await OnHandleRemoveCommand(title, ctx.GetCancellationToken());
        });
    }

    private async Task OnHandleRemoveCommand(string title, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Log.Error(Messages.ValidationMessages.TitleCannotBeEmpty);
            _messageWriter.ShowMessage(MessageType.Error, [Messages.ValidationMessages.TitleCannotBeEmpty]);
            return;
        }

        try
        {
            var result = await _bookwormService.RemoveBookAsync(title, cancellationToken);

            if (result)
            {
                Log.Information("Book '{Title}' removed successfully.", title);
                _messageWriter.ShowMessage(MessageType.Info, [Messages.RemoveCommandMessages.BookRemovedSuccessfully]);
            }
            else
            {
                Log.Warning("Book '{Title}' could not be removed.", title);
                _messageWriter.ShowMessage(MessageType.Warning, [Messages.RemoveCommandMessages.BookRemovedSuccessfully]);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"{ex.Message}", ex);
            return;
        }
    }
}
