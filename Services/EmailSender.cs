using Blogifier.Core.Data.Domain;
using Blogifier.Core.Data.Interfaces;
using Blogifier.Core.Extensions;
using Blogifier.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Linq;
using System.Threading.Tasks;

namespace Blogifier.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly IUnitOfWork _db;
        private readonly IConfiguration _config;
        private SignInManager<ApplicationUser> _signInManager;

        public EmailSender(IUnitOfWork db, SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _db = db;
            _signInManager = signInManager;
            _config = config;
        }

        public bool EmailServiceEnabled()
        {
            var section = _config.GetSection("Blogifier");
            if (section == null)
                return false;

            return !string.IsNullOrEmpty(section.GetValue<string>("SendGridEmail")) && 
                !string.IsNullOrEmpty(section.GetValue<string>("SendGridApiKey"));
        }

        public Task SendEmailAsync(string to, string subject, string message)
        {
            if (!EmailServiceEnabled())
                return null;

            // default application email settings from appsettings.json
            var from = _config.GetSection("Blogifier").GetValue<string>("SendGridEmail");
            var apiKey = _config.GetSection("Blogifier").GetValue<string>("SendGridApiKey");

            // override with user email settings
            if (_signInManager.Context.User.Identity.IsAuthenticated)
            {
                var profile = _db.Profiles.Single(p => p.IdentityName == _signInManager.Context.User.Identity.Name);

                from = profile.AuthorEmail;
                var dbFields = _db.CustomFields.Find(f => f.CustomType == CustomType.Profile && f.ParentId == profile.Id);
                if (dbFields != null && dbFields.ToList().Count > 0)
                {
                    foreach (var field in dbFields)
                    {
                        if(field.CustomKey == "SendGridApiKey" && !string.IsNullOrEmpty(field.CustomValue))
                        {
                            apiKey = field.CustomValue;
                            break;
                        }
                    }
                }
            }
            return Execute(to, from, apiKey, subject, message);
        }

        static async Task Execute(string emailTo, string emailFrom, string apiKey, string subject, string message)
        {
            var from = new EmailAddress(emailFrom);
            var to = new EmailAddress(emailTo);
            var client = new SendGridClient(apiKey);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, message.StripHtml(), message);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
