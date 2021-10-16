using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace OpenStore.Application.Email;

public class MailBuilder
{
    private readonly MailMessage _message;

    public MailBuilder()
    {
        _message = new MailMessage();
    }

    public MailBuilder AddAttachment(Attachment attachment)
    {
        _message.Attachments.Add(attachment);
        return this;
    }

    public MailBuilder AddAttachment(string filePath)
    {
        _message.Attachments.Add(new Attachment(new FileStream(filePath, FileMode.Open), Path.GetFileName(filePath)));
        return this;
    }

    public MailBuilder AddAttachments(List<Attachment> attachments)
    {
        foreach (var attachment in attachments)
        {
            _message.Attachments.Add(attachment);
        }

        return this;
    }

    public MailBuilder AddCc(MailAddress ccAddress)
    {
        _message.CC.Add(ccAddress);
        return this;
    }

    public MailBuilder AddCc(string email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            _message.CC.Add(new MailAddress(email));
        }
        return this;
    }
        
    public MailBuilder AddBcc(MailAddress bccAddress)
    {
        _message.Bcc.Add(bccAddress);
        return this;
    }

    public MailBuilder AddBcc(string email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            _message.Bcc.Add(new MailAddress(email));
        }
        return this;
    }
        
    public MailBuilder AddReplyTo(MailAddress replyToAddress)
    {
        _message.ReplyToList.Add(replyToAddress);
        return this;
    }

    public MailBuilder UseHtmlBody(bool isBodyHtml = true)
    {
        _message.IsBodyHtml = isBodyHtml;
        return this;
    }

    public MailBuilder AddTo(MailAddress toAddress)
    {
        _message.To.Add(toAddress);
        return this;
    }

    public MailBuilder AddTo(string email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            _message.To.Add(new MailAddress(email));
        }
        return this;
    }

    public MailBuilder Subject(string subject)
    {
        _message.Subject = subject;
        return this;
    }

    public MailBuilder Body(string body)
    {
        _message.Body = body;
        return this;
    }

    public MailBuilder BodyEncoding(Encoding bodyEncoding)
    {
        _message.BodyEncoding = bodyEncoding;
        return this;
    }

    public MailBuilder From(string address, string displayName)
    {
        _message.From = new MailAddress(address, displayName);
        return this;
    }

    public MailMessage Build()
    {
        return _message;
    }
}