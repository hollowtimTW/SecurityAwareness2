namespace SecurityAwareness.Infrastructure.Entities;

/// <summary>
/// Backend administrator account. Custom authentication — password is BCrypt-hashed.
/// </summary>
public class SystemAccount
{
    public int AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public byte Role { get; set; } // SystemRole
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
