namespace Ddu.Core.Models;
public class PeopleRecord : CsvRecord
{
    // Typical columns expected (add more as needed)
    public string PersonId => TryGetValue("PersonId", out var v) ? v : string.Empty;
    public string FirstName => TryGetValue("FirstName", out var v) ? v : string.Empty;
    public string LastName  => TryGetValue("LastName", out var v) ? v : string.Empty;
    public string Email     => TryGetValue("Email", out var v) ? v : string.Empty;
    public string EmploymentStatus => TryGetValue("EmploymentStatus", out var v) ? v : string.Empty;
}