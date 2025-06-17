using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Redis;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using System;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CurrentUserServiceTests
{
    private Mock<ICurrentUser> _mockCurrentUser;
    private Mock<IWorkshopRepository> _mockWorkshopRepository;
    private Mock<IParentRepository> _mockParentRepository;
    private Mock<IEntityRepositorySoftDeleted<Guid, Child>> _mockChildRepository;
    private Mock<ILogger<CurrentUserService>> _mockLogger;
    private Mock<ICacheService> _mockCache;
    private Mock<IOptions<AppDefaultsConfig>> _mockOptions;
    private Mock<ISensitiveEntityRepositorySoftDeleted<Moderator>> _mockModeratorRepository;
    private Mock<ISensitiveEntityRepositorySoftDeleted<TechAdmin>> _mockTechAdminRepository;
    private CurrentUserService _currentUserService;
    private AppDefaultsConfig _appConfig;

    [SetUp]
    public void SetUp()
    {
        // Arrange
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockWorkshopRepository = new Mock<IWorkshopRepository>();
        _mockParentRepository = new Mock<IParentRepository>();
        _mockChildRepository = new Mock<IEntityRepositorySoftDeleted<Guid, Child>>();
        _mockLogger = new Mock<ILogger<CurrentUserService>>();
        _mockCache = new Mock<ICacheService>();
        _mockOptions = new Mock<IOptions<AppDefaultsConfig>>();
        _mockModeratorRepository = new Mock<ISensitiveEntityRepositorySoftDeleted<Moderator>>();
        _mockTechAdminRepository = new Mock<ISensitiveEntityRepositorySoftDeleted<TechAdmin>>();

        _appConfig = new AppDefaultsConfig { AccessLogEnabled = true };
        _mockOptions.Setup(x => x.Value).Returns(_appConfig);

        _currentUserService = new CurrentUserService(
            _mockCurrentUser.Object,
            _mockWorkshopRepository.Object,
            _mockParentRepository.Object,
            _mockChildRepository.Object,
            _mockLogger.Object,
            _mockCache.Object,
            _mockOptions.Object,
            _mockModeratorRepository.Object,
            _mockTechAdminRepository.Object);
    }

    #region ModeratorHasRights Tests

    [Test]
    public async Task ModeratorHasRights_UserIsNotModerator_ReturnsFalse()
    {
        // Arrange
        var userId = "test-user-id";
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("moderator")).Returns(false);

        // Act
        var result = await InvokePrivateMethod<bool>("ModeratorHasRights");

        // Assert
        Assert.IsFalse(result);
        _mockModeratorRepository.Verify(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<Moderator, bool>>>()), Times.Never);
    }

    [Test]
    public async Task ModeratorHasRights_ValidIndividualIdAndExistsInRepository_ReturnsTrue()
    {
        // Arrange
        var userId = "test-user-id";
        var individualId = Guid.NewGuid();
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("moderator")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns(individualId.ToString());
        _mockModeratorRepository.Setup(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<Moderator, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await InvokePrivateMethod<bool>("ModeratorHasRights");

        // Assert
        Assert.IsTrue(result);
        _mockModeratorRepository.Verify(x => x.Any(It.Is<System.Linq.Expressions.Expression<System.Func<Moderator, bool>>>(
            expr => expr.Compile()(new Moderator { Id = individualId }))), Times.Once);
    }

    [Test]
    public async Task ModeratorHasRights_InvalidIndividualIdClaim_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var userId = "test-user-id";
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("moderator")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns("invalid-guid");

        // Act
        var result = await InvokePrivateMethod<bool>("ModeratorHasRights");

        // Assert
        Assert.IsFalse(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("invalid individual ID claim")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        _mockModeratorRepository.Verify(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<Moderator, bool>>>()), Times.Never);
    }

    [Test]
    public async Task ModeratorHasRights_EmptyIndividualIdClaim_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var userId = "test-user-id";
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("moderator")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns(string.Empty);

        // Act
        var result = await InvokePrivateMethod<bool>("ModeratorHasRights");

        // Assert
        Assert.IsFalse(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("invalid individual ID claim")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        _mockModeratorRepository.Verify(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<Moderator, bool>>>()), Times.Never);
    }

    [Test]
    public async Task ModeratorHasRights_ValidIndividualIdButNotInRepository_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var userId = "test-user-id";
        var individualId = Guid.NewGuid();
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("moderator")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns(individualId.ToString());
        _mockModeratorRepository.Setup(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<Moderator, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await InvokePrivateMethod<bool>("ModeratorHasRights");

        // Assert
        Assert.IsFalse(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unauthorized access") && v.ToString().Contains("Moderator")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task ModeratorHasRights_AccessLogDisabled_DoesNotLog()
    {
        // Arrange
        var userId = "test-user-id";
        var individualId = Guid.NewGuid();
        _appConfig.AccessLogEnabled = false;
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("moderator")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns(individualId.ToString());
        _mockModeratorRepository.Setup(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<Moderator, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await InvokePrivateMethod<bool>("ModeratorHasRights");

        // Assert
        Assert.IsFalse(result);
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }

    #endregion

    #region TechAdminHasRights Tests

    [Test]
    public async Task TechAdminHasRights_UserIsNotTechAdmin_ReturnsFalse()
    {
        // Arrange
        var userId = "test-user-id";
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("techadmin")).Returns(false);

        // Act
        var result = await InvokePrivateMethod<bool>("TechAdminHasRights");

        // Assert
        Assert.IsFalse(result);
        _mockTechAdminRepository.Verify(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<TechAdmin, bool>>>()), Times.Never);
    }

    [Test]
    public async Task TechAdminHasRights_ValidIndividualIdAndExistsInRepository_ReturnsTrue()
    {
        // Arrange
        var userId = "test-user-id";
        var individualId = Guid.NewGuid();
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("techadmin")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns(individualId.ToString());
        _mockTechAdminRepository.Setup(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<TechAdmin, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await InvokePrivateMethod<bool>("TechAdminHasRights");

        // Assert
        Assert.IsTrue(result);
        _mockTechAdminRepository.Verify(x => x.Any(It.Is<System.Linq.Expressions.Expression<System.Func<TechAdmin, bool>>>(
            expr => expr.Compile()(new TechAdmin { Id = individualId }))), Times.Once);
    }

    [Test]
    public async Task TechAdminHasRights_InvalidIndividualIdClaim_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var userId = "test-user-id";
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("techadmin")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns("invalid-guid");

        // Act
        var result = await InvokePrivateMethod<bool>("TechAdminHasRights");

        // Assert
        Assert.IsFalse(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("invalid individual ID claim")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        _mockTechAdminRepository.Verify(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<TechAdmin, bool>>>()), Times.Never);
    }

    [Test]
    public async Task TechAdminHasRights_EmptyIndividualIdClaim_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var userId = "test-user-id";
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("techadmin")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns(string.Empty);

        // Act
        var result = await InvokePrivateMethod<bool>("TechAdminHasRights");

        // Assert
        Assert.IsFalse(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("invalid individual ID claim")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        _mockTechAdminRepository.Verify(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<TechAdmin, bool>>>()), Times.Never);
    }

    [Test]
    public async Task TechAdminHasRights_ValidIndividualIdButNotInRepository_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var userId = "test-user-id";
        var individualId = Guid.NewGuid();
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("techadmin")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns(individualId.ToString());
        _mockTechAdminRepository.Setup(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<TechAdmin, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await InvokePrivateMethod<bool>("TechAdminHasRights");

        // Assert
        Assert.IsFalse(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unauthorized access") && v.ToString().Contains("TechAdmin")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task TechAdminHasRights_AccessLogDisabled_DoesNotLog()
    {
        // Arrange
        var userId = "test-user-id";
        var individualId = Guid.NewGuid();
        _appConfig.AccessLogEnabled = false;
        _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        _mockCurrentUser.Setup(x => x.IsInRole("techadmin")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns(individualId.ToString());
        _mockTechAdminRepository.Setup(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<TechAdmin, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await InvokePrivateMethod<bool>("TechAdminHasRights");

        // Assert
        Assert.IsFalse(result);
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }

    #endregion

    #region Integration Tests for UserHasRights

    [Test]
    public async Task UserHasRights_WithModeratorRights_ValidIndividualId_DoesNotThrow()
    {
        // Arrange
        var individualId = Guid.NewGuid();
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUser.Setup(x => x.IsInRole("moderator")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns(individualId.ToString());
        _mockModeratorRepository.Setup(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<Moderator, bool>>>()))
            .ReturnsAsync(true);

        var moderatorRights = new ModeratorRights();

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _currentUserService.UserHasRights(moderatorRights));
        _mockModeratorRepository.Verify(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<Moderator, bool>>>()), Times.Once);
    }

    [Test]
    public async Task UserHasRights_WithTechAdminRights_ValidIndividualId_DoesNotThrow()
    {
        // Arrange
        var individualId = Guid.NewGuid();
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUser.Setup(x => x.IsInRole("techadmin")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns(individualId.ToString());
        _mockTechAdminRepository.Setup(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<TechAdmin, bool>>>()))
            .ReturnsAsync(true);

        var techAdminRights = new TechAdminRights();

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _currentUserService.UserHasRights(techAdminRights));
        _mockTechAdminRepository.Verify(x => x.Any(It.IsAny<System.Linq.Expressions.Expression<System.Func<TechAdmin, bool>>>()), Times.Once);
    }

    [Test]
    public void UserHasRights_WithModeratorRights_InvalidIndividualId_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUser.Setup(x => x.IsInRole("moderator")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns("invalid-guid");
        _mockCurrentUser.Setup(x => x.UserId).Returns("user123");

        var moderatorRights = new ModeratorRights();

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _currentUserService.UserHasRights(moderatorRights));

        Assert.That(ex.Message, Is.EqualTo("User has no rights to perform operation"));
    }

    [Test]
    public void UserHasRights_WithTechAdminRights_InvalidIndividualId_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUser.Setup(x => x.IsInRole("techadmin")).Returns(true);
        _mockCurrentUser.Setup(x => x.GetClaimValue(Constants.ClaimTypes.IndividualId)).Returns("invalid-guid");
        _mockCurrentUser.Setup(x => x.UserId).Returns("user123");

        var techAdminRights = new TechAdminRights();

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _currentUserService.UserHasRights(techAdminRights));

        Assert.That(ex.Message, Is.EqualTo("User has no rights to perform operation"));
    }

    #endregion

    private async Task<T> InvokePrivateMethod<T>(string methodName, params object[] parameters)
    {
        var method = typeof(CurrentUserService).GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method.Invoke(_currentUserService, parameters);

        if (result is Task<T> taskResult)
        {
            return await taskResult;
        }

        return (T)result;
    }
}
