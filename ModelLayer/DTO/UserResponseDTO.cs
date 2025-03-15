using System;
using System.Text.Json.Serialization;
using ModelLayer.Model;




namespace ModelLayer.DTO
{

    public enum Role
    {
        Admin=0,
        User=1
    }
    public class UserResponseDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Role UserRole { get; set; }
        public string Token { get; set; } // JWT Token
    }
}
