using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using Serilog;

namespace bookworm.Commands.Book;

public class RemoveCommand
    : Command
{
    private readonly BookwormService _bookwormService;
    private Option<string> titleOption = new(
            ["--title", "-t"],
            "The title of the book to remove"
        );
    public RemoveCommand(BookwormService bookwormService, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;

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
            Log.Error("Title cannot be null or empty.");
            Helper.ShowMessage(MessageType.Error, ["Title cannot be null or empty."]);
            return;
        }

        try
        {
            await _bookwormService.RemoveBookAsync(title, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error($"{ex.Message}", ex);
            return;
        }
    }
}
