using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Hubs;
using OutOfSchool.BusinessLogic.Models.ChatWorkshop;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.WebApi.Tests.Hubs;

[TestFixture]
public class ChatWorkshopHubTests
{
    private const string UserId = "someUserId";

    private Mock<ILogger<ChatWorkshopHub>> loggerMock;
    private Mock<IChatMessageWorkshopService> messageServiceMock;
    private Mock<IChatRoomWorkshopService> roomServiceMock;
    private Mock<IValidationService> validationServiceMock;
    private Mock<IWorkshopRepository> workshopRepositoryMock;
    private Mock<IParentRepository> parentRepositoryMock;
    private Mock<IEmployeeRepository> employeeRepositoryMock;
    private Mock<IBlockedProviderParentService> blockedProviderParentServiceMock;

    private ChatWorkshopHub chatHub;

    private Mock<IHubCallerClients> clientsMock;
    private Mock<ISingleClientProxy> clientProxyMock;
    private Mock<HubCallerContext> hubCallerContextMock;
    private Mock<IGroupManager> groupsMock;
    private Mock<IStringLocalizer<SharedResource>> localizerMock;
    private Mock<ICurrentUser> currentUserMock;

    [SetUp]
    public void SetUp()
    {
        loggerMock = new Mock<ILogger<ChatWorkshopHub>>();
        messageServiceMock = new Mock<IChatMessageWorkshopService>();
        roomServiceMock = new Mock<IChatRoomWorkshopService>();
        validationServiceMock = new Mock<IValidationService>();
        workshopRepositoryMock = new Mock<IWorkshopRepository>();
        parentRepositoryMock = new Mock<IParentRepository>();
        blockedProviderParentServiceMock = new Mock<IBlockedProviderParentService>();

        clientsMock = new Mock<IHubCallerClients>();
        clientProxyMock = new Mock<ISingleClientProxy>();
        hubCallerContextMock = new Mock<HubCallerContext>();
        groupsMock = new Mock<IGroupManager>();
        localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        employeeRepositoryMock = new Mock<IEmployeeRepository>();
        currentUserMock = new Mock<ICurrentUser>();

        chatHub = new ChatWorkshopHub(
            loggerMock.Object,
            messageServiceMock.Object,
            roomServiceMock.Object,
            validationServiceMock.Object,
            workshopRepositoryMock.Object,
            parentRepositoryMock.Object,
            localizerMock.Object,
            employeeRepositoryMock.Object,
            blockedProviderParentServiceMock.Object,
            currentUserMock.Object)
        {
            Clients = clientsMock.Object,
            Context = hubCallerContextMock.Object,
            Groups = groupsMock.Object,
        };

        hubCallerContextMock.Setup(x => x.User.FindFirst(IdentityResourceClaimsTypes.Sub))
            .Returns(new Claim(IdentityResourceClaimsTypes.Sub, UserId));
    }

    // TODO: use fakers
    [Test]
    public async Task OnConnectedAsync_ShouldAddConnectionToGroups()
    {
        // Arrange
        var userRole = Role.Provider.ToString();
        hubCallerContextMock.Setup(x => x.User.FindFirst(IdentityResourceClaimsTypes.Role))
            .Returns(new Claim(IdentityResourceClaimsTypes.Role, userRole));

        var validProviderId = Guid.NewGuid();
        validationServiceMock.Setup(x => x.GetParentOrProviderIdByUserRoleAsync(UserId, Role.Provider)).ReturnsAsync(validProviderId);

        var validChatRoomIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
        roomServiceMock.Setup(x => x.GetChatRoomIdsByProviderIdAsync(validProviderId))
            .ReturnsAsync(validChatRoomIds);

        groupsMock.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await chatHub.OnConnectedAsync();

        // Assert
        groupsMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(validChatRoomIds.Count));
    }

    [Test]
    public void OnConnectedAsync_IfOneOfTheClaimsIsNotFoundInJWT_ThrowsAuthenticationException()
    {
        // Arrange
        hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
            .Returns(default(Claim));

        // Act and Assert
        Assert.ThrowsAsync<AuthenticationException>(async () => await chatHub.OnConnectedAsync());
    }

    // TODO: all the tests below are fake because of invalid string, cannot parse from json string to Guid
    [Test]
    public async Task SendMessageToOthersInGroup_IfOneOfTheClaimsIsNotFoundInJWT_ShouldWriteMessageToCallerWithException()
    {
        // Arrange
        var userRole = Role.Provider.ToString();
        hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
            .Returns(new Claim(IdentityResourceClaimsTypes.Role, userRole));
        hubCallerContextMock.Setup(x => x.User.FindFirst("sub"))
            .Returns(default(Claim));

        var chatNewMessage = $"{{'workshopId':'{Guid.NewGuid()}', 'parentId':'{Guid.NewGuid()}', 'text':'hi', 'senderRoleIsProvider':false}}";
        clientsMock.Setup(clients => clients.Caller).Returns(clientProxyMock.Object);

        // Act
        await chatHub.SendMessageToOthersInGroupAsync(chatNewMessage).ConfigureAwait(false);

        // Assert
        clientsMock.Verify(clients => clients.Caller, Times.Once);
        clientsMock.Verify(clients => clients.Group(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SendMessageToOthersInGroup_WhenStringIsInvalid_ShouldWriteMessageToCallerWithException()
    {
        // Arrange
        var chatNewMessage = "string with wrong format";
        clientsMock.Setup(clients => clients.Caller).Returns(clientProxyMock.Object);

        // Act
        await chatHub.SendMessageToOthersInGroupAsync(chatNewMessage).ConfigureAwait(false);

        // Assert
        clientsMock.Verify(clients => clients.Caller, Times.Once);
        clientsMock.Verify(clients => clients.Group(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SendMessageToOthersInGroup_WhenUserSetsNotOwnParentId_ShouldWriteMessageToCallerWithException()
    {
        // Arrange
        var userRole = Role.Parent.ToString();
        hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
            .Returns(new Claim(IdentityResourceClaimsTypes.Role, userRole));

        var invalidParentId = Guid.NewGuid();

        var chatNewMessage = $"{{'workshopId':'{Guid.NewGuid()}', 'parentId':'{invalidParentId}', 'chatRoomId':'{Guid.NewGuid()}', 'text':'hi', 'senderRoleIsProvider':false}}";

        validationServiceMock.Setup(x => x.UserIsParentOwnerAsync(UserId, invalidParentId)).ReturnsAsync(false);

        roomServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ChatRoomWorkshopDto());

        clientsMock.Setup(clients => clients.Caller).Returns(clientProxyMock.Object);

        // Act
        await chatHub.SendMessageToOthersInGroupAsync(chatNewMessage).ConfigureAwait(false);

        // Assert
        clientsMock.Verify(clients => clients.Caller, Times.Once);
        clientsMock.Verify(clients => clients.Group(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SendMessageToOthersInGroup_WhenParamsAreValidAndChatRoomExists_ShouldSaveMessageAndSendMessageToGroup()
    {
        // Arrange
        var userRole = Role.Provider.ToString();
        hubCallerContextMock.Setup(x => x.User.FindFirst(IdentityResourceClaimsTypes.Role))
            .Returns(new Claim(IdentityResourceClaimsTypes.Role, userRole));

        var validWorkshopId = Guid.NewGuid();
        var validParentId = Guid.NewGuid();
        var validChatRoomId = Guid.NewGuid();
        var validNewMessage = $"{{'workshopId':'{validWorkshopId}', 'parentId':'{validParentId}', 'chatRoomId':'{validChatRoomId}', 'text':'hi', 'senderRoleIsProvider':true}}";

        validationServiceMock.Setup(x => x.UserIsWorkshopOwnerAsync(UserId, validWorkshopId)).ReturnsAsync(true);

        roomServiceMock.Setup(x => x.GetByIdAsync(validChatRoomId)).ReturnsAsync(new ChatRoomWorkshopDto());

        var validCreatedMessage = new ChatMessageWorkshopDto()
        {
            Id = Guid.NewGuid(),
            SenderRoleIsProvider = true,
            Text = "hi",
            ChatRoomId = Guid.NewGuid(),
            CreatedDateTime = DateTimeOffset.UtcNow,
            ReadDateTime = null,
        };
        messageServiceMock.Setup(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopCreateDto>(), It.IsAny<Role>()))
            .ReturnsAsync(validCreatedMessage);

        var validParent = new Parent() { Id = validParentId, UserId = UserId };
        parentRepositoryMock.Setup(x => x.GetById(validParent.Id)).ReturnsAsync(validParent);

        var validWorkshops = new List<Workshop>() { new Workshop() { Id = validWorkshopId, Provider = new Provider() { UserId = "someId" } } };

        workshopRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>())).ReturnsAsync(validWorkshops);

        groupsMock.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        clientsMock.Setup(clients => clients.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

        var validProviderAdmins = new List<Employee>();
        employeeRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Employee, bool>>>(), It.IsAny<string>())).ReturnsAsync(validProviderAdmins);

        workshopRepositoryMock
            .Setup(x => x.GetById(validWorkshopId))
            .ReturnsAsync(new Workshop() { ProviderId = Guid.NewGuid() });

        blockedProviderParentServiceMock
            .Setup(x => x.IsBlocked(validParentId, It.IsAny<Guid>()))
            .ReturnsAsync(false);
        
        currentUserMock.Setup(x => x.IsInRole(userRole)).Returns(true);

        // Act
        await chatHub.SendMessageToOthersInGroupAsync(validNewMessage.Replace('\'', '"')).ConfigureAwait(false);

        // Assert
        messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopCreateDto>(), It.IsAny<Role>()), Times.Once);
        clientsMock.Verify(clients => clients.Group(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task SendMessageToOthersInGroup_WhenChatRoomDoesNotExist_ShouldWriteMessageToCallerWithException()
    {
        // Arrange
        var chatNewMessage = $"{{'workshopId':'{Guid.NewGuid()}', 'parentId':'{Guid.NewGuid()}', 'chatRoomId':'{Guid.NewGuid()}', 'text':'hi', 'senderRoleIsProvider':false}}";

        roomServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as ChatRoomWorkshopDto);

        clientsMock.Setup(clients => clients.Caller).Returns(clientProxyMock.Object);

        // Act
        await chatHub.SendMessageToOthersInGroupAsync(chatNewMessage).ConfigureAwait(false);

        // Assert
        clientsMock.Verify(clients => clients.Caller, Times.Once);
        clientsMock.Verify(clients => clients.Group(It.IsAny<string>()), Times.Never);
    }
}