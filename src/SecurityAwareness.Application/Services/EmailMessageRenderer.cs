using System.Text;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Application.Services;

/// <summary>
/// Renders phishing email HTML from a template, replacing {{variable}} placeholders.
/// Default template is the "Password Expiry" scenario.
/// </summary>
public class EmailMessageRenderer
{
    public EmailMessage Render(
        PhishingCampaign campaign,
        EmployeeCampaignAssignment assignment,
        Employee employee,
        SenderMailbox mailbox,
        string baseUrl)
    {
        var messageId = Guid.NewGuid().ToString();
        var trackingUrl = assignment.TokenUrl ?? $"{baseUrl.TrimEnd('/')}/r/{assignment.TrackingToken}";
        var scenario = InferScenario(campaign);
        var template = GetTemplate(scenario);
        var subject = ReplaceTokens(template.Subject, campaign, employee, trackingUrl);
        var html = ReplaceTokens(template.Html, campaign, employee, trackingUrl);

        return new EmailMessage(
            MessageId: messageId,
            SenderUpn: mailbox.Email,
            SenderDisplayName: mailbox.DisplayName,
            ToEmail: employee.Email,
            ToDisplayName: employee.DisplayName,
            Subject: subject,
            HtmlBody: html);
    }

    private static string InferScenario(PhishingCampaign c)
    {
        // Choose template from campaign.Code prefix
        var code = c.Code.ToUpperInvariant();
        if (code.StartsWith("IT", StringComparison.Ordinal) || code.StartsWith("SYS", StringComparison.Ordinal)) return "password";
        if (code.StartsWith("HR", StringComparison.Ordinal)) return "bonus";
        if (code.StartsWith("CEO", StringComparison.Ordinal)) return "document";
        return "password"; // default
    }

    private static string ReplaceTokens(string template,
        PhishingCampaign campaign, Employee employee, string trackingUrl)
    {
        return template
            .Replace("{{employee.DisplayName}}", employee.DisplayName)
            .Replace("{{campaign.Title}}", campaign.Title)
            .Replace("{{trackingUrl}}", trackingUrl)
            .Replace("{{deadlineHours}}", "24");
    }

    private sealed record EmailTemplate(string Subject, string Html);

    private static EmailTemplate GetTemplate(string scenario) => scenario switch
    {
        "bonus" => BonusTemplate,
        "document" => DocumentTemplate,
        _ => PasswordTemplate
    };

    private static readonly EmailTemplate PasswordTemplate = new(
        Subject: "【資安通告】您的帳號將於 {{deadlineHours}} 小時後過期",
        Html: """
<!DOCTYPE html>
<html lang="zh-Hant"><head><meta charset="utf-8"/></head>
<body style="font-family: 'Microsoft JhengHei', sans-serif;">
<div style="max-width: 600px; margin: 0 auto; padding: 20px;">
    <p>{{employee.DisplayName}} 您好,</p>
    <p>我們注意到您的帳號已超過 90 天未更新密碼,請於 <strong>{{deadlineHours}} 小時內</strong> 點擊下方連結:</p>
    <p style="margin: 30px 0;"><a href="{{trackingUrl}}" style="background:#0d6efd;color:white;padding:10px 24px;text-decoration:none;border-radius:4px;">👉 立即更新密碼</a></p>
    <p style="margin-top:40px;font-size:12px;color:#6c757d;">
        此為系統自動發送,請勿回覆。<br/>
        <a href="{{trackingUrl}}">取消訂閱</a>
    </p>
</div></body></html>
""");

    private static readonly EmailTemplate BonusTemplate = new(
        Subject: "【重要通知】2026 年度年終獎金發放明細",
        Html: """
<!DOCTYPE html>
<html lang="zh-Hant"><head><meta charset="utf-8"/></head>
<body style="font-family: 'Microsoft JhengHei', sans-serif;">
<div style="max-width: 600px; margin: 0 auto; padding: 20px;">
    <p>{{employee.DisplayName}} 您好,</p>
    <p>2026 年度年終獎金發放明細已出爐,請於 <strong>{{deadlineHours}} 小時內</strong> 登入確認:</p>
    <p style="margin: 30px 0;"><a href="{{trackingUrl}}" style="background:#198754;color:white;padding:10px 24px;text-decoration:none;border-radius:4px;">💰 查看年終明細</a></p>
    <p style="margin-top:40px;font-size:12px;color:#6c757d;">人力資源部</p>
</div></body></html>
""");

    private static readonly EmailTemplate DocumentTemplate = new(
        Subject: "【急件】請於 {{deadlineHours}} 小時內簽核這份文件",
        Html: """
<!DOCTYPE html>
<html lang="zh-Hant"><head><meta charset="utf-8"/></head>
<body style="font-family: 'Microsoft JhengHei', sans-serif;">
<div style="max-width: 600px; margin: 0 auto; padding: 20px;">
    <p>{{employee.DisplayName}} 您好,</p>
    <p>有一份需要您簽核的緊急文件,請於 <strong>{{deadlineHours}} 小時內</strong> 點擊下方連結處理:</p>
    <p style="margin: 30px 0;"><a href="{{trackingUrl}}" style="background:#dc3545;color:white;padding:10px 24px;text-decoration:none;border-radius:4px;">📄 立即簽核</a></p>
    <p style="margin-top:40px;font-size:12px;color:#6c757d;">總經理室</p>
</div></body></html>
""");
}

