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
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ChatWorkshopControllerTests
    {
        // TODO: refactor tests to make more specific asserts, wrap it into Assert.Multiple

        // TODO: use HttpStatusCode enum instead
        private const int Ok = 200;
        private const int NoContent = 204;
        private const int Created = 201;
        private const int BadRequest = 400;
        private const int Forbidden = 403;
        private const string UserId = "someUserId";

        private ChatWorkshopController controller;
        private Mock<IChatMessageWorkshopService> messageServiceMock;
        private Mock<IChatRoomWorkshopService> roomServiceMock;
        private Mock<IValidationService> validationServiceMock;
        private Mock<IStringLocalizer<SharedResource>> localizerMock;

        private Mock<HttpContext> httpContextMoq;

        [SetUp]
        public void SetUp()
        {
            messageServiceMock = new Mock<IChatMessageWorkshopService>();
            roomServiceMock = new Mock<IChatRoomWorkshopService>();
            validationServiceMock = new Mock<IValidationService>();
            localizerMock = new Mock<IStringLocalizer<SharedResource>>();

            httpContextMoq = new Mock<HttpContext>();

            httpContextMoq.Setup(x => x.User.FindFirst("sub"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, UserId));

            controller = new ChatWorkshopController(
                messageServiceMock.Object,
                roomServiceMock.Object,
                validationServiceMock.Object,
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
            // TODO: add methods e.g.: CreateFakeChatMessageWorkshop()
            var testMessage = new ChatMessageWorkshopCreateDto()
            {
                ParentId = 1,
                WorkshopId = 1,
                SenderRoleIsProvider = true,
                Text = "new text",
            };
            controller.ModelState.AddModelError("test", "test");

            // Act
            var result = await controller.CreateMessageAsync(testMessage).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(BadRequest, result.StatusCode);
            Assert.IsNotNull(result.Value);
        }

        [Test]
        public async Task CreateMessage_WhenParamsAreValidAndChatRoomExists_Returns201ObjectResult()
        {
            // Arrange
            httpContextMoq.Setup(x => x.User.FindFirst("role"))
                            .Returns(new Claim(ClaimTypes.NameIdentifier, Role.Parent.ToString()));
            var validMessageCreateDto = new ChatMessageWorkshopCreateDto()
            {
                ParentId = 1,
                WorkshopId = 1,
                SenderRoleIsProvider = false,
                Text = "new text",
            };

            var validChatRoom = new ChatRoomWorkshopDto()
            {
                Id = 1,
                ParentId = validMessageCreateDto.ParentId,
                WorkshopId = validMessageCreateDto.WorkshopId,
            };

            var validSavedMessage = new ChatMessageWorkshopDto()
            {
                Id = 1,
                ChatRoomId = validChatRoom.Id,
                Text = validMessageCreateDto.Text,
                SenderRoleIsProvider = validMessageCreateDto.SenderRoleIsProvider,
            };

            validationServiceMock.Setup(x => x.UserIsParentOwnerAsync(UserId, validMessageCreateDto.ParentId)).ReturnsAsync(true);
            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(validMessageCreateDto.WorkshopId, validMessageCreateDto.ParentId))
                .ReturnsAsync(validChatRoom);
            messageServiceMock.Setup(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()))
                .ReturnsAsync(validSavedMessage);

            // Act
            var result = await controller.CreateMessageAsync(validMessageCreateDto).ConfigureAwait(false) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(validMessageCreateDto.WorkshopId, validMessageCreateDto.ParentId), Times.AtLeastOnce);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Once);
            Assert.AreEqual(Created, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf<ChatMessageWorkshopDto>(result.Value);
        }

        [Test]
        public async Task CreateMessage_WhenParamsAreValidAndRoomDoesNotExist_CreatesRoomAndReturns201ObjectResult()
        {
            // Arrange
            httpContextMoq.Setup(x => x.User.FindFirst("role"))
                            .Returns(new Claim(ClaimTypes.NameIdentifier, Role.Provider.ToString()));
            var validMessageCreateDto = new ChatMessageWorkshopCreateDto()
            {
                ParentId = 1,
                WorkshopId = 1,
                SenderRoleIsProvider = true,
                Text = "new text",
            };

            var validChatRoom = new ChatRoomWorkshopDto()
            {
                Id = 1,
                ParentId = validMessageCreateDto.ParentId,
                WorkshopId = validMessageCreateDto.WorkshopId,
            };

            var validSavedMessage = new ChatMessageWorkshopDto()
            {
                Id = 1,
                ChatRoomId = validChatRoom.Id,
                Text = validMessageCreateDto.Text,
                SenderRoleIsProvider = validMessageCreateDto.SenderRoleIsProvider,
            };

            validationServiceMock.Setup(x => x.UserIsWorkshopOwnerAsync(UserId, validMessageCreateDto.WorkshopId)).ReturnsAsync(true);
            roomServiceMock.Setup(x => x.GetUniqueChatRoomAsync(validMessageCreateDto.WorkshopId, validMessageCreateDto.ParentId))
                .ReturnsAsync(default(ChatRoomWorkshopDto));
            roomServiceMock.Setup(x => x.CreateOrReturnExistingAsync(validMessageCreateDto.WorkshopId, validMessageCreateDto.ParentId))
                .ReturnsAsync(validChatRoom);
            messageServiceMock.Setup(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()))
                .ReturnsAsync(validSavedMessage);

            // Act
            var result = await controller.CreateMessageAsync(validMessageCreateDto).ConfigureAwait(false) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(validMessageCreateDto.WorkshopId, validMessageCreateDto.ParentId), Times.AtLeastOnce);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(validMessageCreateDto.WorkshopId, validMessageCreateDto.ParentId), Times.Once);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Once);
            Assert.AreEqual(Created, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf<ChatMessageWorkshopDto>(result.Value);
        }

        [Test]
        public async Task CreateMessage_WhenUserSetsInvalidSenderRole_Returns403ObjectResult()
        {
            // Arrange
            httpContextMoq.Setup(x => x.User.FindFirst("role"))
                            .Returns(new Claim(ClaimTypes.NameIdentifier, Role.Provider.ToString()));
            var validMessageCreateDto = new ChatMessageWorkshopCreateDto()
            {
                ParentId = 1,
                WorkshopId = 1,
                SenderRoleIsProvider = false,
                Text = "new text",
            };

            validationServiceMock.Setup(x => x.UserIsProviderOwnerAsync(UserId, validMessageCreateDto.WorkshopId)).ReturnsAsync(true);

            // Act
            var result = await controller.CreateMessageAsync(validMessageCreateDto).ConfigureAwait(false) as ObjectResult;

            // Assert
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(Forbidden, result.StatusCode);
        }

        [Test]
        public async Task CreateMessage_WhenUserSetsNotHisOwnWorkshopId_Returns403ObjectResult()
        {
            // Arrange
            httpContextMoq.Setup(x => x.User.FindFirst("role"))
                            .Returns(new Claim(ClaimTypes.NameIdentifier, Role.Provider.ToString()));
            var validMessageCreateDto = new ChatMessageWorkshopCreateDto()
            {
                ParentId = 1,
                WorkshopId = 1,
                SenderRoleIsProvider = true,
                Text = "new text",
            };

            validationServiceMock.Setup(x => x.UserIsWorkshopOwnerAsync(UserId, validMessageCreateDto.WorkshopId)).ReturnsAsync(false);

            // Act
            var result = await controller.CreateMessageAsync(validMessageCreateDto).ConfigureAwait(false) as ObjectResult;

            // Assert
            roomServiceMock.Verify(x => x.GetUniqueChatRoomAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            roomServiceMock.Verify(x => x.CreateOrReturnExistingAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            messageServiceMock.Verify(x => x.CreateAsync(It.IsAny<ChatMessageWorkshopDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(Forbidden, result.StatusCode);
        }
        #endregion
    }
}
