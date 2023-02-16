using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MG.Utils.Smtp.Test
{
    public class MailboxSmtpProviderTest
    {
        [Fact(Skip = "Only for manual execution")]
        public async Task SendAsync()
        {
            var target = new AwesomeSender();
            await target.SendAsync(new EmailMessage("Hello", "put your email here", "Hello world"));
        }

        public class AwesomeSender : MailboxSmtpProvider
        {
            private static Dictionary<string, string> _settings = new Dictionary<string, string>
            {
                { "Azure:Smtp:From", "noreply@example.com" },
                { "Azure:Smtp:Server", string.Empty },
                { "Azure:Smtp:Port", "587" },
                { "Azure:Smtp:Password", string.Empty },
                { "Azure:Smtp:UserName", string.Empty },
            };

            public AwesomeSender()
                : base(new SmtpEmailSettings(
                    new ConfigurationBuilder()
                        .AddInMemoryCollection(_settings)
                        .Build()))
            {
            }
        }
    }
}
