using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Hubs;
using OutOfSchool.BusinessLogic.Models.Notifications;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class NotificationServiceTests
{
    private INotificationService notificationService;
    private Mock<INotificationRepository> notificationRepositoryMock;
    private IMapper mapper;
    private Mock<IOptions<NotificationsConfig>> notificationsConfigMock;
    private Mock<IHubContext<NotificationHub>> notificationHub;

    private string userId;
    private List<Notification> notifications;

    [SetUp]
    public void SetUp()
    {
        notificationRepositoryMock = new Mock<INotificationRepository>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        notificationsConfigMock = new Mock<IOptions<NotificationsConfig>>();

        notificationHub = new Mock<IHubContext<NotificationHub>>();
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();

        notificationHub.Setup(nh => nh.Clients).Returns(clientsMock.Object);
        clientsMock.Setup(client => client.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

        notificationService = new NotificationService(
            notificationRepositoryMock.Object,
            new Mock<ILogger<NotificationService>>().Object,
            new Mock<IStringLocalizer<SharedResource>>().Object,
            mapper,
            notificationHub.Object,
            notificationsConfigMock.Object);

        userId = Guid.NewGuid().ToString();
        notifications = CreateNotificationsForUser(userId);
    }

    #region Create
    [Test]
    public async Task Create_NotificationsDisabled_EntityNotCreated()
    {
        // Arrange
        IEnumerable<string> recipientsIds = null;

        var notificationsConfig = new NotificationsConfig
        {
            Enabled = false,
        };

        notificationsConfigMock.Setup(x => x.Value).Returns(notificationsConfig);

        // Act
        await notificationService.Create(
            It.IsAny<NotificationType>(),
            It.IsAny<NotificationAction>(),
            It.IsAny<Guid>(),
            recipientsIds).ConfigureAwait(false);

        // Assert
        notificationRepositoryMock.Verify(x => x.Create(It.IsAny<Notification>()), Times.Never);
    }

    [Test]
    public async Task Create_RecipientsIdsNull_EntityNotCreated()
    {
        // Arrange
        IEnumerable<string> recipientsIds = null;

        var notificationsConfig = new NotificationsConfig
        {
            Enabled = true,
        };

        notificationsConfigMock.Setup(x => x.Value).Returns(notificationsConfig);

        // Act
        await notificationService.Create(
            It.IsAny<NotificationType>(),
            It.IsAny<NotificationAction>(),
            It.IsAny<Guid>(),
            recipientsIds).ConfigureAwait(false);

        // Assert
        notificationRepositoryMock.Verify(x => x.Create(It.IsAny<Notification>()), Times.Never);
    }

    [Test]
    public async Task Create_RecipientsIdsEmpty_EntityNotCreated()
    {
        // Arrange
        IEnumerable<string> recipientsIds = new List<string>();

        var notificationsConfig = new NotificationsConfig
        {
            Enabled = true,
        };

        notificationsConfigMock.Setup(x => x.Value).Returns(notificationsConfig);

        // Act
        await notificationService.Create(
            It.IsAny<NotificationType>(),
            It.IsAny<NotificationAction>(),
            It.IsAny<Guid>(),
            recipientsIds).ConfigureAwait(false);

        // Assert
        notificationRepositoryMock.Verify(x => x.Create(It.IsAny<Notification>()), Times.Never);
    }

    [Test]
    public async Task Create_RecipientsIdsValid_CreatesSpecificNumberOfTimes()
    {
        // Arrange
        IEnumerable<string> recipientsIds = new List<string>()
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
            };

        var notificationsConfig = new NotificationsConfig
        {
            Enabled = true,
        };

        notificationsConfigMock.Setup(x => x.Value).Returns(notificationsConfig);
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(clients => clients.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        notificationHub.Setup(x => x.Clients).Returns(() => mockClients.Object);

        // Act
        await notificationService.Create(
            It.IsAny<NotificationType>(),
            It.IsAny<NotificationAction>(),
            It.IsAny<Guid>(),
            recipientsIds).ConfigureAwait(false);

        // Assert
        notificationRepositoryMock.Verify(x => x.Create(It.IsAny<Notification>()), Times.Exactly(recipientsIds.Count()));
        notificationHub.Verify(x => x.Clients.Group(It.IsAny<string>()), Times.Exactly(recipientsIds.Count()));
        foreach (var recipientId in recipientsIds)
        {
            notificationHub.Verify(x => x.Clients.Group(recipientId), Times.Once);
        }
    }

    #endregion

    #region ReadAll
    [Test]
    public async Task ReadAll_WhenUserIdIsValid_ShouldSetReadDateTimeForAllUnreaded()
    {
        // Arrange
        notificationRepositoryMock
            .Setup(n => n.SetReadDateTimeForAllUnreaded(userId, It.IsAny<DateTimeOffset>()))
            .Returns(Task.CompletedTask);

        // Act
        await notificationService.ReadAll(userId).ConfigureAwait(false);

        // Assert
        notificationRepositoryMock.VerifyAll();
    }

    [Test]
    public void ReadAll_WhenUserIsValidAndDbHasConcurrencyViolation_ShouldThrowDbUpdateConcurrencyException()
    {
        // Arrange
        notificationRepositoryMock
            .Setup(n => n.SetReadDateTimeForAllUnreaded(It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
            .Throws<DbUpdateConcurrencyException>();

        // Act & Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => notificationService.ReadAll(userId));
    }
    #endregion

    #region GetAllUsersNotificationsGroupedAsync
    [Test]
    public async Task GetAllUsersNotificationsGroupedAsync_WithValidUserId_ShouldReturnGroupedNotifications()
    {
        // Arrange
        var notificationsConfig = new NotificationsConfig
        {
            Grouped = new List<string> { "Application", },
        };

        notificationsConfigMock.Setup(x => x.Value).Returns(notificationsConfig);

        notificationRepositoryMock
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Notification, bool>>>(),
                It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<Notification>>(notifications));

        var expectedGrouped = notifications
            .Where(n => notificationsConfig.Grouped.Contains(n.Type.ToString()))
            .GroupBy(n => new { n.Type, n.Action, n.GroupedData })
            .Select(n => new NotificationGrouped
            {
                Type = n.Key.Type,
                Action = n.Key.Action,
                GroupedData = n.Key.GroupedData,
                Amount = n.Count(),
            })
            .ToList();

        var expectedGroupedByType = expectedGrouped
            .GroupBy(n => n.Type)
            .Select(n => new NotificationGroupedByType
            {
                Type = n.Key,
                Amount = n.Sum(g => g.Amount),
                GroupedByAdditionalData = n.ToList(),
            })
            .ToList();

        var expectedNotifications = notifications
            .Where(n => !notificationsConfig.Grouped.Contains(n.Type.ToString()))
            .Select(notification => mapper.Map<NotificationDto>(notification))
            .OrderByDescending(n => n.CreatedDateTime)
            .ToList();

        // Act
        var result = await notificationService.GetAllUsersNotificationsGroupedAsync(userId).ConfigureAwait(false);

        // Assert
        notificationRepositoryMock.VerifyAll();
        TestHelper.AssertTwoCollectionsEqualByValues(expectedNotifications, result.Notifications);
        TestHelper.AssertTwoCollectionsEqualByValues(
            expectedGroupedByType.Select(x => x.Amount),
            result.NotificationsGroupedByType.Select(x => x.Amount));
        Assert.AreEqual(
            expectedGroupedByType.Count(),
            result.NotificationsGroupedByType.Count());
        TestHelper.AssertTwoCollectionsEqualByValues(
            expectedGroupedByType.SelectMany(_ => _.GroupedByAdditionalData),
            result.NotificationsGroupedByType.SelectMany(_ => _.GroupedByAdditionalData));
    }
    #endregion

    #region GetAllUsersNotificationsByFilterAsync
    [Test]
    public async Task GetAllUsersNotificationsByFilterAsync_WithValidUserIdAndNotificationType_ShouldReturnFilteredNotifications()
    {
        // Arrange
        var notificationType = NotificationType.Parent;
        notificationRepositoryMock
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Notification, bool>>>(),
                It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<Notification>>(notifications));
        var expected = notifications
            .Where(n => n.Type == notificationType)
            .Select(n => mapper.Map<NotificationDto>(n))
            .OrderByDescending(n => n.CreatedDateTime)
            .ToList();

        // Act
        var result = await notificationService
            .GetAllUsersNotificationsByFilterAsync(userId, notificationType)
            .ConfigureAwait(false);

        // Assert
        notificationRepositoryMock.VerifyAll();
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result);
    }

    [Test]
    public async Task GetAllUsersNotificationsByFilterAsync_WithValidUserIdAndNotificationTypeIsNull_ShouldReturnAllNotifications()
    {
        // Arrange
        notificationRepositoryMock
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Notification, bool>>>(),
                It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<Notification>>(notifications));
        var expected = notifications
            .Select(n => mapper.Map<NotificationDto>(n))
            .OrderByDescending(n => n.CreatedDateTime)
            .ToList();

        // Act
        var result = await notificationService
            .GetAllUsersNotificationsByFilterAsync(userId, null)
            .ConfigureAwait(false);

        // Assert
        notificationRepositoryMock.VerifyAll();
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result);
    }
    #endregion

    private List<Notification> CreateNotificationsForUser(string userId)
    {
        return new List<Notification>()
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Application,
                Action = NotificationAction.Create,
                CreatedDateTime = DateTimeOffset.UtcNow.AddHours(-12),
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Workshop,
                Action = NotificationAction.Create,
                CreatedDateTime = DateTimeOffset.UtcNow.AddHours(-3),
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Parent,
                Action = NotificationAction.Block,
                CreatedDateTime = DateTimeOffset.UtcNow.AddHours(-2),
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Parent,
                Action = NotificationAction.Unblock,
                CreatedDateTime = DateTimeOffset.UtcNow.AddHours(-1),
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Application,
                Action = NotificationAction.Create,
                CreatedDateTime = DateTimeOffset.UtcNow.AddHours(-5),
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Application,
                Action = NotificationAction.Message,
                CreatedDateTime = DateTimeOffset.UtcNow.AddHours(-8),
            },
        };
    }
}