using System.Threading.Tasks;

namespace Blogifier.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
