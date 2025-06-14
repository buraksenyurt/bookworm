using Dapper;
using Microsoft.Data.Sqlite;

namespace bookworm_api;

public static class Database
{
    public static void Initialize(string connStr)
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        var createTableCmd = @"CREATE TABLE IF NOT EXISTS Books (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Title TEXT NOT NULL,
                                Category TEXT NOT NULL,
                                Read INTEGER NOT NULL DEFAULT 0,
                                Created TEXT NOT NULL,
                                Modified TEXT
        )";
        conn.Execute(createTableCmd);
    }
}
