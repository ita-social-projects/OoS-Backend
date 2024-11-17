using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ChatWorkshop;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ChatWorkshopControllerTests
{
    private const int Ok = 200;
    private const int NoContent = 204;

    private ChatWorkshopController controller;
    private Mock<IChatMessageWorkshopService> chatMessageWorkshopServiceMoq;
    private Mock<IChatRoomWorkshopService> roomServiceMoq;
    private Mock<IValidationService> validationServiceMoq;
    private Mock<IStringLocalizer<SharedResource>> localizerMoq;
    private Mock<ILogger<ChatWorkshopController>> loggerMoq;
    private Mock<IEmployeeService> providerAdminServiceMoq;
    private Mock<IApplicationService> applicationServiceMoq;

    private string userId;
    private Mock<HttpContext> httpContextMoq;

    private List<ChatRoomWorkshopDtoWithLastMessage> chatRoomWorkshopDtoWithLastMessageList;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        chatRoomWorkshopDtoWithLastMessageList = new List<ChatRoomWorkshopDtoWithLastMessage>
        {
            new ChatRoomWorkshopDtoWithLastMessage(),
            new ChatRoomWorkshopDtoWithLastMessage(),
        };
    }

    [SetUp]
    public void Setup()
    {
        chatMessageWorkshopServiceMoq = new Mock<IChatMessageWorkshopService>();
        roomServiceMoq = new Mock<IChatRoomWorkshopService>();
        validationServiceMoq = new Mock<IValidationService>();
        localizerMoq = new Mock<IStringLocalizer<SharedResource>>();
        loggerMoq = new Mock<ILogger<ChatWorkshopController>>();
        providerAdminServiceMoq = new Mock<IEmployeeService>();
        applicationServiceMoq = new Mock<IApplicationService>();

        userId = "someUserId";
        httpContextMoq = new Mock<HttpContext>();
        httpContextMoq.Setup(x => x.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));
        httpContextMoq.Setup(x => x.User.FindFirst("role"))
            .Returns(new Claim(ClaimTypes.Role, "parent"));
        httpContextMoq.Setup(x => x.User.FindFirst("subrole"))
            .Returns(new Claim(ClaimTypes.Role, "None"));

        controller = new ChatWorkshopController(
            chatMessageWorkshopServiceMoq.Object,
            roomServiceMoq.Object,
            validationServiceMoq.Object,
            localizerMoq.Object,
            loggerMoq.Object,
            providerAdminServiceMoq.Object,
            applicationServiceMoq.Object)
        {
            ControllerContext = new ControllerContext()
            { HttpContext = httpContextMoq.Object },
        };
    }

    #region GetParentsRoomsAsync
    [Test]
    public async Task GetParentsRoomsAsync_WhenIdIsValid_ShouldReturnOkResultObject()
    {
        // Arrange
        var validUserId = Guid.NewGuid();
        validationServiceMoq.Setup(
            x => x.GetParentOrProviderIdByUserRoleAsync(
                userId, It.IsAny<Role>()))
            .Returns(Task.FromResult(validUserId));

        var expectedSearchResult =
            new SearchResult<ChatRoomWorkshopDtoWithLastMessage>
            { Entities = chatRoomWorkshopDtoWithLastMessageList };

        roomServiceMoq.Setup(
            x => x.GetChatRoomByFilter(
                It.IsAny<ChatWorkshopFilter>(), validUserId, false))
            .Returns(Task.FromResult(expectedSearchResult));

        // Act
        var result = await controller
            .GetParentsRoomsAsync(
            new ChatWorkshopFilter())
            .ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(expectedSearchResult, result.Value);
    }

    [Test]
    public async Task GetParentsRoomsAsync_WhenIdIsNotValid_ShouldReturnNoContentResultObject()
    {
        // Arrange
        Guid invalidUserId = default;
        validationServiceMoq.Setup(
            x => x.GetParentOrProviderIdByUserRoleAsync(
                userId, It.IsAny<Role>()))
            .Returns(Task.FromResult(invalidUserId));

        // Act
        var result = await controller
            .GetParentsRoomsAsync(
            new ChatWorkshopFilter { Size = 8 })
            .ConfigureAwait(false) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }

    #endregion

    #region GetProvidersRoomsAsync
    [Test]
    public async Task GetProvidersRoomsAsync_WhenUserIsEmployee_ShouldReturnOkResult()
    {
        // Arrange
        httpContextMoq
            .Setup(x => x.User.FindFirst("role"))
            .Returns(new Claim(ClaimTypes.Role, "employee"));

        var workShopIds = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
        };

        providerAdminServiceMoq
            .Setup(x => x.GetRelatedWorkshopIdsForEmployees(It.IsAny<string>()))
            .ReturnsAsync(workShopIds);

        var expectedSearchResult = new SearchResult<ChatRoomWorkshopDtoWithLastMessage>
        {
            Entities = chatRoomWorkshopDtoWithLastMessageList,
        };

        roomServiceMoq
            .Setup(x => x.GetChatRoomByFilter(It.IsAny<ChatWorkshopFilter>(), Guid.Empty, true))
            .Returns(Task.FromResult(expectedSearchResult));

        // Act
        var result = await controller
            .GetProvidersRoomsAsync(new ChatWorkshopFilter())
            .ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(expectedSearchResult, result.Value);
    }

    [Test]
    public async Task GetProvidersRoomsAsync_WhenUserIsInvalidEmployee_ShouldReturnNoContentResult()
    {
        // Arrange
        httpContextMoq
            .Setup(x => x.User.FindFirst("role"))
            .Returns(new Claim(ClaimTypes.Role, "employee"));

        providerAdminServiceMoq
            .Setup(x => x.GetRelatedWorkshopIdsForEmployees(It.IsAny<string>()))
            .ReturnsAsync(new List<Guid>());

        roomServiceMoq
            .Setup(x => x.GetChatRoomByFilter(It.IsAny<ChatWorkshopFilter>(), Guid.Empty, true))
            .Returns(Task.FromResult(new SearchResult<ChatRoomWorkshopDtoWithLastMessage>
            {
                Entities = new List<ChatRoomWorkshopDtoWithLastMessage>(),
            }));

        // Act
        var result = await controller
            .GetProvidersRoomsAsync(new ChatWorkshopFilter())
            .ConfigureAwait(false) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }

    [Test]
    public async Task GetProvidersRoomsAsync_WhenIdIsValid_ShouldReturnOkResultObject()
    {
        // Arrange
        var validUserId = Guid.NewGuid();
        validationServiceMoq.Setup(
            x => x.GetParentOrProviderIdByUserRoleAsync(
                userId, It.IsAny<Role>()))
            .Returns(Task.FromResult(validUserId));

        var expectedSearchResult =
            new SearchResult<ChatRoomWorkshopDtoWithLastMessage>
            { Entities = chatRoomWorkshopDtoWithLastMessageList };

        roomServiceMoq.Setup(
            x => x.GetChatRoomByFilter(
                It.IsAny<ChatWorkshopFilter>(), validUserId, false))
            .Returns(Task.FromResult(expectedSearchResult));

        // Act
        var result = await controller
            .GetProvidersRoomsAsync(
            new ChatWorkshopFilter())
            .ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(expectedSearchResult, result.Value);
    }

    [Test]
    public async Task GetProvidersRoomsAsync_WhenIdIsNotValid_ShouldReturnNoContentResultObject()
    {
        // Arrange
        Guid invalidUserId = default;
        validationServiceMoq.Setup(
            x => x.GetParentOrProviderIdByUserRoleAsync(
                userId, It.IsAny<Role>()))
            .Returns(Task.FromResult(invalidUserId));

        // Act
        var result = await controller
            .GetProvidersRoomsAsync(
            new ChatWorkshopFilter { Size = 8 })
            .ConfigureAwait(false) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }

    #endregion
}