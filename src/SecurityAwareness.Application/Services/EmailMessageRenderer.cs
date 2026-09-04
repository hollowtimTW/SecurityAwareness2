using System.Text;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Application.Services;

/// <summary>
/// Builds HTML email body from a PhishingCampaign + EmployeeCampaignAssignment + SenderMailbox.
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
        var subject = $"【資安通告】{campaign.Title} - 您的帳號將於 24 小時後過期";
        var trackingUrl = assignment.TokenUrl ?? $"{baseUrl.TrimEnd('/')}/r/{assignment.TrackingToken}";
        var html = $$"""
<!DOCTYPE html>
<html lang="zh-Hant">
<head><meta charset="utf-8" /><title>資安通告</title></head>
<body style="font-family: 'Microsoft JhengHei', sans-serif; line-height: 1.6;">
<div style="max-width: 600px; margin: 0 auto; padding: 20px;">
    <p>{{employee.DisplayName}} 您好,</p>

    <p>我們注意到您的帳號已超過 90 天未更新密碼,為符合公司資安規範,
       請於 <strong>24 小時內</strong> 點擊下方連結更新您的密碼:</p>

    <p style="margin: 30px 0;">
        <a href="{{trackingUrl}}"
           style="background: #0d6efd; color: white; padding: 10px 24px; text-decoration: none; border-radius: 4px;">
            👉 立即更新密碼
        </a>
    </p>

    <p>若您未於期限內更新,將無法登入公司系統。</p>

    <p style="margin-top: 40px; font-size: 12px; color: #6c757d;">
        此為系統自動發送,請勿回覆。<br/>
        如有疑問請洽資訊部 分機 1234<br/>
        <a href="{{baseUrl}}/unsubscribe">取消訂閱</a>
    </p>
</div>
</body>
</html>
""";

        return new EmailMessage(
            MessageId: messageId,
            SenderUpn: mailbox.Email,
            SenderDisplayName: mailbox.DisplayName,
            ToEmail: employee.Email,
            ToDisplayName: employee.DisplayName,
            Subject: subject,
            HtmlBody: html);
    }
}
