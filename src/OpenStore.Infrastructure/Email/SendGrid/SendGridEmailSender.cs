using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using OpenStore.Application.Email;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OpenStore.Infrastructure.Email.SendGrid;

public class SendGridEmailSender : EmailSenderBase, IAppEmailSender
{
    private readonly ISendGridClient _sendGridClient;

    public SendGridEmailSender(ISendGridClient sendGridClient, IOptions<SendGridSenderConfiguration> configuration) : base(configuration.Value)
    {
        _sendGridClient = sendGridClient;
    }

    protected override async Task SendEmailAsync(MailMessage mail, CancellationToken cancellationToken = default)
    {
        var response = await _sendGridClient.SendEmailAsync(ConvertToSendGridMessage(mail), cancellationToken);

        if (!IsSuccessStatusCode(response))
        {
            throw new MailSenderException(await response.Body.ReadAsStringAsync(cancellationToken));
        }
    }

    private SendGridMessage ConvertToSendGridMessage(MailMessage mailMessage)
    {
        var sendGridMessage = new SendGridMessage
        {
            From = new EmailAddress(mailMessage.From.Address, mailMessage.From.DisplayName),
            Subject = mailMessage.Subject,
        };
            
        if (mailMessage.IsBodyHtml)
        {
            sendGridMessage.HtmlContent = mailMessage.Body;
        }
        else
        {             
            sendGridMessage.PlainTextContent = mailMessage.Body;
        }
            
        foreach (var mailAddress in mailMessage.To)
        {
            sendGridMessage.AddTo(mailAddress.Address, mailAddress.DisplayName);
        }

        return sendGridMessage;
    }

    private bool IsSuccessStatusCode(Response response) => response.StatusCode >= HttpStatusCode.OK && response.StatusCode <= (HttpStatusCode) 299;
}