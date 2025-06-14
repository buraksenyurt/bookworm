using Dapper;
using Microsoft.Data.Sqlite;

namespace bookworm_api;

public class BookRepository(string connStr)
{
    private readonly string _connStr = connStr;

    public void Add(Book book)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Execute(@"INSERT INTO Books (Title, Category, Read, Created, Modified)
                    VALUES (@Title,@Category,@Read,@Created,@Modified)", book);
    }
    public void Delete(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Execute("DELETE FROM Books Where Id = @id", new { id });
    }

    public Book? GetByTitle(string title)
    {
        using var conn = new SqliteConnection(_connStr);
        var row = conn.Query("SELECT Id, Title, Category, Read, Created, Modified FROM Books WHERE LOWER(Title) = LOWER(@title)", new { title });
        return row.Select(MapTo()).FirstOrDefault();
    }

    public IEnumerable<Book> GetAll()
    {
        using var conn = new SqliteConnection(_connStr);

        var rows = conn.Query("SELECT Id, Title, Category, Read, Created, Modified FROM Books");

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
