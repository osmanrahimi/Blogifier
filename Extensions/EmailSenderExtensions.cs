using Blogifier.Core.Common;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Blogifier.Services
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }

        public static Task SendEmailWelcomeAsync(this IEmailSender emailSender, string email, string name, string link)
        {
            string dir = ApplicationSettings.WebRootPath;
            string file = Path.Combine(dir, @"blogifier\admin\" + ApplicationSettings.AdminTheme + @"\emails.json");

            using (StreamReader r = new StreamReader(file))
            {
                var json = r.ReadToEnd();
                var obj = JObject.Parse(json);

                var subject = (string)obj["welcome-subject"];
                var body = (string)obj["welcome-body"];

                return emailSender.SendEmailAsync(email, 
                    string.Format(subject, ApplicationSettings.Title),
                    string.Format(body, name, link));
            }
        }
    }
}
