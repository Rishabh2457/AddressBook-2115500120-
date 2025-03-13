using BusinessLayer.Interface;
using FluentValidation;
using FluentValidation.Results;
using ModelLayer.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AddressBookApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressBookController : ControllerBase
    {
        private readonly IAddressBookBL _addressBookBL;
        private readonly IValidator<AddressBookDTO> _validator;

        public AddressBookController(IAddressBookBL addressBookBL, IValidator<AddressBookDTO> validator)
        {
            _addressBookBL = addressBookBL;
            _validator = validator;
        }

        [HttpGet]
        public ActionResult<List<AddressBookDTO>> GetAllContacts()
        {
            var contacts = _addressBookBL.GetAllContacts();
            return Ok(contacts);
        }

        [HttpGet("{id}")]
        public ActionResult<AddressBookDTO> GetContactById(int id)
        {
            var contact = _addressBookBL.GetContactById(id);
            if (contact == null)
                return NotFound();
            return Ok(contact);
        }

        [HttpPost]
        public ActionResult<AddressBookDTO> AddContact([FromBody] AddressBookDTO contactDTO)
        {
            // Validate DTO
            ValidationResult result = _validator.Validate(contactDTO);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            var contact = _addressBookBL.AddContact(contactDTO);
            return CreatedAtAction(nameof(GetContactById), new { id = contact.Id }, contact);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateContact(int id, [FromBody] AddressBookDTO contactDTO)
        {
            // Validate DTO
            ValidationResult result = _validator.Validate(contactDTO);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            var updatedContact = _addressBookBL.UpdateContact(id, contactDTO);
            if (updatedContact == null)
                return NotFound();
            return Ok(updatedContact);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteContact(int id)
        {
            var deleted = _addressBookBL.DeleteContact(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
