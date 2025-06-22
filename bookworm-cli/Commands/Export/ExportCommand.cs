using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using bookworm_cli;
using Serilog;
using Services;

namespace Commands.Export;

public class ExportCommand
    : Command
{
    private readonly IBookwormService _bookwormService;
    private readonly INotifier _notifier;

    private readonly Option<string> exportFileOption = new(
            ["--file", "-f"],
            "The file path to export the books to json format (default is 'books.json')"
        )
    {
        IsRequired = true,
    };

    private void SetupOptions()
    {
        exportFileOption.LegalFileNamesOnly();
        exportFileOption.SetDefaultValue("books.json");
        exportFileOption.AddValidator(result =>
        {
            var filePath = result.GetValueForOption(exportFileOption);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                result.ErrorMessage = Messages.ValidationMessages.FilePathCannotBeEmpty;
            }
            else if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                result.ErrorMessage = Messages.ValidationMessages.FileMustHaveJsonExtension;
            }
        });
    }
    public ExportCommand(IBookwormService bookwormService, INotifier notifier, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;
        _notifier = notifier;

        AddOption(exportFileOption);
        SetupOptions();

        Handler = CommandHandler.Create<string, InvocationContext>(
        async (file, ctx) =>
        {
            await OnHandleExportCommand(file, ctx.GetCancellationToken());
        });
    }

    private async Task OnHandleExportCommand(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _bookwormService.ExportBooksAsync(filePath, cancellationToken);
            if (result > 0)
            {
                Log.Information("Books exported successfully to {filePath}.", filePath);
                _notifier.ShowMessage(MessageType.Info, [Messages.ExportCommandMessages.ExportedToFile]);
            }
            else
            {
                Log.Warning(Messages.ExportCommandMessages.NoBooksExported);
                _notifier.ShowMessage(MessageType.Warning, [Messages.ExportCommandMessages.NoBooksExported]);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error exporting books to {FilePath}.", filePath);
            _notifier.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }
}
