using Microsoft.Extensions.Logging;
using SecurityAwareness.Application.Interfaces;

namespace SecurityAwareness.Application.Services;

/// <summary>
/// Fake delivery service — writes .eml files to local artifacts/eml/ folder.
/// Used during development when no real mail server is configured.
/// </summary>
public class FakeEmailDeliveryService : IEmailDeliveryService
{
    private readonly ILogger<FakeEmailDeliveryService> _logger;
    private readonly string _outputDir;

    public FakeEmailDeliveryService(ILogger<FakeEmailDeliveryService> logger)
    {
        _logger = logger;
        _outputDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "eml");
        _outputDir = Path.GetFullPath(_outputDir);
        Directory.CreateDirectory(_outputDir);
    }

    public Task<EmailSendOutcome> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        try
        {
            var now = DateTime.Now;
            var fileName = $"{now:yyyyMMdd_HHmmss}_{message.MessageId[..8]}.eml";
            var path = Path.Combine(_outputDir, fileName);

            var body = message.HtmlBody;
            var eml = $""""
From: "{message.SenderDisplayName}" <{message.SenderUpn}>
To: "{message.ToDisplayName}" <{message.ToEmail}>
Subject: {message.Subject}
Date: {now:R}
Message-ID: <{message.MessageId}@awareness.local>
MIME-Version: 1.0
Content-Type: text/html; charset=utf-8

{body}
"""";

            File.WriteAllText(path, eml);
            _logger.LogInformation("Fake email written to {Path}", path);
            return Task.FromResult(new EmailSendOutcome(message.MessageId, true, null, path));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fake email write failed");
            return Task.FromResult(new EmailSendOutcome(message.MessageId, false, ex.Message, null));
        }
    }
}
