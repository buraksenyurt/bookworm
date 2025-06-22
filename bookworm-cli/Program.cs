using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Client;
using Commands.Book;
using Commands.Export;
using Commands.Import;
using Commands.Interactive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Services;

namespace bookworm_cli;

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

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                var apiBaseAddress = context.Configuration["Api:BaseUrl"] ?? "http://localhost:5112";
                services.AddHttpClient<IBookwormApiClient, BookwormApiClient>(client =>
                {
                    client.BaseAddress = new Uri(apiBaseAddress);
                });
                services.AddSingleton<IBookwormService, BookwormService>();
                services.AddSingleton<INotifier, SpectreNotifier>();
            })
            .Build();

        var bookwormService = host.Services.GetRequiredService<IBookwormService>();
        var notifier = host.Services.GetRequiredService<INotifier>();

        rootCmd.AddCommand(new AddCommand(bookwormService, notifier, "add", "Add a new book to store"));
        rootCmd.AddCommand(new ListCommand(bookwormService, notifier, "list", "List all books from store"));
        rootCmd.AddCommand(new RemoveCommand(bookwormService, notifier, "remove", "Remove a book from store"));
        rootCmd.AddCommand(new ExportCommand(bookwormService, notifier,"export", "Export books to a file (default:books.json)"));
        rootCmd.AddCommand(new ImportCommand(bookwormService, notifier, "import", "Import books from file (default:books.json)"));
        rootCmd.AddCommand(new InteractiveCommand(bookwormService, notifier, "interactive", "Manage book store interactively"));

        // Parser
        var parser = new CommandLineBuilder(rootCmd)
            .UseHost(_ => Host.CreateDefaultBuilder(), host =>
            {
                _ = host.ConfigureServices(services =>
                {
                    _ = services.AddSerilog(config =>
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
