using Microsoft.Data.Sqlite;
using Vakilaw.Models;

namespace Vakilaw.Services;

public class LawDatabase
{
    private readonly string _dbPath;

    public LawDatabase()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "vakilaw.db");
        Initialize();
    }

    private void Initialize()
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
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

    public async Task<List<LawItem>> GetLawsByTypeAsync(string lawType)
    {
        var items = new List<LawItem>();

        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Laws WHERE LawType = @type ORDER BY ArticleNumber ASC";
        cmd.Parameters.AddWithValue("@type", lawType);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(MapReaderToLaw(reader));
        }

        return items;
    }

    public async Task InsertLawAsync(LawItem law)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Laws (ArticleNumber, LawType, Title, Text, IsBookmarked, IsExpanded)
            VALUES (@ArticleNumber, @LawType, @Title, @Text, @IsBookmarked, @IsExpanded)";
        cmd.Parameters.AddWithValue("@ArticleNumber", law.ArticleNumber);
        cmd.Parameters.AddWithValue("@LawType", law.LawType ?? "");
        cmd.Parameters.AddWithValue("@Title", law.Title ?? "");
        cmd.Parameters.AddWithValue("@Text", law.Text ?? "");
        cmd.Parameters.AddWithValue("@IsBookmarked", law.IsBookmarked ? 1 : 0);
        cmd.Parameters.AddWithValue("@IsExpanded", law.IsExpanded ? 1 : 0);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateLawAsync(LawItem law)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Laws
            SET ArticleNumber=@ArticleNumber, LawType=@LawType, Title=@Title, Text=@Text,
                IsBookmarked=@IsBookmarked, IsExpanded=@IsExpanded
            WHERE Id=@Id";
        cmd.Parameters.AddWithValue("@ArticleNumber", law.ArticleNumber);
        cmd.Parameters.AddWithValue("@LawType", law.LawType ?? "");
        cmd.Parameters.AddWithValue("@Title", law.Title ?? "");
        cmd.Parameters.AddWithValue("@Text", law.Text ?? "");
        cmd.Parameters.AddWithValue("@IsBookmarked", law.IsBookmarked ? 1 : 0);
        cmd.Parameters.AddWithValue("@IsExpanded", law.IsExpanded ? 1 : 0);
        cmd.Parameters.AddWithValue("@Id", law.Id);

        await cmd.ExecuteNonQueryAsync();
    }

    private LawItem MapReaderToLaw(SqliteDataReader reader)
    {
        return new LawItem
        {
            Id = reader.GetInt32(0),
            ArticleNumber = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
            LawType = reader.IsDBNull(2) ? "" : reader.GetString(2),
            Title = reader.IsDBNull(3) ? "" : reader.GetString(3),
            Text = reader.IsDBNull(4) ? "" : reader.GetString(4),
            IsBookmarked = !reader.IsDBNull(5) && reader.GetInt32(5) == 1,
            IsExpanded = !reader.IsDBNull(6) && reader.GetInt32(6) == 1
        };
    }
}