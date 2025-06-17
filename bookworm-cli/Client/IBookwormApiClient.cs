using bookworm_cli;

namespace Client;

public interface IBookwormApiClient
{
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> AddAsync(Book book, CancellationToken cancellationToken);
    Task<bool> RemoveAsync(string title, CancellationToken cancellationToken);
}
