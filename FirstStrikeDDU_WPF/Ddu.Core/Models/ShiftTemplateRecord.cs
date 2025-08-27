namespace Ddu.Core.Models;
public class ShiftTemplateRecord : CsvRecord
{
    public string Name => TryGetValue("Name", out var v) ? v : string.Empty;
    public string Label => TryGetValue("Label", out var v) ? v : string.Empty;
    public string StartTime => TryGetValue("StartTime", out var v) ? v : string.Empty; // HH:mm
    public string EndTime   => TryGetValue("EndTime", out var v) ? v : string.Empty;   // HH:mm
}