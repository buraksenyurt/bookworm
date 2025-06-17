using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using bookworm_cli;
using Serilog;
using Services;

namespace Commands.Import;

public class ImportCommand
    : Command
{
    private readonly IBookwormService _bookwormService;
    private readonly Option<string> importFileOption = new(
            ["--file", "-f"],
            "The file path to import books from json format"
        )
    {
        IsRequired = true,
    };

    public ImportCommand(IBookwormService bookwormService, string name, string? description = null)
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
            var result = await _bookwormService.ImportBooksAsync(filePath, cancellationToken);
            if (result > 0)
            {
                Log.Information($"'{result}' books imported successfully from {filePath}.");
                Helper.ShowMessage(MessageType.Info, ["Books imported successfully."]);
            }
            else
            {
                Log.Warning("No books could be added.");
                Helper.ShowMessage(MessageType.Warning, ["No books could be added."]);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error importing books from {FilePath}.", filePath);
            Helper.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }
}
