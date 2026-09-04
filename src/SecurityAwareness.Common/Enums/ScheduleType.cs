namespace SecurityAwareness.Common.Enums;

/// <summary>
/// Schedule type for automatic campaign dispatch.
/// 1 = OneTime (single send at StartAt), 2 = Daily, 3 = Weekly (every Monday).
/// </summary>
public enum ScheduleType : byte
{
    OneTime = 1,
    Daily = 2,
    Weekly = 3
}
