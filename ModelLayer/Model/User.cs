using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json.Serialization;


public enum Role
{
    Admin = 0, User = 1
}
public class User
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required]
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    [Required]
    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    [Required, EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [Required]
    [JsonIgnore] // Do not expose PasswordHash in Redis JSON
    public string PasswordHash { get; set; }

    [Required]
    [JsonPropertyName("user_role")]
    public Role UserRole { get; set; } = Role.User;

    [JsonPropertyName("reset_token")]
    public string? ResetToken { get; set; }

    [JsonPropertyName("reset_token_expiry")]
    public DateTime? ResetTokenExpiry { get; set; }
}
