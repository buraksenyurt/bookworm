using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using bookworm.Client;
using Microsoft.Extensions.DependencyInjection;
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

        addCommand.Handler = CommandHandler.Create<string, string, bool, InvocationContext>(
        async (title, category, read, ctx) =>
        {
            Log.Information("Before context call");
            var commands = ctx.GetHost().Services.GetRequiredService<Commands>();
            Log.Information("After context call");
            await commands.OnHandleAddCommand(title, category, read, ctx.GetCancellationToken());
        });

        var listCommand = new Command("list", "List all books")
        {
        };
        rootCmd.AddCommand(listCommand);
        listCommand.Handler = CommandHandler.Create<InvocationContext>(
        async ctx =>
        {
            var commands = ctx.GetHost().Services.GetRequiredService<Commands>();
            await commands.OnHandleListCommand(ctx.GetCancellationToken());
        });

        var removeCommand = new Command("remove", "Remove a book")
        {
        };
        rootCmd.AddCommand(removeCommand);
        var removeTitleOption = new Option<string>(
            ["--title", "-t"],
            "The title of the book to remove"
        );
        removeCommand.AddOption(removeTitleOption);
        removeCommand.Handler = CommandHandler.Create<string, InvocationContext>(
        async (title, ctx) =>
        {
            var commands = ctx.GetHost().Services.GetRequiredService<Commands>();
            await commands.OnHandleRemoveCommand(title, ctx.GetCancellationToken());
        });

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
        exportCommand.Handler = CommandHandler.Create<string, InvocationContext>(
        async (file, ctx) =>
        {
            var commands = ctx.GetHost().Services.GetRequiredService<Commands>();
            await commands.OnHandleExportCommand(file, ctx.GetCancellationToken());
        });


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
        importCommand.Handler = CommandHandler.Create<string, InvocationContext>(
        async (file, ctx) =>
        {
            var commands = ctx.GetHost().Services.GetRequiredService<Commands>();
            await commands.OnHandleImportCommand(file, ctx.GetCancellationToken());
        });

        var parser = new CommandLineBuilder(rootCmd)
            .UseHost(_ => Host.CreateDefaultBuilder(), host =>
            {
                host.ConfigureServices((context, services) =>
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

                    var apiBaseAddress = context.Configuration["Api:BaseUrl"] ?? "http://localhost:5112";
                    services.AddHttpClient<IBookwormApiClient, BookwormApiClient>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseAddress);
                    });
                    services.AddSingleton<BookwormService>();
                    services.AddSingleton<Commands>();
                });
            })
            .UseDefaults()
            .Build();

        return await parser.InvokeAsync(args);
    }
}
