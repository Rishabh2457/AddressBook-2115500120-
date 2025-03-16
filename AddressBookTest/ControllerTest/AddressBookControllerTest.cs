
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AddressBookApp.Controllers;
using BusinessLayer.Interface;
using ModelLayer.DTO;
using RepositoryLayer.Context;
using RepositoryLayer.Interface;
using ModelLayer.Model;
using Microsoft.EntityFrameworkCore;



namespace AddressBookTests
{
    [TestFixture]
    public class AddressBookControllerTests
    {
        private Mock<IAddressBookBL> _addressBookBLMock;
        private Mock<IRabbitMqProducer> _rabbitMqMock;
        private AddressBookContext _context;
        private AddressBookController _controller;

        [SetUp]
        public void Setup()
        {
            _addressBookBLMock = new Mock<IAddressBookBL>();
            _rabbitMqMock = new Mock<IRabbitMqProducer>();

            var options = new DbContextOptionsBuilder<AddressBookContext>()
                              .UseInMemoryDatabase(databaseName: "TestDB")
                              .Options;

            _context = new AddressBookContext(options);

            //Ensure test user exists with correct properties
            _context.Users.Add(new User
            {
                Id = 3,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword", // Required due to [Required] attribute
                UserRole = Role.User
            });

            _context.SaveChanges(); // Save to In-Memory DB

            _controller = new AddressBookController(_addressBookBLMock.Object, _rabbitMqMock.Object, _context);
        }






        /// <summary>
        /// Helper method to set up user claims
        /// </summary>
        private void SetUserClaims(string role, int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role),
                new Claim("UserId", userId.ToString())
            };

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // GetAll (Admin Only)
        [Test]
        public async Task GetAll_ShouldReturnAllContacts_WhenAdmin()
        {
            // Arrange
            SetUserClaims("Admin", 1);
            var contacts = new List<AddressBookDTO>
            {
                new AddressBookDTO { Id = 1, Name = "Alice", Email = "alice@example.com", Phone = "1234567890", Address = "123 St", UserId = 1 },
                new AddressBookDTO { Id = 2, Name = "Bob", Email = "bob@example.com", Phone = "0987654321", Address = "456 Ave", UserId = 2 }
            };

            _addressBookBLMock.Setup(bl => bl.GetAllContacts()).ReturnsAsync(contacts);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        //  GetById (User can only access their own contacts)
        [Test]
        public async Task GetById_ShouldReturnContact_WhenUserHasAccess()
        {
            // Arrange
            SetUserClaims("User", 2);
            var contact = new AddressBookDTO { Id = 2, Name = "Bob", Email = "bob@example.com", Phone = "0987654321", Address = "456 Ave", UserId = 2 };

            _addressBookBLMock.Setup(bl => bl.GetContactById(2)).ReturnsAsync(contact);

            // Act
            var result = await _controller.GetById(2);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [Test]
        public async Task GetById_ShouldReturnForbidden_WhenUserTriesToAccessOtherContact()
        {
            // Arrange
            SetUserClaims("User", 3);
            var contact = new AddressBookDTO { Id = 2, Name = "Bob", Email = "bob@example.com", Phone = "0987654321", Address = "456 Ave", UserId = 2 };

            _addressBookBLMock.Setup(bl => bl.GetContactById(2)).ReturnsAsync(contact);

            // Act
            var result = await _controller.GetById(2);

            // Assert
            Assert.IsInstanceOf<ForbidResult>(result);
        }

        //  Create Contact
        [Test]
        public async Task CreateContact_ShouldReturnOk_WhenContactIsAdded()
        {
            // Arrange
            SetUserClaims("User", 3); // Ensure the test user exists

            var contact = new AddressBookDTO
            {
                Id = 3,
                Name = "Charlie",
                Email = "charlie@example.com",
                Phone = "1112223333",
                Address = "789 Street",
                UserId = 3
            };

            _addressBookBLMock.Setup(bl => bl.AddContact(It.IsAny<AddressBookDTO>())).ReturnsAsync(true);

            // Act
            var result = await _controller.CreateContact(contact);

            // Assert
            Assert.NotNull(result, "Expected non-null result but got null");  // ✅ Ensure result is not null
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult, "Expected OkObjectResult but got null");  // ✅ Ensure it's an OK response
            Assert.AreEqual(200, okResult.StatusCode, "Expected status code 200 but got different");
        }



        // Update Contact
        [Test]
        public async Task Update_ShouldReturnOk_WhenContactIsUpdated()
        {
            // Arrange
            SetUserClaims("User", 3);
            var updatedContact = new AddressBookDTO { Id = 3, Name = "Charlie Updated", Email = "charlie@example.com", Phone = "1112223333", Address = "789 Street", UserId = 3 };

            _addressBookBLMock.Setup(bl => bl.GetContactById(3)).ReturnsAsync(updatedContact);
            _addressBookBLMock.Setup(bl => bl.Update(3, updatedContact)).ReturnsAsync(true);

            // Act
            var result = await _controller.Update(3, updatedContact);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        //  Delete Contact
        [Test]
        public async Task Delete_ShouldReturnOk_WhenContactIsDeleted()
        {
            // Arrange
            SetUserClaims("User", 3);
            var contact = new AddressBookDTO { Id = 3, Name = "Charlie", Email = "charlie@example.com", Phone = "1112223333", Address = "789 Street", UserId = 3 };

            _addressBookBLMock.Setup(bl => bl.GetContactById(3)).ReturnsAsync(contact);
            _addressBookBLMock.Setup(bl => bl.DeleteContact(3)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(3);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [Test]
        public async Task Delete_ShouldReturnForbidden_WhenUserTriesToDeleteOtherUserContact()
        {
            // Arrange
            SetUserClaims("User", 4);
            var contact = new AddressBookDTO { Id = 3, Name = "Charlie", Email = "charlie@example.com", Phone = "1112223333", Address = "789 Street", UserId = 3 };

            _addressBookBLMock.Setup(bl => bl.GetContactById(3)).ReturnsAsync(contact);

            // Act
            var result = await _controller.Delete(3);

            // Assert
            Assert.IsInstanceOf<ForbidResult>(result);
        }
    }
}
