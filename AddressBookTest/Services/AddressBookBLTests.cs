using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.Service;
using Microsoft.Extensions.Configuration;
using ModelLayer.DTO;
using ModelLayer.Model;
using Moq;
using NUnit.Framework;
using RepositoryLayer.Interface;
using StackExchange.Redis;

namespace BusinessLayer.Tests
{
    [TestFixture]
    public class AddressBookBLTests
    {
        private Mock<IAddressBookRL> _addressBookRLMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IDatabase> _cacheMock;
        private Mock<IConnectionMultiplexer> _redisMock;
        private IConfiguration _configuration;
        private AddressBookBL _addressBookBL;

        [SetUp]
        public void Setup()
        {
            _addressBookRLMock = new Mock<IAddressBookRL>();
            _mapperMock = new Mock<IMapper>();
            _cacheMock = new Mock<IDatabase>();
            _redisMock = new Mock<IConnectionMultiplexer>();

            var inMemorySettings = new Dictionary<string, string>
            {
                { "Redis:CacheDuration", "300" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_cacheMock.Object);

            _addressBookBL = new AddressBookBL(_addressBookRLMock.Object, _mapperMock.Object, _redisMock.Object, _configuration);
        }

        [Test]
        public async Task GetAllContacts_ShouldReturnContacts_WhenCacheIsEmpty()
        {
            // Arrange
            _cacheMock.Setup(db => db.StringGetAsync("contact_list", It.IsAny<CommandFlags>()))
                      .ReturnsAsync(RedisValue.Null);

            var contacts = new List<AddressBook>
            {
                new AddressBook { Id = 1, Name = "John Doe", Email = "john@example.com", Phone = "1234567890", Address = "123 Street", UserId = 1 }
            };

            _addressBookRLMock.Setup(repo => repo.GetAll()).Returns(contacts);
            _mapperMock.Setup(mapper => mapper.Map<List<AddressBookDTO>>(contacts)).Returns(new List<AddressBookDTO>
            {
                new AddressBookDTO { Id = 1, Name = "John Doe", Email = "john@example.com", Phone = "1234567890", Address = "123 Street", UserId = 1 }
            });

            // Act
            var result = await _addressBookBL.GetAllContacts();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            _cacheMock.Verify(db => db.StringSetAsync(
                "contact_list",
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            ), Times.Once);
        }

        [Test]
        public async Task GetContactById_ShouldReturnContact_WhenFoundInCache()
        {
            // Arrange
            var contactId = 1;
            var contactDto = new AddressBookDTO { Id = contactId, Name = "John Doe", Email = "john@example.com", Phone = "1234567890", Address = "123 Street", UserId = 1 };
            var serializedContact = JsonSerializer.Serialize(contactDto);

            _cacheMock.Setup(db => db.StringGetAsync($"contact_{contactId}", It.IsAny<CommandFlags>()))
                      .ReturnsAsync(serializedContact);

            // Act
            var result = await _addressBookBL.GetContactById(contactId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(contactId, result.Id);
            Assert.AreEqual("John Doe", result.Name);

            _addressBookRLMock.Verify(repo => repo.GetById(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task AddContact_ShouldSaveContactAndUpdateCache()
        {
            // Arrange
            var contactDto = new AddressBookDTO
            {
                Id = 3,
                Name = "Alice Smith",
                Email = "alice@example.com",
                Phone = "1112223333",
                Address = "789 Street",
                UserId = 3
            };

            var contactModel = new AddressBook
            {
                Id = 3,
                Name = "Alice Smith",
                Email = "alice@example.com",
                Phone = "1112223333",
                Address = "789 Street",
                UserId = 3
            };

            _mapperMock.Setup(m => m.Map<AddressBook>(contactDto)).Returns(contactModel);
            _addressBookRLMock.Setup(repo => repo.AddEntry(contactModel)).Returns(true);
            _addressBookRLMock.Setup(repo => repo.GetAll()).Returns(new List<AddressBook> { contactModel });

            // Act
            var result = await _addressBookBL.AddContact(contactDto);

            // Assert
            Assert.IsTrue(result);

            _cacheMock.Verify(db => db.StringSetAsync(
                $"contact_{contactDto.Id}",
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            ), Times.Once);

            _cacheMock.Verify(db => db.StringSetAsync(
                "contact_list",
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            ), Times.Once);
        }

        [Test]
        public async Task UpdateContact_ShouldUpdateContactAndInvalidateCache()
        {
            // Arrange
            var contactId = 1;
            var updatedContactDto = new AddressBookDTO { Id = contactId, Name = "Jane Doe", Email = "jane@example.com", Phone = "9876543210", Address = "456 Avenue", UserId = 2 };
            var existingContact = new AddressBook { Id = contactId, Name = "John Doe", Email = "john@example.com", Phone = "1234567890", Address = "123 Street", UserId = 1 };

            _addressBookRLMock.Setup(repo => repo.GetById(contactId)).Returns(existingContact);
            _mapperMock.Setup(m => m.Map<AddressBook>(updatedContactDto)).Returns(existingContact);
            _addressBookRLMock.Setup(repo => repo.UpdateEntry(contactId, existingContact));

            // Act
            var result = await _addressBookBL.Update(contactId, updatedContactDto);

            // Assert
            Assert.IsTrue(result);

            _cacheMock.Verify(db => db.KeyDeleteAsync($"contact_{contactId}", It.IsAny<CommandFlags>()), Times.Once);
            _cacheMock.Verify(db => db.KeyDeleteAsync("contact_list", It.IsAny<CommandFlags>()), Times.Once);
        }

        [Test]
        public async Task DeleteContact_ShouldRemoveContactFromCacheAndDatabase()
        {
            // Arrange
            var contactId = 1;
            _addressBookRLMock.Setup(repo => repo.DeleteEntry(contactId)).Returns(true);

            // Act
            var result = await _addressBookBL.DeleteContact(contactId);

            // Assert
            Assert.IsTrue(result);

            _cacheMock.Verify(db => db.KeyDeleteAsync($"contact_{contactId}", It.IsAny<CommandFlags>()), Times.Once);
            _cacheMock.Verify(db => db.KeyDeleteAsync("contact_list", It.IsAny<CommandFlags>()), Times.Once);
        }
    }
}
