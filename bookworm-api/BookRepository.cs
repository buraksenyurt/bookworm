using Dapper;
using Microsoft.Data.Sqlite;

namespace bookworm_api;

public class BookRepository(string connStr)
    : IBookRepository
{
    private readonly string _connStr = connStr;

    public async Task AddAsync(Book book)
    {
        using var conn = new SqliteConnection(_connStr);
        await conn.ExecuteAsync(@"INSERT INTO Books (Title, Category, Read, Created, Modified)
                    VALUES (@Title,@Category,@Read,@Created,@Modified)", book);
    }
    public async Task DeleteAsync(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        await conn.ExecuteAsync("DELETE FROM Books Where Id = @id", new { id });
    }

    public async Task<Book?> GetByTitleAsync(string title)
    {
        using var conn = new SqliteConnection(_connStr);
        var row = await conn.QueryAsync("SELECT Id, Title, Category, Read, Created, Modified FROM Books WHERE LOWER(Title) = LOWER(@title)", new { title });
        return row.Select(MapTo()).FirstOrDefault();
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        using var conn = new SqliteConnection(_connStr);
        var rows = await conn.QueryAsync("SELECT Id, Title, Category, Read, Created, Modified FROM Books");
        return rows.Select(MapTo());
    }

    private static Func<dynamic, Book> MapTo()
    {
        return row => new Book(
                    Id: (int)row.Id,
                    Title: (string)row.Title,
                    Category: (string)row.Category,
                    Read: ((long)row.Read) == 1,
                    Created: DateTime.Parse((string)row.Created),
                    Modified: row.Modified is null ? null : DateTime.Parse((string)row.Modified)
                );
    }
}
