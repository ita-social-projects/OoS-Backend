using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ChatRoomServiceTests
    {
        private IEntityRepository<ChatRoomWorkshop> roomRepository;
        private Mock<IChatRoomWorkshopModelForChatListRepository> roomWithSpecialModelRepositoryMock;
        private Mock<ILogger> loggerMock;

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
            loggerMock = new Mock<ILogger>();

            roomService = new ChatRoomWorkshopService(roomRepository, loggerMock.Object, roomWithSpecialModelRepositoryMock.Object);

            SeedDatabase();
        }

        #region CreateOrReturnExisting
        [Test]
        public async Task CreateOrReturnExisting_WhenRoomExists_ShouldReturnExistingRoom()
        {
            // Arrange
            var roomCount = dbContext.ChatRoomWorkshops.Count();
            var workshopId = 1;
            var parentId = 1;

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
            var workshopId = 3;
            var parentId = 1;

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
        [TestCase(1)]
        public async Task Delete_WhenRoomExist_ShouldDeleteEntities(long id)
        {
            // Arrange
            var roomCount = dbContext.ChatRoomWorkshops.Count();

            // Act
            await roomService.DeleteAsync(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(roomCount - 1, dbContext.ChatRoomWorkshops.Count());
        }

        [TestCase(99)]
        public void Delete_WhenRoomDoesNotExist_ShouldThrowInvalidOperationException(long id)
        {
            // Arrange
            var roomCount = dbContext.ChatRoomWorkshops.Count();

            // Act and Assert
            Assert.That(
                async () => await roomService.DeleteAsync(id).ConfigureAwait(false),
                Throws.Exception.TypeOf<InvalidOperationException>());
            Assert.AreEqual(roomCount, dbContext.ChatRoomWorkshops.Count());
        }
        #endregion

        #region GetById
        [TestCase(1)]
        public async Task GetById_WhenRoomExist_ShouldReturnFoundEntity(long id)
        {
            // Act
            var result = await roomService.GetByIdAsync(id).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<ChatRoomWorkshopDto>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Id);
        }

        [TestCase(99)]
        public async Task GetById_WhenRoomDoesNotExist_ShouldReturnNull(long id)
        {
            // Act
            var result = await roomService.GetByIdAsync(id).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }
        #endregion

        #region GetByParentIdAsync
        [Test]
        public async Task GetByParentIdAsync_WhenRoomExist_ShouldReturnFoundEntity()
        {
            // Arrange
            long existingParentId = 1;
            var validRooms = new List<ChatRoomWorkshopForChatList>()
            {
                new ChatRoomWorkshopForChatList() { Id = 1, ParentId = existingParentId },
            };
            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByParentIdAsync(existingParentId)).ReturnsAsync(validRooms);

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
            long notExistingParentId = 99;
            var validRooms = new List<ChatRoomWorkshopForChatList>();

            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByParentIdAsync(notExistingParentId)).ReturnsAsync(validRooms);

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
            long existingProviderId = 1;
            var validRooms = new List<ChatRoomWorkshopForChatList>()
            {
                new ChatRoomWorkshopForChatList() { Id = 1, Workshop = new WorkshopInfoForChatList() { Id = 1, ProviderId = existingProviderId } },
            };
            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByProviderIdAsync(existingProviderId)).ReturnsAsync(validRooms);

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
            long notExistingProviderId = 99;
            var validRooms = new List<ChatRoomWorkshopForChatList>();

            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByProviderIdAsync(notExistingProviderId)).ReturnsAsync(validRooms);

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
            long existingWorkshopId = 1;
            var validRooms = new List<ChatRoomWorkshopForChatList>()
            {
                new ChatRoomWorkshopForChatList() { Id = 1, WorkshopId = existingWorkshopId, Workshop = new WorkshopInfoForChatList() { Id = existingWorkshopId } },
            };
            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByWorkshopIdAsync(existingWorkshopId)).ReturnsAsync(validRooms);

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
            long notExistingWorkshopId = 99;
            var validRooms = new List<ChatRoomWorkshopForChatList>();

            roomWithSpecialModelRepositoryMock.Setup(x => x.GetByWorkshopIdAsync(notExistingWorkshopId)).ReturnsAsync(validRooms);

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
            long existingParentId = 1;

            // Act
            var result = await roomService.GetChatRoomIdsByParentIdAsync(existingParentId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<long>>(result);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [Test]
        public async Task GetChatRoomIdsByParentIdAsync_WhenRoomDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            long notExistingParentId = 99;

            // Act
            var result = await roomService.GetChatRoomIdsByParentIdAsync(notExistingParentId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<long>>(result);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }
        #endregion

        #region GetChatRoomIdsByProviderIdAsync
        [Test]
        public async Task GetChatRoomIdsByProviderIdAsync_WhenRoomExist_ShouldReturnFoundEntity()
        {
            // Arrange
            long existingProviderId = 1;

            // Act
            var result = await roomService.GetChatRoomIdsByProviderIdAsync(existingProviderId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<long>>(result);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [Test]
        public async Task GetChatRoomIdsByProviderIdAsync_WhenRoomDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            long notExistingProvidertId = 99;

            // Act
            var result = await roomService.GetChatRoomIdsByProviderIdAsync(notExistingProvidertId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<IEnumerable<long>>(result);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }
        #endregion

        #region GetUniqueChatRoomAsync
        [TestCase(1, 1)]
        public async Task GetUniqueChatRoom_WhenRoomExists_ShouldReturnFoundEntity(long workshopId, long parentId)
        {
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
        [TestCase(99, 99)]
        public async Task GetUniqueChatRoom_WhenRoomDoesNotExist_ShouldReturnNull(long workshopId, long parentId)
        {
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

                var users = new List<User>()
                {
                    new User() { Id = "user1", Role = Role.Parent.ToString().ToLower() },
                    new User() { Id = "user2", Role = Role.Parent.ToString().ToLower() },
                    new User() { Id = "user3", Role = Role.Provider.ToString().ToLower() },
                    new User() { Id = "user4", Role = Role.Provider.ToString().ToLower() },
                };
                context.Users.AddRangeAsync(users);

                var parents = new List<Parent>()
                {
                    new Parent() { Id = 1, UserId = users.ToArray()[0].Id },
                    new Parent() { Id = 2, UserId = users.ToArray()[1].Id },
                };
                context.Parents.AddRangeAsync(parents);

                var providers = new List<Provider>()
                {
                    new Provider() { Id = 1, UserId = users.ToArray()[2].Id },
                    new Provider() { Id = 2, UserId = users.ToArray()[3].Id },
                };
                context.Providers.AddRangeAsync(providers);

                var workshops = new List<Workshop>()
                {
                    new Workshop() { Id = 1, Title = "Title1", ProviderId = providers.ToArray()[0].Id, },
                    new Workshop() { Id = 2, Title = "Title2", ProviderId = providers.ToArray()[0].Id, },
                    new Workshop() { Id = 3, Title = "Title3", ProviderId = providers.ToArray()[1].Id, },
                };
                context.Workshops.AddRangeAsync(workshops);

                var rooms = new List<ChatRoomWorkshop>()
                {
                    new ChatRoomWorkshop() { Id = 1, WorkshopId = workshops.ToArray()[0].Id, ParentId = parents.ToArray()[0].Id, },
                    new ChatRoomWorkshop() { Id = 2, WorkshopId = workshops.ToArray()[0].Id, ParentId = parents.ToArray()[1].Id, },
                    new ChatRoomWorkshop() { Id = 3, WorkshopId = workshops.ToArray()[1].Id, ParentId = parents.ToArray()[0].Id, },
                    new ChatRoomWorkshop() { Id = 4, WorkshopId = workshops.ToArray()[1].Id, ParentId = parents.ToArray()[1].Id, },
                };
                context.ChatRoomWorkshops.AddRangeAsync(rooms);

                context.SaveChangesAsync();
            }
        }
    }
}
