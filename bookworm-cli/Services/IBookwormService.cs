using bookworm_cli;

namespace Services;

public interface IBookwormService
{
    Task<bool> AddBookAsync(string title, string category, bool read, CancellationToken cancellationToken);
    Task<int> ExportBooksAsync(string fileName, CancellationToken cancellationToken);
    Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken);
    Task<int> ImportBooksAsync(string fileName, CancellationToken cancellationToken);
    Task<bool> RemoveBookAsync(string title, CancellationToken cancellationToken);
}