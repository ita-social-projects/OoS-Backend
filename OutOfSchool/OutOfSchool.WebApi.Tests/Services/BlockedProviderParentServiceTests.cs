using System;
using System.Collections.Generic;
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
    }

    [Test]
    public async Task Block_WhenBlockedProviderParentBlockDtoAndUserIdIsValid_ShouldSendNotificationAndReturnSuccess()
    {
        // Arrange
        var dto = new BlockedProviderParentBlockDto()
        {
            ParentId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Reason = "Reason to block user",
        };

        var userId = Guid.NewGuid().ToString();

        var entity = new BlockedProviderParent()
        {
            Id = Guid.NewGuid(),
            ParentId = dto.ParentId,
            ProviderId = dto.ProviderId,
        };

        var parent = ParentGenerator.Generate();
        parent.Id = dto.ParentId;

        blockedProviderParentRepositoryMock
            .Setup(x => x.GetByFilter(It.IsAny<Expression<Func<BlockedProviderParent, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<BlockedProviderParent>());
        blockedProviderParentRepositoryMock
            .Setup(x => x.Block(It.IsAny<BlockedProviderParent>()))
            .ReturnsAsync(entity);
        parentRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(parent);

        // Act
        var result = await service.Block(dto, userId).ConfigureAwait(false);

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
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void Block_WhenBlockedProviderParentBlockDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => service.Block(null, userId));
    }
}