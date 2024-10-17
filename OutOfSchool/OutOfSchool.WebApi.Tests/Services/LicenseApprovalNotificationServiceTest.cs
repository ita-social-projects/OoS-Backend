using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.LicenseApprovalNotification;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class LicenseApprovalNotificationServiceTest
{
    private Mock<INotificationService> notificationService;
    private Mock<IEntityRepositorySoftDeleted<string, User>> userRepository;

    private ILicenseApprovalNotificationService licenseApprovalNotificationService;

    [SetUp]
    public void SetUp()
    {
        notificationService = new Mock<INotificationService>();
        userRepository = new Mock<IEntityRepositorySoftDeleted<string, User>>();

        var logger = new Mock<ILogger<LicenseApprovalNotificationService>>();

        licenseApprovalNotificationService = new LicenseApprovalNotificationService(
            notificationService.Object,
            logger.Object,
            userRepository.Object);
    }

    [Test]
    public async Task Generate_WhenCalled_CreateNotificationWithAdditionalData()
    {
        // Arrange
        string statusKey = "Status";

        var user = UserGenerator.Generate();
        var users = new List<User>() { user };
        var recipientsIds = new List<string>() { user.Id };

        var workshop = WorkshopGenerator.Generate();

        userRepository.Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<User, bool>>>(),
                string.Empty)).ReturnsAsync(users.AsTestAsyncEnumerableQuery());

        // Act
        await licenseApprovalNotificationService.Generate().ConfigureAwait(false);

        // Assert
        notificationService.Verify(
            x => x.Create(
                NotificationType.System,
                NotificationAction.LicenseApproval,
                Guid.Empty,
                recipientsIds,
                It.Is<Dictionary<string, string>>(c => c.ContainsKey(statusKey)),
                null),
            Times.Once);
    }
}
