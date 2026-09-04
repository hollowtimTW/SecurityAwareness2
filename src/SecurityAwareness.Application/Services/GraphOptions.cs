namespace SecurityAwareness.Application.Services;

/// <summary>
/// Configuration for Microsoft Graph API email delivery.
/// Empty values mean: use Fake delivery (write .eml to disk).
/// </summary>
public class GraphOptions
{
    public const string SectionName = "Email:Graph";
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
