using System;
using System.ComponentModel.DataAnnotations;
namespace ModelLayer.Model
{
    public enum Role
    {
        Admin =0 , User = 1
    }
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public Role UserRole { get; set; } = Role.User;

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
    }
}
