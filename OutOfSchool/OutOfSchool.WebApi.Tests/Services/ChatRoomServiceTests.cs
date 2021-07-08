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
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ChatRoomServiceTests
    {
        private IEntityRepository<ChatRoom> roomRepository;
        private IEntityRepository<User> userRepository;
        private IWorkshopRepository workshopRepository;
        private Mock<ILogger> loggerMoq;

        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext dbContext;

        private IChatRoomService roomService;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolChatRoomTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            options = builder.Options;
            dbContext = new OutOfSchoolDbContext(options);

            roomRepository = new EntityRepository<ChatRoom>(dbContext);
            userRepository = new EntityRepository<User>(dbContext);
            workshopRepository = new WorkshopRepository(dbContext);
            loggerMoq = new Mock<ILogger>();

            roomService = new ChatRoomService(roomRepository, userRepository, workshopRepository, loggerMoq.Object);

            SeedDatabase();
        }

        #region CreateOrReturnExisting
        [Test]
        public async Task CreateOrReturnExisting_WhenRoomExists_ShouldReturnEntity()
        {
            // Arrange
            var roomCount = dbContext.ChatRooms.Count();

            // Act
            var result = await roomService.CreateOrReturnExisting("user1", "user3", 1).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(4, roomCount);
            Assert.AreEqual(dbContext.ChatRooms.Count(), roomCount);
            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async Task CreateOrReturnExisting_WhenRoomDoesNotExist_ShouldCreateEntity()
        {
            // Arrange
            var roomCount = dbContext.ChatRooms.Count();
            var roomUsersCount = dbContext.ChatRoomUsers.Count();

            // Act
            var result = await roomService.CreateOrReturnExisting("user2", "user3", 2).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(roomCount + 1, dbContext.ChatRooms.Count());
            Assert.AreEqual(roomUsersCount + 2, dbContext.ChatRoomUsers.Count());
            Assert.AreEqual(5, result.Id);
        }
        #endregion

        #region Delete
        [Test]
        [TestCase(1)]
        public async Task Delete_WhenRoomExist_ShouldDeleteEntities(long id)
        {
            // Arrange
            var roomCount = dbContext.ChatRooms.Count();
            var messagesCount = dbContext.ChatMessages.Count();
            var roomUsersCount = dbContext.ChatRoomUsers.Count();

            // Act
            await roomService.Delete(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(roomCount - 1, dbContext.ChatRooms.Count());
            Assert.AreEqual(messagesCount - 2, dbContext.ChatMessages.Count());
            Assert.AreEqual(roomUsersCount - 2, dbContext.ChatRoomUsers.Count());
        }

        [Test]
        [TestCase(5)]
        public void Delete_WhenRoomDoesNotExist_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Arrange
            var roomCount = dbContext.ChatRooms.Count();
            var messagesCount = dbContext.ChatMessages.Count();
            var roomUsersCount = dbContext.ChatRoomUsers.Count();

            // Act and Assert
            Assert.That(
                async () => await roomService.Delete(id).ConfigureAwait(false),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            Assert.AreEqual(roomCount, dbContext.ChatRooms.Count());
            Assert.AreEqual(messagesCount, dbContext.ChatMessages.Count());
            Assert.AreEqual(roomUsersCount, dbContext.ChatRoomUsers.Count());
        }
        #endregion

        #region GetById
        [Test]
        [TestCase(1)]
        public async Task GetById_WhenRoomExist_ShouldReturnFoundEntity(long id)
        {
            // Act
            var result = await roomService.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, dbContext.ChatRooms.Find(id).Id);
            Assert.AreEqual(id, result.Id);
        }

        [Test]
        [TestCase(5)]
        public async Task GetById_WhenRoomDoesNotExist_ShouldReturnNull(long id)
        {
            // Act
            var result = await roomService.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }
        #endregion

        #region GetByUserId
        [Test]
        [TestCase("user1")]
        public async Task GetById_WhenRoomsExist_ShouldReturnFoundEntity(string userId)
        {
            // Act
            var result = await roomService.GetByUserId(userId).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        [TestCase("user5")]
        public async Task GetById_WhenRoomsDoNotExist_ShouldReturnEmptyList(string userId)
        {
            // Act
            var result = await roomService.GetByUserId(userId).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
        #endregion

        #region GetUniqueChatRoomBetweenUsersWithinWorkshop
        [Test]
        [TestCase("user1", "user3", 1)]
        public async Task GetUniqueChatRoomBetweenUsersWithinWorkshop_WhenRoomExists_ShouldReturnFoundEntity(string user1Id, string user2Id, long workshopId)
        {
            // Act
            var result = await roomService.GetUniqueChatRoomBetweenUsersWithinWorkshop(user1Id, user2Id, workshopId).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(workshopId, result.WorkshopId);
        }

        [Test]
        [TestCase("user2", "user4", 2)]
        public async Task GetUniqueChatRoomBetweenUsersWithinWorkshop_WhenRoomDoesNotExist_ShouldReturnNull(string user1Id, string user2Id, long workshopId)
        {
            // Act
            var result = await roomService.GetUniqueChatRoomBetweenUsersWithinWorkshop(user1Id, user2Id, workshopId).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }
        #endregion

        #region UsersCanChatBetweenEachOther
        [Test]
        [TestCase("user1", "user3", 1)]
        [TestCase("user2", "user4", 2)]
        [TestCase("user3", "admin", 1)]
        public async Task UsersCanChatBetweenEachOther_WhenTheyCan_ShouldReturnTrue(string user1Id, string user2Id, long workshopId)
        {
            // Act
            var result = await roomService.UsersCanChatBetweenEachOther(user1Id, user2Id, workshopId).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase("user1", "user2", 1)]
        [TestCase("user3", "user4", 1)]
        [TestCase("user1", "user4", 1)]
        [TestCase("user2", "admin", 1)]
        [TestCase("user3", "admin", 2)]
        public async Task UsersCanChatBetweenEachOther_WhenTheyCanNot_ShouldReturnFalse(string user1Id, string user2Id, long workshopId)
        {
            // Act
            var result = await roomService.UsersCanChatBetweenEachOther(user1Id, user2Id, workshopId).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(false, result);
        }
        #endregion

        private void SeedDatabase()
        {
            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var messages = new List<ChatMessage>()
                {
                    new ChatMessage() { Id = 1, UserId = "user1", ChatRoomId = 1, Text = "text1", CreatedTime = DateTime.Parse("2021-06-18 15:47"), IsRead = false },
                    new ChatMessage() { Id = 2, UserId = "user3", ChatRoomId = 1, Text = "text2", CreatedTime = DateTime.Parse("2021-06-18 15:48"), IsRead = false },
                    new ChatMessage() { Id = 3, UserId = "user1", ChatRoomId = 2, Text = "text3", CreatedTime = DateTime.Now, IsRead = false },
                    new ChatMessage() { Id = 4, UserId = "user4", ChatRoomId = 2, Text = "text4", CreatedTime = DateTime.Now, IsRead = false },
                };
                context.ChatMessages.AddRangeAsync(messages);

                var users = new List<User>()
                {
                    new User() { Id = "user1", Role = Role.Parent.ToString().ToLower() },
                    new User() { Id = "user2", Role = Role.Parent.ToString().ToLower() },
                    new User() { Id = "user3", Role = Role.Provider.ToString().ToLower() },
                    new User() { Id = "user4", Role = Role.Provider.ToString().ToLower() },
                    new User() { Id = "admin", Role = Role.Admin.ToString().ToLower() },
                };
                context.Users.AddRangeAsync(users);

                var workshops = new List<Workshop>()
                {
                    new Workshop()
                    {
                        Id = 1,
                        Title = "Title1",
                        ProviderId = 1,
                    },
                    new Workshop()
                    {
                        Id = 2,
                        Title = "Title2",
                        ProviderId = 2,
                    },
                    new Workshop()
                    {
                        Id = 3,
                        Title = "Title3",
                        ProviderId = 3,
                    },
                };
                context.Workshops.AddRangeAsync(workshops);

                var providers = new List<Provider>()
                {
                    new Provider() { Id = 1, UserId = users.ToArray()[2].Id },
                    new Provider() { Id = 2, UserId = users.ToArray()[3].Id },
                };
                context.Providers.AddRangeAsync(providers);

                var rooms = new List<ChatRoom>()
                {
                    new ChatRoom() { Id = 1, WorkshopId = 1, Users = new List<User>() { users.ToArray()[0], users.ToArray()[2] } },
                    new ChatRoom() { Id = 2, WorkshopId = 2, Users = new List<User>() { users.ToArray()[0], users.ToArray()[3] } },
                    new ChatRoom() { Id = 3, WorkshopId = 3, Users = new List<User>() { users.ToArray()[0], users.ToArray()[3] } },
                    new ChatRoom() { Id = 4, WorkshopId = 1, Users = new List<User>() { users.ToArray()[1], users.ToArray()[2] } },
                };
                context.ChatRooms.AddRangeAsync(rooms);

                var roomUsers = new List<ChatRoomUser>()
                {
                    new ChatRoomUser() { Id = 1, ChatRoomId = 1, UserId = users.ToArray()[0].Id },
                    new ChatRoomUser() { Id = 2, ChatRoomId = 1, UserId = users.ToArray()[2].Id },
                    new ChatRoomUser() { Id = 3, ChatRoomId = 2, UserId = users.ToArray()[0].Id },
                    new ChatRoomUser() { Id = 4, ChatRoomId = 2, UserId = users.ToArray()[3].Id },
                    new ChatRoomUser() { Id = 5, ChatRoomId = 3, UserId = users.ToArray()[0].Id },
                    new ChatRoomUser() { Id = 6, ChatRoomId = 3, UserId = users.ToArray()[3].Id },
                    new ChatRoomUser() { Id = 7, ChatRoomId = 4, UserId = users.ToArray()[1].Id },
                    new ChatRoomUser() { Id = 8, ChatRoomId = 4, UserId = users.ToArray()[2].Id },
                };
                context.ChatRoomUsers.AddRangeAsync(roomUsers);

                context.SaveChangesAsync();
            }
        }
    }
}
