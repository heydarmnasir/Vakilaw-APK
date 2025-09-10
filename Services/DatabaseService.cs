using AsyncAwaitBestPractices;
using Microsoft.Data.Sqlite;

namespace Vakilaw.Services;
public class DatabaseService
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DatabaseService(string dbPath)
    {
        _dbPath = dbPath;

        if (!File.Exists(_dbPath))
            using (File.Create(_dbPath)) { }

        _connectionString = $"Data Source={_dbPath};Mode=ReadWriteCreate;Cache=Shared";

        InitializeDatabase().SafeFireAndForget();
    }

    public async Task InitializeDatabase()
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FullName TEXT NOT NULL,
            PhoneNumber TEXT NOT NULL UNIQUE,
            Role TEXT NOT NULL,
            LicenseNumber TEXT
        );

        CREATE TABLE IF NOT EXISTS Lawyers (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FullName TEXT NOT NULL,
            PhoneNumber TEXT,
            City TEXT,
            Address TEXT
        );

        CREATE TABLE IF NOT EXISTS Laws (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ArticleNumber INTEGER,
            LawType TEXT,
            Title TEXT,
            Text TEXT,
            Notes TEXT,
            IsBookmarked INTEGER,
            IsExpanded INTEGER
        );

        CREATE TABLE IF NOT EXISTS Licenses (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            DeviceId TEXT,
            LicenseKey TEXT,
            UserPhone TEXT,
            StartDate INTEGER NOT NULL,
            EndDate INTEGER NOT NULL,
            IsActive INTEGER NOT NULL,
            SubscriptionType TEXT
        );
    ";
        await cmd.ExecuteNonQueryAsync();
    }

    public SqliteConnection GetConnection() => new SqliteConnection(_connectionString);
}