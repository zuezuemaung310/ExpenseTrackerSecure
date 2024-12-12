using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class EmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Expense Tracker", _configuration["Smtp:From"]));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };
        message.Body = bodyBuilder.ToMessageBody();

        using (var smtpClient = new SmtpClient())
        {
            await smtpClient.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]), true);
            await smtpClient.AuthenticateAsync(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }
    }
}


//namespace ExpenseTracker.Service
//{
//    public class EmailSender : IEmailSender
//    {

//    }
//}
