using Microsoft.AspNetCore.Mvc;
using RepositoryLayer.Context;
using ModelLayer.DTO;
using ModelLayer.Model;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BusinessLayer.Interface;
using RepositoryLayer.Interface;
using Microsoft.EntityFrameworkCore;

namespace AddressBookApp.Controllers
{

    [ApiController]
    [Route("api/addressbook")]
    public class AddressBookController : ControllerBase
    {
        /// <summary>
        /// Object of BusinessLayer Interface
        /// </summary>
        private readonly IAddressBookBL _addressBookBL;
        private readonly IRabbitMqProducer _rabbitMq;
        private readonly AddressBookContext _context;


        /// <summary>
        /// call the constructor of controller
        /// </summary>
        /// <param name="context">DbContext from program.cs</param>
        public AddressBookController(IAddressBookBL addressBookBL, IRabbitMqProducer rabbitMq, AddressBookContext context)
        {
            _addressBookBL = addressBookBL;
            _rabbitMq = rabbitMq;
            _context = context;
        }


        /// <summary>
        /// Get the userId from Token
        /// </summary>
        /// <returns>return user id from token if present</returns>
        private int? GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }


        /// <summary>
        /// function to get the Role
        /// </summary>
        /// <returns>Role of user</returns>
        private string? GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }


        /// <summary>
        /// Get all the addressbook contacts (Admin only)
        /// </summary>
        /// <returns>Response of Success or failure</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var response = new ResponseBody<List<AddressBookDTO>>();
            var data = await _addressBookBL.GetAllContacts();
            if (data != null)
            {
                response.Success = true;
                response.Message = "All AddressBook Entries Read Successfully.";
                response.Data = data;
                return Ok(response);
            }
            response.Success = false;
            response.Message = "Cannot Read Entries";
            return NotFound(response);

        }


        /// <summary>
        /// Get the address book contact by particular id(Admin can access all ids but user can access only its ids)
        /// </summary>
        /// <param name="id">id from user</param>
        /// <returns>Success or failure response</returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = new ResponseBody<AddressBookDTO>();
            var role = GetUserRole();
            var userId = GetLoggedInUserId();
            var data = await _addressBookBL.GetContactById(id);
            if (data == null)
            {
                response.Success = false;
                response.Message = $"Contact with id  {id} not found.";
                return NotFound(response);
            }
            if (role == "User" && data.UserId != userId)
            {
                response.Message = "Not Allowed";
                return Forbid();

            }
            response.Success = true;
            response.Message = $"AddressBook Entry with {id} Read Successfully userId {userId}.";
            response.Data = data;
            return Ok(response);

        }


        /// <summary>
        /// Add the Contact in the Address Book
        /// </summary>
        /// <param name="contact">AddressBookEntry from user in special format</param>
        /// <returns>Success or failure response</returns>
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateContact(AddressBookDTO contact)
        {
            var response = new ResponseBody<AddressBookDTO>();
            var userId = GetLoggedInUserId();

            if (userId == null || userId <= 0)
            {
                response.Success = false;
                response.Message = "Invalid User. Authentication failed.";
                return Unauthorized(response);
            }

            contact.UserId = userId.Value;

            // Ensure the UserId exists in the Users table before proceeding
            var userExists = _context.Users.Any(u => u.Id == contact.UserId);
            if (!userExists)
            {
                response.Success = false;
                response.Message = "User not found.";
                return BadRequest(response);
            }

            var data = await _addressBookBL.AddContact(contact);
            if (!data)
            {
                response.Success = false;
                response.Message = "Unable to add Contact. Please try again.";
                return BadRequest(response);
            }

            var userEvent = new UserEventDTO
            {
                FirstName = contact.Name,
                Email = contact.Email,
                LastName = "",
                EventType = "Contact Created"
            };

            _rabbitMq.PublishMessage(userEvent);

            response.Success = true;
            response.Message = "Contact Added Successfully.";
            response.Data = contact;
            return Ok(response);
        }




        /// <summary>
        /// Update the Address book entry at particular id
        /// </summary>
        /// <param name="id">id of contact to update</param>
        /// <param name="updatedcontact">updated info of contact</param>
        /// <returns>Succes or failure response</returns>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AddressBookDTO updatedcontact)
        {
            var response = new ResponseBody<AddressBookDTO>();
            var role = GetUserRole();
            var userId = GetLoggedInUserId();
            var existingContact = await _addressBookBL.GetContactById(id);
            if (existingContact == null)
            {
                response.Message = "Contact not found";
                return NotFound(response);
            }
            if (role == "User" && existingContact.UserId != userId)
            {
                return Forbid();
            }
            var result = await _addressBookBL.Update(id, updatedcontact);
            if (!result)
            {
                response.Message = "Unable to update contact.";
                return BadRequest(response);
            }
            response.Success = true;
            response.Message = "Contact updated Successfully.";
            response.Data = updatedcontact;
            return Ok(response);


        }


        /// <summary>
        /// Delete the particular id Contact info if present
        /// </summary>
        /// <param name="id">id of Contact entered by user</param>
        /// <returns>Success or failure response</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var response = new ResponseBody<string>();
            var role = GetUserRole();
            var userId = GetLoggedInUserId();
            var existingContact = await _addressBookBL.GetContactById(id);
            if (existingContact == null)
            {
                response.Message = "Contact Not Found";
                return NotFound(response);
            }
            if (role == "User" && existingContact.UserId != userId)
            {
                return Forbid();
            }
            var data = await _addressBookBL.DeleteContact(id);
            if (data)
            {
                response.Success = true;
                response.Message = $"Contact with {id} deleted Successfully.";
                return Ok(response);
            }
            response.Success = false;
            response.Message = $"Unable to delete contact with id {id}.";
            return BadRequest(response);


        }
    }
}

