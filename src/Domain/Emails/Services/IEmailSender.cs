using System.Threading.Tasks;
using Domain.Emails.Requests;

namespace Domain.Emails.Services;

public interface IEmailSender
{
    Task SendAsync(EmailContent email);
}