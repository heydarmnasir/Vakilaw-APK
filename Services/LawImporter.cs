using System.Diagnostics;
using System.Text.Json;
using Vakilaw.Models;

namespace Vakilaw.Services;
public class LawImporter
{
    private readonly LawService _lawService;

    public LawImporter(LawService lawService)
    {
        _lawService = lawService;
    }

    public async IAsyncEnumerable<LawItem> ImportIfEmptyWithProgressAsync(string fileName, string lawType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            yield break;

        var existing = await _lawService.GetLawsByTypeAsync(lawType);
        if (existing.Count > 0)
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
            Debug.WriteLine($"[LawImporter] JSON deserializing failed: {ex.Message}");
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
                Text = raw.Text ?? string.Empty,
                Notes = ParseNotes(raw.Notes),
                ArticleNumber = counter++,
                LawType = lawType,
                IsBookmarked = false,
                IsExpanded = false
            };

            try
            {
                await _lawService.InsertLawAsync(law);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LawImporter] Insert failed: {ex.Message}");
            }

            yield return law;
        }
    }

    private static List<string> ParseNotes(JsonElement element)
    {
        var list = new List<string>();
        if (element.ValueKind == JsonValueKind.Null) return list;

        if (element.ValueKind == JsonValueKind.String)
        {
            var s = element.GetString();
            if (!string.IsNullOrWhiteSpace(s))
                list.Add(s.Trim());
            return list;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String)
                {
                    var s = item.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        list.Add(s.Trim());
                }
        }

        return list;
    }

    private class RawLawItem
    {
        public string? Title { get; set; }
        public string? Text { get; set; }
        public JsonElement Notes { get; set; }
    }
}