namespace SecurityAwareness.Infrastructure.Entities;

/// <summary>
/// A department / division within the organization.
/// </summary>
public class Department
{
    public int DepartmentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    /// <summary>Optional. Employee who manages this department (FK → Employees.EmployeeId).</summary>
    public int? ManagerEmployeeId { get; set; }
    public Employee? Manager { get; set; }
}
