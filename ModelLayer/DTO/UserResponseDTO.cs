using System;
using ModelLayer.Model;

namespace ModelLayer.DTO
{
    public class UserResponseDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Role UserRole { get; set; }
        public string Token { get; set; } // JWT Token
    }
}
