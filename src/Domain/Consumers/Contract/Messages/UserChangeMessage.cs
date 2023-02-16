using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MG.Utils.Abstract;

namespace Domain.Consumers.Contract.Messages
{
    public class UserChangeMessage
    {
        public const string TopicName = "UserChange";

        [JsonPropertyName("users")]
        public IReadOnlyCollection<UserData> Users { get; set; }

        [JsonPropertyName("changeType")]
        public ChangeType ChangeType { get; protected set; }

        public UserChangeMessage()
        {
        }

        public UserChangeMessage(IHasUserData user, ChangeType type)
            : this(new[] { user }, type)
        {
        }

        public UserChangeMessage(IEnumerable<IHasUserData> users, ChangeType type)
        {
            users.ThrowIfNull(nameof(users));

            Users = users.Select(x => new UserData(x)).ToArray();
            ChangeType = type;
        }

        public UserData User()
        {
            if (Users.Count > 1)
            {
                throw new InvalidOperationException($"There are {Users.Count} users in the message");
            }

            return Users.FirstOrDefault();
        }
    }
}