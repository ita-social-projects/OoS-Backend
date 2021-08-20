using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ChatControllerTests
    {
        private const int Ok = 200;
        private const int NoContent = 204;
        private const int Created = 201;
        private const int BadRequest = 400;
        private const int Forbidden = 403;

        private ChatController controller;
        private Mock<IChatMessageService> messageServiceMock;
        private Mock<IChatRoomService> roomServiceMock;
        private Mock<IProviderService> providerServiceMock;
        private Mock<IParentService> parentServiceMock;
        private Mock<IWorkshopService> workshopServiceMock;
        private Mock<IStringLocalizer<SharedResource>> localizerMock;

        private string userId;
        private ParentDTO parent;
        private ProviderDto provider;
        private WorkshopDTO workshop;
        private Mock<HttpContext> httpContextMoq;

        [SetUp]
        public void SetUp()
        {
            messageServiceMock = new Mock<IChatMessageService>();
            roomServiceMock = new Mock<IChatRoomService>();
            parentServiceMock = new Mock<IParentService>();
            providerServiceMock = new Mock<IProviderService>();
            workshopServiceMock = new Mock<IWorkshopService>();
            localizerMock = new Mock<IStringLocalizer<SharedResource>>();

            httpContextMoq = new Mock<HttpContext>();

            // Arrange
            userId = "someUserId";
            httpContextMoq.Setup(x => x.User.FindFirst("sub"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userId));

            parent = new ParentDTO() { Id = 1, UserId = userId };
            provider = new ProviderDto() { Id = 1, UserId = userId };
            workshop = new WorkshopDTO() { Id = 1, ProviderId = 1 };

            controller = new ChatController(
                messageServiceMock.Object,
                roomServiceMock.Object,
                providerServiceMock.Object,
                parentServiceMock.Object,
                workshopServiceMock.Object,
                localizerMock.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
            };
        }

        #region CreateMessage
        [Test]
        public async Task CreateMessage_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var newMessage = new ChatMessageCreateDto()
            {
                ParentId = 1,
                WorkshopId = 1,
                SenderRoleIsProvider = true,
                Text = "new text",
            };
            controller.ModelState.AddModelError("test", "test");

            // Act
            var result = await controller.CreateMessageAsync(newMessage).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(BadRequest, result.StatusCode);
            Assert.IsNotNull(result.Value);
        }

        [Test]
        public async Task CreateMessage_WhenChatRoomExistsUserHasRights_Returns201ObjectResult()
        {
            // Arrange
            httpContextMoq.Setup(x => x.User.FindFirst("role"))
                            .Returns(new Claim(ClaimTypes.NameIdentifier, Role.Parent.ToString()));
            var message = new ChatMessageCreateDto()
            {
                ParentId = 1,
                WorkshopId = 1,
                SenderRoleIsProvider = false,
                Text = "new text",
            };

            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(message.WorkshopId, message.ParentId))
                .ReturnsAsync(() => new ChatRoomDto() { Id = 1, ParentId = message.ParentId, WorkshopId = message.WorkshopId });
            parentServiceMock.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(parent);
            messageServiceMock.Setup(x => x.CreateAsync(It.IsAny<ChatMessageDto>()))
                .ReturnsAsync(new ChatMessageDto() { Text = message.Text, SenderRoleIsProvider = message.SenderRoleIsProvider });

            // Act
            var result = await controller.CreateMessageAsync(message).ConfigureAwait(false) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(message.WorkshopId, message.ParentId), Times.AtLeastOnce);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageDto>()), Times.Once);
            Assert.AreEqual(Created, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf<ChatMessageDto>(result.Value);
            Assert.AreEqual(message.SenderRoleIsProvider, (result.Value as ChatMessageDto).SenderRoleIsProvider);
            Assert.AreEqual(message.Text, (result.Value as ChatMessageDto).Text);
        }

        [Test]
        public async Task CreateMessage_WhenChatRoomDoesNotExistParamsAreValid_Returns201ObjectResult()
        {
            // Arrange
            httpContextMoq.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, Role.Parent.ToString()));
            var message = new ChatMessageCreateDto()
            {
                ParentId = 1,
                WorkshopId = 1,
                SenderRoleIsProvider = false,
                Text = "new text",
            };

            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(message.WorkshopId, message.ParentId))
                .ReturnsAsync(default(ChatRoomDto));
            roomServiceMock.Setup(x => x.CreateOrReturnExistingAsync(message.WorkshopId, message.ParentId))
                .ReturnsAsync(new ChatRoomDto() { Id = 1, WorkshopId = message.WorkshopId, ParentId = message.ParentId });
            parentServiceMock.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(parent);
            workshopServiceMock.Setup(x => x.GetById(message.WorkshopId))
                .ReturnsAsync(workshop);
            messageServiceMock.Setup(x => x.CreateAsync(It.IsAny<ChatMessageDto>()))
                .ReturnsAsync(new ChatMessageDto() { Text = message.Text, SenderRoleIsProvider = message.SenderRoleIsProvider });

            // Act
            var result = await controller.CreateMessageAsync(message).ConfigureAwait(false) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(message.WorkshopId, message.ParentId), Times.AtLeastOnce);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Once);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageDto>()), Times.Once);
            Assert.AreEqual(Created, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf<ChatMessageDto>(result.Value);
            Assert.AreEqual(message.SenderRoleIsProvider, (result.Value as ChatMessageDto).SenderRoleIsProvider);
            Assert.AreEqual(message.Text, (result.Value as ChatMessageDto).Text);
        }

        [Test]
        public async Task CreateMessage_WhenChatRoomExistsProviderSetNotHisWorkshopId_Returns403ObjectResult()
        {
            // Arrange
            httpContextMoq.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, Role.Provider.ToString()));
            var message = new ChatMessageCreateDto()
            {
                ParentId = 1,
                WorkshopId = 2,
                SenderRoleIsProvider = true,
                Text = "new text",
            };

            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(message.WorkshopId, message.ParentId))
                .ReturnsAsync(() => new ChatRoomDto() { Id = 1, ParentId = message.ParentId, WorkshopId = message.WorkshopId });
            providerServiceMock.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(provider);
            parentServiceMock.Setup(x => x.GetById(message.ParentId))
                .ReturnsAsync(parent);
            workshop.ProviderId = 2;
            workshopServiceMock.Setup(x => x.GetById(message.WorkshopId))
                .ReturnsAsync(workshop);

            // Act
            var result = await controller.CreateMessageAsync(message).ConfigureAwait(false) as ObjectResult;

            // Assert
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()), Times.AtLeastOnce);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(Forbidden, result.StatusCode);
        }

        [Test]
        public async Task CreateMessage_WhenChatRoomDoesNotExistsParentSetNotHisParentId_Returns403ObjectResult()
        {
            // Arrange
            httpContextMoq.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, Role.Parent.ToString()));
            var message = new ChatMessageCreateDto()
            {
                ParentId = 2,
                WorkshopId = 1,
                SenderRoleIsProvider = false,
                Text = "new text",
            };

            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(message.WorkshopId, message.ParentId))
                .ReturnsAsync(default(ChatRoomDto));
            roomServiceMock.Setup(x => x.CreateOrReturnExistingAsync(message.WorkshopId, message.ParentId))
                .ReturnsAsync(new ChatRoomDto() { Id = 1, WorkshopId = message.WorkshopId, ParentId = message.ParentId });
            parentServiceMock.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(parent);
            workshopServiceMock.Setup(x => x.GetById(message.WorkshopId))
                .ReturnsAsync(workshop);

            // Act
            var result = await controller.CreateMessageAsync(message).ConfigureAwait(false) as ObjectResult;

            // Assert
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()), Times.AtLeastOnce);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(Forbidden, result.StatusCode);
        }

        [Test]
        public async Task CreateMessage_WhenParameterValidationFailsBecauseEntityWasNotFound_Returns403ObjectResult()
        {
            // Arrange
            httpContextMoq.Setup(x => x.User.FindFirst("role"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, Role.Provider.ToString()));
            var message = new ChatMessageCreateDto()
            {
                ParentId = 1,
                WorkshopId = 1,
                SenderRoleIsProvider = true,
                Text = "new text",
            };
            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(message.WorkshopId, message.ParentId))
                .ReturnsAsync(default(ChatRoomDto));
            providerServiceMock.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(provider);
            parentServiceMock.Setup(x => x.GetById(message.ParentId))
                .ReturnsAsync(parent);
            workshopServiceMock.Setup(x => x.GetById(message.WorkshopId))
                .ReturnsAsync(default(WorkshopDTO));

            // Act
            var result = await controller.CreateMessageAsync(message).ConfigureAwait(false) as ObjectResult;

            // Assert
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()), Times.AtLeastOnce);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(Forbidden, result.StatusCode);
        }
        #endregion
    }
}
