namespace SecurityAwareness.Common.Enums;

/// <summary>
/// When a campaign is dispatched (sent out to employees), each employee gets an Assignment.
/// 0 = Pending (not yet sent), 1 = Dispatched (email sent), 2 = Clicked (employee clicked), 3 = Reported (employee reported phishing).
/// </summary>
public enum AssignmentStatus : byte
{
    Pending = 0,
    Dispatched = 1,
    Clicked = 2,
    Reported = 3
}
