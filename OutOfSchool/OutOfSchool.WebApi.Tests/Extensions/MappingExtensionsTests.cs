using System;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Tests.Extensions
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
                ChatRoomId = 2,
                Text = "test mess",
                SenderRoleIsProvider = true,
                IsRead = true,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
            };

            // Act
            var result = chatMessageDTO.ToDomain();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatMessage>(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual(chatMessageDTO.Id, result.Id);
            Assert.AreEqual(chatMessageDTO.ChatRoomId, result.ChatRoomId);
            Assert.AreEqual(chatMessageDTO.Text, result.Text);
            Assert.AreEqual(chatMessageDTO.SenderRoleIsProvider, result.SenderRoleIsProvider);
            Assert.AreEqual(chatMessageDTO.CreatedTime, result.CreatedTime);
            Assert.AreEqual(chatMessageDTO.IsRead, result.IsRead);
            Assert.IsNull(result.ChatRoom);
        }

        [Test]
        public void Mapping_ChatMessageToModel_IsCorrect()
        {
            // Arrange
            ChatMessage chatMessage = new ChatMessage()
            {
                Id = 1,
                ChatRoomId = 2,
                Text = "test mess",
                SenderRoleIsProvider = true,
                IsRead = true,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                ChatRoom = Mock.Of<ChatRoom>(),
            };

            // Act
            var result = chatMessage.ToModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatMessageDto>(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual(chatMessage.Id, result.Id);
            Assert.AreEqual(chatMessage.ChatRoomId, result.ChatRoomId);
            Assert.AreEqual(chatMessage.Text, result.Text);
            Assert.AreEqual(chatMessage.SenderRoleIsProvider, result.SenderRoleIsProvider);
            Assert.AreEqual(chatMessage.CreatedTime, result.CreatedTime);
            Assert.AreEqual(chatMessage.IsRead, result.IsRead);
        }

        [Test]
        public void Mapping_ChatRoomToModel_IsCorrect()
        {
            // Arrange
            var chatMessage1 = new ChatMessage()
            {
                Id = 1,
                ChatRoomId = 2,
                Text = "test mess",
                SenderRoleIsProvider = true,
                IsRead = true,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                ChatRoom = Mock.Of<ChatRoom>(),
            };
            var chatMessage2 = new ChatMessage()
            {
                Id = 2,
                ChatRoomId = 2,
                Text = "test mess",
                SenderRoleIsProvider = false,
                IsRead = false,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:20", new CultureInfo("uk-UA", false)),
                ChatRoom = Mock.Of<ChatRoom>(),
            };

            var listOfMessages = new List<ChatMessage>() { chatMessage1, chatMessage2 };

            var chatRoom = new ChatRoom()
            {
                Id = 1,
                WorkshopId = 1,
                ParentId = 1,
                ChatMessages = listOfMessages,
                Parent = new Parent() { Id = 1, UserId = "userParent", User = new User() { Id = "userParent" } },
                Workshop = new Workshop() { Id = 1 },
            };

            // Act
            var result = chatRoom.ToModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatRoomDto>(result);
            Assert.IsNotNull(result.ChatMessages);
            Assert.IsNotNull(result.Parent);
            Assert.IsNotNull(result.Workshop);
            Assert.IsNotNull(result.Parent.User);
            Assert.IsInstanceOf<ICollection<ChatMessageDto>>(result.ChatMessages);
            Assert.IsInstanceOf<ParentDtoWithShortUserInfo>(result.Parent);
            Assert.IsInstanceOf<WorkshopCard>(result.Workshop);
            Assert.IsInstanceOf<ShortUserDto>(result.Parent.User);
            Assert.AreEqual(chatRoom.Id, result.Id);
            Assert.AreEqual(chatRoom.WorkshopId, result.WorkshopId);
            Assert.AreEqual(chatRoom.ParentId, result.ParentId);
            Assert.AreEqual(chatRoom.ChatMessages.Count, result.ChatMessages.Count);
            Assert.AreEqual(chatRoom.Parent.Id, result.Parent.Id);
            Assert.AreEqual(chatRoom.Workshop.Id, result.Workshop.WorkshopId);
            Assert.Zero(result.NotReadMessagesCount);
        }

        [Test]
        public void Mapping_ChatRoomToModelWithoutCHatMessages_IsCorrect()
        {
            // Arrange
            var chatMessage1 = new ChatMessage()
            {
                Id = 1,
                ChatRoomId = 2,
                Text = "test mess",
                SenderRoleIsProvider = true,
                IsRead = true,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                ChatRoom = Mock.Of<ChatRoom>(),
            };
            var chatMessage2 = new ChatMessage()
            {
                Id = 2,
                ChatRoomId = 2,
                Text = "test mess",
                SenderRoleIsProvider = false,
                IsRead = false,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:20", new CultureInfo("uk-UA", false)),
                ChatRoom = Mock.Of<ChatRoom>(),
            };

            var listOfMessages = new List<ChatMessage>() { chatMessage1, chatMessage2 };

            var chatRoom = new ChatRoom()
            {
                Id = 1,
                WorkshopId = 1,
                ParentId = 1,
                ChatMessages = listOfMessages,
                Parent = new Parent() { Id = 1, UserId = "userParent", User = new User() { Id = "userParent" } },
                Workshop = new Workshop() { Id = 1 },
            };

            // Act
            var result = chatRoom.ToModelWithoutChatMessages();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatRoomDto>(result);
            Assert.IsNull(result.ChatMessages);
            Assert.IsNotNull(result.Parent);
            Assert.IsNotNull(result.Workshop);
            Assert.IsNotNull(result.Parent.User);
            Assert.IsInstanceOf<ParentDtoWithShortUserInfo>(result.Parent);
            Assert.IsInstanceOf<WorkshopCard>(result.Workshop);
            Assert.IsInstanceOf<ShortUserDto>(result.Parent.User);
            Assert.AreEqual(chatRoom.Id, result.Id);
            Assert.AreEqual(chatRoom.WorkshopId, result.WorkshopId);
            Assert.AreEqual(chatRoom.ParentId, result.ParentId);
            Assert.AreEqual(chatRoom.Parent.Id, result.Parent.Id);
            Assert.AreEqual(chatRoom.Workshop.Id, result.Workshop.WorkshopId);
            Assert.Zero(result.NotReadMessagesCount);
        }

        [Test]
        public void Mapping_WorkshopDTO_ToCardDto_IsCorrect()
        {
            // Arrange
            var workshopDto = new WorkshopDTO()
            {
                Id = 5,
                Title = "Title5",
                Phone = "1111111111",
                Description = "Desc5",
                Price = 5000,
                IsPerMonth = true,
                WithDisabilityOptions = true,
                DaysPerWeek = 5,
                Head = "Head5",
                HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                ProviderTitle = "ProviderTitle",
                DisabilityOptionsDesc = "Desc5",
                Website = "website5",
                Instagram = "insta5",
                Facebook = "facebook5",
                Email = "email5@gmail.com",
                MaxAge = 10,
                MinAge = 4,
                Logo = "image5",
                ProviderId = 5,
                DirectionId = 1,
                Direction = "Some title of direction",
                DepartmentId = 1,
                ClassId = 1,
                AddressId = 17,
                Address = new AddressDto
                {
                    Id = 17,
                    Region = "Region17",
                    District = "District17",
                    City = "City17",
                    Street = "Street17",
                    BuildingNumber = "BuildingNumber17",
                    Latitude = 123.2355,
                    Longitude = 23.1234,
                },
                Teachers = new List<TeacherDTO>()
                        {
                            new TeacherDTO
                            {
                                Id = 9,
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("1990-01-01"),
                                WorkshopId = 5,
                            },
                            new TeacherDTO
                            {
                                Id = 10,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("1990-01-01"),
                                WorkshopId = 5,
                            },
                        },
                Keywords = new List<string>()
                {
                    "dance",
                    "twist",
                },
                Rating = 23.12314f,
            };

            // Act
            var result = workshopDto.ToCardDto();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<WorkshopCard>(result);
            Assert.AreEqual(workshopDto.Id, result.WorkshopId);
            Assert.AreEqual(workshopDto.Title, result.Title);
            Assert.AreEqual(workshopDto.Price, result.Price);
            Assert.AreEqual(workshopDto.IsPerMonth, result.IsPerMonth);
            Assert.AreEqual(workshopDto.ProviderId, result.ProviderId);
            Assert.AreEqual(workshopDto.ProviderTitle, result.ProviderTitle);
            Assert.AreEqual(workshopDto.MinAge, result.MinAge);
            Assert.AreEqual(workshopDto.MaxAge, result.MaxAge);
            Assert.AreEqual(workshopDto.Logo, result.Photo);
            Assert.AreEqual(workshopDto.Direction, result.Direction);
            Assert.IsNotNull(result.Address);
            Assert.IsInstanceOf<AddressDto>(result.Address);
            Assert.AreEqual(workshopDto.Address.Latitude, result.Address.Latitude);
            Assert.AreEqual(workshopDto.Rating, result.Rating);
        }
    }
}
