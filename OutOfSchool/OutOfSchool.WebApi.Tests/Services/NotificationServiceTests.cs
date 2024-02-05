using System;
using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.Mapping;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class NotificationServiceTests
{
    private INotificationService notificationService;
    private Mock<INotificationRepository> notificationRepositoryMock;
    private Mock<ILogger<NotificationService>> loggerMock;
    private Mock<IStringLocalizer<SharedResource>> localizerMock;
    private IMapper mapper;
    private Mock<IHubContext<NotificationHub>> notificationHubMock;
    private Mock<IOptions<NotificationsConfig>> notificationsConfigMock;

    [SetUp]
    public void SetUp()
    {
        notificationRepositoryMock = new Mock<INotificationRepository>();
        loggerMock = new Mock<ILogger<NotificationService>>();
        localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        notificationHubMock = new Mock<IHubContext<NotificationHub>>();
        notificationsConfigMock = new Mock<IOptions<NotificationsConfig>>();

        notificationService = new NotificationService(
            notificationRepositoryMock.Object,
            loggerMock.Object,
            localizerMock.Object,
            mapper,
            notificationHubMock.Object,
            notificationsConfigMock.Object);
    }

    [Test]
    public async Task ReadAll_WhenUserIdIsValid_ShouldSetReadDateTimeForAllUnreaded()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        notificationRepositoryMock
            .Setup(n => n.SetReadDateTimeForAllUnreaded(It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
            .Returns(Task.CompletedTask);

        // Act
        await notificationService.ReadAll(userId);

        // Assert
        notificationRepositoryMock
            .Verify(
            x => x.SetReadDateTimeForAllUnreaded(
                userId,
                It.IsAny<DateTimeOffset>()),
            Times.Once());
    }

    [Test]
    public void ReadAll_WhenUserIsValidAndDbHasConcurrencyViolation_ShouldThrowDbUpdateConcurrencyException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();

        notificationRepositoryMock
            .Setup(n => n.SetReadDateTimeForAllUnreaded(It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
            .Throws<DBConcurrencyException>();

        // Act & Assert
        Assert.ThrowsAsync<DBConcurrencyException>(() => notificationService.ReadAll(userId));
    }
}