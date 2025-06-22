using Commands.Export;
using Moq;
using Services;
using System.CommandLine;

namespace bookworm_cli.UnitTests.Commands.Export;

public class ExportCommandTests
{
    private readonly Mock<IBookwormService> _bookwormServiceMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly ExportCommand _command;

    public ExportCommandTests()
    {
        _bookwormServiceMock = new Mock<IBookwormService>();
        _messageWriterMock = new Mock<IMessageWriter>();
        _command = new ExportCommand(_bookwormServiceMock.Object, _messageWriterMock.Object, "export", "Export books to a JSON file (default:book.json)");
    }

    [Fact]
    public async Task ShouldExportBooks_WhenJsonFileIsValid()
    {
        var filePath = "books.json";
        _bookwormServiceMock.Setup(
            s => s.ExportBooksAsync(filePath, It.IsAny<CancellationToken>())
        ).ReturnsAsync(5);

        var result = await _command.InvokeAsync($"--file \"{filePath}\"");

        _bookwormServiceMock.Verify(
            s => s.ExportBooksAsync(filePath, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _messageWriterMock
            .Verify(m => m.ShowMessage(
                MessageType.Info,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { Messages.ExportCommandMessages.ExportedToFile }))),
                Times.Once);
    }

    [Fact]
    public async Task ShouldShowWarnings_WhenNoBooksExported()
    {
        var filePath = "books.json";
        _bookwormServiceMock.Setup(
            s => s.ExportBooksAsync(filePath, It.IsAny<CancellationToken>())
        ).ReturnsAsync(0);

        var result = await _command.InvokeAsync($"--file \"{filePath}\"");

        _bookwormServiceMock.Verify(
            s => s.ExportBooksAsync(filePath, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _messageWriterMock
            .Verify(m => m.ShowMessage(
                MessageType.Warning,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { Messages.ExportCommandMessages.NoBooksExported }))),
                Times.Once);
    }

    [Fact]
    public async Task ShouldLogError_WhenServiceThrowsException()
    {
        var filePath = "books.json";
        _bookwormServiceMock.Setup(
            s => s.ExportBooksAsync(filePath, It.IsAny<CancellationToken>())
        ).ThrowsAsync(new FileNotFoundException("Source file not found"));

        var result = await _command.InvokeAsync($"--file \"{filePath}\"");

        _bookwormServiceMock.Verify(
            s => s.ExportBooksAsync(filePath, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _messageWriterMock
            .Verify(m => m.ShowMessage(
                MessageType.Error,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { "Source file not found" }))),
                Times.Once);
    }
}
