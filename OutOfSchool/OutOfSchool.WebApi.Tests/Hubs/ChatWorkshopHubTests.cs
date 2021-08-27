using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Hubs
{
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

        private ChatWorkshopHub chatHub;

        private Mock<IHubCallerClients> clientsMock;
        private Mock<IClientProxy> clientProxyMock;
        private Mock<HubCallerContext> hubCallerContextMock;
        private Mock<IGroupManager> groupsMock;

        [SetUp]
        public void SetUp()
        {
            loggerMock = new Mock<ILogger<ChatWorkshopHub>>();
            messageServiceMock = new Mock<IChatMessageWorkshopService>();
            roomServiceMock = new Mock<IChatRoomWorkshopService>();
            validationServiceMock = new Mock<IValidationService>();
            workshopRepositoryMock = new Mock<IWorkshopRepository>();
            parentRepositoryMock = new Mock<IParentRepository>();

            clientsMock = new Mock<IHubCallerClients>();
            clientProxyMock = new Mock<IClientProxy>();
            hubCallerContextMock = new Mock<HubCallerContext>();
            groupsMock = new Mock<IGroupManager>();

            chatHub = new ChatWorkshopHub(
                loggerMock.Object,
                messageServiceMock.Object,
                roomServiceMock.Object,
                validationServiceMock.Object,
                workshopRepositoryMock.Object,
                parentRepositoryMock.Object)
            {
                Clients = clientsMock.Object,
                Context = hubCallerContextMock.Object,
                Groups = groupsMock.Object,
            };

            hubCallerContextMock.Setup(x => x.User.FindFirst("sub"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, UserId));
        }

        [Test]
        public async Task OnConnectedAsync_ShouldAddConnectionToGroups()
        {
            // Arrange
            var userRole = Role.Provider.ToString();
            hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userRole));

            var validProviderId = 1L;
            validationServiceMock.Setup(x => x.GetEntityIdAccordingToUserRole(UserId, userRole)).ReturnsAsync(validProviderId);

            var validChatRoomIds = new List<long>() { 1, 2 };
            roomServiceMock.Setup(x => x.GetChatRoomIdsByProviderIdAsync(validProviderId))
                .ReturnsAsync(validChatRoomIds);

            groupsMock.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await chatHub.OnConnectedAsync();

            // Assert
            validationServiceMock.Verify(x => x.GetEntityIdAccordingToUserRole(UserId, userRole), Times.Once);
            roomServiceMock.Verify(x => x.GetChatRoomIdsByProviderIdAsync(validProviderId), Times.Once);
            groupsMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(validChatRoomIds.Count));
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
            clientsMock.Verify(clients => clients.OthersInGroup(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenUserSetsNotOwnParentId_ShouldSendForbiddenMessage()
        {
            // Arrange
            var userRole = Role.Parent.ToString();
            hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userRole));

            var invalidParentId = 1;

            var chatNewMessage = "{'workshopId':1, 'parentId':1, 'text':'hi', 'senderRoleIsProvider':false }";

            validationServiceMock.Setup(x => x.UserIsParentOwnerAsync(UserId, invalidParentId)).ReturnsAsync(false);

            clientsMock.Setup(clients => clients.Caller).Returns(clientProxyMock.Object);

            // Act
            await chatHub.SendMessageToOthersInGroupAsync(chatNewMessage).ConfigureAwait(false);

            // Assert
            validationServiceMock.Verify(x => x.UserIsParentOwnerAsync(UserId, invalidParentId), Times.Once);

            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Never);

            clientsMock.Verify(clients => clients.Caller, Times.Once);
            clientsMock.Verify(clients => clients.OthersInGroup(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenWhenUserSetsInvalidSenderRole_ShouldSendForbiddenMessage()
        {
            // Arrange
            var userRole = Role.Provider.ToString();
            hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userRole));

            var validWorkshopId = 1;

            var chatNewMessage = "{'workshopId':1, 'parentId':1, 'text':'hi', 'senderRoleIsProvider':false }";

            validationServiceMock.Setup(x => x.UserIsWorkshopOwnerAsync(UserId, validWorkshopId)).ReturnsAsync(true);

            clientsMock.Setup(clients => clients.Caller).Returns(clientProxyMock.Object);

            // Act
            await chatHub.SendMessageToOthersInGroupAsync(chatNewMessage).ConfigureAwait(false);

            // Assert
            validationServiceMock.Verify(x => x.UserIsWorkshopOwnerAsync(UserId, validWorkshopId), Times.Once);

            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Never);

            clientsMock.Verify(clients => clients.Caller, Times.Once);
            clientsMock.Verify(clients => clients.OthersInGroup(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenParamsAreValidAndChatRoomExists_ShouldNotCreateNewChatRoomButSaveMessageAndSendMessageToGroup()
        {
            // Arrange
            var userRole = Role.Provider.ToString();
            hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userRole));

            var validWorkshopId = 1;
            var validNewMessage = "{'workshopId':1, 'parentId':1, 'text':'hi', 'senderRoleIsProvider':true }";

            validationServiceMock.Setup(x => x.UserIsWorkshopOwnerAsync(UserId, validWorkshopId)).ReturnsAsync(true);

            var validChatRoom = new ChatRoomWorkshopDto() { Id = 1, WorkshopId = 1, ParentId = 1, };
            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(validChatRoom);

            var validCreatedMessage = new ChatMessageWorkshopDto()
            {
                Id = 1,
                SenderRoleIsProvider = true,
                Text = "hi",
                ChatRoomId = validChatRoom.Id,
                CreatedDateTime = DateTimeOffset.UtcNow,
                ReadDateTime = null,
            };
            messageServiceMock.Setup(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()))
                .ReturnsAsync(validCreatedMessage);

            clientsMock.Setup(clients => clients.OthersInGroup(It.IsAny<string>())).Returns(clientProxyMock.Object);

            // Act
            await chatHub.SendMessageToOthersInGroupAsync(validNewMessage).ConfigureAwait(false);

            // Assert
            validationServiceMock.Verify(x => x.UserIsWorkshopOwnerAsync(UserId, validWorkshopId), Times.Once);

            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(validWorkshopId, It.IsAny<long>()), Times.Once);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);

            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Once);
            clientsMock.Verify(clients => clients.OthersInGroup(validChatRoom.Id + "WorkshopChat"), Times.Once);
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenParamsAreValidAndChatRoomDoesNotExists_ShouldCreateNewChatRoomAndSaveMessageAndSendMessageToGroup()
        {
            // Arrange
            var userRole = Role.Parent.ToString();
            hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userRole));

            var validParentId = 1;
            var validNewMessage = "{'workshopId':1, 'parentId':1, 'text':'hi', 'senderRoleIsProvider':false }";

            validationServiceMock.Setup(x => x.UserIsParentOwnerAsync(UserId, validParentId)).ReturnsAsync(true);

            var validChatRoom = new ChatRoomWorkshopDto() { Id = 1, WorkshopId = 1, ParentId = 1, };
            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(default(ChatRoomWorkshopDto));
            roomServiceMock.Setup(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(validChatRoom);

            var validCreatedMessage = new ChatMessageWorkshopDto()
            {
                Id = 1,
                SenderRoleIsProvider = false,
                Text = "hi",
                ChatRoomId = validChatRoom.Id,
                CreatedDateTime = DateTimeOffset.UtcNow,
                ReadDateTime = null,
            };
            messageServiceMock.Setup(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()))
                .ReturnsAsync(validCreatedMessage);

            var validParent = new Parent() { Id = 1, UserId = UserId };
            parentRepositoryMock.Setup(x => x.GetById(validParentId)).ReturnsAsync(validParent);

            var validWorkshops = new List<Workshop>() { new Workshop() { Id = 1, Provider = new Provider() { UserId = "someId" } } };
            workshopRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>())).ReturnsAsync(validWorkshops);

            groupsMock.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            clientsMock.Setup(clients => clients.OthersInGroup(It.IsAny<string>())).Returns(clientProxyMock.Object);

            // Act
            await chatHub.SendMessageToOthersInGroupAsync(validNewMessage).ConfigureAwait(false);

            // Assert
            validationServiceMock.Verify(x => x.UserIsParentOwnerAsync(UserId, validParentId), Times.Once);

            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(validParentId, It.IsAny<long>()), Times.AtLeastOnce);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Once);

            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Once);

            clientsMock.Verify(clients => clients.OthersInGroup(validChatRoom.Id + "WorkshopChat"), Times.Once);
        }
    }
}
