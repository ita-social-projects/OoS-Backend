using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Hubs
{
    [TestFixture]
    public class ChatHubTests
    {
        private Mock<ILogger> loggerMoq;
        private Mock<IChatMessageService> messageServiceMoq;
        private Mock<IChatRoomService> roomServiceMoq;

        private ChatHub chatHub;

        private Mock<IHubCallerClients> clientsMoq;
        private Mock<IClientProxy> clientProxyMoq;

        private string userId;
        private List<ChatRoomDto> rooms;
        private Mock<HubCallerContext> hubCallerContextMoq;
        private Mock<IGroupManager> groupsMoq;

        [SetUp]
        public void SetUp()
        {
            loggerMoq = new Mock<ILogger>();
            messageServiceMoq = new Mock<IChatMessageService>();
            roomServiceMoq = new Mock<IChatRoomService>();

            clientsMoq = new Mock<IHubCallerClients>();
            clientProxyMoq = new Mock<IClientProxy>();

            userId = "someUserId";
            rooms = new List<ChatRoomDto>()
            {
                new ChatRoomDto() { Id = 1, WorkshopId = 1, Users = new List<UserDto>() { new UserDto() { Id = "someUserId" }, new UserDto() { Id = "anotherUserId" } } },
                new ChatRoomDto() { Id = 2, WorkshopId = 2, Users = new List<UserDto>() { new UserDto() { Id = "someUserId" }, new UserDto() { Id = "anotherUserId" } } },
            };
            hubCallerContextMoq = new Mock<HubCallerContext>();
            groupsMoq = new Mock<IGroupManager>();

            chatHub = new ChatHub(loggerMoq.Object, messageServiceMoq.Object, roomServiceMoq.Object)
            {
                Clients = clientsMoq.Object,
                Context = hubCallerContextMoq.Object,
                Groups = groupsMoq.Object,
            };

            hubCallerContextMoq.Setup(x => x.User.FindFirst("sub"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        [Test]
        public async Task OnConnectedAsync_ShouldAddConnectionToGroups()
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>()))
                .ReturnsAsync(rooms);
            groupsMoq.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await chatHub.OnConnectedAsync();

            // Assert
            hubCallerContextMoq.Verify(x => x.User.FindFirst(It.IsAny<string>()), Times.Once);
            roomServiceMoq.Verify(x => x.GetByUserId(userId), Times.Once);
            groupsMoq.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task DisconnectedAsync_ShouldRetriveUserFromToken()
        {
            // Act
            await chatHub.OnDisconnectedAsync(new Exception());

            // Assert
            hubCallerContextMoq.Verify(x => x.User.FindFirst(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void SendMessageToOthersInGroup_WhenObjectIsInvalid_ShouldThrowException()
        {
            // Arrange
            var chatNewMessage = "string with wrong format";
            clientsMoq.Setup(clients => clients.Caller).Returns(clientProxyMoq.Object);

            // Act and Assert
            Assert.ThrowsAsync<JsonReaderException>(async () => await chatHub.SendMessageToOthersInGroup(chatNewMessage));

            clientsMoq.Verify(clients => clients.Caller, Times.Once);
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenChatRoomIdIsSetAndValid_ShouldNotCreateNewChatRoom()
        {
            // Arrange
            var chatNewMessage = "{'receiverUserId':'anotherUserId', 'text':'hi', 'workshopId':1, 'chatRoomId':1 }";
            roomServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>()))
                .ReturnsAsync(rooms);

            var newMessage = new ChatMessageDto()
            {
                Id = 1,
                UserId = userId,
                Text = "hi",
                ChatRoomId = 1,
                CreatedTime = DateTime.Now,
                IsRead = false,
            };
            messageServiceMoq.Setup(x => x.Create(It.IsAny<ChatMessageDto>()))
                .ReturnsAsync(newMessage);

            clientsMoq.Setup(clients => clients.OthersInGroup(It.IsAny<string>())).Returns(clientProxyMoq.Object);

            // Act
            await chatHub.SendMessageToOthersInGroup(chatNewMessage).ConfigureAwait(false);

            // Assert
            hubCallerContextMoq.Verify(x => x.User.FindFirst("sub"), Times.Once);

            roomServiceMoq.Verify(x => x.GetByUserId(userId), Times.Once);
            roomServiceMoq.Verify(x => x.CreateOrReturnExisting(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);

            messageServiceMoq.Verify(x => x.Create(It.IsAny<ChatMessageDto>()), Times.Once);
            clientsMoq.Verify(clients => clients.OthersInGroup("1"), Times.Once);
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenChatRoomDoesNotExistUsersCanChat_ShouldCreateRoom()
        {
            // Arrange
            var chatNewMessage = "{'receiverUserId':'anotherUserId', 'text':'hi', 'workshopId':1, 'chatRoomId':3 }";
            roomServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>()))
                .ReturnsAsync(rooms);

            roomServiceMoq.Setup(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync(true);
            var newRoom = new ChatRoomDto() { Id = 3, WorkshopId = 1 };
            roomServiceMoq.Setup(x => x.CreateOrReturnExisting(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync(newRoom);

            var newMessage = new ChatMessageDto()
            {
                Id = 1,
                UserId = userId,
                Text = "hi",
                ChatRoomId = 3,
                CreatedTime = DateTime.Now,
                IsRead = false,
            };
            messageServiceMoq.Setup(x => x.Create(It.IsAny<ChatMessageDto>()))
                .ReturnsAsync(newMessage);

            clientsMoq.Setup(clients => clients.OthersInGroup(It.IsAny<string>())).Returns(clientProxyMoq.Object);

            // Act
            await chatHub.SendMessageToOthersInGroup(chatNewMessage).ConfigureAwait(false);

            // Assert
            hubCallerContextMoq.Verify(x => x.User.FindFirst("sub"), Times.Once);

            roomServiceMoq.Verify(x => x.GetByUserId(userId), Times.Once);
            roomServiceMoq.Verify(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Once);
            roomServiceMoq.Verify(x => x.CreateOrReturnExisting(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Once);

            messageServiceMoq.Verify(x => x.Create(It.IsAny<ChatMessageDto>()), Times.Once);
            clientsMoq.Verify(clients => clients.OthersInGroup("3"), Times.Once);
        }

        [Test]
        public async Task SendMessageToOthersInGroup_WhenUsersCanNotChat_ShouldSendForbiddenMessage()
        {
            // Arrange
            var chatNewMessage = "{'receiverUserId':'anotherUserId', 'text':'hi', 'workshopId':1, 'chatRoomId':3 }";
            roomServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>()))
                .ReturnsAsync(rooms);

            roomServiceMoq.Setup(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync(false);

            clientsMoq.Setup(clients => clients.Caller).Returns(clientProxyMoq.Object);

            // Act
            await chatHub.SendMessageToOthersInGroup(chatNewMessage).ConfigureAwait(false);

            // Assert
            hubCallerContextMoq.Verify(x => x.User.FindFirst("sub"), Times.Once);

            roomServiceMoq.Verify(x => x.GetByUserId(userId), Times.Once);
            roomServiceMoq.Verify(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Once);
            roomServiceMoq.Verify(x => x.CreateOrReturnExisting(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
            clientsMoq.Verify(clients => clients.Caller, Times.Once);

            messageServiceMoq.Verify(x => x.Create(It.IsAny<ChatMessageDto>()), Times.Never);
            clientsMoq.Verify(clients => clients.OthersInGroup(It.IsAny<string>()), Times.Never);
        }
    }
}
