using Ddu.Core.Mapping;
using Ddu.Core.Models;

namespace Ddu.Core.Services;

public class PayCodeImportService
{
    // TODO: confirm endpoint
    private const string Endpoint = "/wfd/replaceme/paycodes";
    public async Task<(int ok, int failed, List<CsvRecord> failures, List<string> messages)> ImportAsync(
        IEnumerable<CsvRecord> rows, WfdApiClient api, string tenantId, int concurrency = 6, CancellationToken ct = default)
    {
        int ok = 0, failed = 0;
        var failures = new List<CsvRecord>();
        var messages = new List<string>();

        using var sem = new SemaphoreSlim(concurrency);
        var tasks = new List<Task>();

        foreach (var raw in rows)
        {
            var row = raw is PayCodeRecord pr ? pr : new PayCodeRecord { };
            foreach (var kv in raw) row[kv.Key] = kv.Value;

            await sem.WaitAsync(ct);
            tasks.Add(Task.Run(async () => {
                try
                {
                    var payload = PayCodeMapper.ToPayload(row, tenantId);
                    var resp = await api.PostJsonAsync(Endpoint, payload, ct);
                    if ((int)resp.StatusCode >= 400)
                    {
                        Interlocked.Increment(ref failed);
                        failures.Add(raw);
                        messages.Add(await resp.Content.ReadAsStringAsync(ct));
                    }
                    else
                    {
                        Interlocked.Increment(ref ok);
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failed);
                    failures.Add(raw);
                    messages.Add(ex.Message);
                }
                finally { sem.Release(); }
            }, ct));
        }

        await Task.WhenAll(tasks);
        return (ok, failed, failures, messages);
    }
}