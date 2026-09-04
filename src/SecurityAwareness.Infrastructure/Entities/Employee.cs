namespace SecurityAwareness.Infrastructure.Entities;

/// <summary>
/// An employee in the organization who can be targeted by phishing simulations.
/// </summary>
public class Employee
{
    public int EmployeeId { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Department? Department { get; set; }
}
