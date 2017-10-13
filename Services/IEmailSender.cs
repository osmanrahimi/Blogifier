using System.Threading.Tasks;

namespace Blogifier.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string message);
    }
}