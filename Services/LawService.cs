using Microsoft.Data.Sqlite;
using System.Text.Json;
using Vakilaw.Models;
using Vakilaw.Services;

namespace Vakilaw.Services;
public class LawService
{
    private readonly DatabaseService _db;

    public LawService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<LawItem>> GetLawsByTypeAsync(string lawType)
    {
        var list = new List<LawItem>();
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, ArticleNumber, LawType, Title, Text, Notes, IsBookmarked, IsExpanded
            FROM Laws
            WHERE LawType = $type
            ORDER BY ArticleNumber ASC";
        cmd.Parameters.AddWithValue("$type", lawType);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapReaderToLaw(reader));
        }

        return list;
    }

    public async Task InsertLawAsync(LawItem law)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        using var tran = conn.BeginTransaction();
        var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = @"
        INSERT INTO Laws (ArticleNumber, LawType, Title, Text, Notes, IsBookmarked, IsExpanded)
        VALUES ($num, $type, $title, $text, $notes, $book, $exp);
        SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("$num", law.ArticleNumber);
        cmd.Parameters.AddWithValue("$type", law.LawType ?? "");
        cmd.Parameters.AddWithValue("$title", law.Title ?? "");
        cmd.Parameters.AddWithValue("$text", law.Text ?? "");
        cmd.Parameters.AddWithValue("$notes", JsonSerializer.Serialize(law.Notes ?? new List<string>()));
        cmd.Parameters.AddWithValue("$book", law.IsBookmarked ? 1 : 0);
        cmd.Parameters.AddWithValue("$exp", law.IsExpanded ? 1 : 0);

        var result = await cmd.ExecuteScalarAsync();
        if (result != null && long.TryParse(result.ToString(), out var lastId))
        {
            law.Id = (int)lastId;
        }

        tran.Commit();
    }

    public async Task<LawItem?> GetLawByIdAsync(int id)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT Id, ArticleNumber, LawType, Title, Text, Notes, IsBookmarked, IsExpanded 
                        FROM Laws 
                        WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new LawItem
            {
                Id = reader.GetInt32(0),
                ArticleNumber = reader.IsDBNull(1) ? 0 : reader.GetInt32(1), // 🔥 اصلاح شد
                LawType = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Title = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Text = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Notes = reader.IsDBNull(5)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(reader.GetString(5)),
                IsBookmarked = reader.GetInt32(6) == 1,
                IsExpanded = reader.GetInt32(7) == 1
            };
        }

        return null;
    }

    public async Task UpdateLawAsync(LawItem law)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Laws
            SET ArticleNumber=$num, LawType=$type, Title=$title, Text=$text, Notes=$notes, IsBookmarked=$book, IsExpanded=$exp
            WHERE Id=$id";
        cmd.Parameters.AddWithValue("$num", law.ArticleNumber);
        cmd.Parameters.AddWithValue("$type", law.LawType ?? "");
        cmd.Parameters.AddWithValue("$title", law.Title ?? "");
        cmd.Parameters.AddWithValue("$text", law.Text ?? "");
        cmd.Parameters.AddWithValue("$notes", JsonSerializer.Serialize(law.Notes ?? new List<string>()));
        cmd.Parameters.AddWithValue("$book", law.IsBookmarked ? 1 : 0);
        cmd.Parameters.AddWithValue("$exp", law.IsExpanded ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", law.Id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<LawItem>> GetBookmarkedLawsAsync()
    {
        var list = new List<LawItem>();
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, ArticleNumber, LawType, Title, Text, Notes, IsBookmarked, IsExpanded
            FROM Laws
            WHERE IsBookmarked = 1
            ORDER BY LawType, ArticleNumber ASC";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapReaderToLaw(reader));
        }

        return list;
    }

    private readonly Dictionary<int, LawItem> _lawCache = new();

    private LawItem MapReaderToLaw(SqliteDataReader reader)
    {
        var id = reader.GetInt32(0);

        if (id > 0 && _lawCache.TryGetValue(id, out var existing))
        {
            // update properties
            existing.ArticleNumber = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            existing.LawType = reader.IsDBNull(2) ? "" : reader.GetString(2);
            existing.Title = reader.IsDBNull(3) ? "" : reader.GetString(3);
            existing.Text = reader.IsDBNull(4) ? "" : reader.GetString(4);
            existing.Notes = reader.IsDBNull(5)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(reader.GetString(5)) ?? new List<string>();
            existing.IsBookmarked = !reader.IsDBNull(6) && reader.GetInt32(6) == 1;
            existing.IsExpanded = !reader.IsDBNull(7) && reader.GetInt32(7) == 1;

            return existing;
        }

        var law = new LawItem
        {
            Id = id,
            ArticleNumber = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
            LawType = reader.IsDBNull(2) ? "" : reader.GetString(2),
            Title = reader.IsDBNull(3) ? "" : reader.GetString(3),
            Text = reader.IsDBNull(4) ? "" : reader.GetString(4),
            Notes = reader.IsDBNull(5)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(reader.GetString(5)) ?? new List<string>(),
            IsBookmarked = !reader.IsDBNull(6) && reader.GetInt32(6) == 1,
            IsExpanded = !reader.IsDBNull(7) && reader.GetInt32(7) == 1
        };

        if (id > 0)
            _lawCache[id] = law;

        return law;
    }
}