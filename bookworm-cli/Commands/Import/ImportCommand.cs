using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using Serilog;
using Services;

namespace Commands.Import;

public class ImportCommand
    : Command
{
    private readonly IBookwormService _bookwormService;
    private readonly IMessageWriter _messageWriter;

    private readonly Option<string> importFileOption = new(
            ["--file", "-f"],
            "The file path to import books from json format"
        )
    {
        IsRequired = true,
    };

    public ImportCommand(IBookwormService bookwormService, IMessageWriter messageWriter, string name, string? description = null)
        : base(name, description)
    {
        _bookwormService = bookwormService;
        _messageWriter = messageWriter;

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
                _messageWriter.ShowMessage(MessageType.Info, [result.ToString(), "Books imported successfully."]);
            }
            else
            {
                Log.Warning("No books could be added.");
                _messageWriter.ShowMessage(MessageType.Warning, ["No books could be added."]);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error importing books from {FilePath}.", filePath);
            _messageWriter.ShowMessage(MessageType.Error, [ex.Message]);
        }
    }
}
