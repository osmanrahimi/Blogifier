using System.Threading.Tasks;

namespace Blogifier.Services
{
    public interface IEmailSender
    {
        bool EmailServiceEnabled();
        Task SendEmailAsync(string to, string subject, string message);
    }
}