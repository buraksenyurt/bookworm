using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace bookworm;

class Program
{
    static async Task<int> Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
        Console.CancelKeyPress += (s, e) => Log.CloseAndFlush();

        var rootCmd = new RootCommand(Constants.AppDescription)
        {
            Name = Constants.AppName,
        };

        var addCommand = new Command("add", "Add a new book")
        {
        };
        rootCmd.AddCommand(addCommand);
        var titleOption = new Option<string>(
            ["--title", "-t"],
            "The title of the book to add"
        )
        {
            IsRequired = true
        };
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
        addCommand.AddOption(titleOption);
        var categoryOption = new Option<string>(
            ["--category", "-c"],
            "The category of the book (optional)"
        )
        {
            IsRequired = false,
        };

        categoryOption.SetDefaultValue("Uncategorized");
        categoryOption.FromAmong(Constants.Categories);
        categoryOption.AllowMultipleArgumentsPerToken = true;
        categoryOption.AddCompletions(Constants.Categories);

        addCommand.AddOption(categoryOption);
        var readOption = new Option<bool>(
            ["--read", "-r"],
            "Indicates if the book has been read (default is false)"
        )
        {
            IsRequired = false,
        };
        addCommand.AddOption(readOption);
        readOption.SetDefaultValue(false);

        addCommand.SetHandler(async (title, category, read) =>
        {
            await Commands.OnHandleAddCommand(title, category, read);
        }, titleOption, categoryOption, readOption);

        var listCommand = new Command("list", "List all books")
        {
        };
        rootCmd.AddCommand(listCommand);
        listCommand.SetHandler(async () => await Commands.OnHandleListCommand());
        var removeCommand = new Command("remove", "Remove a book")
        {
        };
        rootCmd.AddCommand(removeCommand);
        var removeTitleOption = new Option<string>(
            ["--title", "-t"],
            "The title of the book to remove"
        );
        removeCommand.AddOption(removeTitleOption);
        removeCommand.SetHandler(async (title) => await Commands.OnHandleRemoveCommand(title), removeTitleOption);

        var exportCommand = new Command("export", "Export books to a file")
        {
        };
        rootCmd.AddCommand(exportCommand);
        var exportFileOption = new Option<string>(
            ["--file", "-f"],
            "The file path to export the books to json format (default is 'books.json')"
        )
        {
            IsRequired = true,
        };
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
        exportCommand.AddOption(exportFileOption);
        exportCommand.SetHandler(async (file) => await Commands.OnHandleExportCommand(file), exportFileOption);

        var importCommand = new Command("import", "Import books from a file")
        {
        };
        rootCmd.AddCommand(importCommand);
        var importFileOption = new Option<string>(
            ["--file", "-f"],
            "The file path to import books from json format"
        )
        {
            IsRequired = true,
        };
        importFileOption.LegalFileNamesOnly();
        importFileOption.SetDefaultValue("books.json");
        importCommand.AddOption(importFileOption);
        importCommand.SetHandler(async (file) => await Commands.OnHandleImportCommand(file), importFileOption);

        var parser = new CommandLineBuilder(rootCmd)
            .UseHost(_ => Host.CreateDefaultBuilder(), host =>
            {
                host.ConfigureServices(services =>
                {
                    services.AddSerilog(config =>
                    {
                        config.MinimumLevel.Information();
                        config.WriteTo.Console();
                        config.WriteTo.File(
                            "logs/bookworm.log",
                            rollingInterval: RollingInterval.Day,
                            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error);
                    });
                });
            })
            .UseDefaults()
            .Build();

        return await parser.InvokeAsync(args);
    }
}
