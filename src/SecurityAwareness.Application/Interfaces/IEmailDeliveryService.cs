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
/// AssignmentId links the delivery log back to the campaign assignment
/// so the EmailDeliveryLog UI can show which employee was targeted.
/// </summary>
public sealed record EmailMessage(
    int AssignmentId,
    int MailboxId,
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
