namespace SecurityAwareness.Common.Enums;

/// <summary>
/// How the phishing link was delivered.
/// 1 = URL link in email body, 2 = File attachment (e.g., fake doc, executable).
/// </summary>
public enum LinkType : byte
{
    Url = 1,
    File = 2
}
