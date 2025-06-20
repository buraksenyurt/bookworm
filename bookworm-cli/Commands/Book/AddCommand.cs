using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using bookworm_cli;
using Serilog;
using Services;

namespace Commands.Book;

public class AddCommand
    : Command
{
    private readonly IBookwormService _bookwormService;
    private readonly IMessageWriter _messageWriter;

    private readonly Option<string> titleOption = new(
            ["--title", "-t"],
            "The title of the book to add"
        )
    {
        IsRequired = true,
    };

    private readonly Option<string> categoryOption = new(
            ["--category", "-c"],
            "The category of the book (optional)"
        )
    {
        IsRequired = false,
    };

    private readonly Option<bool> readOption = new(
            ["--read", "-r"],
            "Indicates if the book has been read (default is false)"
        )
    {
        IsRequired = false,
    };

    private void SetupOptions()
    {
        titleOption.AddValidator(result =>
       {
           var title = result.GetValueForOption(titleOption);
           if (string.IsNullOrWhiteSpace(title))
           {
               result.ErrorMessage = "Title cannot be null or empty.";
           }
           else if (title.Length > 50)
           {
               result.ErrorMessage = "Title cannot exceed 50 characters.";
           }
       });

        categoryOption.SetDefaultValue("Uncategorized");
        categoryOption.FromAmong(Constants.Categories);
        categoryOption.AllowMultipleArgumentsPerToken = true;
        categoryOption.AddCompletions(Constants.Categories);

        readOption.SetDefaultValue(false);
    }

    public AddCommand(IBookwormService bookwormService,IMessageWriter messageWriter, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;
        _messageWriter = messageWriter;

        AddOption(titleOption);
        AddOption(categoryOption);
        AddOption(readOption);

        Handler = CommandHandler.Create<string, string, bool, InvocationContext>(
        async (title, category, read, ctx) =>
        {
            await OnHandleAddCommand(title, category, read, ctx.GetCancellationToken());
        });

        SetupOptions();
    }

    private async Task OnHandleAddCommand(string title, string category, bool read, CancellationToken cancellationToken = default)
    {
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
    }
}
