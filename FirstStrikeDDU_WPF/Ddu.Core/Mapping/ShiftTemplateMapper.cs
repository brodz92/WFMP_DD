using Ddu.Core.Models;

namespace Ddu.Core.Mapping;

public static class ShiftTemplateMapper
{
    public static object ToPayload(ShiftTemplateRecord row, string tenantId) => new
    {
        tenantId,
        shiftTemplate = new {
            name = row.Name,
            label = row.Label,
            segments = new [] {
                new { kind = "REGULAR_SEGMENT", startTime = row.StartTime, endTime = row.EndTime }
            }
        }
    };
}