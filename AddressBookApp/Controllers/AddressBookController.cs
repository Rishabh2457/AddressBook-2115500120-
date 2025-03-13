using BusinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;
using ModelLayer;
using ModelLayer.DTO;

namespace AddressBookApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressBookController : ControllerBase
    {
        private readonly IAddressBookBL _addressBookBL;

        public AddressBookController(IAddressBookBL addressBookBL)
        {
            _addressBookBL = addressBookBL;
        }

        
        [HttpGet]
        public IActionResult GetAllContacts()
        {
            var contacts = _addressBookBL.GetAllContacts();
            return Ok(contacts);
        }

        
        [HttpGet("{id}")]
        public IActionResult GetContactById(int id)
        {
            var contact = _addressBookBL.GetContactById(id);
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        
        [HttpPost]
        public IActionResult AddContact([FromBody] AddressBook contact)
        {
            var newContact = _addressBookBL.AddContact(contact);
            return CreatedAtAction(nameof(GetContactById), new { id = newContact.Id }, newContact);
        }

        
        [HttpPut("{id}")]
        public IActionResult UpdateContact(int id, [FromBody] AddressBook contact)
        {
            var updatedContact = _addressBookBL.UpdateContact(id, contact);
            if (updatedContact == null)
            {
                return NotFound();
            }
            return Ok(updatedContact);
        }

        
        [HttpDelete("{id}")]
        public IActionResult DeleteContact(int id)
        {
            bool isDeleted = _addressBookBL.DeleteContact(id);
            if (!isDeleted)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
