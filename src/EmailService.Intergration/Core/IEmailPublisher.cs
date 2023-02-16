using EmailService.Integration.Core.Models;

namespace EmailService.Integration.Core;

public interface IEmailPublisher
    : IKafkaPublisher<EmailContent>
{
}