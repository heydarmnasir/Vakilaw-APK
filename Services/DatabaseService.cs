using Microsoft.Data.Sqlite;

namespace Vakilaw.Services;

public class DatabaseService
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public string DbPath => _dbPath;

    public DatabaseService(string dbPath)
    {
        _dbPath = dbPath;

        if (!File.Exists(_dbPath))
        {
            using var fs = File.Create(_dbPath);
        }

        _connectionString = $"Data Source={_dbPath};Mode=ReadWriteCreate;Cache=Shared";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                PhoneNumber TEXT NOT NULL UNIQUE,
                Role TEXT NOT NULL,
                LicenseNumber TEXT
            );

            CREATE TABLE IF NOT EXISTS Laws (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ArticleNumber INTEGER,
                LawType TEXT,
                Title TEXT,
                Text TEXT,
                IsBookmarked INTEGER,
                IsExpanded INTEGER
            );
        ";
        cmd.ExecuteNonQuery();
    }

    public SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}