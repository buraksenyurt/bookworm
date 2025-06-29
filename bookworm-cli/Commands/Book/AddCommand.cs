using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using bookworm_cli;
using Serilog;
using Services;

namespace Commands.Book;

public class AddCommand
    : Command
{
    private readonly IBookwormService _bookwormService;
    private readonly INotifier _notifier;

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

    private readonly Option<string> tokenOption = new(
        ["--token", "-tk"], "Add a book with a user token (optional)"
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

    public AddCommand(IBookwormService bookwormService, INotifier notifier, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;
        _notifier = notifier;

        AddOption(titleOption);
        AddOption(categoryOption);
        AddOption(readOption);
        AddOption(tokenOption);

        Handler = CommandHandler.Create<string, string, bool, string, InvocationContext>(
        async (title, category, read, token, ctx) =>
        {
            await OnHandleAddCommand(title, category, read, token, ctx.GetCancellationToken());
        });

        SetupOptions();
    }

    private async Task OnHandleAddCommand(string title, string category, bool read, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Log.Error(Messages.ValidationMessages.TitleCannotBeEmpty);
            _notifier.ShowMessage(MessageType.Error, [Messages.ValidationMessages.TitleCannotBeEmpty]);
            return;
        }

        if (title.Length > 50)
        {
            Log.Error(Messages.ValidationMessages.TitleExceedsMaxLength);
            _notifier.ShowMessage(MessageType.Error, [Messages.ValidationMessages.TitleExceedsMaxLength]);
            return;
        }

        try
        {
            Guid? userToken = null;
            if (Guid.TryParse(token, out var parsedToken))
            {
                userToken = parsedToken;
            }
            var result = false;
            if (userToken.HasValue)
            {
                result = await _bookwormService.AddBookAsync(title, category, read, userToken.Value, cancellationToken);
            }
            else
            {
                result = await _bookwormService.AddBookAsync(title, category, read, cancellationToken);
            }

            if (result)
            {
                Log.Information("Book '{Title}' added successfully.", title);
                _notifier.ShowMessage(MessageType.Info, [Messages.AddCommandMessages.BookAddedSuccessfully]);
            }
            else
            {
                Log.Warning("Book '{Title}' could not be added.", title);
                _notifier.ShowMessage(MessageType.Warning, [Messages.AddCommandMessages.BookCouldNotBeAdded]);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"{ex.Message}", ex);
            return;
        }
    }
}
