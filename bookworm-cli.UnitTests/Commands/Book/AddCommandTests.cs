using Commands.Book;
using Moq;
using Services;
using System.CommandLine;

namespace bookworm_cli.UnitTests.Commands.Book;

public class AddCommandTests
{
    private readonly Mock<IBookwormService> _bookwormServiceMock;
    private readonly AddCommand _command;

    public AddCommandTests()
    {
        _bookwormServiceMock = new Mock<IBookwormService>();
        _command = new AddCommand(_bookwormServiceMock.Object, "add", "Add a new book");
    }

    [Fact]
    public async Task ShouldReturnError_WhenTitleIsNullOrEmpty()
    {
        var actual = await Record.ExceptionAsync(() => _command.InvokeAsync("--title\"\" --category \"Fiction\" --read true"));
        _bookwormServiceMock.Verify(
            s => s.AddBookAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ShouldReturnError_WhenTitleExceedMaxLength()
    {
        var longTitle = new string('B', 51);
        var actual = await Record.ExceptionAsync(() => _command.InvokeAsync($"--title \"{longTitle}\" --category \"Fiction\" --read true"));

        _bookwormServiceMock.Verify(
            s => s.AddBookAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>())
            , Times.Never);
    }

    [Fact]
    public async Task ShouldCallService_WhenAllInputsAreValid()
    {
        var title = "System Programming with Rust";
        var category = "Technical-Books";
        var read = true;

        _bookwormServiceMock.Setup(
            s => s.AddBookAsync(
                title,
                category,
                read,
                It.IsAny<CancellationToken>()
        )).ReturnsAsync(true);

        var result = await _command.InvokeAsync($"--title \"{title}\" --category \"{category}\" --read {read}");

        _bookwormServiceMock.Verify(
            s => s.AddBookAsync(title, category, read, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldLogError_WhenServiceThrowsException()
    {
        var title = "System Programming with Rust";
        var category = "Technical-Books";
        var read = true;

        _bookwormServiceMock.Setup(
            s => s.AddBookAsync(
                 It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
                ))
            .ThrowsAsync(new Exception("Service Unavailable"));

        var result = await _command.InvokeAsync($"--title \"{title}\" --category \"{category}\" --read {read}");

        _bookwormServiceMock.Verify(
            s => s.AddBookAsync(
                title,
                category,
                read,
                It.IsAny<CancellationToken>())
            , Times.Once);
    }
}
