namespace SecurityAwareness.Application.Services;

/// <summary>
/// Stage B placeholder: real Microsoft Graph API client.
/// Currently routes to Fake delivery until IT provides Entra ID credentials.
/// To enable: uncomment lines in DI registration + provide appsettings.json values.
/// </summary>
public class GraphApiEmailDeliveryServicePlaceholder
{
    public const string ActivationHelp = "To enable real Graph API delivery: " +
        "1) Get TenantId/ClientId/ClientSecret from IT, " +
        "2) Fill appsettings.json Email:Graph section, " +
        "3) In Program.cs swap FakeEmailDeliveryService with GraphApiEmailDeliveryService.";
}
