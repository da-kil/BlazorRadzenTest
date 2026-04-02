using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate;

/// <summary>
/// Groups identifiers assigned by the external HR system.
/// All three values arrive together during HR sync operations.
/// </summary>
public class Identity : ValueObject
{
    public string EmployeeId { get; private set; }
    public string ManagerId { get; private set; }
    public int OrganizationNumber { get; private set; }

    public Identity(string employeeId, string managerId, int organizationNumber)
    {
        EmployeeId = employeeId;
        ManagerId = managerId;
        OrganizationNumber = organizationNumber;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return EmployeeId;
        yield return ManagerId;
        yield return OrganizationNumber;
    }
}
