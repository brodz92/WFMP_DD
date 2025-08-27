using Ddu.Core.Models;

namespace Ddu.Core.Mapping;

public static class PeopleMapper
{
    public static object ToPayload(PeopleRecord row, string tenantId) => new
    {
        tenantId,
        person = new {
            personId = row.PersonId,
            personInformation = new {
                firstName = row.FirstName,
                lastName  = row.LastName,
                emailAddress = row.Email
            },
            employmentStatus = row.EmploymentStatus
        }
    };
}