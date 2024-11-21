using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class BlockedProviderParentRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private List<Parent> parents;
    private List<Provider> providers;
    private List<Workshop> workshops;
    private List<Application> applications;
    private List<ChatRoomWorkshop> chatRooms;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .Options;

        await Seed();
    }

    [Test]
    public async Task Block_ShouldAddBlockedProviderParentModelToDatabase()
    {
        // Arrange
        using var context = GetContext();
        var repository = GetRepository(context);
        var blockedProviderParent = new BlockedProviderParent
        {
            Id = Guid.NewGuid(),
            ParentId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Reason = "Test",
            UserIdBlock = "TestId",
        };

        var expectedBlockedProviderParentCount = context.BlockedProviderParents.Count() + 1;

        // Act
        var result = await repository.Block(blockedProviderParent);

        // Assert
        Assert.AreEqual(expectedBlockedProviderParentCount, context.BlockedProviderParents.Count());
        Assert.That(result is not null);
        Assert.AreEqual(result.Id, blockedProviderParent.Id);
        Assert.That(result.DateTimeTo is null);
    }

    [Test]
    public async Task UnBlock_ShouldModifyBlockedProviderParentDateTimeToProperty()
    {
        // Arrange
        using var context = GetContext();
        var repository = GetRepository(context);
        var blockedProviderParent = new BlockedProviderParent
        {
            Id = Guid.NewGuid(),
            ParentId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Reason = "Test",
            UserIdBlock = "TestId",
        };
        var expectedBlockedProviderParentCount = context.BlockedProviderParents.Count() + 1;

        await repository.Block(blockedProviderParent);

        // Act
        blockedProviderParent.DateTimeTo = DateTime.Now;
        blockedProviderParent.UserIdUnblock = "TestId";
        var result = await repository.UnBlock(blockedProviderParent);

        // Assert
        Assert.AreEqual(expectedBlockedProviderParentCount, context.BlockedProviderParents.Count());
        Assert.That(result is not null);
        Assert.AreEqual(result.Id, blockedProviderParent.Id);
        Assert.That(result.DateTimeTo is not null);
    }

    [Test]
    public async Task Block_ShouldBlockApplicationAndChatRoomWorkshop()
    {
        // Arrange
        using var context = GetContext();
        var repository = GetRepository(context);
        var parentId = parents[0].Id;
        var providerId = providers[0].Id;
        var blockedProviderParent = new BlockedProviderParent
        {
            Id = Guid.NewGuid(),
            ParentId = parentId,
            ProviderId = providerId,
            Reason = "Test",
            UserIdBlock = "TestId",
        };

        var expectedBlockedApplications = context.Applications.Count(a => a.IsBlockedByProvider) + 1;
        var expectedBlockedChatRooms = context.ChatRoomWorkshops.Count(c => c.IsBlockedByProvider) + 1;
        var aplication = context.Applications.FirstOrDefault(
            a => a.ParentId == parentId &&
            a.Workshop.ProviderId == providerId);

        var chatRoom = context.ChatRoomWorkshops.FirstOrDefault(
            c => c.ParentId == parentId &&
            c.Workshop.ProviderId == providerId);

        // Act
        var result = await repository.Block(blockedProviderParent);

        // Assert
        Assert.That(result is not null);
        Assert.AreEqual(expectedBlockedApplications, context.Applications.Count(a => a.IsBlockedByProvider));
        Assert.AreEqual(expectedBlockedChatRooms, context.ChatRoomWorkshops.Count(c => c.IsBlockedByProvider));
        Assert.True(aplication.IsBlockedByProvider);
        Assert.True(chatRoom.IsBlockedByProvider);
    }

    [Test]
    public async Task UnBlock_ShouldUnBlockApplicationAndChatRoomWorkshop()
    {
        // Arrange
        using var context = GetContext();
        var repository = GetRepository(context);
        var parentId = parents[1].Id;
        var providerId = providers[1].Id;
        var blockedProviderParent = new BlockedProviderParent
        {
            Id = Guid.NewGuid(),
            ParentId = parentId,
            ProviderId = providerId,
            Reason = "Test",
            UserIdBlock = "TestId",
        };

        await repository.Block(blockedProviderParent);

        var expectedUnBlockedApplications = context.Applications.Count(a => !a.IsBlockedByProvider) + 1;
        var expectedUnBlockedChatRooms = context.ChatRoomWorkshops.Count(c => !c.IsBlockedByProvider) + 1;
        var aplication = context.Applications.FirstOrDefault(
            a => a.ParentId == parentId &&
            a.Workshop.ProviderId == providerId);

        var chatRoom = context.ChatRoomWorkshops.FirstOrDefault(
            c => c.ParentId == parentId &&
            c.Workshop.ProviderId == providerId);

        // Act
        var result = await repository.UnBlock(blockedProviderParent);

        // Assert
        Assert.That(result is not null);
        Assert.AreEqual(expectedUnBlockedApplications, context.Applications.Count(a => !a.IsBlockedByProvider));
        Assert.AreEqual(expectedUnBlockedChatRooms, context.ChatRoomWorkshops.Count(c => !c.IsBlockedByProvider));
        Assert.False(aplication.IsBlockedByProvider);
        Assert.False(chatRoom.IsBlockedByProvider);
    }

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

    private IBlockedProviderParentRepository GetRepository(TestOutOfSchoolDbContext dbContext)
        => new BlockedProviderParentRepository(dbContext);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        parents = ParentGenerator.Generate(2);
        context.AddRange(parents);

        providers = ProvidersGenerator.Generate(2);
        context.AddRange(providers);

        workshops = WorkshopGenerator.Generate(2);
        workshops[0].ProviderId = providers[0].Id;
        workshops[1].ProviderId = providers[1].Id;
        context.AddRange(workshops);

        applications = ApplicationGenerator.Generate(2);
        applications[0].Workshop = workshops[0];
        applications[1].Workshop = workshops[1];
        applications[0].ParentId = parents[0].Id;
        applications[1].ParentId = parents[1].Id;
        context.AddRange(applications);

        chatRooms = new List<ChatRoomWorkshop>
        {
            new ChatRoomWorkshop()
            {
                Id = Guid.NewGuid(),
                Workshop = workshops[0],
                ParentId = parents[0].Id,
            },
            new ChatRoomWorkshop()
            {
                Id = Guid.NewGuid(),
                Workshop = workshops[1],
                ParentId = parents[1].Id,
            },
        };
        context.AddRange(chatRooms);

        await context.SaveChangesAsync();
    }
}