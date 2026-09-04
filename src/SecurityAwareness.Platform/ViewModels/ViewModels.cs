namespace SecurityAwareness.Platform.ViewModels;

public sealed record LoginViewModel(string? ReturnUrl, string? ErrorMessage);

public sealed record EmployeeListItemViewModel(
    int EmployeeId, string EmployeeNo, string Email, string DisplayName,
    string DepartmentName, bool IsActive, DateTime CreatedAt);

public sealed class EmployeeEditViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<DepartmentOption> Departments { get; set; } = new List<DepartmentOption>();
}

public sealed record DepartmentOption(int DepartmentId, string Code, string Name);

public sealed record DepartmentListItemViewModel(int DepartmentId, string Code, string Name, bool IsActive);

public sealed record CampaignListItemViewModel(
    int CampaignId, string Code, string Title, DateTime StartAt, DateTime EndAt,
    string Status, int AssignmentCount, int ClickCount, DateTime CreatedAt);

public sealed class CampaignEditViewModel
{
    public int? CampaignId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? BaseUrl { get; set; }
}

public sealed record CampaignAssignmentsViewModel(
    int CampaignId, string CampaignCode, string CampaignTitle,
    int TotalAssigned, int TotalClicked, int TotalReported,
    IReadOnlyList<AssignmentRow> Rows);

public sealed record AssignmentRow(
    int AssignmentId, string EmployeeNo, string DisplayName, string Email,
    string Status, int ClickCount, DateTime? DispatchedAt, DateTime? FirstClickedAt);

public sealed record MailboxListItemViewModel(
    int SenderMailboxId, string Email, string DisplayName, byte Provider,
    int DailyQuota, int DailyQuotaUsed, bool IsActive);

public sealed class MailboxEditViewModel
{
    public int SenderMailboxId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public byte Provider { get; set; }
    public int DailyQuota { get; set; } = 500;
    public int DailyQuotaUsed { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record DispatchViewModel(
    int CampaignId, string CampaignCode, string CampaignTitle,
    int PendingCount, IReadOnlyList<MailboxOption> Mailboxes, IReadOnlyList<int> SelectedMailboxIds);

public sealed record MailboxOption(int SenderMailboxId, string Email, string DisplayName, int DailyQuota, int DailyQuotaUsed, bool IsActive);

public sealed record DispatchResultViewModel(
    int CampaignId, string CampaignCode,
    int Total, int Queued, IReadOnlyList<string> Errors,
    string? ArtifactDirectory);

public sealed record ScheduleViewModel
{
    public int ScheduleId { get; set; }
    public int CampaignId { get; set; }
    public byte ScheduleType { get; set; } = 1;
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ReportDashboardViewModel
{
    public int CampaignId { get; set; }
    public string CampaignCode { get; set; } = string.Empty;
    public string CampaignTitle { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int ClickedCount { get; set; }
    public int ReportedCount { get; set; }
    public double ClickRate { get; set; }
    public double ReportRate { get; set; }
    public List<DepartmentMetric> Departments { get; set; } = new();
    public List<HourlyClick> HourlyClicks { get; set; } = new();
}

public sealed record DepartmentMetric(string Code, string Name, int TotalAssigned, int ClickedCount, double ClickRate);

public sealed record HourlyClick(int HourOfDay, int ClickCount);

public sealed record AuditLogItemViewModel(
    long AuditLogId, string Action, string EntityType, string? EntityId,
    string ActorKey, DateTime OccurredAt, string? DetailJson);

public sealed record AuditLogPageViewModel(
    IReadOnlyList<AuditLogItemViewModel> Items, int TotalCount,
    int Page, int PageSize, int TotalPages,
    string? FilterEntityType, string? FilterAction,
    string? FilterActorKey, DateTime? FilterFromUtc, DateTime? FilterToUtc);
