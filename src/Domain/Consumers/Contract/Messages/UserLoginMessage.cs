using System.Text.Json.Serialization;

namespace Domain.Consumers.Contract.Messages;

public class UserLoginMessage
{
    public const string TopicName = "UserLogin";

    [JsonPropertyName("identityId")]
    public string IdentityId { get; protected set; }

    [JsonConstructor]
    public UserLoginMessage(string identityId)
    {
        IdentityId = identityId;
    }

    public UserLoginMessage()
    {
    }
}