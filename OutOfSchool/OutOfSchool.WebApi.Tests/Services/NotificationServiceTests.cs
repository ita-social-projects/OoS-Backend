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
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Models.Notifications;
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

    private string userId;
    private List<Notification> notifications;

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

        userId = Guid.NewGuid().ToString();
        notifications = CreateNotificationsForUser(userId);
    }

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
            expectedGroupedByType.Select(x => x.GroupedByAdditionalData).Count(),
            result.NotificationsGroupedByType.Select(x => x.GroupedByAdditionalData).Count());
        for (int i = 0; i < expectedGroupedByType.Count; i++)
        {
            TestHelper.AssertTwoCollectionsEqualByValues(
                expectedGroupedByType[i].GroupedByAdditionalData,
                result.NotificationsGroupedByType.ElementAt(i).GroupedByAdditionalData);
        }
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
            .Select(n => mapper.Map<NotificationDto>(n))
            .Where(n => n.Type == notificationType)
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