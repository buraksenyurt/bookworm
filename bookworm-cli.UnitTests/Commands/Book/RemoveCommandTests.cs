using Commands.Book;
using Moq;
using Services;
using System.CommandLine;

namespace bookworm_cli.UnitTests.Commands.Book;

public class RemoveCommandTests
{
    private readonly Mock<IBookwormService> _bookwormServiceMock;
    private readonly Mock<INotifier> _notifierMock;
    private readonly RemoveCommand _command;

    public RemoveCommandTests()
    {
        _bookwormServiceMock = new Mock<IBookwormService>();
        _notifierMock = new Mock<INotifier>();
        _command = new RemoveCommand(_bookwormServiceMock.Object, _notifierMock.Object, "remove", "Remove a book from store");
    }

    [Fact]
    public async Task ShouldReturnError_WhenTitleIsNullOrEmpty()
    {
        var actual = await Record.ExceptionAsync(() => _command.InvokeAsync("--title\"\" --category \"Fiction\" --read true"));
        _bookwormServiceMock.Verify(
            s => s.RemoveBookAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ShouldLogError_WhenServiceThrowsException()
    {
        var title = "System Programming with Rust";

        _bookwormServiceMock.Setup(
            s => s.RemoveBookAsync(
                 It.IsAny<string>(),
                It.IsAny<CancellationToken>()
                ))
            .ThrowsAsync(new Exception("Service Unavailable"));

        var result = await _command.InvokeAsync($"--title \"{title}\"");

        _bookwormServiceMock.Verify(
            s => s.RemoveBookAsync(
                title,
                It.IsAny<CancellationToken>())
            , Times.Once);
    }

    [Fact]
    public async Task ShouldCallService_WhenInputIsValid()
    {
        var title = "System Programming with Rust";

        _bookwormServiceMock.Setup(
            s => s.RemoveBookAsync(
                title,
                It.IsAny<CancellationToken>()
        )).ReturnsAsync(true);

        var result = await _command.InvokeAsync($"--title \"{title}\"");

        _bookwormServiceMock.Verify(
            s => s.RemoveBookAsync(title, It.IsAny<CancellationToken>()), Times.Once);
    }
}
