namespace SecurityAwareness.Application.Services;

/// <summary>
/// BCrypt password hashing wrapper.
/// WorkFactor: 11 (≈ 100ms hash time on modern hardware).
/// </summary>
public class PasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password is empty.", nameof(password));
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
