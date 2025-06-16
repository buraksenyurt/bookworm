using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using bookworm.Client;
using bookworm.Commands.Book;
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

        // Interactive mode Command
        var uxCommand = new Command("interactive", "Manage book store interactively")
        {
        };
        rootCmd.Add(uxCommand);
        uxCommand.Handler = CommandHandler.Create<InvocationContext>(
        async (ctx) =>
        {
            var commands = ctx.GetHost().Services.GetRequiredService<CommandsOld>();
            await commands.OnHandleInteractiveMode(ctx.GetCancellationToken());
        });

        // List Command
        var listCommand = new Command("list", "List all books")
        {
        };
        rootCmd.AddCommand(listCommand);
        listCommand.Handler = CommandHandler.Create<InvocationContext>(
        async ctx =>
        {
            var commands = ctx.GetHost().Services.GetRequiredService<CommandsOld>();
            await commands.OnHandleListCommand(ctx.GetCancellationToken());
        });

        // Remove Command
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
            var commands = ctx.GetHost().Services.GetRequiredService<CommandsOld>();
            await commands.OnHandleRemoveCommand(title, ctx.GetCancellationToken());
        });

        // Export Command
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
            var commands = ctx.GetHost().Services.GetRequiredService<CommandsOld>();
            await commands.OnHandleExportCommand(file, ctx.GetCancellationToken());
        });

        // Import Command
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
            var commands = ctx.GetHost().Services.GetRequiredService<CommandsOld>();
            await commands.OnHandleImportCommand(file, ctx.GetCancellationToken());
        });

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                var apiBaseAddress = context.Configuration["Api:BaseUrl"] ?? "http://localhost:5112";
                services.AddHttpClient<IBookwormApiClient, BookwormApiClient>(client =>
                {
                    client.BaseAddress = new Uri(apiBaseAddress);
                });
                services.AddSingleton<BookwormService>();
                services.AddSingleton<CommandsOld>();
            })
            .Build();

        var bookwormService = host.Services.GetRequiredService<BookwormService>();

        rootCmd.AddCommand(new AddCommand(bookwormService, "add", "Add a new book"));

        // Parser
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
