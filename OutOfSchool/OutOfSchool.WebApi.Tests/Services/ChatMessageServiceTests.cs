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
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
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
        private List<ChatMessage> chatMessages;

        private ChatRoom chatRoom;


        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolChatMessageTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            options = builder.Options;

            // TODO: get rid of dbContext
            dbContext = new OutOfSchoolDbContext(options);

            messageRepository = new EntityRepository<ChatMessage>(dbContext);
            loggerMoq = new Mock<ILogger<ChatMessageService>>();

            messageService = new ChatMessageService(messageRepository, loggerMoq.Object);

            chatRoom = new ChatRoom { Id = Guid.NewGuid() };
            newMessage = ChatMessagesDtoGenerator.Generate();
            newMessage.ChatRoomId = chatRoom.Id;

            chatMessages = ChatMessagesGenerator.Generate(TestDataHelper.GetPositiveInt(10)).WithChatRoom(chatRoom);
            SeedDatabase(chatMessages);
        }

        #region Create
        [Test]
        public async Task Create_WhenEntityIsValid_ShouldCreateEntity()
        {
            // Arrange
            var messagesCount = dbContext.ChatMessages.Count();

            // Act
            var result = await messageService.Create(newMessage).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(dbContext.ChatMessages.Last().Id, result.Id);
            Assert.AreEqual(5, result.Id);
            Assert.AreEqual(newMessage.Text, result.Text);
            Assert.AreEqual(messagesCount + 1, dbContext.ChatMessages.Count());
        }
        #endregion

        #region GetAllByChatRoomId
        [Test]
        public async Task GetAllByChatRoomId_WhenCalled_ShouldReturnFoundEntities()
        {
            // Arrange
            var expectedMessages = chatMessages.Where(x => x.ChatRoomId.Equals(chatRoom.Id)).ToList();

            // Act
            var result = await messageService.GetAllByChatRoomId(chatRoom.Id).ConfigureAwait(false);

            // Assert
            CollectionAssert.AreEqual(result, expectedMessages);
        }

        //[Test]
        //[TestCase(5)]
        //public async Task GetAllByChatRoomId_WhenCalledWithUnexistedId_ShouldReturnEmptyList(long id)
        //{
        //    // Act
        //    var result = await messageService.GetAllByChatRoomId(id).ConfigureAwait(false);

        //    // Assert
        //    Assert.AreEqual(0, result.Count());
        //}
        #endregion

        //#region GetAllNotReadByUserInChatRoom
        //[Test]
        //[TestCase(1, "user1")]
        //public async Task GetAllNotReadByUserInChatRoom_WhenCalled_ShouldReturnFoundEntities(long id, string userId)
        //{
        //    // Act
        //    var result = await messageService.GetAllNotReadByUserInChatRoom(id, userId).ConfigureAwait(false);

        //    // Assert
        //    Assert.AreEqual(1, result.Count());
        //    Assert.AreEqual(id, result.First().ChatRoomId);
        //    Assert.AreNotEqual(userId, result.First().UserId);
        //    Assert.AreEqual(false, result.First().IsRead);
        //}

        //[Test]
        //[TestCase(99, "user1")]
        //public async Task GetAllNotReadByUserInChatRoom_WhenCalledWithUnexistedChatId_ShouldReturnEmptyList(long id, string userId)
        //{
        //    // Act
        //    var result = await messageService.GetAllNotReadByUserInChatRoom(id, userId).ConfigureAwait(false);

        //    // Assert
        //    Assert.AreEqual(0, result.Count());
        //}
        //#endregion

        //#region GetById
        //[Test]
        //[TestCase(1)]
        //public async Task GetById_WhenIdIsValid_ShouldReturnEntity(long id)
        //{
        //    // Arrange
        //    var expected = await messageRepository.GetById(id);

        //    // Act
        //    var result = await messageService.GetById(id).ConfigureAwait(false);

        //    // Assert
        //    Assert.AreEqual(expected.Id, result.Id);
        //}

        //[Test]
        //[TestCase(0)]
        //[TestCase(99)]
        //public async Task GetById_WhenThereIsNoEntityWithId_ShouldReturnNull(long id)
        //{
        //    // Act
        //    var result = await messageService.GetById(id).ConfigureAwait(false);

        //    // Assert
        //    Assert.IsNull(result);
        //}
        //#endregion

        //#region Update
        //[Test]
        //public async Task Update_WhenEntityIsValid_ShouldUpdateEntity()
        //{
        //    // Arrange
        //    var messagesCount = dbContext.ChatMessages.Count();
        //    var updMessage = new ChatMessageDto()
        //    {
        //        Id = 1,
        //        UserId = "user1",
        //        Text = "newtext",
        //        ChatRoomId = 1,
        //        CreatedTime = DateTimeOffset.UtcNow,
        //        IsRead = true,
        //    };

        //    // Act
        //    var result = await messageService.Update(updMessage).ConfigureAwait(false);

        //    // Assert
        //    Assert.AreEqual(updMessage.Id, result.Id);
        //    Assert.AreEqual(updMessage.Text, result.Text);
        //    Assert.AreEqual(messagesCount, dbContext.ChatMessages.Count());
        //    Assert.AreEqual("newtext", dbContext.ChatMessages.Find(1L).Text);
        //}

        //[Test]
        //public void Update_WhenIdIsInvalid_ShouldThrowArgumentOutOfRangeException()
        //{
        //    // Arrange
        //    var messagesCount = dbContext.ChatMessages.Count();
        //    var updMessage = new ChatMessageDto()
        //    {
        //        Id = 99,
        //        UserId = "user1",
        //        Text = "newtext",
        //        ChatRoomId = 1,
        //        CreatedTime = DateTimeOffset.UtcNow,
        //        IsRead = true,
        //    };

        //    // Act and Assert
        //    Assert.That(
        //        async () => await messageService.Update(updMessage).ConfigureAwait(false),
        //        Throws.Exception.TypeOf<DbUpdateConcurrencyException>());
        //}

        //#endregion

        //#region UpdateIsRead
        //[Test]
        //public async Task UpdateIsRead_WhenEntityIsValid_ShouldUpdateEntityAndSetIsReadTrue()
        //{
        //    // Arrange
        //    var messagesCount = dbContext.ChatMessages.Count();
        //    var list = new List<ChatMessageDto>()
        //    {
        //        new ChatMessageDto()
        //        {
        //            Id = 1,
        //        },
        //        new ChatMessageDto()
        //        {
        //            Id = 2,
        //        },
        //    };

        //    // Act
        //    var result = await messageService.UpdateIsRead(list).ConfigureAwait(false);

        //    // Assert
        //    Assert.AreEqual(list.Count, result.Count());
        //    Assert.AreEqual(messagesCount, dbContext.ChatMessages.Count());
        //    Assert.AreEqual(true, dbContext.ChatMessages.Find(1L).IsRead);
        //    Assert.AreEqual(true, dbContext.ChatMessages.Find(2L).IsRead);
        //}

        //[Test]
        //public void UpdateIsRead_WhenOneOfTheIdsIsInvalid_ShouldUpdateSomeEntitiesBeforeThroeException()
        //{
        //    // Arrange
        //    var messagesCount = dbContext.ChatMessages.Count();
        //    var list = new List<ChatMessageDto>()
        //    {
        //        new ChatMessageDto()
        //        {
        //            Id = 3,
        //        },
        //        new ChatMessageDto()
        //        {
        //            Id = 9,
        //        },
        //        new ChatMessageDto()
        //        {
        //            Id = 4,
        //        },
        //    };

        //    // Act and Assert
        //    Assert.That(
        //        async () => await messageService.UpdateIsRead(list).ConfigureAwait(false),
        //        Throws.Exception.TypeOf<DbUpdateConcurrencyException>());

        //    Assert.AreEqual(true, dbContext.ChatMessages.Find(3L).IsRead);
        //    Assert.AreNotEqual(true, dbContext.ChatMessages.Find(4L).IsRead);
        //}
        //#endregion

        //#region Delete
        //[Test]
        //[TestCase(1)]
        //public async Task Delete_WhenIdIsValid_ShouldDeleteEntity(long id)
        //{
        //    // Arrange
        //    var countMessagesBeforeDeleting = (await messageRepository.GetAll().ConfigureAwait(false)).Count();
        //    var item = await messageRepository.GetById(1).ConfigureAwait(false);

        //    // Act
        //    await messageService.Delete(id).ConfigureAwait(false);

        //    var countMessagesAfterDeleting = (await messageRepository.GetAll().ConfigureAwait(false)).Count();

        //    // Assert
        //    Assert.IsNotNull(item);
        //    Assert.AreEqual(countMessagesBeforeDeleting - 1, countMessagesAfterDeleting);
        //}

        //[Test]
        //[TestCase(7)]
        //public void Delete_WhenIdIsInvalid_ShouldThrowNullReferenceException(long id)
        //{
        //    // Assert
        //    Assert.That(
        //        async () => await messageService.Delete(id).ConfigureAwait(false),
        //        Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        //}
        //#endregion

        private void SeedDatabase(List<ChatMessage> chatMessages)
        {
            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                chatMessages.WithUser(new User { Id = Guid.NewGuid().ToString() });
                context.ChatMessages.AddRangeAsync(chatMessages);

                context.SaveChangesAsync();
            }
        }
    }
}
