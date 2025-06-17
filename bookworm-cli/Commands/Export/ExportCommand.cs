using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using bookworm_cli;
using Serilog;

namespace Commands.Export;

public class ExportCommand
    : Command
{
    private readonly BookwormService _bookwormService;
    private Option<string> exportFileOption = new(
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
                result.ErrorMessage = "File path cannot be null or empty.";
            }
            else if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                result.ErrorMessage = "File must have a json extension.";
            }
        });
    }
    public ExportCommand(BookwormService bookwormService, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;

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
            await _bookwormService.ExportBooksAsync(filePath, cancellationToken);
            Log.Information("Books exported successfully to {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Info, ["Books exported successfully."]);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error exporting books to {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }
}
