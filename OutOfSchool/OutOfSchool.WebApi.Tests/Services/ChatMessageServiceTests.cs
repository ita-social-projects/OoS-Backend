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
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ChatMessageServiceTests
    {
        private IEntityRepository<ChatMessageWorkshop> messageRepository;
        private Mock<IChatRoomWorkshopService> roomServiceMock;
        private Mock<ILogger> loggerMock;

        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext dbContext;

        private IChatMessageWorkshopService messageService;

        private ChatMessageWorkshopCreateDto newMessage;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolChatMessageTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            options = builder.Options;
            dbContext = new OutOfSchoolDbContext(options);

            messageRepository = new EntityRepository<ChatMessageWorkshop>(dbContext);
            roomServiceMock = new Mock<IChatRoomWorkshopService>();
            loggerMock = new Mock<ILogger>();

            messageService = new ChatMessageWorkshopService(messageRepository, roomServiceMock.Object, loggerMock.Object);

            SeedDatabase();
        }

        #region Create
        [Test]
        public void Create_WhenParameterIsNull_ShouldThrowArgumentNullException()
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await messageService.CreateAsync(default, Role.Provider));
        }

        [Test]
        public async Task Create_WhenMessageIsValid_ShouldSaveMessageInDatabase()
        {
            // Arrange
            var messagesCountBeforeInsert = dbContext.ChatMessageWorkshops.Count();
            var validChatRoom = new ChatRoomWorkshopDto() { Id = 1 };
            roomServiceMock.Setup(x => x.CreateOrReturnExistingAsync(newMessage.WorkshopId, newMessage.ParentId)).ReturnsAsync(validChatRoom);

            // Act
            var result = await messageService.CreateAsync(newMessage, Role.Provider).ConfigureAwait(false);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.AreNotEqual(default(long), result.Id);
                Assert.AreEqual(messagesCountBeforeInsert + 1, dbContext.ChatMessageWorkshops.Count());
            });
        }
        #endregion

        #region GetMessagesForChatRoomAsync
        [Test]
        public void GetMessagesForChatRoomAsync_WhenOffsetfilterIsNull_ShouldNotThrowException()
        {
            // Arrange
            var existingRoomId = 1;
            OffsetFilter offsetFilter = null;

            // Act and Assert
            Assert.DoesNotThrowAsync(async () => await messageService.GetMessagesForChatRoomAsync(existingRoomId, offsetFilter));
        }

        [Test]
        public async Task GetMessagesForChatRoomAsync_WhenCalledWithAllValidParameters_ShouldReturnFoundMessages()
        {
            // Arrange
            var existingRoomId = 1;
            var offsetFilter = new OffsetFilter() { From = 1, Size = 2 };

            // Act
            var result = await messageService.GetMessagesForChatRoomAsync(existingRoomId, offsetFilter).ConfigureAwait(false);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.AreEqual(offsetFilter.Size, result.Count);
                Assert.AreEqual(existingRoomId, result.FirstOrDefault()?.ChatRoomId);
            });
        }

        [Test]
        public async Task GetMessagesForChatRoomAsync_WhenCalledWithUnexistedRoomId_ShouldReturnEmptyList()
        {
            // Arrange
            var notExistingRoomId = 5;
            var offsetFilter = new OffsetFilter() { From = 0, Size = 2 };

            // Act
            var result = await messageService.GetMessagesForChatRoomAsync(notExistingRoomId, offsetFilter).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(default(int), result.Count);
        }
        #endregion

        #region GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync
        [Test]
        public async Task GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync_WhenCalledWithAllValidParameters_ShouldSetReadDateTimeToUnreadMessages()
        {
            // Arrange
            var existingChatRoomId = 1;
            var offsetFilter = new OffsetFilter() { From = 0, Size = 4 };
            var currentUserRoleIsProvider = Role.Provider;

            // Act
            await messageService.GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync(existingChatRoomId, offsetFilter, currentUserRoleIsProvider).ConfigureAwait(false);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(dbContext.ChatMessageWorkshops.Find(3L).ReadDateTime);
                Assert.IsNotNull(dbContext.ChatMessageWorkshops.Find(4L).ReadDateTime);
            });
        }

        [Test]
        public async Task GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync_WhenCalledWithAllValidParameters_ShouldReturnFoundMessages()
        {
            // Arrange
            var existingChatRoomId = 1;
            var offsetFilter = new OffsetFilter() { From = 0, Size = 4 };
            var currentUserRoleIsProvider = Role.Provider;

            // Act
            var result = await messageService.GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync(existingChatRoomId, offsetFilter, currentUserRoleIsProvider).ConfigureAwait(false);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.AreEqual(offsetFilter.Size, result.Count);
                Assert.AreEqual(existingChatRoomId, result.FirstOrDefault()?.ChatRoomId);
            });
        }
        #endregion

        private void SeedDatabase()
        {
            newMessage = new ChatMessageWorkshopCreateDto()
            {
                Text = "Привіт всім!",
                ParentId = 1,
                WorkshopId = 1,
            };

            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var messages = new List<ChatMessageWorkshop>()
                {
                    new ChatMessageWorkshop() { Id = 1, ChatRoomId = 1, Text = "text1", SenderRoleIsProvider = true, CreatedDateTime = DateTimeOffset.Parse("2021-06-18 15:47"), ReadDateTime = DateTimeOffset.Parse("2021-06-18 16:47") },
                    new ChatMessageWorkshop() { Id = 2, ChatRoomId = 1, Text = "text2", SenderRoleIsProvider = true, CreatedDateTime = DateTimeOffset.Parse("2021-06-18 15:48"), ReadDateTime = DateTimeOffset.Parse("2021-06-18 16:47") },
                    new ChatMessageWorkshop() { Id = 3, ChatRoomId = 1, Text = "text3", SenderRoleIsProvider = false, CreatedDateTime = DateTimeOffset.UtcNow, ReadDateTime = null },
                    new ChatMessageWorkshop() { Id = 4, ChatRoomId = 1, Text = "text4", SenderRoleIsProvider = false, CreatedDateTime = DateTimeOffset.UtcNow, ReadDateTime = null },
                    new ChatMessageWorkshop() { Id = 5, ChatRoomId = 2, Text = "text5", SenderRoleIsProvider = false, CreatedDateTime = DateTimeOffset.UtcNow, ReadDateTime = DateTimeOffset.UtcNow },
                    new ChatMessageWorkshop() { Id = 6, ChatRoomId = 2, Text = "text6", SenderRoleIsProvider = true, CreatedDateTime = DateTimeOffset.UtcNow, ReadDateTime = DateTimeOffset.UtcNow },
                };
                context.ChatMessageWorkshops.AddRangeAsync(messages);

                context.SaveChangesAsync();
            }
        }
    }
}
