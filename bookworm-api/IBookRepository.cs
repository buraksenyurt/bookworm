namespace bookworm_api;

public interface IBookRepository
{
    Task AddAsync(Book book);
    Task DeleteAsync(int id);
    Task<Book?> GetByTitleAsync(string title);
    Task<IEnumerable<Book>> GetAllAsync();
}
