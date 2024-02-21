using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Models.BlockedProviderParent;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.Mapping;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class BlockedProviderParentServiceTests
{
    private const string ProviderIdKey = "ProviderId";
    private const string ProviderFullTitleKey = "ProviderFullTitle";
    private const string ProviderShortTitleKey = "ProviderShortTitle";

    private BlockedProviderParentService service;

    private Mock<IBlockedProviderParentRepository> blockedProviderParentRepositoryMock;
    private Mock<ILogger<BlockedProviderParentService>> loggerMock;
    private Mock<IStringLocalizer<SharedResource>> localizerMock;
    private IMapper mapper;
    private Mock<INotificationService> notificationServiceMock;
    private Mock<IParentRepository> parentRepositoryMock;

    private BlockedProviderParentBlockDto blockDto;
    private BlockedProviderParentUnblockDto unblockDto;
    private string userId;
    private Provider provider;
    private BlockedProviderParent blockEntity;
    private BlockedProviderParent unblockEntity;

    [SetUp]
    public void SetUp()
    {
        blockedProviderParentRepositoryMock = new Mock<IBlockedProviderParentRepository>();
        loggerMock = new Mock<ILogger<BlockedProviderParentService>>();
        localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        notificationServiceMock = new Mock<INotificationService>();
        parentRepositoryMock = new Mock<IParentRepository>();

        service = new BlockedProviderParentService(
            blockedProviderParentRepositoryMock.Object,
            loggerMock.Object,
            localizerMock.Object,
            mapper,
            notificationServiceMock.Object,
            parentRepositoryMock.Object);

        blockDto = new BlockedProviderParentBlockDto()
        {
            ParentId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Reason = "Reason to block user",
        };
        unblockDto = new BlockedProviderParentUnblockDto()
        {
            ParentId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
        };
        userId = Guid.NewGuid().ToString();
        provider = ProvidersGenerator.Generate();
        provider.Id = blockDto.ProviderId;
        blockEntity = new BlockedProviderParent()
        {
            Id = Guid.NewGuid(),
            ParentId = blockDto.ParentId,
            ProviderId = blockDto.ProviderId,
            Provider = provider,
        };
        unblockEntity = new BlockedProviderParent()
        {
            Id = Guid.NewGuid(),
            ParentId = unblockDto.ParentId,
            ProviderId = unblockDto.ProviderId,
            Provider = provider,
        };
    }

    #region Block

    [Test]
    public async Task Block_WhenDtoAndUserIdIsValid_ShouldSendNotificationAndReturnSuccess()
    {
        // Arrange
        var parent = ParentGenerator.Generate().WithUserId(userId);
        parent.Id = blockDto.ParentId;
        var blockedParentUserId = Guid.Parse(parent.UserId);
        var additionalData = new Dictionary<string, string>()
        {
            { ProviderIdKey, blockEntity.ProviderId.ToString() },
            { ProviderFullTitleKey, blockEntity.Provider.FullTitle },
            { ProviderShortTitleKey, blockEntity.Provider.ShortTitle },
        };

        blockedProviderParentRepositoryMock
            .Setup(x => x.GetBlockedProviderParentEntities(blockEntity.ParentId, blockEntity.ProviderId))
            .Returns(new List<BlockedProviderParent>().AsQueryable().BuildMock());
        blockedProviderParentRepositoryMock
            .Setup(x => x.Block(It.IsAny<BlockedProviderParent>()))
            .ReturnsAsync(blockEntity);
        parentRepositoryMock.Setup(x => x.GetById(parent.Id)).ReturnsAsync(parent);

        // Act
        var result = await service.Block(blockDto, userId).ConfigureAwait(false);

        // Assert
        blockedProviderParentRepositoryMock.VerifyAll();
        parentRepositoryMock.VerifyAll();
        notificationServiceMock
            .Verify(
                x => x.Create(
                NotificationType.Parent,
                NotificationAction.ProviderBlock,
                blockedParentUserId,
                service,
                additionalData,
                null),
                Times.Once);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Succeeded);
    }

    [Test]
    public void Block_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => service.Block(null, userId));
    }

    [Test]
    public async Task Block_WhenDtoIsAlreadyBlocked_ShouldReturnFailed()
    {
        // Arrange
        var blockedParents = new List<BlockedProviderParent>
        {
            blockEntity,
        };
        blockedProviderParentRepositoryMock
            .Setup(x => x.GetBlockedProviderParentEntities(blockEntity.ParentId, blockEntity.ProviderId))
            .Returns(blockedParents.AsQueryable().BuildMock());

        string expectedErrorCode = "400";

        // Act
        var result = await service.Block(blockDto, userId).ConfigureAwait(false);

        // Assert
        blockedProviderParentRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors, Has.Exactly(1).Items);
        Assert.That(result.OperationResult.Errors.First().Code, Is.EqualTo(expectedErrorCode));
    }

    #endregion

    #region Unblock

    [Test]
    public async Task Unblock_WhenDtoAndUserIdIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var parent = ParentGenerator.Generate().WithUserId(userId);
        parent.Id = blockDto.ParentId;
        unblockEntity.Parent = parent;
        var unblockedParentUserId = Guid.Parse(parent.UserId);
        var additionalData = new Dictionary<string, string>()
        {
            { ProviderIdKey, unblockEntity.ProviderId.ToString() },
            { ProviderFullTitleKey, unblockEntity.Provider.FullTitle },
            { ProviderShortTitleKey, unblockEntity.Provider.ShortTitle },
        };

        var blockedParents = new List<BlockedProviderParent>
        {
            unblockEntity,
        };

        blockedProviderParentRepositoryMock
            .Setup(x => x.GetBlockedProviderParentEntities(unblockEntity.ParentId, unblockEntity.ProviderId))
            .Returns(blockedParents.AsQueryable().BuildMock());
        blockedProviderParentRepositoryMock
            .Setup(x => x.UnBlock(It.IsAny<BlockedProviderParent>()))
            .ReturnsAsync(unblockEntity);

        // Act
        var result = await service.Unblock(unblockDto, userId).ConfigureAwait(false);

        // Assert
        blockedProviderParentRepositoryMock.VerifyAll();
        notificationServiceMock
           .Verify(
                x => x.Create(
                NotificationType.Parent,
                NotificationAction.ProviderUnblock,
                unblockedParentUserId,
                service,
                additionalData,
                null),
                Times.Once);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Succeeded);
    }

    [Test]
    public void Unblock_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => service.Unblock(null, userId));
    }

    [Test]
    public async Task Unblock_WhenDtoIsNotBlocked_ShouldReturnFailed()
    {
        // Arrange
        blockedProviderParentRepositoryMock
            .Setup(x => x.GetBlockedProviderParentEntities(unblockEntity.ParentId, unblockEntity.ProviderId))
            .Returns(new List<BlockedProviderParent>().AsQueryable().BuildMock());

        string expectedErrorCode = "400";

        // Act
        var result = await service.Unblock(unblockDto, userId).ConfigureAwait(false);

        // Assert
        blockedProviderParentRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors, Has.Exactly(1).Items);
        Assert.That(result.OperationResult.Errors.First().Code, Is.EqualTo(expectedErrorCode));
    }

    #endregion

    #region GetBlock
    [Test]
    public async Task GetBlock_WithParentIdAndProviderId_ShouldReturnDto()
    {
        // Arrange
        var parentId = blockEntity.ParentId;
        var providerId = blockEntity.ProviderId;
        var entities = new List<BlockedProviderParent>()
        {
            blockEntity,
        };
        var expected = new BlockedProviderParentDto()
        {
            Id = blockEntity.Id,
            ParentId = blockEntity.ParentId,
            ProviderId = blockEntity.ProviderId,
            Reason = blockEntity.Reason,
            UserIdBlock = blockEntity.UserIdBlock,
            DateTimeFrom = blockEntity.DateTimeFrom,
        };
        blockedProviderParentRepositoryMock
            .Setup(x => x.GetBlockedProviderParentEntities(parentId, providerId))
            .Returns(entities.AsQueryable().BuildMock());

        // Act
        var result = await service.GetBlock(parentId, providerId).ConfigureAwait(false);

        // Assert
        blockedProviderParentRepositoryMock.VerifyAll();
        Assert.NotNull(result);
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public async Task GetBlock_WithNoMatchingEntities_ShouldReturnNull()
    {
        // Arrange
        var parentId = blockEntity.ParentId;
        var providerId = blockEntity.ProviderId;
        blockedProviderParentRepositoryMock
            .Setup(x => x.GetBlockedProviderParentEntities(parentId, providerId))
            .Returns(new List<BlockedProviderParent>().AsQueryable().BuildMock());

        // Act
        var result = await service.GetBlock(parentId, providerId).ConfigureAwait(false);

        // Assert
        blockedProviderParentRepositoryMock.VerifyAll();
        Assert.IsNull(result);
    }
    #endregion

    #region GetNotificationRecipientIds
    [Test]
    public async Task GetNotificationsRecipientIds_WhenObjectIdIsValid_ShouldReturnIEnumerableWithObjectId()
    {
        // Arrange
        var action = NotificationAction.ProviderBlock;
        var additionalData = new Dictionary<string, string>();
        var objectId = Guid.NewGuid();
        var expected = new List<string>() { objectId.ToString() };

        // Act
        var result = await service.GetNotificationsRecipientIds(action, additionalData, objectId);

        // Assert
        Assert.AreEqual(expected, result);
    }
    #endregion
}