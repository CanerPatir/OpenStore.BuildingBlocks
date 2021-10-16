namespace OpenStore.Infrastructure.Email.SendGrid;

public class SendGridSenderConfiguration : EmailSenderConfiguration
{
    public string ApiKey { get; set; }
}