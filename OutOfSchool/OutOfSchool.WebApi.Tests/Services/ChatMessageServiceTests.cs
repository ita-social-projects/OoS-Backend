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
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ChatMessageServiceTests
    {
        private IEntityRepository<ChatMessage> messageRepository;
        private Mock<ILogger<ChatMessageService>> loggerMoq;

        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext dbContext;

        private IChatMessageService messageService;

        private ChatMessageDto newMessage;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolChatMessageTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            options = builder.Options;
            dbContext = new OutOfSchoolDbContext(options);

            messageRepository = new EntityRepository<ChatMessage>(dbContext);
            loggerMoq = new Mock<ILogger<ChatMessageService>>();

            messageService = new ChatMessageService(messageRepository, loggerMoq.Object);

            SeedDatabase();
        }

        #region Create
        [Test]
        public async Task Create_WhenEntityIsValid_ShouldCreateEntity()
        {
            // Arrange
            var messagesCount = dbContext.ChatMessages.Count();

            // Act
            var result = await messageService.CreateAsync(newMessage).ConfigureAwait(false);

            // Assert
            Assert.AreNotEqual(default(long), result.Id);
            Assert.AreEqual(dbContext.ChatMessages.Last().Id, result.Id);
            Assert.AreEqual(newMessage.Text, result.Text);
            Assert.AreEqual(messagesCount + 1, dbContext.ChatMessages.Count());
        }
        #endregion

        #region GetMessagesForChatRoomAsync
        [Test]
        public async Task GetMessagesForChatRoomAsync_WhenCalled_ShouldReturnFoundEntities()
        {
            // Arrange
            var roomId = 1;
            var offsetFilter = new OffsetFilter() { From = 1, Size = 2 };

            // Act
            var result = await messageService.GetMessagesForChatRoomAsync(roomId, offsetFilter).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(offsetFilter.Size, result.Count());
            Assert.AreEqual(roomId, result.FirstOrDefault()?.ChatRoomId);
        }

        [Test]
        public async Task GetMessagesForChatRoomAsync_WhenCalledWithUnexistedRoomId_ShouldReturnEmptyList()
        {
            // Arrange
            var roomId = 5;
            var offsetFilter = new OffsetFilter() { From = 0, Size = 2 };

            // Act
            var result = await messageService.GetMessagesForChatRoomAsync(roomId, offsetFilter).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(default(int), result.Count());
        }
        #endregion

        #region GetById
        [TestCase(1)]
        public async Task GetById_WhenIdIsValid_ShouldReturnEntity(long id)
        {
            // Arrange
            var expected = await messageRepository.GetById(id);

            // Act
            var result = await messageService.GetByIdNoTrackingAsync(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Id, result.Id);
            Assert.AreEqual(expected.Text, result.Text);
        }

        [TestCase(0)]
        [TestCase(99)]
        public async Task GetById_WhenThereIsNoEntityWithId_ShouldReturnNull(long id)
        {
            // Act
            var result = await messageService.GetByIdNoTrackingAsync(id).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }
        #endregion

        #region Update
        [Test]
        public async Task Update_WhenEntityIsValid_ShouldUpdateEntity()
        {
            // Arrange
            var messagesCount = dbContext.ChatMessages.Count();
            var updMessage = new ChatMessageDto()
            {
                Id = 1,
                Text = "newtext",
                SenderRoleIsProvider = true,
                ChatRoomId = 1,
                CreatedTime = DateTimeOffset.UtcNow,
                IsRead = true,
            };

            // Act
            var result = await messageService.UpdateAsync(updMessage).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(updMessage.Id, result.Id);
            Assert.AreEqual(updMessage.Text, result.Text);
            Assert.AreEqual(messagesCount, dbContext.ChatMessages.Count());
            Assert.AreEqual(updMessage.Text, dbContext.ChatMessages.Find(1L).Text);
        }

        [Test]
        public void Update_WhenIdIsInvalid_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var messagesCount = dbContext.ChatMessages.Count();
            var updMessage = new ChatMessageDto()
            {
                Id = 99,
                Text = "newtext",
                SenderRoleIsProvider = true,
                ChatRoomId = 1,
                CreatedTime = DateTimeOffset.UtcNow,
                IsRead = true,
            };

            // Act and Assert
            Assert.That(
                async () => await messageService.UpdateAsync(updMessage).ConfigureAwait(false),
                Throws.Exception.TypeOf<DbUpdateConcurrencyException>());
        }
        #endregion

        #region UpdateIsReadByCurrentUserInChatRoomAsync
        [Test]
        public async Task UpdateIsRead_WhenEntityIsValid_ShouldSetIsReadTrueAndReturnNumberOfFoundEntites()
        {
            // Arrange
            var messagesCount = dbContext.ChatMessages.Count();
            var chatRoomId = 1;
            var currentUserRoleIsProvider = true;

            // Act
            var result = await messageService.UpdateIsReadByCurrentUserInChatRoomAsync(chatRoomId, currentUserRoleIsProvider).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(messagesCount, dbContext.ChatMessages.Count());
            Assert.AreEqual(2, result);
            Assert.AreEqual(true, dbContext.ChatMessages.Find(3L).IsRead);
            Assert.AreEqual(true, dbContext.ChatMessages.Find(4L).IsRead);
        }

        [Test]
        public async Task UpdateIsRead_WhenAllAreAlreadyRead_ShouldReturnNumberZero()
        {
            // Arrange
            var messagesCount = dbContext.ChatMessages.Count();
            var chatRoomId = 2;
            var currentUserRoleIsProvider = true;

            // Act
            var result = await messageService.UpdateIsReadByCurrentUserInChatRoomAsync(chatRoomId, currentUserRoleIsProvider).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(messagesCount, dbContext.ChatMessages.Count());
            Assert.AreEqual(default(int), result);
        }
        #endregion

        #region Delete
        [TestCase(1)]
        public async Task Delete_WhenIdIsValid_ShouldDeleteEntity(long id)
        {
            // Arrange
            var countMessagesBeforeDeleting = (await messageRepository.GetAll().ConfigureAwait(false)).Count();
            var item = await messageRepository.GetById(1).ConfigureAwait(false);

            // Act
            await messageService.DeleteAsync(id).ConfigureAwait(false);

            var countMessagesAfterDeleting = (await messageRepository.GetAll().ConfigureAwait(false)).Count();

            // Assert
            Assert.IsNotNull(item);
            Assert.AreEqual(countMessagesBeforeDeleting - 1, countMessagesAfterDeleting);
        }

        [TestCase(7)]
        public void Delete_WhenIdIsInvalid_ShouldThrowNullReferenceException(long id)
        {
            // Assert
            Assert.That(
                async () => await messageService.DeleteAsync(id).ConfigureAwait(false),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }
        #endregion

        private void SeedDatabase()
        {
            newMessage = new ChatMessageDto()
            {
                Text = "Привіт всім!",
                ChatRoomId = 1,
                SenderRoleIsProvider = true,
                CreatedTime = DateTimeOffset.UtcNow,
                IsRead = false,
            };

            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var messages = new List<ChatMessage>()
                {
                    new ChatMessage() { Id = 1, ChatRoomId = 1, Text = "text1", SenderRoleIsProvider = true, CreatedTime = DateTimeOffset.Parse("2021-06-18 15:47"), IsRead = true },
                    new ChatMessage() { Id = 2, ChatRoomId = 1, Text = "text2", SenderRoleIsProvider = true, CreatedTime = DateTimeOffset.Parse("2021-06-18 15:48"), IsRead = true },
                    new ChatMessage() { Id = 3, ChatRoomId = 1, Text = "text3", SenderRoleIsProvider = false, CreatedTime = DateTimeOffset.UtcNow, IsRead = false },
                    new ChatMessage() { Id = 4, ChatRoomId = 1, Text = "text4", SenderRoleIsProvider = false, CreatedTime = DateTimeOffset.UtcNow, IsRead = false },
                    new ChatMessage() { Id = 5, ChatRoomId = 2, Text = "text5", SenderRoleIsProvider = false, CreatedTime = DateTimeOffset.UtcNow, IsRead = true },
                    new ChatMessage() { Id = 6, ChatRoomId = 2, Text = "text6", SenderRoleIsProvider = true, CreatedTime = DateTimeOffset.UtcNow, IsRead = true },
                };
                context.ChatMessages.AddRangeAsync(messages);

                context.SaveChangesAsync();
            }
        }
    }
}
