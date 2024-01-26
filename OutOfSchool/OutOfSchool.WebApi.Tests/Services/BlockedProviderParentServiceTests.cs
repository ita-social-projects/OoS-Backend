using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
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
        blockEntity = new BlockedProviderParent()
        {
            Id = Guid.NewGuid(),
            ParentId = blockDto.ParentId,
            ProviderId = blockDto.ProviderId,
        };
        unblockEntity = new BlockedProviderParent()
        {
            Id = Guid.NewGuid(),
            ParentId = unblockDto.ParentId,
            ProviderId = unblockDto.ProviderId,
        };
    }

    #region Block

    [Test]
    public async Task Block_WhenDtoAndUserIdIsValid_ShouldSendNotificationAndReturnSuccess()
    {
        // Arrange
        var parent = ParentGenerator.Generate();
        parent.Id = blockDto.ParentId;

        blockedProviderParentRepositoryMock
            .Setup(x => x.GetByFilter(It.IsAny<Expression<Func<BlockedProviderParent, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<BlockedProviderParent>());
        blockedProviderParentRepositoryMock
            .Setup(x => x.Block(It.IsAny<BlockedProviderParent>()))
            .ReturnsAsync(blockEntity);
        parentRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(parent);

        // Act
        var result = await service.Block(blockDto, userId).ConfigureAwait(false);

        // Assert
        notificationServiceMock
            .Verify(
                x => x.Create(
                NotificationType.Parent,
                NotificationAction.ProviderBlock,
                It.IsAny<Guid>(),
                It.IsAny<BlockedProviderParentService>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>()),
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
            .Setup(x => x.GetByFilter(It.IsAny<Expression<Func<BlockedProviderParent, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(blockedParents);

        string expectedErrorCode = "400";

        // Act
        var result = await service.Block(blockDto, userId).ConfigureAwait(false);

        // Assert
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
        var parent = ParentGenerator.Generate();
        parent.Id = unblockDto.ParentId;
        var blockedParents = new List<BlockedProviderParent>
        {
            unblockEntity,
        };

        blockedProviderParentRepositoryMock
            .Setup(x => x.GetByFilter(It.IsAny<Expression<Func<BlockedProviderParent, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(blockedParents);
        blockedProviderParentRepositoryMock
            .Setup(x => x.UnBlock(It.IsAny<BlockedProviderParent>()))
            .ReturnsAsync(unblockEntity);
        parentRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(parent);

        // Act
        var result = await service.Unblock(unblockDto, userId).ConfigureAwait(false);

        // Assert
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
            .Setup(x => x.GetByFilter(It.IsAny<Expression<Func<BlockedProviderParent, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<BlockedProviderParent>());

        string expectedErrorCode = "400";

        // Act
        var result = await service.Unblock(unblockDto, userId).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors, Has.Exactly(1).Items);
        Assert.That(result.OperationResult.Errors.First().Code, Is.EqualTo(expectedErrorCode));
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