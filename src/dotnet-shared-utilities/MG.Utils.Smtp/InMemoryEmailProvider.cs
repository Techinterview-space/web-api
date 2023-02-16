using System.Threading.Tasks;
using MG.Utils.Helpers;
using Microsoft.Extensions.Logging;

namespace MG.Utils.Smtp
{
    public class InMemoryEmailProvider : IEmail
    {
        private readonly ILogger<InMemoryEmailProvider> _logger;

        public InMemoryEmailProvider(ILogger<InMemoryEmailProvider> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string serializedMessage)
        {
            return SendAsync(serializedMessage.DeserializeAs<EmailMessage>());
        }

        public Task SendAsync(EmailMessage message)
        {
            _logger.LogInformation(message.DebugInfo());
            return Task.CompletedTask;
        }
    }
}