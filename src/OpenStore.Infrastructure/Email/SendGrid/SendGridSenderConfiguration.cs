namespace OpenStore.Infrastructure.Email.Smtp
{
    public class SendGridSenderConfiguration : EmailSenderConfiguration
    {
        public string ApiKey { get; set; }
    }
}