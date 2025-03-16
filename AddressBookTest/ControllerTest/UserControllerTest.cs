using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Interface;
using ModelLayer.DTO;
using AddressBookApp.Controllers;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModelLayer.Model;

namespace AddressBookTests
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserBL> _userBLMock;
        private UserController _controller;

        [SetUp]
        public void Setup()
        {
            _userBLMock = new Mock<IUserBL>();
            _controller = new UserController(_userBLMock.Object);
        }

        [Test]
        public void Register_ShouldReturnBadRequest_WhenUserAlreadyExists()
        {
            var registerDTO = new RegisterDTO { Email = "test@example.com", Password = "password" };
            _userBLMock.Setup(x => x.RegisterUser(registerDTO)).Returns((User)null); // Simulate existing user

            var result = _controller.Register(registerDTO) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("User Already Exist", result.Value);
        }

        [Test]
        public void Register_ShouldReturnOk_WhenUserRegisteredSuccessfully()
        {
            var registerDTO = new RegisterDTO { Email = "test@example.com", Password = "password" };
            _userBLMock.Setup(x => x.RegisterUser(registerDTO)).Returns(new User { Id = 1, Email = "test@example.com" });

            var result = _controller.Register(registerDTO) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("User Registered Successfully", result.Value);
        }

        [Test]
        public void Login_ShouldReturnUnauthorized_WhenInvalidCredentials()
        {
            // Arrange
            var loginDTO = new LoginDTO { Email = "wrong@example.com", Password = "wrongpass" };

            
            _userBLMock.Setup(x => x.LoginUser(loginDTO)).Returns((UserResponseDTO)null);

            // Act
            var result = _controller.Login(loginDTO) as UnauthorizedObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, result.StatusCode);
            Assert.AreEqual("Invalid credentials", result.Value);
        }



        [Test]
        public void Login_ShouldReturnOk_WhenValidCredentials()
        {
            var loginDTO = new LoginDTO { Email = "test@example.com", Password = "password" };

            // Simulating a valid user response from business layer
            var userResponse = new UserResponseDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "test@example.com",
                UserRole = (ModelLayer.DTO.Role)Role.User
            };

            _userBLMock.Setup(x => x.LoginUser(loginDTO)).Returns(userResponse);

            var result = _controller.Login(loginDTO) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            // Compare actual response with expected UserResponseDTO
            var actualResponse = result.Value as UserResponseDTO;
            Assert.IsNotNull(actualResponse);
            Assert.AreEqual(userResponse.FirstName, actualResponse.FirstName);
            Assert.AreEqual(userResponse.LastName, actualResponse.LastName);
            Assert.AreEqual(userResponse.Email, actualResponse.Email);
            Assert.AreEqual(userResponse.UserRole, actualResponse.UserRole);
        }


        [Test]
        public void GetAllUsers_ShouldReturnUnauthorized_WhenUserIsNotAdmin()
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "User") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = principal };

            var result = _controller.GetAllUsers() as UnauthorizedObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(401, result.StatusCode);
            Assert.AreEqual("Only admin can access this.", result.Value);
        }
    }
}
