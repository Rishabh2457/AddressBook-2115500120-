using System;
using System.ComponentModel.DataAnnotations;

namespace ModelLayer.DTO
{
    public class AddressBookDTO
    {
       
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string Phone { get; set; }

        public string Address { get; set; }

       
        public int UserId { get; set; }
    }
}
