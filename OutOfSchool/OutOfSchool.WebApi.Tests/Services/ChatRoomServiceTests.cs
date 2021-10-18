//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;
//using OutOfSchool.Services;
//using OutOfSchool.Services.Enums;
//using OutOfSchool.Services.Models;
//using OutOfSchool.Services.Repository;
//using OutOfSchool.Tests.Common;
//using OutOfSchool.WebApi.Services;

//namespace OutOfSchool.WebApi.Tests.Services
//{
//    [TestFixture]
//    public class ChatRoomServiceTests
//    {
//        private IEntityRepository<ChatRoom> roomRepository;
//        private IEntityRepository<User> userRepository;
//        private IWorkshopRepository workshopRepository;
//        private Mock<ILogger<ChatRoomService>> loggerMoq;

//        private DbContextOptions<OutOfSchoolDbContext> options;
//        private OutOfSchoolDbContext dbContext;

//        private IChatRoomService roomService;

//        private List<ChatRoom> chatRooms;

//        [SetUp]
//        public void SetUp()
//        {
//            var builder =
//                new DbContextOptionsBuilder<OutOfSchoolDbContext>()
//                .UseInMemoryDatabase(databaseName: "OutOfSchoolChatRoomTestDB")
//                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

//            options = builder.Options;
//            dbContext = new OutOfSchoolDbContext(options);

//            roomRepository = new EntityRepository<ChatRoom>(dbContext);
//            userRepository = new EntityRepository<User>(dbContext);
//            workshopRepository = new WorkshopRepository(dbContext);
//            loggerMoq = new Mock<ILogger<ChatRoomService>>();

//            roomService = new ChatRoomService(roomRepository, userRepository, workshopRepository, loggerMoq.Object);

//            SeedDatabase();
//        }

//        #region CreateOrReturnExisting
//        [Test]
//        public async Task CreateOrReturnExisting_WhenRoomExists_ShouldReturnEntity()
//        {
//            // Arrange
//            var roomCount = dbContext.ChatRooms.Count();

//            // Act
//            var result = await roomService.CreateOrReturnExisting("user1", "user3", 1).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(4, roomCount);
//            Assert.AreEqual(dbContext.ChatRooms.Count(), roomCount);
//            Assert.AreEqual(1, result.Id);
//        }

//        [Test]
//        public async Task CreateOrReturnExisting_WhenRoomDoesNotExist_ShouldCreateEntity()
//        {
//            // Arrange
//            var roomCount = dbContext.ChatRooms.Count();
//            var roomUsersCount = dbContext.ChatRoomUsers.Count();

//            // Act
//            var result = await roomService.CreateOrReturnExisting("user2", "user3", 2).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(roomCount + 1, dbContext.ChatRooms.Count());
//            Assert.AreEqual(roomUsersCount + 2, dbContext.ChatRoomUsers.Count());
//            Assert.AreEqual(5, result.Id);
//        }
//        #endregion

//        #region Delete
//        [Test]
//        public async Task Delete_WhenRoomExist_ShouldDeleteEntities()
//        {
//            // Arrange
//            var roomCount = dbContext.ChatRooms.Count();
//            var messagesCount = dbContext.ChatMessages.Count();
//            var roomUsersCount = dbContext.ChatRoomUsers.Count();
//            var chatRoomToDelete = TestDataHelper.RandomItem(chatRooms);

//            // Act
//            await roomService.Delete(chatRoomToDelete.Id).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(roomCount - 1, dbContext.ChatRooms.Count());
//            Assert.AreEqual(messagesCount - 2, dbContext.ChatMessages.Count());
//            Assert.AreEqual(roomUsersCount - 2, dbContext.ChatRoomUsers.Count());
//        }

//        [Test]
//        public void Delete_WhenRoomDoesNotExist_ShouldThrowArgumentOutOfRangeException()
//        {
//            // Arrange
//            var roomCount = dbContext.ChatRooms.Count();
//            var messagesCount = dbContext.ChatMessages.Count();
//            var roomUsersCount = dbContext.ChatRoomUsers.Count();

//            // Act and Assert
//            Assert.That(
//                async () => await roomService.Delete(Guid.NewGuid()).ConfigureAwait(false),
//                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
//            Assert.AreEqual(roomCount, dbContext.ChatRooms.Count());
//            Assert.AreEqual(messagesCount, dbContext.ChatMessages.Count());
//            Assert.AreEqual(roomUsersCount, dbContext.ChatRoomUsers.Count());
//        }
//        #endregion

//        #region GetById
//        //[Test]
//        //public async Task GetById_WhenRoomExist_ShouldReturnFoundEntity()
//        //{
//        //    // Act
//        //    var chatRoom = TestDataHelper.RandomItem(chatRooms);
//        //    var result = await roomService.GetById(chatRoom.Id).ConfigureAwait(false);

//        //    // Assert
//        //    Assert.IsNotNull(result);
//        //    Assert.AreEqual(id, result.Id);
//        //}

//        [Test]
//        public async Task GetById_WhenRoomDoesNotExist_ShouldReturnNull()
//        {
//            // Act
//            var result = await roomService.GetById(Guid.NewGuid()).ConfigureAwait(false);

//            // Assert
//            Assert.IsNull(result);
//        }
//        #endregion

//        #region GetByUserId
//        [Test]
//        [TestCase("user1")]
//        public async Task GetById_WhenRoomsExist_ShouldReturnFoundEntity(string userId)
//        {
//            // Act
//            var result = await roomService.GetByUserId(userId).ConfigureAwait(false);

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual(3, result.Count());
//        }

//        [Test]
//        [TestCase("user5")]
//        public async Task GetById_WhenRoomsDoNotExist_ShouldReturnEmptyList(string userId)
//        {
//            // Act
//            var result = await roomService.GetByUserId(userId).ConfigureAwait(false);

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual(0, result.Count());
//        }
//        #endregion

//        #region GetUniqueChatRoomBetweenUsersWithinWorkshop
//        [Test]
//        [TestCase("user1", "user3", 1)]
//        public async Task GetUniqueChatRoomBetweenUsersWithinWorkshop_WhenRoomExists_ShouldReturnFoundEntity(string user1Id, string user2Id, long workshopId)
//        {
//            // Act
//            var result = await roomService.GetUniqueChatRoomBetweenUsersWithinWorkshop(user1Id, user2Id, workshopId).ConfigureAwait(false);

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual(1, result.Id);
//            Assert.AreEqual(workshopId, result.WorkshopId);
//        }

//        [Test]
//        [TestCase("user2", "user4", 2)]
//        public async Task GetUniqueChatRoomBetweenUsersWithinWorkshop_WhenRoomDoesNotExist_ShouldReturnNull(string user1Id, string user2Id, long workshopId)
//        {
//            // Act
//            var result = await roomService.GetUniqueChatRoomBetweenUsersWithinWorkshop(user1Id, user2Id, workshopId).ConfigureAwait(false);

//            // Assert
//            Assert.IsNull(result);
//        }
//        #endregion

//        #region UsersCanChatBetweenEachOther
//        [Test]
//        [TestCase("user1", "user3", 1)]
//        [TestCase("user2", "user4", 2)]
//        [TestCase("user3", "admin", 1)]
//        public async Task UsersCanChatBetweenEachOther_WhenTheyCan_ShouldReturnTrue(string user1Id, string user2Id, long workshopId)
//        {
//            // Act
//            var result = await roomService.UsersCanChatBetweenEachOther(user1Id, user2Id, workshopId).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(true, result);
//        }

//        [Test]
//        [TestCase("user1", "user2", 1)]
//        [TestCase("user3", "user4", 1)]
//        [TestCase("user1", "user4", 1)]
//        [TestCase("user2", "admin", 1)]
//        [TestCase("user3", "admin", 2)]
//        public async Task UsersCanChatBetweenEachOther_WhenTheyCanNot_ShouldReturnFalse(string user1Id, string user2Id, long workshopId)
//        {
//            // Act
//            var result = await roomService.UsersCanChatBetweenEachOther(user1Id, user2Id, workshopId).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(false, result);
//        }
//        #endregion

//        private void SeedDatabase()
//        {
//            using var context = new OutOfSchoolDbContext(options);
//            {
//                context.Database.EnsureDeleted();
//                context.Database.EnsureCreated();

//                var workshops = new List<Workshop>()
//                {
//                    new Workshop()
//                    {
//                        Id = 1,
//                        Title = "Title1",
//                        ProviderId = 1,
//                    },
//                    new Workshop()
//                    {
//                        Id = 2,
//                        Title = "Title2",
//                        ProviderId = 2,
//                    },
//                    new Workshop()
//                    {
//                        Id = 3,
//                        Title = "Title3",
//                        ProviderId = 3,
//                    },
//                };
//                context.Workshops.AddRangeAsync(workshops);

//                var users = new List<User>()
//                {
//                    new User() { Id = "user1", Role = Role.Parent.ToString().ToLower() },
//                    new User() { Id = "user2", Role = Role.Parent.ToString().ToLower() },
//                    new User() { Id = "user3", Role = Role.Provider.ToString().ToLower() },
//                    new User() { Id = "user4", Role = Role.Provider.ToString().ToLower() },
//                    new User() { Id = "admin", Role = Role.Admin.ToString().ToLower() },
//                };
//                context.Users.AddRangeAsync(users);

//                chatRooms = new List<ChatRoom>()
//                {
//                    new ChatRoom() { Id = Guid.NewGuid(), WorkshopId = 1, Users = new List<User>() { users[0], users[2] } },
//                    new ChatRoom() { Id = Guid.NewGuid(), WorkshopId = 2, Users = new List<User>() { users[0], users[3] } },
//                    new ChatRoom() { Id = Guid.NewGuid(), WorkshopId = 3, Users = new List<User>() { users[0], users[3] } },
//                    new ChatRoom() { Id = Guid.NewGuid(), WorkshopId = 1, Users = new List<User>() { users[1], users[2] } },
//                };

//                context.ChatRooms.AddRangeAsync(chatRooms);

//                var messages = new List<ChatMessage>()
//                {
//                    new ChatMessage() { Id = Guid.NewGuid(), UserId = "user1", ChatRoomId = chatRooms[0].Id, ChatRoom = chatRooms[0], Text = "text1", CreatedTime = DateTimeOffset.Parse("2021-06-18 15:47"), IsRead = false },
//                    new ChatMessage() { Id = Guid.NewGuid(), UserId = "user3", ChatRoomId = chatRooms[0].Id, ChatRoom = chatRooms[0], Text = "text2", CreatedTime = DateTimeOffset.Parse("2021-06-18 15:48"), IsRead = false },
//                    new ChatMessage() { Id = Guid.NewGuid(), UserId = "user1", ChatRoomId = chatRooms[1].Id, ChatRoom = chatRooms[1], Text = "text3", CreatedTime = DateTimeOffset.UtcNow, IsRead = false },
//                    new ChatMessage() { Id = Guid.NewGuid(), UserId = "user4", ChatRoomId = chatRooms[1].Id, ChatRoom = chatRooms[1], Text = "text4", CreatedTime = DateTimeOffset.UtcNow, IsRead = false },
//                };
//                context.ChatMessages.AddRangeAsync(messages);

//                var providers = new List<Provider>()
//                {
//                    new Provider() { Id = 1, UserId = users[2].Id },
//                    new Provider() { Id = 2, UserId = users[3].Id },
//                };
//                context.Providers.AddRangeAsync(providers);

//                var roomUsers = new List<ChatRoomUser>()
//                {
//                    new ChatRoomUser() { Id = Guid.NewGuid(), ChatRoomId = chatRooms[0].Id, ChatRoom = chatRooms[0], UserId = users[0].Id },
//                    new ChatRoomUser() { Id = Guid.NewGuid(), ChatRoomId = chatRooms[0].Id, ChatRoom = chatRooms[0], UserId = users[2].Id },
//                    new ChatRoomUser() { Id = Guid.NewGuid(), ChatRoomId = chatRooms[1].Id, ChatRoom = chatRooms[1], UserId = users[0].Id },
//                    new ChatRoomUser() { Id = Guid.NewGuid(), ChatRoomId = chatRooms[1].Id, ChatRoom = chatRooms[1], UserId = users[3].Id },
//                    new ChatRoomUser() { Id = Guid.NewGuid(), ChatRoomId = chatRooms[2].Id, ChatRoom = chatRooms[2], UserId = users[0].Id },
//                    new ChatRoomUser() { Id = Guid.NewGuid(), ChatRoomId = chatRooms[2].Id, ChatRoom = chatRooms[2], UserId = users[3].Id },
//                    new ChatRoomUser() { Id = Guid.NewGuid(), ChatRoomId = chatRooms[3].Id, ChatRoom = chatRooms[3], UserId = users[1].Id },
//                    new ChatRoomUser() { Id = Guid.NewGuid(), ChatRoomId = chatRooms[3].Id, ChatRoom = chatRooms[3], UserId = users[2].Id },
//                };
//                context.ChatRoomUsers.AddRangeAsync(roomUsers);

//                context.SaveChangesAsync();
//            }
//        }
//    }
//}
