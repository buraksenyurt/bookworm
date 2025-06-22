using Commands.Book;
using Moq;
using Services;
using System.CommandLine;

namespace bookworm_cli.UnitTests.Commands.Book;

public class ListCommandTests
{
    private readonly Mock<IBookwormService> _bookwormServiceMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;

    private readonly ListCommand _command;

    public ListCommandTests()
    {
        _bookwormServiceMock = new Mock<IBookwormService>();
        _messageWriterMock = new Mock<IMessageWriter>();
        _command = new ListCommand(_bookwormServiceMock.Object, _messageWriterMock.Object, "list", "List of books");
    }
    [Fact]
    public async Task ShouldPrintBooks_WhenBooksAreReturedFromService()
    {
        var books = new List<bookworm_cli.Book>{
            new()
            {
                Title= "Programming With C#",
                Category = "Technical-Books",
                Read=true
            },
            new(){
                Title= "Denizler Altında Yirmibin Fersah",
                Category = "Fiction",
                Read=true
            }
        };
        _bookwormServiceMock
            .Setup(s => s.GetAllBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var _ = await _command.InvokeAsync("");

        _messageWriterMock
            .Verify(m => m.ShowMessage(
                MessageType.Info,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { "2", Messages.ListCommandMessages.BooksFound }))),
                Times.Once);

    }

    [Fact]
    public async Task ShouldPrintNoBooks_WhenBooksAreReturedEmptyFromService()
    {
        var noBooks = new List<bookworm_cli.Book>();
        _bookwormServiceMock
            .Setup(s => s.GetAllBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(noBooks);


        var _ = await _command.InvokeAsync("");

        _messageWriterMock
            .Verify(m => m.ShowMessage(
                MessageType.Warning,
                It.Is<string[]>(arr => arr.SequenceEqual(new[] { Messages.ListCommandMessages.NoBooksFound }))),
                Times.Once);
    }
}
