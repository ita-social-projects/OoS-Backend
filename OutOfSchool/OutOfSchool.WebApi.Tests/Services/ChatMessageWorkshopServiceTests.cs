using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Hubs;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ChatWorkshop;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ChatMessageWorkshopServiceTests
{
    private static Guid chatRoomId1 = Guid.NewGuid();
    private static Guid chatRoomId2 = Guid.NewGuid();

    private static ChatMessageWorkshopCreateDto newMessage = new ChatMessageWorkshopCreateDto()
    {
        Text = "Привіт всім!",
        ParentId = Guid.NewGuid(),
        WorkshopId = Guid.NewGuid(),
    };

    private IEntityRepositorySoftDeleted<Guid, ChatMessageWorkshop> messageRepository;
    private Mock<IChatRoomWorkshopService> roomServiceMock;
    private Mock<IHubContext<ChatWorkshopHub>> workshopHub;
    private Mock<ILogger<ChatMessageWorkshopService>> loggerMock;
    private IMapper mapper;

    private DbContextOptions<OutOfSchoolDbContext> options;
    private TestOutOfSchoolDbContext dbContext;

    private IChatMessageWorkshopService messageService;

    private Mock<IHubClients> clientsMock;
    private Mock<IClientProxy> clientProxyMock;

    [SetUp]
    public void SetUp()
    {
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolChatMessageTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        options = builder.Options;
        dbContext = new TestOutOfSchoolDbContext(options);

        messageRepository = new EntityRepositorySoftDeleted<Guid, ChatMessageWorkshop>(dbContext);
        roomServiceMock = new Mock<IChatRoomWorkshopService>();
        workshopHub = new Mock<IHubContext<ChatWorkshopHub>>();
        loggerMock = new Mock<ILogger<ChatMessageWorkshopService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();

        clientsMock = new Mock<IHubClients>();
        clientProxyMock = new Mock<IClientProxy>();

        workshopHub.Setup(wh => wh.Clients).Returns(clientsMock.Object);
        clientsMock.Setup(client => client.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

        messageService = new ChatMessageWorkshopService(
            messageRepository,
            roomServiceMock.Object,
            workshopHub.Object,
            loggerMock.Object,
            mapper);

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
        var validChatRoom = new ChatRoomWorkshopDto() { Id = chatRoomId1 };
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
    [Ignore("Testing of unused method")]
    public void GetMessagesForChatRoomAsync_WhenOffsetfilterIsNull_ShouldNotThrowException()
    {
        // Arrange
        var existingRoomId = chatRoomId1;
        OffsetFilter offsetFilter = null;

        // Act and Assert
        Assert.DoesNotThrowAsync(async () => await messageService.GetMessagesForChatRoomAsync(existingRoomId, offsetFilter));
    }

    [Test]
    [Ignore("Testing of unused method")]
    public async Task GetMessagesForChatRoomAsync_WhenCalledWithAllValidParameters_ShouldReturnFoundMessages()
    {
        // Arrange
        var existingRoomId = chatRoomId1;
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
    [Ignore("Testing of unused method")]
    public async Task GetMessagesForChatRoomAsync_WhenCalledWithUnexistedRoomId_ShouldReturnEmptyList()
    {
        // Arrange
        var notExistingRoomId = Guid.NewGuid();
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
        var existingChatRoomId = chatRoomId1;
        var offsetFilter = new OffsetFilter() { From = 0, Size = 4 };
        var currentUserRoleIsProvider = Role.Provider;

        // Act
        await messageService.GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync(existingChatRoomId, offsetFilter, currentUserRoleIsProvider).ConfigureAwait(false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(dbContext.ChatMessageWorkshops.ToArray()[2].ReadDateTime);
            Assert.IsNotNull(dbContext.ChatMessageWorkshops.ToArray()[3].ReadDateTime);
        });
    }

    [Test]
    public async Task GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync_WhenCalledWithAllValidParameters_ShouldReturnFoundMessages()
    {
        // Arrange
        var existingChatRoomId = chatRoomId1;
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

    [Test]
    public async Task GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync_WhenCalledWithInValidParameters_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidChatRoomId = Guid.NewGuid();

        // Act
        var result = await messageService.GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync(invalidChatRoomId, new OffsetFilter(), Role.Provider).ConfigureAwait(false);

        // Assert
        Assert.IsEmpty(result);
    }
    #endregion

    private void SeedDatabase()
    {
        using var context = new TestOutOfSchoolDbContext(options);
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var messages = new List<ChatMessageWorkshop>()
            {
                new ChatMessageWorkshop() { Id = Guid.NewGuid(), ChatRoomId = chatRoomId1, Text = "text1", SenderRoleIsProvider = true, CreatedDateTime = DateTimeOffset.Parse("2021-06-18 15:47"), ReadDateTime = DateTimeOffset.Parse("2021-06-18 16:47") },
                new ChatMessageWorkshop() { Id = Guid.NewGuid(), ChatRoomId = chatRoomId1, Text = "text2", SenderRoleIsProvider = true, CreatedDateTime = DateTimeOffset.Parse("2021-06-18 15:48"), ReadDateTime = DateTimeOffset.Parse("2021-06-18 16:47") },
                new ChatMessageWorkshop() { Id = Guid.NewGuid(), ChatRoomId = chatRoomId1, Text = "text3", SenderRoleIsProvider = false, CreatedDateTime = DateTimeOffset.UtcNow, ReadDateTime = null },
                new ChatMessageWorkshop() { Id = Guid.NewGuid(), ChatRoomId = chatRoomId1, Text = "text4", SenderRoleIsProvider = false, CreatedDateTime = DateTimeOffset.UtcNow, ReadDateTime = null },
                new ChatMessageWorkshop() { Id = Guid.NewGuid(), ChatRoomId = chatRoomId2, Text = "text5", SenderRoleIsProvider = false, CreatedDateTime = DateTimeOffset.UtcNow, ReadDateTime = DateTimeOffset.UtcNow },
                new ChatMessageWorkshop() { Id = Guid.NewGuid(), ChatRoomId = chatRoomId2, Text = "text6", SenderRoleIsProvider = true, CreatedDateTime = DateTimeOffset.UtcNow, ReadDateTime = DateTimeOffset.UtcNow },
            };
            context.ChatMessageWorkshops.AddRange(messages);

            context.SaveChanges();
        }
    }
}