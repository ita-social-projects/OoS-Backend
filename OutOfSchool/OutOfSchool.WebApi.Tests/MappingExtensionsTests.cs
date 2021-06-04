using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AutoMapper;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Tests
{
    [TestFixture]
    public class MappingExtensionsTests
    {
        [Test]
        public void Mapping_ChatMessageDtoToDomain_IsCorrect()
        {
            // Arrange
            ChatMessageDto chatMessageDTO = new ChatMessageDto()
            {
                Id = 1,
                UserId = "test",
                ChatRoomId = 2,
                Text = "test mess",
                IsRead = true,
                CreatedTime = DateTime.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
            };

            // Act
            var result = chatMessageDTO.ToDomain();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatMessage>(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual(chatMessageDTO.Id, result.Id);
            Assert.AreEqual(chatMessageDTO.UserId, result.UserId);
            Assert.AreEqual(chatMessageDTO.ChatRoomId, result.ChatRoomId);
            Assert.AreEqual(chatMessageDTO.Text, result.Text);
            Assert.AreEqual(chatMessageDTO.CreatedTime, result.CreatedTime);
            Assert.AreEqual(chatMessageDTO.IsRead, result.IsRead);
            Assert.IsNull(result.User);
            Assert.IsNull(result.ChatRoom);
        }

        [Test]
        public void Mapping_ChatMessageToModel_IsCorrect()
        {
            // Arrange
            ChatMessage chatMessage = new ChatMessage()
            {
                Id = 1,
                UserId = "test",
                ChatRoomId = 2,
                Text = "test mess",
                IsRead = true,
                CreatedTime = DateTime.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                User = Mock.Of<User>(),
                ChatRoom = Mock.Of<ChatRoom>(),
            };

            // Act
            var result = chatMessage.ToModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatMessageDto>(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual(chatMessage.Id, result.Id);
            Assert.AreEqual(chatMessage.UserId, result.UserId);
            Assert.AreEqual(chatMessage.ChatRoomId, result.ChatRoomId);
            Assert.AreEqual(chatMessage.Text, result.Text);
            Assert.AreEqual(chatMessage.CreatedTime, result.CreatedTime);
            Assert.AreEqual(chatMessage.IsRead, result.IsRead);
        }

        [Test]
        public void Mapping_ChatRoomDtoToDomain_IsCorrect()
        {
            // Arrange
            var user1 = new UserDto() { Id = "test" };
            var user2 = new UserDto() { Id = "test2" };

            var chatMessage1 = new ChatMessageDto()
            {
                Id = 1,
                UserId = "test",
                ChatRoomId = 2,
                Text = "test mess",
                IsRead = true,
                CreatedTime = DateTime.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
            };
            var chatMessage2 = new ChatMessageDto()
            {
                Id = 2,
                UserId = "test2",
                ChatRoomId = 2,
                Text = "test mess",
                IsRead = false,
                CreatedTime = DateTime.Parse("2021-05-24T12:15:20", new CultureInfo("uk-UA", false)),
            };

            var listOfUsers = new List<UserDto>();
            listOfUsers.Add(user1);
            listOfUsers.Add(user2);

            var listOfMessages = new List<ChatMessageDto>();
            listOfMessages.Add(chatMessage1);
            listOfMessages.Add(chatMessage2);

            var chatRoomDto = new ChatRoomDto()
            {
                Id = 1,
                WorkshopId = 1,
                ChatMessages = new List<ChatMessageDto>(),
                Users = listOfUsers,
            };

            // Act
            var result = chatRoomDto.ToDomain();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatRoom>(result);
            Assert.IsNull(result.Workshop);
            Assert.IsNotNull(result.ChatMessages);
            Assert.IsInstanceOf<ICollection<ChatMessage>>(result.ChatMessages);
            Assert.IsNotNull(result.Users);
            Assert.IsInstanceOf<ICollection<User>>(result.Users);
            Assert.IsNull(result.ChatRoomUsers);
            Assert.AreEqual(chatRoomDto.Id, result.Id);
            Assert.AreEqual(chatRoomDto.WorkshopId, result.WorkshopId);
            foreach (var el in result.ChatMessages)
            {
                Assert.AreEqual(chatMessage1.Text, el.Text);
            }
        }

        [Test]
        public void Mapping_ChatRoomToModel_IsCorrect()
        {
            // Arrange
            var user1 = new User() { Id = "test" };
            var user2 = new User() { Id = "test2" };

            var chatMessage1 = new ChatMessage()
            {
                Id = 1,
                UserId = "test",
                ChatRoomId = 2,
                Text = "test mess",
                IsRead = true,
                CreatedTime = DateTime.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                User = Mock.Of<User>(),
                ChatRoom = Mock.Of<ChatRoom>(),
            };
            var chatMessage2 = new ChatMessage()
            {
                Id = 2,
                UserId = "test2",
                ChatRoomId = 2,
                Text = "test mess",
                IsRead = false,
                CreatedTime = DateTime.Parse("2021-05-24T12:15:20", new CultureInfo("uk-UA", false)),
                User = Mock.Of<User>(),
                ChatRoom = Mock.Of<ChatRoom>(),
            };

            var listOfUsers = new List<User>();
            listOfUsers.Add(user1);
            listOfUsers.Add(user2);

            var listOfMessages = new List<ChatMessage>();
            listOfMessages.Add(chatMessage1);
            listOfMessages.Add(chatMessage2);

            var chatRoom = new ChatRoom()
            {
                Id = 1,
                WorkshopId = 1,
                Workshop = Mock.Of<Workshop>(),
                ChatMessages = listOfMessages,
                Users = listOfUsers,
                ChatRoomUsers = new List<ChatRoomUser>(),
            };

            // Act
            var result = chatRoom.ToModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatRoomDto>(result);
            Assert.IsNotNull(result.ChatMessages);
            Assert.IsInstanceOf<IEnumerable<ChatMessageDto>>(result.ChatMessages);
            Assert.IsNotNull(result.Users);
            Assert.IsInstanceOf<IEnumerable<UserDto>>(result.Users);
            Assert.AreEqual(chatRoom.Id, result.Id);
            Assert.AreEqual(chatRoom.WorkshopId, result.WorkshopId);
            foreach (var el in result.ChatMessages)
            {
                Assert.AreEqual(chatMessage1.Text, el.Text);
            }
        }

        [Test]
        public void Mapping_ChatRoomToModelWithoutCHatMessages_IsCorrect()
        {
            // Arrange
            var user1 = new User() { Id = "test" };
            var user2 = new User() { Id = "test2" };

            var chatMessage1 = new ChatMessage()
            {
                Id = 1,
                UserId = "test",
                ChatRoomId = 2,
                Text = "test mess",
                IsRead = true,
                CreatedTime = DateTime.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                User = Mock.Of<User>(),
                ChatRoom = Mock.Of<ChatRoom>(),
            };
            var chatMessage2 = new ChatMessage()
            {
                Id = 2,
                UserId = "test2",
                ChatRoomId = 2,
                Text = "test mess",
                IsRead = false,
                CreatedTime = DateTime.Parse("2021-05-24T12:15:20", new CultureInfo("uk-UA", false)),
                User = Mock.Of<User>(),
                ChatRoom = Mock.Of<ChatRoom>(),
            };

            var listOfUsers = new List<User>();
            listOfUsers.Add(user1);
            listOfUsers.Add(user2);

            var listOfMessages = new List<ChatMessage>();
            listOfMessages.Add(chatMessage1);
            listOfMessages.Add(chatMessage2);

            var chatRoom = new ChatRoom()
            {
                Id = 1,
                WorkshopId = 1,
                Workshop = Mock.Of<Workshop>(),
                ChatMessages = listOfMessages,
                Users = listOfUsers,
                ChatRoomUsers = new List<ChatRoomUser>(),
            };

            // Act
            var result = chatRoom.ToModelWithoutChatMessages();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatRoomDto>(result);
            Assert.IsNull(result.ChatMessages);
            Assert.IsNotNull(result.Users);
            Assert.IsInstanceOf<IEnumerable<UserDto>>(result.Users);
            Assert.AreEqual(chatRoom.Id, result.Id);
            Assert.AreEqual(chatRoom.WorkshopId, result.WorkshopId);
        }
    }
}
