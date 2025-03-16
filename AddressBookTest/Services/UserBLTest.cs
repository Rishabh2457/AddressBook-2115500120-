using BusinessLayer.Interface;
using BusinessLayer.Service;
using ModelLayer.DTO;
using ModelLayer.Model;
using Moq;
using NUnit.Framework;
using RepositoryLayer.Interface;
using System.Collections.Generic;
using System.Linq;

namespace AddressBookApp.Tests
{
    [TestFixture]
    public class UserBLTests
    {
        private Mock<IUserRL> _mockUserRL;
        private Mock<IRabbitMqProducer> _mockRabbitMqProducer;
        private IUserBL _userBL;

        [SetUp]
        public void Setup()
        {
            _mockUserRL = new Mock<IUserRL>();
            _mockRabbitMqProducer = new Mock<IRabbitMqProducer>();
            _userBL = new UserBL(_mockUserRL.Object, _mockRabbitMqProducer.Object);
        }

        [Test]
        public void RegisterUser_ShouldReturnUser_AndPublishEvent()
        {
            // Arrange
            var registerDTO = new RegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "password123",
                UserRole = Role.Admin
            };

            var expectedUser = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                UserRole = Role.Admin
            };

            _mockUserRL.Setup(repo => repo.RegisterUser(registerDTO)).Returns(expectedUser);

            // Act
            var result = _userBL.RegisterUser(registerDTO);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John", result.FirstName);
            Assert.AreEqual("Doe", result.LastName);
            Assert.AreEqual("john@example.com", result.Email);
            Assert.AreEqual(Role.Admin, result.UserRole);

            // Verify that the event is published
            _mockRabbitMqProducer.Verify(mq => mq.PublishMessage(It.IsAny<UserEventDTO>()), Times.Once);
        }

        [Test]
        public void LoginUser_ShouldReturnUserResponseDTO()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Email = "john@example.com",
                Password = "password123"
            };

            var expectedResponse = new UserResponseDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Token = "mocked-jwt-token"
            };

            _mockUserRL.Setup(repo => repo.LoginUser(loginDTO)).Returns(expectedResponse);

            // Act
            var result = _userBL.LoginUser(loginDTO);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse.Email, result.Email);
            Assert.AreEqual(expectedResponse.Token, result.Token);
        }

        [Test]
        public void ForgetPassword_ShouldReturnTrue_WhenEmailExists()
        {
            // Arrange
            string email = "john@example.com";
            _mockUserRL.Setup(repo => repo.ForgetPassword(email)).Returns(true);

            // Act
            var result = _userBL.ForgetPassword(email);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ResetPassword_ShouldReturnTrue_WhenTokenAndNewPasswordAreValid()
        {
            // Arrange
            string token = "valid-reset-token";
            string newPassword = "newSecurePassword123";

            _mockUserRL.Setup(repo => repo.ResetPassword(token, newPassword)).Returns(true);

            // Act
            var result = _userBL.ResetPassword(token, newPassword);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void GetAllUsers_ShouldReturnListOfUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new User { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" }
            };

            _mockUserRL.Setup(repo => repo.GetAll()).Returns(users);

            // Act
            var result = _userBL.GetAllUsers();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("John", result[0].FirstName);
            Assert.AreEqual("Jane", result[1].FirstName);
        }
    }
}
