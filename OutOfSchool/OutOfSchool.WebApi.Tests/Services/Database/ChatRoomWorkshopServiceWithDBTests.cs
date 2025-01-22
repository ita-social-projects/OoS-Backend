using System;
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
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ChatRoomWorkshopServiceWithDBTests
{
    private static User[] users;

    private static Parent[] parents;

    private static Provider[] providers;

    private static Workshop[] workshops;

    private static ChatRoomWorkshop[] rooms;

    private IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop> roomRepository;
    private IChatRoomWorkshopModelForChatListRepository roomWorkshopModelForChatListRepository;
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
            new ChatRoomWorkshop() { Id = Guid.NewGuid(), WorkshopId = workshops[2].Id, ParentId = parents[0].Id, },
            new ChatRoomWorkshop() { Id = Guid.NewGuid(), WorkshopId = workshops[2].Id, ParentId = parents[1].Id, },
        ];

        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolChatRoomTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        options = builder.Options;
        dbContext = new TestOutOfSchoolDbContext(options);

        roomRepository = new EntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>(dbContext);
        roomWorkshopModelForChatListRepository = new ChatRoomWorkshopModelForChatListRepository(dbContext);
        loggerMock = new Mock<ILogger<ChatRoomWorkshopService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        workshopServiceMock = new Mock<IWorkshopService>();
        workshopServiceMock.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(new WorkshopDto() { ProviderId = Guid.Empty });
        blockedProviderParentServiceMock = new Mock<IBlockedProviderParentService>();
        blockedProviderParentServiceMock.Setup(x => x.IsBlocked(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        roomService = new ChatRoomWorkshopService(
            roomRepository,
            loggerMock.Object,
            roomWorkshopModelForChatListRepository,
            workshopServiceMock.Object,
            blockedProviderParentServiceMock.Object,
            mapper);

        SeedDatabase();
    }

    #region GetChatRoomByFilter
    [Test]
    public async Task GetChatRoomByFilter_WhenUserIsValidProvider_ShouldReturnRightSearchResult()
    {
        // Arrange
        var existingProviderId = providers[0].Id;
        var searchMessagesForProvider = true;

        var expectedRoomsCount = rooms
            .Count(item => item.Workshop.ProviderId == existingProviderId);

        // Act
        var result = await roomService
            .GetChatRoomByFilter(new ChatWorkshopFilter(), existingProviderId, searchMessagesForProvider);

        // Assert
        Assert.That(result is not null);
        Assert.AreEqual(expectedRoomsCount, result.TotalAmount);
        Assert.That(result.Entities.All(r => r.Workshop.ProviderId == existingProviderId));
    }

    [Test]
    public async Task GetChatRoomByFilter_WhenUserIsValidParent_ShouldReturnRightSearchResult()
    {
        // Arrange
        var existingParentId = parents[0].Id;
        var searchMessagesForProvider = false;
        var expectedRoomsCount = rooms
            .Count(item => item.ParentId == existingParentId);

        // Act
        var result = await roomService
            .GetChatRoomByFilter(new ChatWorkshopFilter(), existingParentId, searchMessagesForProvider);

        // Assert
        Assert.That(result is not null);
        Assert.AreEqual(expectedRoomsCount, result.TotalAmount);
        Assert.That(result.Entities.All(r => r.ParentId == existingParentId));
    }

    [Test]
    public async Task GetChatRoomByFilter_WhenFilterIsAppliedForProvider_ShouldReturnRightSearchResult()
    {
        // Arrange
        var filterSize = 2;
        var filterFrom = 1;
        var searchMessagesForProvider = true;
        var existingProviderId = providers[0].Id;
        var expectedRooms = rooms
            .Where(item => item.Workshop.ProviderId == existingProviderId);

        var expectedTotalAmount = expectedRooms.Count();
        var expectedResultCount = expectedRooms
            .Skip(filterFrom)
            .Take(filterSize)
            .Count();

        // Act
        var result = await roomService
            .GetChatRoomByFilter(new ChatWorkshopFilter { Size = filterSize, From = filterFrom }, existingProviderId, searchMessagesForProvider);

        // Assert
        Assert.That(result is not null);
        Assert.AreEqual(expectedTotalAmount, result.TotalAmount);
        Assert.AreEqual(expectedResultCount, result.Entities.Count);
        Assert.That(result.Entities.All(r => r.Workshop.ProviderId == existingProviderId));
    }

    [Test]
    public async Task GetChatRoomByFilter_WhenFilterIsAppliedForParent_ShouldReturnRightSearchResult()
    {
        // Arrange
        var filterSize = 2;
        var filterFrom = 1;
        var searchMessagesForProvider = false;
        var existingParentId = parents[0].Id;
        var expectedRooms = rooms
            .Where(item => item.ParentId == existingParentId);

        var expectedTotalAmount = expectedRooms.Count();
        var expectedResultCount = expectedRooms
            .Skip(filterFrom)
            .Take(filterSize)
            .Count();

        // Act
        var result = await roomService
            .GetChatRoomByFilter(new ChatWorkshopFilter { Size = filterSize, From = filterFrom }, existingParentId, searchMessagesForProvider);

        // Assert
        Assert.That(result is not null);
        Assert.AreEqual(expectedTotalAmount, result.TotalAmount);
        Assert.AreEqual(expectedResultCount, result.Entities.Count);
        Assert.That(result.Entities.All(r => r.ParentId == existingParentId));
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