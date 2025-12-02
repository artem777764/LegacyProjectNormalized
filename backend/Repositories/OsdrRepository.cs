using System.Globalization;
using System.Text.Json;
using backend.Models;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class OsdrRepository : IOsdrRepository
{
    private readonly ApplicationDbContext _context;

    public OsdrRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveOsdrItemsAsync(JsonDocument doc)
    {
        // получаем пары (datasetIdFromKey?, element)
        var items = ExtractItemsWithOptionalKeys(doc);
        int count = 0;

        foreach (var pair in items)
        {
            string? datasetIdFromKey = pair.DatasetId; // может быть null
            var el = pair.Element;

            // сначала пробуем взять dataset id из ключа (если он был),
            // иначе используем TryExtractStringValue, как раньше
            var datasetId = !string.IsNullOrWhiteSpace(datasetIdFromKey)
                ? datasetIdFromKey
                : TryExtractStringValue(el, new[] { "dataset_id", "id", "uuid", "studyId", "accession" });

            var title = TryExtractStringValue(el, new[] { "title", "name", "label" });
            var status = TryExtractStringValue(el, new[] { "status", "state", "lifecycle" });
            var updated = TryExtractDateTimeOffset(el, new[] { "updated", "updated_at", "modified", "timestamp" });

            string raw = el.GetRawText();
            OsdrItemEntity? existing = null;

            if (!string.IsNullOrWhiteSpace(datasetId))
            {
                existing = await _context.OsdrItems.FirstOrDefaultAsync(x => x.DatasetId == datasetId);
            }

            if (existing != null)
            {
                existing.Title = title;
                existing.Status = status;
                existing.UpdatedAt = updated;
                existing.Raw = raw;
            }
            else
            {
                var newItem = new OsdrItemEntity
                {
                    DatasetId = datasetId,
                    Title = title,
                    Status = status,
                    UpdatedAt = updated,
                    Raw = raw
                };

                _context.OsdrItems.Add(newItem);
            }

            count++;
        }

        await _context.SaveChangesAsync();
        return count;
    }

    private static List<(string? DatasetId, JsonElement Element)> ExtractItemsWithOptionalKeys(JsonDocument doc)
    {
        var root = doc.RootElement;
        var result = new List<(string?, JsonElement)>();

        // 1) если корень — массив => каждый элемент (datasetId = null)
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var e in root.EnumerateArray())
                result.Add((null, e));
            return result;
        }

        // 2) если корень — объект
        if (root.ValueKind == JsonValueKind.Object)
        {
            // a) если есть items: [...]
            if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in itemsProp.EnumerateArray())
                    result.Add((null, e));
                return result;
            }

            // b) если есть results: [...]
            if (root.TryGetProperty("results", out var resultsProp) && resultsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in resultsProp.EnumerateArray())
                    result.Add((null, e));
                return result;
            }

            // c) special case: объект-словарь вида { "OSD-1": { ... }, "OSD-2": { ... } }
            //    — если *все* свойства корня являются объектами (или по крайней мере большинство),
            //      считаем это map-форматом и разбираем пары (key -> value)
            bool looksLikeMap = true;
            int propCount = 0;
            foreach (var p in root.EnumerateObject())
            {
                propCount++;
                if (p.Value.ValueKind != JsonValueKind.Object)
                {
                    // если хоть одно значение не объект — не считаем map-форматом
                    looksLikeMap = false;
                    break;
                }
            }

            if (propCount > 0 && looksLikeMap)
            {
                foreach (var p in root.EnumerateObject())
                {
                    // p.Name — это dataset id (например "OSD-1")
                    // p.Value — объект с полями (может содержать REST_URL, title и т.д.)
                    result.Add((p.Name, p.Value));
                }
                return result;
            }
        }

        // 3) fallback: единственный объект — возвращаем как одиночный элемент
        result.Add((null, root));
        return result;
    }

    private static string? TryExtractStringValue(JsonElement jsonObject, string[] possibleKeys)
    {
        if (jsonObject.ValueKind != JsonValueKind.Object)
            return null;

        foreach (string key in possibleKeys)
        {
            if (!jsonObject.TryGetProperty(key, out var value)) continue;

            if (value.ValueKind == JsonValueKind.String)
            {
                string? stringValue = value.GetString();
                if (!string.IsNullOrWhiteSpace(stringValue)) return stringValue;
            }

            else if (value.ValueKind == JsonValueKind.Number)
            {
                if (value.TryGetInt64(out var intValue)) return intValue.ToString(CultureInfo.InvariantCulture);
                if (value.TryGetDouble(out var doubleValue)) return doubleValue.ToString(CultureInfo.InvariantCulture);
            }
        }

        return null;
    }

    private static DateTimeOffset? TryExtractDateTimeOffset(JsonElement jsonObject, string[] possibleKeys)
    {
        if (jsonObject.ValueKind != JsonValueKind.Object) return null;

        foreach (var key in possibleKeys)
        {
            if (!jsonObject.TryGetProperty(key, out var value)) continue;

            if (value.ValueKind == JsonValueKind.String)
            {
                string? stringValue = value.GetString();
                if (string.IsNullOrWhiteSpace(stringValue)) continue;

                if (DateTimeOffset.TryParse(stringValue, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
                {
                    return dto;
                }

                if (DateTime.TryParseExact(stringValue, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsed))
                {
                    return new DateTimeOffset(parsed.ToUniversalTime());
                }

                if (long.TryParse(stringValue, out var unixSeconds))
                {
                    try
                    {
                        return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
                    }
                    catch
                    {

                    }
                }
            }

            else if (value.ValueKind == JsonValueKind.Number)
            {
                if (value.TryGetInt64(out var unixSeconds))
                {
                    try
                    {
                        return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
                    }
                    catch
                    {

                    }
                }
            }
        }

        return null;
    }
}