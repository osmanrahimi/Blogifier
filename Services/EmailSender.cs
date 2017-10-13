using Blogifier.Core.Data.Interfaces;
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
        private readonly IUnitOfWork _db;

        public EmailSender(IUnitOfWork db)
        {
            _db = db;
        }

        public Task SendEmailAsync(string to, string subject, string message)
        {
            return Execute(to, subject, message);
        }

        static async Task Execute(string emailTo, string subject, string message)
        {
            

            var from = new EmailAddress("aaa");
            var to = new EmailAddress(emailTo);

            var client = new SendGridClient("bbb");

            var msg = MailHelper.CreateSingleEmail(from, to, subject, message.StripHtml(), message);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
