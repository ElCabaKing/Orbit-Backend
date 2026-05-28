using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Orbit.Application.Interfaces;

namespace Orbit.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly MailOptions _options;

    public EmailService(MailOptions options)
    {
        _options = options;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        client.ServerCertificateValidationCallback = CertificateValidationCallback;
        await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.Auto);
        await client.AuthenticateAsync(_options.Username, _options.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private static bool CertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
    {
        return errors == SslPolicyErrors.None
            || errors == SslPolicyErrors.RemoteCertificateNameMismatch;
    }
}
