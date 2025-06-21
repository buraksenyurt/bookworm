using Commands.Import;
using Moq;
using Services;
using System.CommandLine;

namespace bookworm_cli.UnitTests.Commands.Import;

public class ImportCommandTests
{
    private readonly Mock<IBookwormService> _bookwormServiceMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly ImportCommand _command;

    public ImportCommandTests()
    {
        _bookwormServiceMock = new Mock<IBookwormService>();
        _messageWriterMock = new Mock<IMessageWriter>();
        _command = new ImportCommand(_bookwormServiceMock.Object, _messageWriterMock.Object, "import", "Import books from a JSON file");
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

        _messageWriterMock
            .Verify(m => m.ShowMessage(
                MessageType.Info,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { "5", "Books imported successfully." }))),
                Times.AtLeastOnce);
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

        _messageWriterMock
            .Verify(m => m.ShowMessage(
                MessageType.Warning,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { "No books could be added." }))),
                Times.Once);
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

        _messageWriterMock
            .Verify(m => m.ShowMessage(
                MessageType.Error,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { "Source file not found" }))),
                Times.Once);
    }
}
