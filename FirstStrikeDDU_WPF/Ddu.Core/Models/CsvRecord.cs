namespace Ddu.Core.Models;
public class CsvRecord : Dictionary<string, string>
{
    public CsvRecord() : base(StringComparer.OrdinalIgnoreCase) {}
}