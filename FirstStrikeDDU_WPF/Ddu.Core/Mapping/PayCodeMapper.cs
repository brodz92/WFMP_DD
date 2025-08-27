using Ddu.Core.Models;

namespace Ddu.Core.Mapping;

public static class PayCodeMapper
{
    public static object ToPayload(PayCodeRecord row, string tenantId) => new
    {
        tenantId,
        payCode = new {
            code = row.Code,
            description = row.Description,
            type = row.Type, // e.g. HOUR | DAY | MONEY
            active = bool.TryParse(row.Active, out var b) ? b : true
        }
    };
}