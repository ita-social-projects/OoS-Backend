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
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class BlockedProviderParentRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private List<Parent> parents;
    private List<Provider> providers;
    private List<Workshop> workshops;
    private List<Application> aplications;
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

        var blockedProviderParentToUnblock = new BlockedProviderParent
        {
            Id = blockedProviderParent.Id,
            ParentId = blockedProviderParent.ParentId,
            ProviderId = blockedProviderParent.ProviderId,
            DateTimeTo = DateTime.Now,
            UserIdUnblock = "TestId",
        };

        // Act
        var result = await repository.UnBlock(blockedProviderParentToUnblock);

        // Assert
        Assert.AreEqual(expectedBlockedProviderParentCount, context.BlockedProviderParents.Count());
        Assert.That(result is not null);
        Assert.AreEqual(result.Id, blockedProviderParentToUnblock.Id);
        Assert.That(result.DateTimeTo is not null);
    }

    [Test]
    public async Task Block_ShouldBlockApplicaionAndChatRoomWorkshop()
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

        var expectedBlockedAplications = context.Applications.Count(a => a.IsBlockedByProvider) + 1;
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
        Assert.AreEqual(expectedBlockedAplications, context.Applications.Count(a => a.IsBlockedByProvider));
        Assert.AreEqual(expectedBlockedChatRooms, context.ChatRoomWorkshops.Count(c => c.IsBlockedByProvider));
        Assert.True(aplication.IsBlockedByProvider);
        Assert.True(chatRoom.IsBlockedByProvider);
    }

    [Test]
    public async Task UnBlock_ShouldUnBlockApplicaionAndChatRoomWorkshop()
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

        var expectedUnBlockedAplications = context.Applications.Count(a => !a.IsBlockedByProvider) + 1;
        var expectedUnBlockedChatRooms = context.ChatRoomWorkshops.Count(c => !c.IsBlockedByProvider) + 1;
        var aplication = context.Applications.FirstOrDefault(
            a => a.ParentId == parentId &&
            a.Workshop.ProviderId == providerId);

        var chatRoom = context.ChatRoomWorkshops.FirstOrDefault(
            c => c.ParentId == parentId &&
            c.Workshop.ProviderId == providerId);

        var unBlockedProviderParent = new BlockedProviderParent
        {
            Id = blockedProviderParent.Id,
            ParentId = parentId,
            ProviderId = providerId,
            DateTimeTo = DateTime.Now,
        };

        // Act
        var result = await repository.UnBlock(unBlockedProviderParent);

        // Assert
        Assert.That(result is not null);
        Assert.AreEqual(expectedUnBlockedAplications, context.Applications.Count(a => !a.IsBlockedByProvider));
        Assert.AreEqual(expectedUnBlockedChatRooms, context.ChatRoomWorkshops.Count(c => !c.IsBlockedByProvider));
        Assert.False(aplication.IsBlockedByProvider);
        Assert.False(chatRoom.IsBlockedByProvider);
    }

    private OutOfSchoolDbContext GetContext() => new OutOfSchoolDbContext(dbContextOptions);

    private IBlockedProviderParentRepository GetRepository(OutOfSchoolDbContext dbContext)
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

        aplications = ApplicationGenerator.Generate(2);
        aplications[0].Workshop = workshops[0];
        aplications[1].Workshop = workshops[1];
        aplications[0].ParentId = parents[0].Id;
        aplications[1].ParentId = parents[1].Id;
        context.AddRange(aplications);

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