using CsvHelper;
using CsvHelper.Configuration;
using Ddu.Core.Models;
using System.Globalization;

namespace Ddu.Core.Services;

public class CsvService
{
    public async Task<List<CsvRecord>> LoadAsync(string path, CancellationToken ct = default)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture){ BadDataFound = null, DetectDelimiter = true, IgnoreBlankLines = true });
        var rows = new List<CsvRecord>();
        await foreach (var rec in csv.GetRecordsAsync<dynamic>(ct))
        {
            var dict = new CsvRecord();
            foreach (var kv in (IDictionary<string, object>)rec)
                dict[kv.Key] = kv.Value?.ToString() ?? string.Empty;
            rows.Add(dict);
        }
        return rows;
    }

    public async Task SaveAsync(string path, IEnumerable<CsvRecord> rows, CancellationToken ct = default)
    {
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        // headers from union of keys
        var headers = rows.SelectMany(r => r.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        foreach (var h in headers) csv.WriteField(h);
        await csv.NextRecordAsync();
        foreach (var r in rows)
        {
            foreach (var h in headers) csv.WriteField(r.TryGetValue(h, out var v) ? v : string.Empty);
            await csv.NextRecordAsync();
        }
    }
}