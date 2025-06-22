using Client;
using Moq;
using Services;

namespace bookworm_cli.UnitTests.Services;

public class BookwormServiceTests
{
    [Fact]
    public async Task AddBookAsync_ShouldReturnTrue_WhenApiCallSucceeds()
    {
        var apiClientMock = new Mock<IBookwormApiClient>();
        var notifierMock = new Mock<INotifier>();
        var book = new Book
        {
            Title = "Fahrenheit 451",
            Category = "Fiction",
            Read = true
        };

        apiClientMock
            .Setup(c => c.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new BookwormService(apiClientMock.Object, notifierMock.Object);

        var actual = await service.AddBookAsync(book.Title, book.Category, book.Read, CancellationToken.None);

        apiClientMock.Verify(c => c.AddAsync(It.Is<Book>(b =>
             b.Title == book.Title &&
             b.Category == book.Category &&
             b.Read == book.Read), CancellationToken.None),
            Times.Once);

        Assert.True(actual);
    }

    [Fact]
    public async Task AddBookAsync_ShouldReturnFalse_WhenApiCallReturnsFalse()
    {
        var apiClientMock = new Mock<IBookwormApiClient>();
        var notifierMock = new Mock<INotifier>();

        apiClientMock
            .Setup(c => c.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new BookwormService(apiClientMock.Object, notifierMock.Object);

        var actual = await service.AddBookAsync("No Book", "Fiction", true, CancellationToken.None);

        Assert.False(actual);
    }

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnBookList_WhenApiCallSucceeds()
    {
        var apiClientMock = new Mock<IBookwormApiClient>();
        var notifierMock = new Mock<INotifier>();
        var expected = new List<Book>() {
            new() {
                Title = "Fahrenheit 451",
                Category = "Fiction",
                Read = true
            },
            new() {
                Title = "1984",
                Category = "Fiction",
                Read = true
            }
        };

        apiClientMock
            .Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = new BookwormService(apiClientMock.Object, notifierMock.Object);

        var actual = await service.GetAllBooksAsync(CancellationToken.None);

        Assert.Equal(expected.Count, actual.Count());
        Assert.Contains(actual, b => b.Title == "Fahrenheit 451");
        Assert.Contains(actual, b => b.Title == "1984");
    }

    [Fact]
    public async Task RemoveBookAsync_ShouldReturnTrue_WhenApiCallSucceeds()
    {
        var apiClientMock = new Mock<IBookwormApiClient>();
        var notifierMock = new Mock<INotifier>();

        apiClientMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new BookwormService(apiClientMock.Object, notifierMock.Object);

        var actual = await service.RemoveBookAsync(It.IsAny<string>(), CancellationToken.None);

        apiClientMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), CancellationToken.None),
            Times.Once);

        Assert.True(actual);
    }

    [Fact]
    public async Task RemoveBookAsync_ShouldReturnFalse_WhenApiCallReturnsFalse()
    {
        var apiClientMock = new Mock<IBookwormApiClient>();
        var notifierMock = new Mock<INotifier>();

        apiClientMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new BookwormService(apiClientMock.Object, notifierMock.Object);

        var actual = await service.RemoveBookAsync(It.IsAny<string>(), CancellationToken.None);

        apiClientMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), CancellationToken.None),
            Times.Once);

        Assert.False(actual);
    }
}
