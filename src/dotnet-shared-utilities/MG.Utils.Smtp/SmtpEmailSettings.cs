using MG.Utils.Abstract.NonNullableObjects;
using Microsoft.Extensions.Configuration;

namespace MG.Utils.Smtp
{
    public record SmtpEmailSettings
    {
        public NonNullableString Server { get; }

        public NonNullableInt Port { get; }

        public NonNullableString UserName { get; }

        public NonNullableString Password { get; }

        public NonNullableString From { get; }

        public SmtpEmailSettings(IConfiguration configuration)
        {
            var section = configuration.GetSection("Azure").GetSection("Smtp");
            Server = new NonNullableString(section[nameof(Server)]);
            Port = new NonNullableInt(section[nameof(Port)]);
            UserName = new NonNullableString(section[nameof(UserName)]);
            Password = new NonNullableString(section[nameof(Password)]);
            From = new NonNullableString(section[nameof(From)]);
        }
    }
}