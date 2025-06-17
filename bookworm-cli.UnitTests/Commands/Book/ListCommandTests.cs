using Commands.Book;
using Moq;
using Services;
using System.CommandLine;

namespace bookworm_cli.UnitTests.Commands.Book;

public class ListCommandTests
{
    private readonly Mock<IBookwormService> _bookwormServiceMock;
    private readonly ListCommand _command;

    public ListCommandTests()
    {
        _bookwormServiceMock = new Mock<IBookwormService>();
        _command = new ListCommand(_bookwormServiceMock.Object, "list", "List of books");
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

        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        var _ = await _command.InvokeAsync("");

        var output = consoleOutput.ToString();
        Assert.Contains("Programming With C#", output);
        Assert.Contains("Denizler Altında Yirmibin Fersah", output);
    }

    [Fact]
    public async Task ShouldPrintNoBooks_WhenBooksAreReturedEmptyFromService()
    {
        var noBooks = new List<bookworm_cli.Book>();
        _bookwormServiceMock
            .Setup(s => s.GetAllBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(noBooks);

        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        var _ = await _command.InvokeAsync("");

        var output = consoleOutput.ToString();
        Assert.Contains("No books found.", output);
    }
}
