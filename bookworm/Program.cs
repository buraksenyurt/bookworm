using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using bookworm.Client;
using bookworm.Commands.Book;
using bookworm.Commands.Export;
using bookworm.Commands.Import;
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

        rootCmd.AddCommand(new AddCommand(bookwormService, "add", "Add a new book to store"));
        rootCmd.AddCommand(new ListCommand(bookwormService, "list", "List all books from store"));
        rootCmd.AddCommand(new RemoveCommand(bookwormService, "remove", "Remove a book from store"));
        rootCmd.AddCommand(new ExportCommand(bookwormService, "export", "Export books to a file (default:books.json)"));
        rootCmd.AddCommand(new ImportCommand(bookwormService, "import", "Import books from file (default:books.json)"));

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
