using Commands.Import;
using Moq;
using Services;
using System.CommandLine;

namespace bookworm_cli.UnitTests.Commands.Import;

public class ImportCommandTests
{
    private readonly Mock<IBookwormService> _bookwormServiceMock;
    private readonly ImportCommand _command;

    public ImportCommandTests()
    {
        _bookwormServiceMock = new Mock<IBookwormService>();
        _command = new ImportCommand(_bookwormServiceMock.Object, "import", "Import books from a JSON file");
    }

    [Fact]
    public async Task ShouldImportBooks_WhenJsonFileIsValid()
    {
        var filePath = "books.json";
        _bookwormServiceMock.Setup(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>())
        ).ReturnsAsync(5);

        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        var result = await _command.InvokeAsync($"--file \"{filePath}\"");

        _bookwormServiceMock.Verify(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>()),
            Times.Once
        );

        var output = consoleOutput.ToString();
        Assert.Contains("5", output);
        Assert.Contains("Books imported successfully", output);
    }

    [Fact]
    public async Task ShouldShowWarnings_WhenNoBooksImported()
    {
        var filePath = "books.json";
        _bookwormServiceMock.Setup(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>())
        ).ReturnsAsync(0);

        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        var result = await _command.InvokeAsync($"--file \"{filePath}\"");

        _bookwormServiceMock.Verify(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>()),
            Times.Once
        );

        var output = consoleOutput.ToString();
        Assert.Contains("No books could be added", output);
    }

    [Fact]
    public async Task ShouldLogError_WhenServiceThrowsException()
    {
        var filePath = "books.json";
        _bookwormServiceMock.Setup(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>())
        ).ThrowsAsync(new FileNotFoundException("Source file not found"));

        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        var result = await _command.InvokeAsync($"--file \"{filePath}\"");

        _bookwormServiceMock.Verify(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>()),
            Times.Once
        );

        var output = consoleOutput.ToString();
        Assert.Contains("Source file not found", output);
    }
}
