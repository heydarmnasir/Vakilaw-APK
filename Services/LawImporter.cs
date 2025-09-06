using System.Diagnostics;
using System.Text.Json;
using Vakilaw.Models;

namespace Vakilaw.Services;

public class LawImporter
{
    private readonly LawDatabase _database;

    public LawImporter(LawDatabase database)
    {
        _database = database;
    }

    public async IAsyncEnumerable<LawItem> ImportIfEmptyWithProgressAsync(string fileName, string lawType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            yield break;

        // اگر قبلاً این نوع داده وجود داره، ایمپورت نکن
        var existingOfType = await _database.GetLawsByTypeAsync(lawType);
        if (existingOfType.Count > 0)
            yield break;

        using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        using var reader = new StreamReader(stream);

        string json = await reader.ReadToEndAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        List<RawLawItem>? rawItems;
        try
        {
            rawItems = JsonSerializer.Deserialize<List<RawLawItem>>(json, options);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LawImporter] JSON deserializing failed for '{fileName}': {ex.Message}");
            yield break;
        }

        if (rawItems == null || rawItems.Count == 0)
            yield break;

        int counter = 1;
        foreach (var raw in rawItems)
        {
            var law = new LawItem
            {
                Title = raw.Title ?? string.Empty,
                Text = raw.Text ?? string.Empty, // ✅ اینو درست کن
                ArticleNumber = counter++,
                LawType = lawType,
                IsBookmarked = false,
                IsExpanded = false
            };

            try
            {
                await _database.InsertLawAsync(law);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LawImporter] Insert failed: {ex.Message}");
            }

            yield return law;
        }
    }

    private class RawLawItem
    {
        public string? Title { get; set; }
        public string? Text { get; set; } // ✅ به جای Text
    }
}