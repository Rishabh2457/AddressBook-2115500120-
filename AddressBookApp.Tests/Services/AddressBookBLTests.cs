using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.Service;
using BusinessLayer.Interface;
using ModelLayer.DTO;
using ModelLayer.Model;
using RepositoryLayer.Interface;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AddressBookApp.Tests.BusinessLayer
{
    [TestFixture]
    public class AddressBookBLTests
    {
        private Mock<IAddressBookRL> _mockRepository;
        private Mock<IMapper> _mockMapper;
        private Mock<IConnectionMultiplexer> _mockRedis;
        private Mock<IDatabase> _mockDatabase;
        private Mock<IConfiguration> _mockConfiguration;
        private AddressBookBL _addressBookBL;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IAddressBookRL>();
            _mockMapper = new Mock<IMapper>();
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Mock Redis Database
            _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDatabase.Object);

            // Mock Cache Duration
            _mockConfiguration.Setup(c => c["Redis:CacheDuration"]).Returns("300");

            // Initialize AddressBookBL with mocked dependencies
            _addressBookBL = new AddressBookBL(_mockRepository.Object, _mockMapper.Object, _mockRedis.Object, _mockConfiguration.Object);
        }

        [Test]
        public async Task GetAllContacts_ShouldReturnContacts_WhenCacheIsEmpty()
        {
            // Arrange
            _mockDatabase.Setup(db => db.StringGetAsync("contact_list", It.IsAny<CommandFlags>()))
                         .ReturnsAsync(RedisValue.Null); // Simulate empty cache

            var contacts = new List<AddressBook>
            {
                new AddressBook { Id = 1, Name = "John Doe", Email = "john@example.com" },
                new AddressBook { Id = 2, Name = "Jane Doe", Email = "jane@example.com" }
            };

            _mockRepository.Setup(repo => repo.GetAll()).Returns(contacts);
            _mockMapper.Setup(mapper => mapper.Map<List<AddressBookDTO>>(contacts)).Returns(new List<AddressBookDTO>
            {
                new AddressBookDTO { Id = 1, Name = "John Doe", Email = "john@example.com" },
                new AddressBookDTO { Id = 2, Name = "Jane Doe", Email = "jane@example.com" }
            });

            // Act
            var result = await _addressBookBL.GetAllContacts();

            // Assert
            Assert.AreEqual(2, result.Count);
            _mockDatabase.Verify(db => db.StringSetAsync("contact_list", It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
        }

        [Test]
        public async Task GetContactById_ShouldReturnContact_WhenCacheIsAvailable()
        {
            // Arrange
            var contactDTO = new AddressBookDTO { Id = 1, Name = "John Doe", Email = "john@example.com" };
            var cachedData = JsonSerializer.Serialize(contactDTO);

            _mockDatabase.Setup(db => db.StringGetAsync("contact_1", It.IsAny<CommandFlags>()))
                         .ReturnsAsync(cachedData);

            // Act
            var result = await _addressBookBL.GetContactById(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John Doe", result.Name);
        }

        [Test]
        public async Task GetContactById_ShouldFetchFromRepo_WhenCacheIsEmpty()
        {
            // Arrange
            _mockDatabase.Setup(db => db.StringGetAsync("contact_1", It.IsAny<CommandFlags>()))
                         .ReturnsAsync(RedisValue.Null); // No cache available

            var contact = new AddressBook { Id = 1, Name = "John Doe", Email = "john@example.com" };
            _mockRepository.Setup(repo => repo.GetById(1)).Returns(contact);
            _mockMapper.Setup(mapper => mapper.Map<AddressBookDTO>(contact)).Returns(new AddressBookDTO { Id = 1, Name = "John Doe", Email = "john@example.com" });

            // Act
            var result = await _addressBookBL.GetContactById(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John Doe", result.Name);
            _mockDatabase.Verify(db => db.StringSetAsync("contact_1", It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
        }
        [Test]
        public async Task AddContact_ShouldSaveContactAndUpdateCache()
        {
            // Arrange
            var contactDTO = new AddressBookDTO { Id = 3, Name = "Alice Smith", Email = "alice@example.com" };
            var contact = new AddressBook { Id = 3, Name = "Alice Smith", Email = "alice@example.com" };

            _mockMapper.Setup(mapper => mapper.Map<AddressBook>(contactDTO)).Returns(contact);
            _mockRepository.Setup(repo => repo.AddEntry(contact)).Returns(true);

            // Mock GetAll() to return existing + new contacts
            var contacts = new List<AddressBook>
    {
        new AddressBook { Id = 1, Name = "John Doe", Email = "john@example.com" },
        new AddressBook { Id = 2, Name = "Jane Doe", Email = "jane@example.com" },
        contact // New contact added
    };

            _mockRepository.Setup(repo => repo.GetAll()).Returns(contacts);
            _mockMapper.Setup(mapper => mapper.Map<List<AddressBookDTO>>(contacts))
                       .Returns(new List<AddressBookDTO>
            {
        new AddressBookDTO { Id = 1, Name = "John Doe", Email = "john@example.com" },
        new AddressBookDTO { Id = 2, Name = "Jane Doe", Email = "jane@example.com" },
        new AddressBookDTO { Id = 3, Name = "Alice Smith", Email = "alice@example.com" }
            });

            // Act
            var result = await _addressBookBL.AddContact(contactDTO);

            // Assert
            Assert.IsTrue(result);
            _mockDatabase.Verify(db => db.StringSetAsync(
    $"contact_{contact.Id}",
    It.Is<RedisValue>(v => v.ToString().Contains("Alice Smith")), // Ensure correct value
    It.IsAny<TimeSpan>(),
    It.IsAny<When>(),
    It.IsAny<CommandFlags>()
), Times.Once);

            _mockDatabase.Verify(db => db.StringSetAsync("contact_list", It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
        }


        [Test]
        public async Task DeleteContact_ShouldRemoveFromRepoAndCache()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteEntry(1)).Returns(true);

            // Act
            var result = await _addressBookBL.DeleteContact(1);

            // Assert
            Assert.IsTrue(result);
            _mockDatabase.Verify(db => db.KeyDeleteAsync($"contact_1", It.IsAny<CommandFlags>()), Times.Once);
            _mockDatabase.Verify(db => db.KeyDeleteAsync("contact_list", It.IsAny<CommandFlags>()), Times.Once);
        }
    }
}
