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

    public async IAsyncEnumerable<LawItem> ImportIfEmptyWithProgressAsync()
    {
        var existing = await _database.GetLawsAsync();
        if (existing.Count > 0) yield break;

        using var stream = await FileSystem.OpenAppPackageFileAsync("Sea_Law.json");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var rawItems = JsonSerializer.Deserialize<List<RawLawItem>>(json);
        if (rawItems == null) yield break;

        int counter = 1;
        foreach (var raw in rawItems)
        {
            var law = new LawItem
            {
                Id = raw.Id,
                Title = raw.Title,
                Content = raw.Content,
                ArticleNumber = counter++,
                LawType = "قانون دریایی",
                IsBookmarked = false,
                IsExpanded = false
            };

            await _database.InsertLawAsync(law);
            yield return law;
        }
    }

    public async Task<List<LawItem>> GetAllAsync()
    {
        return await _database.GetLawsAsync();
    }

    private class RawLawItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
    }
}