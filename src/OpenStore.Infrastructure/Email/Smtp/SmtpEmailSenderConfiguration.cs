namespace OpenStore.Infrastructure.Email.Smtp;

public class SmtpEmailSenderConfiguration : EmailSenderConfiguration
{
    public string Host { get; set; }
    public int Port { get; set; } = 587;
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Domain { get; set; }
    public bool EnableSsl { get; set; }
    public bool UseDefaultCredentials { get; set; }
}