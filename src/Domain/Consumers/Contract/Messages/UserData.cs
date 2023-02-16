using System.Collections.Generic;
using System.Text.Json.Serialization;
using Domain.Consumers.Contract.Enums;
using MG.Utils.Abstract;

namespace Domain.Consumers.Contract.Messages
{
    public class UserData : IHasUserData
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("roles")]
        public ICollection<SharedUserRole> Roles { get; }

        public UserData()
        {
        }

        public UserData(IHasUserData data)
        {
            data.ThrowIfNull(nameof(data));

            Id = data.Id;
            Email = data.Email;
            FirstName = data.FirstName;
            LastName = data.LastName;
            Roles = data.Roles;
        }
    }
}