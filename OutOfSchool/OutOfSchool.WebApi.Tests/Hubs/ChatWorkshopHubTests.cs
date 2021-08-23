using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Hubs
{
    [TestFixture]
    public class ChatWorkshopHubTests
    {
        private Mock<ILogger<ChatWorkshopHub>> loggerMock;
        private Mock<IChatMessageWorkshopService> messageServiceMock;
        private Mock<IChatRoomWorkshopService> roomServiceMock;
        private Mock<IProviderService> providerServiceMock;
        private Mock<IParentService> parentServiceMock;
        private Mock<IWorkshopService> workshopServiceMock;

        private ChatWorkshopHub chatHub;

        private Mock<IHubCallerClients> clientsMock;
        private Mock<IClientProxy> clientProxyMock;
        private Mock<HubCallerContext> hubCallerContextMock;
        private Mock<IGroupManager> groupsMock;

        private string userId;
        private string userRole;
        private List<ChatRoomWorkshopDtoWithLastMessage> rooms;
        private ChatRoomWorkshopDto chatRoom;

        private ParentDTO parent;
        private ProviderDto provider;
        private WorkshopDTO workshop;

        [SetUp]
        public void SetUp()
        {
            loggerMock = new Mock<ILogger<ChatWorkshopHub>>();
            messageServiceMock = new Mock<IChatMessageWorkshopService>();
            roomServiceMock = new Mock<IChatRoomWorkshopService>();
            providerServiceMock = new Mock<IProviderService>();
            parentServiceMock = new Mock<IParentService>();
            workshopServiceMock = new Mock<IWorkshopService>();

            clientsMock = new Mock<IHubCallerClients>();
            clientProxyMock = new Mock<IClientProxy>();
            hubCallerContextMock = new Mock<HubCallerContext>();
            groupsMock = new Mock<IGroupManager>();

            userId = "someUserId";
            userRole = Role.Provider.ToString();
            rooms = new List<ChatRoomWorkshopDtoWithLastMessage>()
            {
                new ChatRoomWorkshopDtoWithLastMessage() { Id = 1, WorkshopId = 1, ParentId = 1, },
                new ChatRoomWorkshopDtoWithLastMessage() { Id = 2, WorkshopId = 2, ParentId = 1, },
            };

            chatRoom = new ChatRoomWorkshopDto() { Id = 1, WorkshopId = 1, ParentId = 1, };

            parent = new ParentDTO() { Id = 1, UserId = userId };
            provider = new ProviderDto() { Id = 1, UserId = userId };
            workshop = new WorkshopDTO() { Id = 1, ProviderId = 1 };

            chatHub = new ChatWorkshopHub(loggerMock.Object, messageServiceMock.Object, roomServiceMock.Object, providerServiceMock.Object, parentServiceMock.Object, workshopServiceMock.Object)
            {
                Clients = clientsMock.Object,
                Context = hubCallerContextMock.Object,
                Groups = groupsMock.Object,
            };

            hubCallerContextMock.Setup(x => x.User.FindFirst("sub"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        [Test]
        public async Task OnConnectedAsync_ShouldAddConnectionToGroups()
        {
            // Arrange
            userRole = Role.Provider.ToString();
            hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userRole));

            providerServiceMock.Setup(x => x.GetByUserId(It.IsAny<string>()))
                .ReturnsAsync(provider);
            roomServiceMock.Setup(x => x.GetByProviderIdAsync(It.IsAny<long>()))
                .ReturnsAsync(rooms);

            groupsMock.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await chatHub.OnConnectedAsync();

            // Assert
            hubCallerContextMock.Verify(x => x.User.FindFirst(It.IsAny<string>()), Times.AtLeastOnce);
            providerServiceMock.Verify(x => x.GetByUserId(It.IsAny<string>()), Times.Once);
            roomServiceMock.Verify(x => x.GetByProviderIdAsync(It.IsAny<long>()), Times.Once);
            groupsMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(rooms.Count));
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
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenSenderIsParentAndParentIdNotHisOwne_ShouldSendForbiddenMessage()
        {
            // Arrange
            userRole = Role.Parent.ToString();
            hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userRole));
            var chatNewMessage = "{'workshopId':1, 'parentId':2, 'text':'hi', 'senderRoleIsProvider':false }";

            parentServiceMock.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(parent);
            workshopServiceMock.Setup(x => x.GetById(1))
                .ReturnsAsync(workshop);

            clientsMock.Setup(clients => clients.Caller).Returns(clientProxyMock.Object);

            // Act
            await chatHub.SendMessageToOthersInGroupAsync(chatNewMessage).ConfigureAwait(false);

            // Assert
            hubCallerContextMock.Verify(x => x.User.FindFirst("sub"), Times.Once);
            hubCallerContextMock.Verify(x => x.User.FindFirst("role"), Times.Once);

            providerServiceMock.Verify(x => x.GetByUserId(It.IsAny<string>()), Times.Never);
            parentServiceMock.Verify(x => x.GetByUserId(userId), Times.Once);
            workshopServiceMock.Verify(x => x.GetById(1), Times.Once);

            clientsMock.Verify(clients => clients.Caller, Times.Once);

            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenSenderIsProviderAndWorkshopIdIsNotHisOwne_ShouldSendForbiddenMessage()
        {
            // Arrange
            userRole = Role.Provider.ToString();
            hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userRole));
            var chatNewMessage = "{'workshopId':1, 'parentId':1, 'text':'hi', 'senderRoleIsProvider':true }";

            providerServiceMock.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(provider);
            parentServiceMock.Setup(x => x.GetById(1))
                .ReturnsAsync(parent);

            workshop.ProviderId = 2;
            workshopServiceMock.Setup(x => x.GetById(1))
                .ReturnsAsync(workshop);

            clientsMock.Setup(clients => clients.Caller).Returns(clientProxyMock.Object);

            // Act
            await chatHub.SendMessageToOthersInGroupAsync(chatNewMessage).ConfigureAwait(false);

            // Assert
            hubCallerContextMock.Verify(x => x.User.FindFirst("sub"), Times.Once);
            hubCallerContextMock.Verify(x => x.User.FindFirst("role"), Times.Once);

            providerServiceMock.Verify(x => x.GetByUserId(userId), Times.Once);
            parentServiceMock.Verify(x => x.GetById(1), Times.Once);
            workshopServiceMock.Verify(x => x.GetById(1), Times.Once);

            clientsMock.Verify(clients => clients.Caller, Times.Once);

            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenMessageIsValidAndRoomExists_ShouldNotCreateNewChatRoomButSaveMessage()
        {
            // Arrange
            userRole = Role.Provider.ToString();
            hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userRole));
            var chatNewMessage = "{'workshopId':1, 'parentId':1, 'text':'hi', 'senderRoleIsProvider':true }";

            providerServiceMock.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(provider);
            parentServiceMock.Setup(x => x.GetById(1))
                .ReturnsAsync(parent);
            workshopServiceMock.Setup(x => x.GetById(1))
                .ReturnsAsync(workshop);

            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(chatRoom);

            var newMessage = new ChatMessageWorkshopDto()
            {
                Id = 1,
                SenderRoleIsProvider = true,
                Text = "hi",
                ChatRoomId = 1,
                CreatedTime = DateTimeOffset.UtcNow,
                IsRead = false,
            };
            messageServiceMock.Setup(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()))
                .ReturnsAsync(newMessage);

            clientsMock.Setup(clients => clients.OthersInGroup(It.IsAny<string>())).Returns(clientProxyMock.Object);

            // Act
            await chatHub.SendMessageToOthersInGroupAsync(chatNewMessage).ConfigureAwait(false);

            // Assert
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(1, 1), Times.Once);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);

            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Once);
            clientsMock.Verify(clients => clients.OthersInGroup("1WorkshopChat"), Times.Once);
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenChatRoomDoesNotExist_ShouldCreateRoomAndSaveMessage()
        {
            // Arrange
            userRole = Role.Provider.ToString();
            hubCallerContextMock.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userRole));
            var chatNewMessage = "{'workshopId':1, 'parentId':1, 'text':'hi', 'senderRoleIsProvider':true }";

            providerServiceMock.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(provider);
            parentServiceMock.Setup(x => x.GetById(1))
                .ReturnsAsync(parent);
            workshopServiceMock.Setup(x => x.GetById(1))
                .ReturnsAsync(workshop);

            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(default(ChatRoomWorkshopDto));
            roomServiceMock.Setup(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(chatRoom);

            var newMessage = new ChatMessageWorkshopDto()
            {
                Id = 1,
                SenderRoleIsProvider = true,
                Text = "hi",
                ChatRoomId = 1,
                CreatedTime = DateTimeOffset.UtcNow,
                IsRead = false,
            };
            messageServiceMock.Setup(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()))
                .ReturnsAsync(newMessage);

            clientsMock.Setup(clients => clients.OthersInGroup(It.IsAny<string>())).Returns(clientProxyMock.Object);

            // Act
            await chatHub.SendMessageToOthersInGroupAsync(chatNewMessage).ConfigureAwait(false);

            // Assert
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(1, 1), Times.AtLeastOnce);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(1, 1), Times.Once);

            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Once);
            clientsMock.Verify(clients => clients.OthersInGroup("1WorkshopChat"), Times.Once);
        }
    }
}
