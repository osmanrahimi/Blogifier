using Blogifier.Core.Extensions;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Blogifier.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(email, subject, message);
        }

        static async Task Execute(string email, string subject, string message)
        {
            var apiKey = "your-sendgrid-api-key";

            var from = new EmailAddress("we@us.com");
            var to = new EmailAddress(email);

            var client = new SendGridClient(apiKey);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, message.StripHtml(), message);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
