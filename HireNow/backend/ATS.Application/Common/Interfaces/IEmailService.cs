using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATS.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendEmailTemplateAsync(string toEmail, string templateName, Dictionary<string, string> templateVariables);
    }
}
