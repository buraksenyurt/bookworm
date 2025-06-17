using bookworm_cli;

namespace Client;

public interface IBookwormApiClient
{
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Book book, CancellationToken cancellationToken);
    Task RemoveAsync(string title, CancellationToken cancellationToken);
}
