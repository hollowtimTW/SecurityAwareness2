namespace SecurityAwareness.Common.Enums;

/// <summary>
/// Backend administrator account role.
/// 0 = Admin (everything), 1 = Operator (CRUD only), 2 = Auditor (read-only).
/// </summary>
public enum SystemRole : byte
{
    Admin = 0,
    Operator = 1,
    Auditor = 2
}
