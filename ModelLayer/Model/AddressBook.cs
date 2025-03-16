using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace ModelLayer.Model
{
    public class AddressBook
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        public int UserId { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }


    }
}

