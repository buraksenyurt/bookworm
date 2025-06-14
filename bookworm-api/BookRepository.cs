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
        return conn.QueryFirstOrDefault<Book>("SELECT * FROM Books WHERE LOWER(Title) = LOWER(@title)", new { title });
    }

    public IEnumerable<Book> GetAll()
    {
        using var conn = new SqliteConnection(_connStr);
        return conn.Query<Book>("SELECT * FROM Books");
    }
}
