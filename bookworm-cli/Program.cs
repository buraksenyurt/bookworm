using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using bookworm_cli.Client;
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
            })
            .Build();

        var bookwormService = host.Services.GetRequiredService<IBookwormService>();

        rootCmd.AddCommand(new AddCommand(bookwormService, "add", "Add a new book to store"));
        rootCmd.AddCommand(new ListCommand(bookwormService, "list", "List all books from store"));
        rootCmd.AddCommand(new RemoveCommand(bookwormService, "remove", "Remove a book from store"));
        rootCmd.AddCommand(new ExportCommand(bookwormService, "export", "Export books to a file (default:books.json)"));
        rootCmd.AddCommand(new ImportCommand(bookwormService, "import", "Import books from file (default:books.json)"));
        rootCmd.AddCommand(new InteractiveCommand(bookwormService, "interactive", "Manage book store interactively"));

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
