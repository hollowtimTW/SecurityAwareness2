namespace SecurityAwareness.Common.Enums;

/// <summary>
/// Email provider used by a sender mailbox.
/// 0 = Fake (writes .eml to local disk), 1 = Microsoft Graph API (M365), 2 = SMTP.
/// </summary>
public enum EmailProvider : byte
{
    Fake = 0,
    Graph = 1,
    Smtp = 2
}
