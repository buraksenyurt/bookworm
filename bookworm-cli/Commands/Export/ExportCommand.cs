using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using Serilog;
using Services;

namespace Commands.Export;

public class ExportCommand
    : Command
{
    private readonly IBookwormService _bookwormService;
    private readonly IMessageWriter _messageWriter;

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
                result.ErrorMessage = "File path cannot be null or empty.";
            }
            else if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                result.ErrorMessage = "File must have a json extension.";
            }
        });
    }
    public ExportCommand(IBookwormService bookwormService,IMessageWriter messageWriter, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;
        _messageWriter = messageWriter;

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
                _messageWriter.ShowMessage(MessageType.Info, ["Books imported successfully."]);
            }
            else
            {
                Log.Warning("No books could be exported.");
                _messageWriter.ShowMessage(MessageType.Warning, ["No books could be exported."]);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error exporting books to {FilePath}.", filePath);
            _messageWriter.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }
}
