using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ChatRoomWorkshopServiceTests
    {
        private static User[] users = new User[4]
        {
                    new User() { Id = "user1", Role = Role.Parent.ToString().ToLower() },
                    new User() { Id = "user2", Role = Role.Parent.ToString().ToLower() },
                    new User() { Id = "user3", Role = Role.Provider.ToString().ToLower() },
                    new User() { Id = "user4", Role = Role.Provider.ToString().ToLower() },
        };

        private static Parent[] parents = new Parent[2]
               {
                    new Parent() { Id = Guid.NewGuid(), UserId = users[0].Id },
                    new Parent() { Id = Guid.NewGuid(), UserId = users[1].Id },
               };

        private static Provider[] providers = new Provider[2]
                {
                    new Provider() { Id = Guid.NewGuid(), UserId = users[2].Id },
                    new Provider() { Id = Guid.NewGuid(), UserId = users[3].Id },
                };

        private static Workshop[] workshops = new Workshop[3]
                {
                    new Workshop() { Id = Guid.NewGuid(), Title = "Title1", ProviderId = providers[0].Id, },
                    new Workshop() { Id = Guid.NewGuid(), Title = "Title2", ProviderId = providers[0].Id, },
                    new Workshop() { Id = Guid.NewGuid(), Title = "Title3", ProviderId = providers[1].Id, },
                };

        private static ChatRoomWorkshop[] rooms = new ChatRoomWorkshop[4]
                {
                    new ChatRoomWorkshop() { Id = Guid.NewGuid(), WorkshopId = workshops[0].Id, ParentId = parents[0].Id, },
                    new ChatRoomWorkshop() { Id = Guid.NewGuid(), WorkshopId = workshops[0].Id, ParentId = parents[1].Id, },
                    new ChatRoomWorkshop() { Id = Guid.NewGuid(), WorkshopId = workshops[1].Id, ParentId = parents[0].Id, },
                    new ChatRoomWorkshop() { Id = Guid.NewGuid(), WorkshopId = workshops[1].Id, ParentId = parents[1].Id, },
                };

        private IEntityRepository<ChatRoomWorkshop> roomRepository;
        private Mock<IChatRoomWorkshopModelForChatListRepository> roomWithSpecialModelRepositoryMock;
        private Mock<ILogger<ChatRoomWorkshopService>> loggerMock;

        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext dbContext;

        private IChatRoomWorkshopService roomService;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolChatRoomTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            options = builder.Options;
            dbContext = new OutOfSchoolDbContext(options);

            roomRepository = new EntityRepository<ChatRoomWorkshop>(dbContext);
            roomWithSpecialModelRepositoryMock = new Mock<IChatRoomWorkshopModelForChatListRepository>();
            loggerMock = new Mock<ILogger<ChatRoomWorkshopService>>();

            roomService = new ChatRoomWorkshopService(roomRepository, loggerMock.Object, roomWithSpecialModelRepositoryMock.Object);

            SeedDatabase();
        }

        #region CreateOrReturnExisting
        [Test]
        public async Task CreateOrReturnExisting_WhenRoomExists_ShouldReturnExistingRoom()
        {
            // Arrange
            var roomCount = dbContext.ChatRoomWorkshops.Count();
            var workshopId = workshops[0].Id;
            var parentId = parents[0].Id;

            // Act
            var result = await roomService.CreateOrReturnExistingAsync(workshopId, parentId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<ChatRoomWorkshopDto>(result);
            Assert.AreNotEqual(default(int), roomCount);
            Assert.AreEqual(roomCount, dbContext.ChatRoomWorkshops.Count());
            Assert.AreEqual(dbContext.ChatRoomWorkshops.Where(x => x.ParentId == parentId && x.WorkshopId == workshopId).FirstOrDefault()?.Id, result.Id);
        }

        [Test]
        public async Task CreateOrReturnExisting_WhenRoomDoesNotExist_ShouldCreateAndReturnNewRoom()
        {
            // Arrange
            var roomCount = dbContext.ChatRoomWorkshops.Count();
            var workshopId = workshops[2].Id;
            var parentId = parents[0].Id;

            // Act
            var result = await roomService.CreateOrReturnExistingAsync(workshopId, parentId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<ChatRoomWorkshopDto>(result);
            Assert.AreEqual(roomCount + 1, dbContext.ChatRoomWorkshops.Count());
            Assert.AreNotEqual(default(int), result.Id);
            Assert.AreEqual(dbContext.ChatRoomWorkshops.LastOrDefault()?.Id, result.Id);
        }
        #endregion

        #region Delete
        [Test]
        public async Task Delete_WhenRoomExist_ShouldDeleteEntities()
        {
            // Arrange
            var existingId = rooms[0].Id;
            var roomCount = dbContext.ChatRoomWorkshops.Count();

            // Act
            await roomService.DeleteAsync(existingId).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(roomCount - 1, dbContext.ChatRoomWorkshops.Count());
        }

        [Test]
        public void Delete_WhenRoomDoesNotExist_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var notExistingId = Guid.NewGuid();
            var roomCount = dbContext.ChatRoomWorkshops.Count();

            // Act and Assert
            Assert.That(
                async () => await roomService.DeleteAsync(notExistingId).ConfigureAwait(false),
                Throws.Exception.TypeOf<InvalidOperationException>());
            Assert.AreEqual(roomCount, dbContext.ChatRoomWorkshops.Count());
        }
        #endregion

        #region GetById
        [Test]
        public async Task GetById_WhenRoomExist_ShouldReturnFoundEntity()
        {
            // Arrange
            var existingId = rooms[0].Id;

            // Act
            var result = await roomService.GetByIdAsync(existingId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<ChatRoomWorkshopDto>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(existingId, result.Id);
        }

        [Test]
        public async Task GetById_WhenRoomDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var notExistingId = Guid.NewGuid();

            // Act
            var result = await roomService.GetByIdAsync(notExistingId).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }
        #endregion

        #region GetByParentIdAsync
        [Test]
        public async Task GetByParentIdAsync_WhenRoomExist_ShouldReturnFoundEntity()
        {
            // Arrange
            var existingParentId = parents[0].Id;
            var validRooms = new List<ChatRoomWorkshopForChatList>()
            {
                new ChatRoomWorkshopForChatList() { Id = Guid.NewGuid(), ParentId = existingParentId },
            };
            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByParentIdAsync(existingParentId, It.IsAny<bool>())).ReturnsAsync(validRooms);

            // Act
            var result = await roomService.GetByParentIdAsync(existingParentId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(existingParentId, result.First().ParentId);
        }

        [Test]
        public async Task GetByParentIdAsync_WhenRoomDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var notExistingParentId = Guid.NewGuid();
            var validRooms = new List<ChatRoomWorkshopForChatList>();

            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByParentIdAsync(notExistingParentId, It.IsAny<bool>())).ReturnsAsync(validRooms);

            // Act
            var result = await roomService.GetByParentIdAsync(notExistingParentId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>>(result);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }
        #endregion

        #region GetByProviderIdAsync
        [Test]
        public async Task GetByProviderIdAsync_WhenRoomExist_ShouldReturnFoundEntity()
        {
            // Arrange
            var existingProviderId = providers[0].Id;
            var validRooms = new List<ChatRoomWorkshopForChatList>()
            {
                new ChatRoomWorkshopForChatList() { Id = Guid.NewGuid(), Workshop = new WorkshopInfoForChatList() { Id = Guid.NewGuid(), ProviderId = existingProviderId } },
            };
            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByProviderIdAsync(existingProviderId, It.IsAny<bool>())).ReturnsAsync(validRooms);

            // Act
            var result = await roomService.GetByProviderIdAsync(existingProviderId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(existingProviderId, result.First().Workshop.ProviderId);
        }

        [Test]
        public async Task GetByProviderIdAsync_WhenRoomDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var notExistingProviderId = Guid.NewGuid();
            var validRooms = new List<ChatRoomWorkshopForChatList>();

            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByProviderIdAsync(notExistingProviderId, It.IsAny<bool>())).ReturnsAsync(validRooms);

            // Act
            var result = await roomService.GetByProviderIdAsync(notExistingProviderId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>>(result);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }
        #endregion

        #region GetByWorkshopIdAsync
        [Test]
        public async Task GetByWorkshopIdAsync_WhenRoomExist_ShouldReturnFoundEntity()
        {
            // Arrange
            var existingWorkshopId = workshops[0].Id;
            var validRooms = new List<ChatRoomWorkshopForChatList>()
            {
                new ChatRoomWorkshopForChatList() { Id = Guid.NewGuid(), WorkshopId = existingWorkshopId, Workshop = new WorkshopInfoForChatList() { Id = existingWorkshopId } },
            };
            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByWorkshopIdAsync(existingWorkshopId, It.IsAny<bool>())).ReturnsAsync(validRooms);

            // Act
            var result = await roomService.GetByWorkshopIdAsync(existingWorkshopId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(existingWorkshopId, result.First().WorkshopId);
        }

        [Test]
        public async Task GetByWorkshopIdAsync_WhenRoomDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var notExistingWorkshopId = Guid.NewGuid();
            var validRooms = new List<ChatRoomWorkshopForChatList>();

            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByWorkshopIdAsync(notExistingWorkshopId, It.IsAny<bool>())).ReturnsAsync(validRooms);

            // Act
            var result = await roomService.GetByWorkshopIdAsync(notExistingWorkshopId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>>(result);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }
        #endregion

        #region GetChatRoomIdsByParentIdAsync
        [Test]
        public async Task GetChatRoomIdsByParentIdAsync_WhenRoomExist_ShouldReturnFoundEntity()
        {
            // Arrange
            var existingParentId = parents[0].Id;

            // Act
            var result = await roomService.GetChatRoomIdsByParentIdAsync(existingParentId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<Guid>>(result);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [Test]
        public async Task GetChatRoomIdsByParentIdAsync_WhenRoomDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var notExistingParentId = Guid.NewGuid();

            // Act
            var result = await roomService.GetChatRoomIdsByParentIdAsync(notExistingParentId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<Guid>>(result);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }
        #endregion

        #region GetChatRoomIdsByProviderIdAsync
        [Test]
        public async Task GetChatRoomIdsByProviderIdAsync_WhenRoomExist_ShouldReturnFoundEntity()
        {
            // Arrange
            var existingProviderId = providers[0].Id;

            // Act
            var result = await roomService.GetChatRoomIdsByProviderIdAsync(existingProviderId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<Guid>>(result);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [Test]
        public async Task GetChatRoomIdsByProviderIdAsync_WhenRoomDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var notExistingProvidertId = Guid.NewGuid();

            // Act
            var result = await roomService.GetChatRoomIdsByProviderIdAsync(notExistingProvidertId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<Guid>>(result);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }
        #endregion

        #region GetUniqueChatRoomAsync
        [Test]
        public async Task GetUniqueChatRoom_WhenRoomExists_ShouldReturnFoundEntity()
        {
            // Arrange
            var workshopId = workshops[0].Id;
            var parentId = parents[0].Id;

            // Act
            var result = await roomService.GetUniqueChatRoomAsync(workshopId, parentId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<ChatRoomWorkshopDto>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(dbContext.ChatRoomWorkshops.Where(x => x.ParentId == parentId && x.WorkshopId == workshopId).FirstOrDefault()?.Id, result.Id);
            Assert.AreEqual(workshopId, result.WorkshopId);
            Assert.AreEqual(parentId, result.ParentId);
        }

        [Test]
        public async Task GetUniqueChatRoom_WhenRoomDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var workshopId = Guid.NewGuid();
            var parentId = Guid.NewGuid();

            // Act
            var result = await roomService.GetUniqueChatRoomAsync(workshopId, parentId).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }
        #endregion

        private void SeedDatabase()
        {
            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Users.AddRangeAsync(users);
                context.Parents.AddRangeAsync(parents);
                context.Providers.AddRangeAsync(providers);
                context.Workshops.AddRangeAsync(workshops);
                context.ChatRoomWorkshops.AddRangeAsync(rooms);

                context.SaveChangesAsync();
            }
        }
    }
}
