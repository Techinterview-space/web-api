using System.Threading.Tasks;
using EmailService.Integration.Core.Models;

namespace EmailService.Integration.Core.Clients;

public interface IEmailSender
{
    Task SendAsync(EmailContent email);
}