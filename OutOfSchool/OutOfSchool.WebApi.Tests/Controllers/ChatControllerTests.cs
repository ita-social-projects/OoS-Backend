using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Middlewares;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ChatControllerTests
    {
        private ChatController controller;
        private Mock<IChatMessageService> messageServiceMoq;
        private Mock<IChatRoomService> roomServiceMoq;
        private Mock<IStringLocalizer<SharedResource>> localizerMoq;

        private string userId;
        private Mock<HttpContext> httpContextMoq;

        [SetUp]
        public void SetUp()
        {
            messageServiceMoq = new Mock<IChatMessageService>();
            roomServiceMoq = new Mock<IChatRoomService>();
            localizerMoq = new Mock<IStringLocalizer<SharedResource>>();

            httpContextMoq = new Mock<HttpContext>();

            // Arrange
            userId = "someUserId";
            httpContextMoq.Setup(x => x.User.FindFirst("sub"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userId));

            controller = new ChatController(messageServiceMoq.Object, roomServiceMoq.Object, localizerMoq.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
            };
        }

#pragma warning disable SA1124 // Do not use regions

        #region CreateMessage
        [Test]
        public async Task CreateMessage_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var newMessage = new ChatNewMessageDto()
            {
                ChatRoomId = 0,
                ReceiverUserId = "anotherUserId",
                Text = "new text",
                WorkshopId = 1,
            };
            controller.ModelState.AddModelError("test", "test");

            // Act
            var result = await controller.CreateMessage(newMessage).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            messageServiceMoq.Verify(x => x.Create(It.IsAny<ChatMessageDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.IsNotNull(result.Value);
        }

        [Test]
        public async Task CreateMessage_WhenUsersValidationFails_Returns403ObjectResult()
        {
            // Arrange
            var newMessage = new ChatNewMessageDto()
            {
                ChatRoomId = 0,
                ReceiverUserId = "anotherUserId",
                Text = "new text",
                WorkshopId = 1,
            };
            roomServiceMoq.Setup(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync(false);
            roomServiceMoq.Setup(x => x.CreateOrReturnExisting(userId, "anotherUserId", 1))
                .Throws<ArgumentException>();

            // Act
            var result = await controller.CreateMessage(newMessage).ConfigureAwait(false) as ObjectResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(It.IsAny<long>()), Times.Never);
            roomServiceMoq.Verify(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Once);
            roomServiceMoq.Verify(x => x.CreateOrReturnExisting(userId, "anotherUserId", 1), Times.Never);
            messageServiceMoq.Verify(x => x.Create(It.IsAny<ChatMessageDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        public void CreateMessage_WhenUsersValidationFailsBecauseEntityWasNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var newMessage = new ChatNewMessageDto()
            {
                ChatRoomId = 0,
                ReceiverUserId = "anotherUserId",
                Text = "new text",
                WorkshopId = 1,
            };
            roomServiceMoq.Setup(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Throws<InvalidOperationException>();

            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.CreateMessage(newMessage).ConfigureAwait(false));

            roomServiceMoq.Verify(x => x.GetById(It.IsAny<long>()), Times.Never);
            roomServiceMoq.Verify(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Once);
            roomServiceMoq.Verify(x => x.CreateOrReturnExisting(userId, "anotherUserId", 1), Times.Never);
            messageServiceMoq.Verify(x => x.Create(It.IsAny<ChatMessageDto>()), Times.Never);
        }

        [Test]
        public async Task CreateMessage_WhenChatRoomIdIsSetButRoomWasNotFound_Returns403ObjectResult()
        {
            // Arrange
            var newMessage = new ChatNewMessageDto()
            {
                ChatRoomId = 1,
                ReceiverUserId = "anotherUserId",
                Text = "new text",
                WorkshopId = 1,
            };
            roomServiceMoq.Setup(x => x.GetById(1))
                .ReturnsAsync(() => null);

            // Act
            var result = await controller.CreateMessage(newMessage).ConfigureAwait(false) as ObjectResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(1), Times.Once);
            roomServiceMoq.Verify(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
            roomServiceMoq.Verify(x => x.CreateOrReturnExisting(userId, "anotherUserId", 1), Times.Never);
            messageServiceMoq.Verify(x => x.Create(It.IsAny<ChatMessageDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        public async Task CreateMessage_WhenChatRoomIdIsSetButUserIsNotItsMember_Returns403ObjectResult()
        {
            // Arrange
            var newMessage = new ChatNewMessageDto()
            {
                ChatRoomId = 1,
                ReceiverUserId = "anotherUserId",
                Text = "new text",
                WorkshopId = 1,
            };
            var chatRoom = new ChatRoomDto()
            {
                Users = new UserDto[] { new UserDto() { Id = "NotCurrentUserId" } },
            };
            roomServiceMoq.Setup(x => x.GetById(1))
                .ReturnsAsync(chatRoom);

            // Act
            var result = await controller.CreateMessage(newMessage).ConfigureAwait(false) as ObjectResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(1), Times.Once);
            roomServiceMoq.Verify(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
            roomServiceMoq.Verify(x => x.CreateOrReturnExisting(userId, "anotherUserId", 1), Times.Never);
            messageServiceMoq.Verify(x => x.Create(It.IsAny<ChatMessageDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        public async Task CreateMessage_WhenChatRoomIdIsSetAndIsValid_ReturnsCreatedAtActionResultWithCreatedObject()
        {
            // Arrange
            var newMessage = new ChatNewMessageDto()
            {
                ChatRoomId = 1,
                ReceiverUserId = "anotherUserId",
                Text = "new text",
                WorkshopId = 1,
            };

            var chatRoom = new ChatRoomDto()
            {
                Id = 1,
                WorkshopId = 1,
                Users = new UserDto[] { new UserDto() { Id = userId } },
            };
            roomServiceMoq.Setup(x => x.GetById(1))
                .ReturnsAsync(chatRoom);

            var message = new ChatMessageDto()
            {
                Id = 1,
                UserId = userId,
                ChatRoomId = 1,
                Text = "new text",
                CreatedTime = DateTime.Now,
                IsRead = false,
            };
            messageServiceMoq.Setup(x => x.Create(It.IsAny<ChatMessageDto>()))
                .ReturnsAsync(message);

            // Act
            var result = await controller.CreateMessage(newMessage).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(1), Times.Once);
            roomServiceMoq.Verify(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
            roomServiceMoq.Verify(x => x.CreateOrReturnExisting(userId, "anotherUserId", 1), Times.Never);
            messageServiceMoq.Verify(x => x.Create(It.IsAny<ChatMessageDto>()), Times.Once);
            Assert.AreEqual(201, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf<ChatMessageDto>(result.Value);
            Assert.AreEqual(message.UserId, (result.Value as ChatMessageDto).UserId);
            Assert.AreEqual(message.ChatRoomId, (result.Value as ChatMessageDto).ChatRoomId);
            Assert.AreEqual(message.Text, (result.Value as ChatMessageDto).Text);
        }

        [Test]
        public async Task CreateMessage_WhenChatRoomIdIsNotSetChatCanBeCreted_ReturnsCreatedAtActionResultWithCreatedObject()
        {
            // Arrange
            var newMessage = new ChatNewMessageDto()
            {
                ChatRoomId = 0,
                ReceiverUserId = "anotherUserId",
                Text = "new text",
                WorkshopId = 1,
            };

            var chatRoom = new ChatRoomDto()
            {
                Id = 1,
                WorkshopId = 1,
                Users = new UserDto[] { new UserDto() { Id = userId } },
            };
            roomServiceMoq.Setup(x => x.CreateOrReturnExisting(userId, "anotherUserId", 1))
                .ReturnsAsync(chatRoom);

            var message = new ChatMessageDto()
            {
                Id = 1,
                UserId = userId,
                ChatRoomId = 1,
                Text = "new text",
                CreatedTime = DateTime.Now,
                IsRead = false,
            };
            roomServiceMoq.Setup(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync(true);
            messageServiceMoq.Setup(x => x.Create(It.IsAny<ChatMessageDto>()))
                .ReturnsAsync(message);

            // Act
            var result = await controller.CreateMessage(newMessage).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(It.IsAny<long>()), Times.Never);
            roomServiceMoq.Verify(x => x.UsersCanChatBetweenEachOther(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Once);
            roomServiceMoq.Verify(x => x.CreateOrReturnExisting(userId, "anotherUserId", 1), Times.Once);
            messageServiceMoq.Verify(x => x.Create(It.IsAny<ChatMessageDto>()), Times.Once);
            Assert.AreEqual(201, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf<ChatMessageDto>(result.Value);
            Assert.AreEqual(message.UserId, (result.Value as ChatMessageDto).UserId);
            Assert.AreEqual(message.ChatRoomId, (result.Value as ChatMessageDto).ChatRoomId);
            Assert.AreEqual(message.Text, (result.Value as ChatMessageDto).Text);
        }
        #endregion

        #region GetMessageById
        [Test]
        [TestCase(0)]
        public void GetMessageById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetRoomById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(1)]
        public async Task GetMessageById_WhenThereIsNoMessageWithId_ReturnsNoContentResult(long id)
        {
            // Arrange
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(() => null);

            // Act
            var result = await controller.GetMessageById(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetMessageById_WhenUserIsNotItsOwner_Returns403ObjectResult(long id)
        {
            // Arrange
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatMessageDto()
                {
                    UserId = "NotCurrentUserId",
                });

            // Act
            var result = await controller.GetMessageById(id).ConfigureAwait(false) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetMessageById_WhenMessageCanBeGot_ReturnsOkObjectResult(long id)
        {
            // Arrange
            var message = new ChatMessageDto()
            {
                UserId = userId,
            };
            messageServiceMoq.Setup(x => x.GetById(id))
                 .ReturnsAsync(message);

            // Act
            var result = await controller.GetMessageById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(result.Value);
        }
        #endregion

        #region GetRoomById
        [Test]
        [TestCase(0)]
        public void GetRoomById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetRoomById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(1)]
        public async Task GetRoomById_WhenThereIsNoChatRoomWithId_ReturnsNoContentResult(long id)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(() => null);

            // Act
            var result = await controller.GetRoomById(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetRoomById_WhenUserIsNotItsOwner_Returns403ObjectResult(long id)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatRoomDto()
                {
                    ChatMessages = new ChatMessageDto[] { },
                    Users = new UserDto[] { new UserDto() { Id = "NotCurrentUserId" } },
                });

            // Act
            var result = await controller.GetRoomById(id).ConfigureAwait(false) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetRoomById_WhenRoomCanBeGot_ReturnsOkObjectResult(long id)
        {
            // Arrange
            var chatRoom = new ChatRoomDto()
            {
                ChatMessages = new ChatMessageDto[] { },
                Users = new UserDto[] { new UserDto() { Id = userId } },
            };
            roomServiceMoq.Setup(x => x.GetById(id))
                 .ReturnsAsync(chatRoom);

            // Act
            var result = await controller.GetRoomById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(result.Value);
        }
        #endregion

        #region GetUsersRooms
        [Test]
        public async Task GetUsersRooms_WhenThereIsNoRooms_ReturnsOkObjectResult()
        {
            // Arrange
            var chatRooms = new List<ChatRoomDto>();
            roomServiceMoq.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(chatRooms);

            // Act
            var result = await controller.GetUsersRooms().ConfigureAwait(false) as OkObjectResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetByUserId(userId), Times.Once);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(() => (result.Value as List<ChatRoomDto>).Count == 0);
        }

        [Test]
        public async Task GetUsersRooms_WhenThereAreRooms_ReturnsOkObjectResult()
        {
            // Arrange
            var chatRooms = new List<ChatRoomDto>()
            {
                new ChatRoomDto()
                {
                    ChatMessages = new ChatMessageDto[] { },
                    Users = new UserDto[] { new UserDto() { Id = userId } },
                },
            };
            roomServiceMoq.Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(chatRooms);

            // Act
            var result = await controller.GetUsersRooms().ConfigureAwait(false) as OkObjectResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetByUserId(userId), Times.Once);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(() => (result.Value as List<ChatRoomDto>).Count > 0);
        }
        #endregion

        #region UpdateMessagesStatus
        [Test]
        [TestCase(0)]
        public void UpdateMessagesStatus_WhenRoomIdIsInvalid_ThrowsArgumentOutOfRangeException(long roomId)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.UpdateMessagesStatus(roomId).ConfigureAwait(false));
        }

        [Test]
        [TestCase(1)]
        public async Task UpdateMessagesStatus_WhenThereIsNoRoomWithId_ReturnsNoContentResult(long roomId)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(roomId))
                .ReturnsAsync(() => null);

            // Act
            var result = await controller.UpdateMessagesStatus(roomId).ConfigureAwait(false) as NoContentResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(roomId), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task UpdateMessagesStatus_WhenRoomUsersDoesNotContainCurrentUser_BadRequestObjectResult(long roomId)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(roomId))
                .ReturnsAsync(new ChatRoomDto()
                {
                    Users = new UserDto[] { new UserDto() { Id = "NotCurrentUserId" } },
                });

            // Act
            var result = await controller.UpdateMessagesStatus(roomId).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(roomId), Times.Once);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task UpdateMessagesStatus_WhenAllMessagesAreRead_ReturnsOkObjectResult(long roomId)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(roomId))
                .ReturnsAsync(new ChatRoomDto()
                {
                    Users = new UserDto[] { new UserDto() { Id = userId } },
                });

            messageServiceMoq.Setup(x => x.GetAllNotReadByUserInChatRoom(roomId, userId))
                .ReturnsAsync(new List<ChatMessageDto>());

            // Act
            var result = await controller.UpdateMessagesStatus(roomId).ConfigureAwait(false) as OkObjectResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(roomId), Times.Once);
            messageServiceMoq.Verify(x => x.GetAllNotReadByUserInChatRoom(roomId, userId), Times.Once);
            messageServiceMoq.Verify(x => x.UpdateIsRead(It.IsAny<IEnumerable<ChatMessageDto>>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public void UpdateMessagesStatus_WhenServerErrorOccured_ThrowsDbUpdateConcurrencyException(long roomId)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(roomId))
                            .ReturnsAsync(new ChatRoomDto()
                            {
                                Users = new UserDto[] { new UserDto() { Id = userId } },
                            });
            var listOfMess = new List<ChatMessageDto>()
                {
                    new ChatMessageDto()
                    {
                    Id = 1,
                    UserId = userId,
                    ChatRoomId = roomId,
                    CreatedTime = DateTime.Now,
                    IsRead = false,
                    },
                };

            messageServiceMoq.Setup(x => x.GetAllNotReadByUserInChatRoom(roomId, userId))
                .ReturnsAsync(listOfMess);

            messageServiceMoq.Setup(x => x.UpdateIsRead(listOfMess))
               .Throws<DbUpdateConcurrencyException>();

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await controller.UpdateMessagesStatus(roomId).ConfigureAwait(false));
            roomServiceMoq.Verify(x => x.GetById(roomId), Times.Once);
            messageServiceMoq.Verify(x => x.GetAllNotReadByUserInChatRoom(roomId, userId), Times.Once);
            messageServiceMoq.Verify(x => x.UpdateIsRead(listOfMess), Times.Once);
        }

        [Test]
        [TestCase(1)]
        public async Task UpdateMessagesStatus_WhenStatusCanBeUpdated_ReturnsOkObjectResult(long roomId)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(roomId))
                            .ReturnsAsync(new ChatRoomDto()
                            {
                                Users = new UserDto[] { new UserDto() { Id = userId } },
                            });
            var listOfMess = new List<ChatMessageDto>()
                {
                    new ChatMessageDto()
                    {
                    Id = 1,
                    UserId = userId,
                    ChatRoomId = roomId,
                    CreatedTime = DateTime.Now,
                    IsRead = false,
                    },
                };

            messageServiceMoq.Setup(x => x.GetAllNotReadByUserInChatRoom(roomId, userId))
                .ReturnsAsync(listOfMess);

            messageServiceMoq.Setup(x => x.UpdateIsRead(listOfMess))
               .ReturnsAsync(listOfMess);

            // Act
            var result = await controller.UpdateMessagesStatus(roomId).ConfigureAwait(false) as OkObjectResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(roomId), Times.Once);
            messageServiceMoq.Verify(x => x.GetAllNotReadByUserInChatRoom(roomId, userId), Times.Once);
            messageServiceMoq.Verify(x => x.UpdateIsRead(listOfMess), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(result.Value);
        }
        #endregion

        #region UpdateMessage
        [Test]
        [TestCase(0)]
        public void UpdateMessage_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Arrange
            var message = new ChatMessageDto() { Id = id };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.UpdateMessage(message).ConfigureAwait(false));
            messageServiceMoq.Verify(x => x.GetById(id), Times.Never);
            messageServiceMoq.Verify(x => x.Update(message), Times.Never);
        }

        [Test]
        [TestCase(1)]
        public async Task UpdateMessage_WhenThereIsNoMessageWithId_ReturnsNoContentResult(long id)
        {
            // Arrange
            var message = new ChatMessageDto() { Id = id };
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(() => null);

            // Act
            var result = await controller.UpdateMessage(message).ConfigureAwait(false) as NoContentResult;

            // Assert
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Update(message), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task UpdateMessage_WhenUserIsNotItsOwner_Returns403ObjectResult(long id)
        {
            // Arrange
            var message = new ChatMessageDto() { Id = id };
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatMessageDto()
                {
                    Id = id,
                    UserId = "NotCurrentUserId",
                });

            // Act
            var result = await controller.UpdateMessage(message).ConfigureAwait(false) as ObjectResult;

            // Assert
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Update(message), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task UpdateMessage_WhenChatRoomIsChanging_Returns403ObjectResult(long id)
        {
            // Arrange
            var message = new ChatMessageDto()
            {
                Id = id,
                ChatRoomId = 1,
            };
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatMessageDto()
                {
                    Id = id,
                    UserId = userId,
                    ChatRoomId = 2,
                });

            // Act
            var result = await controller.UpdateMessage(message).ConfigureAwait(false) as ObjectResult;

            // Assert
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Update(message), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task UpdateMessage_WhenMessageIsTooOldToBeDeleted_Returns403ObjectResult(long id)
        {
            // Arrange
            var message = new ChatMessageDto()
            {
                Id = id,
                ChatRoomId = 1,
            };
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatMessageDto()
                {
                    Id = id,
                    UserId = userId,
                    ChatRoomId = 1,
                    CreatedTime = DateTime.Now.Subtract(new TimeSpan(1, 0, 0)),
                });

            // Act
            var result = await controller.UpdateMessage(message).ConfigureAwait(false) as ObjectResult;

            // Assert
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Update(message), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public void UpdateMessage_WhenServerErrorOccured_ThrowsDbUpdateConcurrencyException(long id)
        {
            // Arrange
            var message = new ChatMessageDto()
            {
                Id = id,
                UserId = userId,
                ChatRoomId = 1,
                CreatedTime = DateTime.Now,
                Text = "new text",
            };

            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(message);

            messageServiceMoq.Setup(x => x.Update(message))
                .Throws<DbUpdateConcurrencyException>();

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await controller.UpdateMessage(message).ConfigureAwait(false));
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Update(message), Times.Once);
        }

        [Test]
        [TestCase(1)]
        public async Task UpdateMessage_WhenMessageCanBeUpdated_ReturnsOkObjectResul(long id)
        {
            // Arrange
            var message = new ChatMessageDto()
            {
                Id = id,
                UserId = userId,
                ChatRoomId = 1,
                CreatedTime = DateTime.Now,
                Text = "new text",
            };

            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(message);

            messageServiceMoq.Setup(x => x.Update(message))
                .ReturnsAsync(message);

            // Act
            var result = await controller.UpdateMessage(message).ConfigureAwait(false) as OkObjectResult;

            // Assert
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Update(message), Times.Once);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(result.Value);
        }
        #endregion

        #region DeleteMessage
        [Test]
        [TestCase(0)]
        public void DeleteMessage_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.DeleteMessage(id).ConfigureAwait(false));
            messageServiceMoq.Verify(x => x.GetById(id), Times.Never);
            messageServiceMoq.Verify(x => x.Delete(id), Times.Never);
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteMessage_WhenThereIsNoMessageWithId_ReturnsNoContentResult(long id)
        {
            // Arrange
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(() => null);

            // Act
            var result = await controller.DeleteMessage(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Delete(id), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteMessage_WhenUserIsNotItsOwner_Returns403ObjectResult(long id)
        {
            // Arrange
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatMessageDto()
                {
                    UserId = "NotCurrentUserId",
                });

            // Act
            var result = await controller.DeleteMessage(id).ConfigureAwait(false) as ObjectResult;

            // Assert
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Delete(id), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public void DeleteMessage_WhenServerErrorOccured_ThrowsDbUpdateConcurrencyException(long id)
        {
            // Arrange
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatMessageDto()
                {
                    UserId = userId,
                });

            messageServiceMoq.Setup(x => x.Delete(id))
                .Throws(new DbUpdateConcurrencyException());

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await controller.DeleteMessage(id).ConfigureAwait(false));
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Delete(id), Times.Once);
        }

        [Test]
        [TestCase(1)]
        public void DeleteMessage_WhenMessageWasDeletedBeforeMethodCallsDelete_ThrowsArgumentOutOfRangeException(long id)
        {
            // Arrange
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatMessageDto()
                {
                    UserId = userId,
                });

            messageServiceMoq.Setup(x => x.Delete(id))
                .Throws(new ArgumentOutOfRangeException());

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.DeleteMessage(id).ConfigureAwait(false));
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Delete(id), Times.Once);
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteMessage_WhenMessageCanBeDeleted_ReturnsNoContentResult(long id)
        {
            // Arrange
            messageServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatMessageDto()
                {
                    UserId = userId,
                });

            messageServiceMoq.Setup(x => x.Delete(id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.DeleteMessage(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            messageServiceMoq.Verify(x => x.GetById(id), Times.Once);
            messageServiceMoq.Verify(x => x.Delete(id), Times.Once);
            Assert.AreEqual(204, result.StatusCode);
        }
        #endregion

        #region DeleteRoom
        [Test]
        [TestCase(0)]
        public void DeleteRoom_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.DeleteRoom(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteRoom_WhenThereIsNoChatRoomWithId_ReturnsNoContentResult(long id)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(() => null);

            // Act
            var result = await controller.DeleteRoom(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(id), Times.Once);
            roomServiceMoq.Verify(x => x.Delete(id), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteRoom_WhenChatRoomHasMessages_Returns403ObjectResult(long id)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatRoomDto()
                {
                    ChatMessages = new ChatMessageDto[] { new ChatMessageDto() },
                });

            // Act
            var result = await controller.DeleteRoom(id).ConfigureAwait(false) as ObjectResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(id), Times.Once);
            roomServiceMoq.Verify(x => x.Delete(id), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteRoom_WhenRoomUsersDoesNotContainCurrentUser_Returns403ObjectResult(long id)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatRoomDto()
                {
                    ChatMessages = new ChatMessageDto[] { },
                    Users = new UserDto[] { new UserDto() { Id = "NotCurrentUserId" } },
                });

            // Act
            var result = await controller.DeleteRoom(id).ConfigureAwait(false) as ObjectResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(id), Times.Once);
            roomServiceMoq.Verify(x => x.Delete(id), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public void DeleteRoom_WhenServerErrorOccured_ThrowsDbUpdateConcurrencyException(long id)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatRoomDto()
                {
                    ChatMessages = new ChatMessageDto[] { },
                    Users = new UserDto[] { new UserDto() { Id = userId } },
                });

            roomServiceMoq.Setup(x => x.Delete(id))
                .Throws(new DbUpdateConcurrencyException());

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await controller.DeleteRoom(id).ConfigureAwait(false));
            roomServiceMoq.Verify(x => x.GetById(id), Times.Once);
            roomServiceMoq.Verify(x => x.Delete(id), Times.Once);
        }

        [Test]
        [TestCase(1)]
        public void DeleteRoom_WhenRoomWasDeletedBeforeMethodCallsDelete_ThrowsArgumentOutOfRangeException(long id)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatRoomDto()
                {
                    ChatMessages = new ChatMessageDto[] { },
                    Users = new UserDto[] { new UserDto() { Id = userId } },
                });

            roomServiceMoq.Setup(x => x.Delete(id))
                .Throws(new ArgumentOutOfRangeException());

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.DeleteRoom(id).ConfigureAwait(false));
            roomServiceMoq.Verify(x => x.GetById(id), Times.Once);
            roomServiceMoq.Verify(x => x.Delete(id), Times.Once);
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteRoom_WhenRoomCanBeDeleted_ReturnsNoContentResult(long id)
        {
            // Arrange
            roomServiceMoq.Setup(x => x.GetById(id))
                .ReturnsAsync(new ChatRoomDto()
                {
                    ChatMessages = new ChatMessageDto[] { },
                    Users = new UserDto[] { new UserDto() { Id = userId } },
                });

            roomServiceMoq.Setup(x => x.Delete(id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.DeleteRoom(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            roomServiceMoq.Verify(x => x.GetById(id), Times.Once);
            roomServiceMoq.Verify(x => x.Delete(id), Times.Once);
            Assert.AreEqual(204, result.StatusCode);
        }
        #endregion

#pragma warning restore SA1124 // Do not use regions
    }
}
