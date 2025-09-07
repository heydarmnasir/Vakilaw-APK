using Microsoft.Data.Sqlite;
using System.Data.Common;
using System.Threading.Tasks;
using Vakilaw.Models;

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

    private async Task InitializeDatabase()
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
                Notes TEXT,
                IsBookmarked INTEGER,
                IsExpanded INTEGER
            );

            CREATE TABLE IF NOT EXISTS LawItem (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                IsBookmarked INTEGER NOT NULL
            );
        ";
       await cmd.ExecuteNonQueryAsync();
    }

    public SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public async Task<List<LawItem>> GetLawsByTypeAsync(string lawType)
    {
        var list = new List<LawItem>();

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, ArticleNumber, LawType, Title, Text, IsBookmarked, IsExpanded
        FROM Laws
        WHERE LawType = @LawType";
        command.Parameters.AddWithValue("@LawType", lawType);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new LawItem
            {
                Id = reader.GetInt32(0),
                ArticleNumber = reader.GetInt32(1),
                LawType = reader.GetString(2),
                Title = reader.GetString(3),
                Text = reader.GetString(4),
                IsBookmarked = reader.GetInt32(5) == 1, // 👈 بوکمارک رو درست بخون
                IsExpanded = reader.GetInt32(6) == 1
            });
        }

        return list;
    }
}