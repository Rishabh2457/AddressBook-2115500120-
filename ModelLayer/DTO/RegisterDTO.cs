using System;
using System.Text.Json.Serialization;

namespace ModelLayer.Model
{
    public class RegisterDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Role UserRole { get; set; }
    }
}
