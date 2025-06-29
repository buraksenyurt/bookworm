﻿using Commands.Import;
using Moq;
using Services;
using System.CommandLine;

namespace bookworm_cli.UnitTests.Commands.Import;

public class ImportCommandTests
{
    private readonly Mock<IBookwormService> _bookwormServiceMock;
    private readonly Mock<INotifier> _notifierMock;
    private readonly ImportCommand _command;

    public ImportCommandTests()
    {
        _bookwormServiceMock = new Mock<IBookwormService>();
        _notifierMock = new Mock<INotifier>();
        _command = new ImportCommand(_bookwormServiceMock.Object, _notifierMock.Object, "import", "Import books from a JSON file");
    }

    [Fact]
    public async Task ShouldImportBooks_WhenJsonFileIsValid()
    {
        var filePath = "books.json";
        _bookwormServiceMock.Setup(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>())
        ).ReturnsAsync(5);

        var result = await _command.InvokeAsync($"--file \"{filePath}\"");

        _bookwormServiceMock.Verify(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _notifierMock
            .Verify(m => m.ShowMessage(
                MessageType.Info,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { "5", Messages.ImportCommandMessages.ImportSuccessfully }))),
                Times.Once);
    }

    [Fact]
    public async Task ShouldShowWarnings_WhenNoBooksImported()
    {
        var filePath = "books.json";
        _bookwormServiceMock.Setup(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>())
        ).ReturnsAsync(0);

        var result = await _command.InvokeAsync($"--file \"{filePath}\"");

        _bookwormServiceMock.Verify(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _notifierMock
            .Verify(m => m.ShowMessage(
                MessageType.Warning,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { Messages.ImportCommandMessages.NoBooksAdded }))),
                Times.Once);
    }

    [Fact]
    public async Task ShouldLogError_WhenServiceThrowsException()
    {
        var filePath = "books.json";
        _bookwormServiceMock.Setup(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>())
        ).ThrowsAsync(new FileNotFoundException("Source file not found"));

        var result = await _command.InvokeAsync($"--file \"{filePath}\"");

        _bookwormServiceMock.Verify(
            s => s.ImportBooksAsync(filePath, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _notifierMock
            .Verify(m => m.ShowMessage(
                MessageType.Error,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { "Source file not found" }))),
                Times.Once);
    }
}
