using System;

namespace MG.Utils.Authentication
{
    public class Jwt
    {
        public Jwt(string apiToken, DateTimeOffset expiresAt)
        {
            ApiToken = apiToken;
            ExpiresAt = expiresAt;
        }

        public string ApiToken { get; }

        public DateTimeOffset ExpiresAt { get; }

        public string TokenType => "Bearer";

        public override string ToString() => ApiToken;
    }
}