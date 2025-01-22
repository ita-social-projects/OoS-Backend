using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.ChatWorkshop;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ChatRoomWorkshopServiceTests
{
    private static User[] users;

    private static Parent[] parents;

    private static Provider[] providers;

    private static Workshop[] workshops;

    private static ChatRoomWorkshop[] rooms;

    private IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop> roomRepository;
    private Mock<IChatRoomWorkshopModelForChatListRepository> roomWithSpecialModelRepositoryMock;
    private Mock<IWorkshopService> workshopServiceMock;
    private Mock<IBlockedProviderParentService> blockedProviderParentServiceMock;
    private Mock<ILogger<ChatRoomWorkshopService>> loggerMock;
    private IMapper mapper;

    private DbContextOptions<OutOfSchoolDbContext> options;
    private TestOutOfSchoolDbContext dbContext;

    private IChatRoomWorkshopService roomService;

    [SetUp]
    public void SetUp()
    {
        users = UserGenerator.Generate(4).ToArray();
        users[0].Role = Role.Parent.ToString().ToLower();
        users[1].Role = Role.Parent.ToString().ToLower();
        users[2].Role = Role.Provider.ToString().ToLower();
        users[3].Role = Role.Provider.ToString().ToLower();

        parents =
        [
            new Parent() { Id = Guid.NewGuid(), UserId = users[0].Id, Gender = Gender.Male, DateOfBirth = DateTime.Today},
            new Parent() { Id = Guid.NewGuid(), UserId = users[1].Id, Gender = Gender.Female, DateOfBirth = DateTime.Today},
        ];

        providers = ProvidersGenerator.Generate(2).ToArray();
        providers[0].UserId = users[2].Id;
        providers[1].UserId = users[3].Id;

        workshops = WorkshopGenerator.Generate(3).ToArray();
        workshops[0].ProviderId = providers[0].Id;
        workshops[1].ProviderId = providers[0].Id;
        workshops[2].ProviderId = providers[1].Id;

        rooms =
        [
            new ChatRoomWorkshop() { Id = Guid.NewGuid(), WorkshopId = workshops[0].Id, ParentId = parents[0].Id, },
            new ChatRoomWorkshop() { Id = Guid.NewGuid(), WorkshopId = workshops[0].Id, ParentId = parents[1].Id, },
            new ChatRoomWorkshop() { Id = Guid.NewGuid(), WorkshopId = workshops[1].Id, ParentId = parents[0].Id, },
            new ChatRoomWorkshop() { Id = Guid.NewGuid(), WorkshopId = workshops[1].Id, ParentId = parents[1].Id, },
        ];

        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolChatRoomTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        options = builder.Options;
        dbContext = new TestOutOfSchoolDbContext(options);

        roomRepository = new EntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>(dbContext);
        roomWithSpecialModelRepositoryMock = new Mock<IChatRoomWorkshopModelForChatListRepository>();
        loggerMock = new Mock<ILogger<ChatRoomWorkshopService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        workshopServiceMock = new Mock<IWorkshopService>();
        workshopServiceMock.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(new WorkshopDto() { ProviderId = Guid.Empty });
        blockedProviderParentServiceMock = new Mock<IBlockedProviderParentService>();
        blockedProviderParentServiceMock.Setup(x => x.IsBlocked(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        roomService = new ChatRoomWorkshopService(
            roomRepository,
            loggerMock.Object,
            roomWithSpecialModelRepositoryMock.Object,
            workshopServiceMock.Object,
            blockedProviderParentServiceMock.Object,
            mapper);

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

    [Test]
    public async Task CreateOrReturnExisting_WhenParentIsBlockedByProvider_ShouldReturnNull()
    {
        // Arrange
        var roomCount = dbContext.ChatRoomWorkshops.Count();
        var workshopId = workshops[2].Id;
        var parentId = parents[0].Id;

        blockedProviderParentServiceMock.Setup(x => x.IsBlocked(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        var chatRoomService = new ChatRoomWorkshopService(
            roomRepository,
            loggerMock.Object,
            roomWithSpecialModelRepositoryMock.Object,
            workshopServiceMock.Object,
            blockedProviderParentServiceMock.Object,
            mapper);

        // Act
        var result = await roomService.CreateOrReturnExistingAsync(workshopId, parentId).ConfigureAwait(false);

        // Assert
        Assert.Null(result);
    }
    #endregion

    #region Delete
    [Test]
    public async Task Delete_WhenRoomExist_ShouldDeleteEntities()
    {
        // Arrange
        var existingId = rooms[0].Id;
        var roomCount = dbContext.ChatRoomWorkshops.Count(x => !x.IsDeleted);

        // Act
        await roomService.DeleteAsync(existingId).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(roomCount - 1, dbContext.ChatRoomWorkshops.Count(x => !x.IsDeleted));
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
    [Ignore("Testing of unused method")]
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
    [Ignore("Testing of unused method")]
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

    #region GetChatRoomIdsByWorkshopIdsAsync
    [Test]
    public async Task GetChatRoomIdsByWorkshopIdsAsync_WhenRoomExist_ShouldReturnFoundEntity()
    {
        // Arrange
        var existingworkshopIds = workshops.Select(x => x.Id);

        // Act
        var result = await roomService.GetChatRoomIdsByWorkshopIdsAsync(existingworkshopIds).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<IEnumerable<Guid>>(result);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Any());
    }

    [Test]
    public async Task GetChatRoomIdsByWorkshopIdsAsync_WhenRoomDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var notExistingWorkshopIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = await roomService.GetChatRoomIdsByWorkshopIdsAsync(notExistingWorkshopIds).ConfigureAwait(false);

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

    #region GetCurrentUserUnreadMessagesCountAsync
    [Test]
    public async Task GetCurrentUserUnreadMessagesCount_WhenUserRoleIsParent_ShouldReturnCorrectCount()
    {
        // Arrange
        var parentId = parents[0].Id;
        var role = Role.Parent;
        var chatRooms = new List<ChatRoomWorkshopForChatList>
        {
            new ChatRoomWorkshopForChatList { NotReadByCurrentUserMessagesCount = 1 },
            new ChatRoomWorkshopForChatList { NotReadByCurrentUserMessagesCount = 1 },
        };

        var expectedCount = chatRooms.Count(r => r.NotReadByCurrentUserMessagesCount > 0);

        roomWithSpecialModelRepositoryMock
            .Setup(x => x.GetByParentIdAsync(parentId, It.IsAny<bool>()))
            .ReturnsAsync(chatRooms);

        // Act
        var result = await roomService.GetCurrentUserUnreadMessagesCountAsync(parentId, role).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expectedCount, result);
    }

    [Test]
    public async Task GetCurrentUserUnreadMessagesCount_WhenUserRoleIsProvider_ShouldReturnCorrectCount()
    {
        // Arrange
        var providerId = providers[0].Id;
        var role = Role.Provider;
        var chatRooms = new List<ChatRoomWorkshopForChatList>
        {
            new ChatRoomWorkshopForChatList { NotReadByCurrentUserMessagesCount = 1 },
            new ChatRoomWorkshopForChatList { NotReadByCurrentUserMessagesCount = 1 },
            new ChatRoomWorkshopForChatList { NotReadByCurrentUserMessagesCount = 1 },
        };

        var expectedCount = chatRooms.Count(r => r.NotReadByCurrentUserMessagesCount > 0);

        roomWithSpecialModelRepositoryMock
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<bool>()))
            .ReturnsAsync(chatRooms);

        // Act
        var result = await roomService.GetCurrentUserUnreadMessagesCountAsync(providerId, role).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expectedCount, result);
    }

    [Test]
    public void GetCurrentUserUnreadMessagesCount_WhenUserRoleIsNotValid_ShouldThrowArgumentException()
    {
        // Arrange
        var providerId = providers[0].Id;
        var role = Role.TechAdmin;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await roomService.GetCurrentUserUnreadMessagesCountAsync(providerId, role).ConfigureAwait(false);
        });
    }
    #endregion

    #region GetChatRoomByFilter
    [Test]
    public async Task GetChatRoomByFilter_WhenUserIsInValidParent_ShouldReturnEmptySearchResult()
    {
        // Arrange
        var invalidParentId = Guid.NewGuid();
        var expectedEmptyList = new List<ChatRoomWorkshopDtoWithLastMessage>();

        roomWithSpecialModelRepositoryMock
            .Setup(x => x.GetByParentIdAsync(invalidParentId, false))
            .Returns(Task.FromResult(new List<ChatRoomWorkshopForChatList>()));

        // Act
        var result = await roomService
            .GetChatRoomByFilter(new ChatWorkshopFilter(), invalidParentId, false);

        // Assert
        Assert.That(result is not null);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(expectedEmptyList, result.Entities);
    }

    [Test]
    public async Task GetChatRoomByFilter_WhenUserIsInValidProvider_ShouldReturnEmptySearchResult()
    {
        // Arrange
        var invalidProviderId = Guid.NewGuid();
        var expectedEmptyList = new List<ChatRoomWorkshopDtoWithLastMessage>();

        roomWithSpecialModelRepositoryMock
            .Setup(x => x.GetByWorkshopIdsAsync(It.IsAny<IEnumerable<Guid>>(), true))
            .Returns(Task.FromResult(new List<ChatRoomWorkshopForChatList>()));

        // Act
        var result = await roomService
            .GetChatRoomByFilter(new ChatWorkshopFilter(), invalidProviderId, true);

        // Assert
        Assert.That(result is not null);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.AreEqual(expectedEmptyList, result.Entities);
    }
    #endregion

    private void SeedDatabase()
    {
        using var context = new TestOutOfSchoolDbContext(options);
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Users.AddRange(users);
            context.Parents.AddRange(parents);
            context.Providers.AddRange(providers);
            context.Workshops.AddRange(workshops);
            context.ChatRoomWorkshops.AddRange(rooms);

            context.SaveChanges();
        }
    }
}