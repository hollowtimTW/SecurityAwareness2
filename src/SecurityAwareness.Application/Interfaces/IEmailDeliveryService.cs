namespace SecurityAwareness.Application.Interfaces;

/// <summary>
/// Result of a phishing email send attempt.
/// </summary>
public sealed record EmailSendOutcome(
    string MessageId,
    bool Success,
    string? Error,
    string? LocalEmlPath);

/// <summary>
/// Plain DTO for an email to send.
/// </summary>
public sealed record EmailMessage(
    string MessageId,
    string SenderUpn,
    string SenderDisplayName,
    string ToEmail,
    string ToDisplayName,
    string Subject,
    string HtmlBody);

public interface IEmailDeliveryService
{
    Task<EmailSendOutcome> SendAsync(EmailMessage message, CancellationToken ct = default);
}
