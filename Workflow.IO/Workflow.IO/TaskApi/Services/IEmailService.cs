using System.Threading.Tasks;

namespace TaskApi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string[] toAddresses, string subject, string htmlBody);
    }
}
