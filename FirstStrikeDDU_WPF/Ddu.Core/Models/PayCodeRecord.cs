namespace Ddu.Core.Models;
public class PayCodeRecord : CsvRecord
{
    public string Code => TryGetValue("Code", out var v) ? v : string.Empty;
    public string Description => TryGetValue("Description", out var v) ? v : string.Empty;
    public string Type => TryGetValue("Type", out var v) ? v : string.Empty; // e.g., HOUR, MONEY
    public string Active => TryGetValue("Active", out var v) ? v : "true";
}