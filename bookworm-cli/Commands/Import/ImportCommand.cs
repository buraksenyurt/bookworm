using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using bookworm_cli;
using Serilog;

namespace Commands.Import;

public class ImportCommand
    : Command
{
    private readonly BookwormService _bookwormService;
    private Option<string> importFileOption = new(
            ["--file", "-f"],
            "The file path to import books from json format"
        )
    {
        IsRequired = true,
    };

    public ImportCommand(BookwormService bookwormService, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;

        AddOption(importFileOption);

        importFileOption.LegalFileNamesOnly();
        importFileOption.SetDefaultValue("books.json");

        Handler = CommandHandler.Create<string, InvocationContext>(
        async (file, ctx) =>
        {
            await OnHandleImportCommand(file, ctx.GetCancellationToken());
        });
    }

    private async Task OnHandleImportCommand(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await _bookwormService.ImportBooksAsync(filePath, cancellationToken);
            Log.Information("Books imported successfully from {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Info, ["Books imported successfully."]);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error importing books from {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }
}
