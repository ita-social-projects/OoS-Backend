using System;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;

namespace OutOfSchool.WebApi.Tests.Extensions
{
    [TestFixture]
    public class MappingExtensionsTests
    {
        [Test]
        public void Mapping_ChatMessageDtoToDomain_IsCorrect()
        {
            // Arrange
            ChatMessageWorkshopDto chatMessageDTO = new ChatMessageWorkshopDto()
            {
                Id = 1,
                ChatRoomId = 2,
                Text = "test mess",
                SenderRoleIsProvider = true,
                ReadDateTime = null,
                CreatedDateTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
            };

            // Act
            var result = chatMessageDTO.ToDomain();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatMessageWorkshop>(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual(chatMessageDTO.Id, result.Id);
            Assert.AreEqual(chatMessageDTO.ChatRoomId, result.ChatRoomId);
            Assert.AreEqual(chatMessageDTO.Text, result.Text);
            Assert.AreEqual(chatMessageDTO.SenderRoleIsProvider, result.SenderRoleIsProvider);
            Assert.AreEqual(chatMessageDTO.CreatedDateTime, result.CreatedDateTime);
            Assert.AreEqual(chatMessageDTO.ReadDateTime, result.ReadDateTime);
            Assert.IsNull(result.ChatRoom);
        }

        [Test]
        public void Mapping_ChatMessageToModel_IsCorrect()
        {
            // Arrange
            ChatMessageWorkshop chatMessage = new ChatMessageWorkshop()
            {
                Id = 1,
                ChatRoomId = 2,
                Text = "test mess",
                SenderRoleIsProvider = true,
                ReadDateTime = DateTimeOffset.Parse("2021-05-25T12:15:12", new CultureInfo("uk-UA", false)),
                CreatedDateTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                ChatRoom = Mock.Of<ChatRoomWorkshop>(),
            };

            // Act
            var result = chatMessage.ToModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatMessageWorkshopDto>(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual(chatMessage.Id, result.Id);
            Assert.AreEqual(chatMessage.ChatRoomId, result.ChatRoomId);
            Assert.AreEqual(chatMessage.Text, result.Text);
            Assert.AreEqual(chatMessage.SenderRoleIsProvider, result.SenderRoleIsProvider);
            Assert.AreEqual(chatMessage.CreatedDateTime, result.CreatedDateTime);
            Assert.AreEqual(chatMessage.ReadDateTime, result.ReadDateTime);
        }

        [Test]
        public void Mapping_ChatRoomToModel_IsCorrect()
        {
            // Arrange
            var chatMessage1 = new ChatMessageWorkshop()
            {
                Id = 1,
                ChatRoomId = 2,
                Text = "test mess",
                SenderRoleIsProvider = true,
                ReadDateTime = DateTimeOffset.Parse("2021-05-25T12:15:12", new CultureInfo("uk-UA", false)),
                CreatedDateTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                ChatRoom = Mock.Of<ChatRoomWorkshop>(),
            };
            var chatMessage2 = new ChatMessageWorkshop()
            {
                Id = 2,
                ChatRoomId = 2,
                Text = "test mess",
                SenderRoleIsProvider = false,
                ReadDateTime = null,
                CreatedDateTime = DateTimeOffset.Parse("2021-05-24T12:15:20", new CultureInfo("uk-UA", false)),
                ChatRoom = Mock.Of<ChatRoomWorkshop>(),
            };

            var listOfMessages = new List<ChatMessageWorkshop>() { chatMessage1, chatMessage2 };

            var chatRoom = new ChatRoomWorkshop()
            {
                Id = 1,
                WorkshopId = 1,
                ParentId = 1,
                ChatMessages = listOfMessages,
                Parent = new Parent()
                {
                    Id = 1,
                    UserId = "userParent",
                    User = new User()
                    {
                        Id = "userParent",
                        PhoneNumber = "123456734",
                        Email = "email",
                        FirstName = "Jack",
                        MiddleName = "Pirate",
                        LastName = "Sparrow",
                    },
                },
                Workshop = new Workshop() { Id = 1 },
            };

            // Act
            var result = chatRoom.ToModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatRoomWorkshopDto>(result);
            Assert.IsNotNull(result.Parent);
            Assert.IsNotNull(result.Workshop);
            Assert.IsInstanceOf<ParentDtoWithContactInfo>(result.Parent);
            Assert.IsInstanceOf<WorkshopInfoForChatListDto>(result.Workshop);
            Assert.AreEqual(chatRoom.Id, result.Id);
            Assert.AreEqual(chatRoom.WorkshopId, result.WorkshopId);
            Assert.AreEqual(chatRoom.Workshop.Id, result.Workshop.Id);
            Assert.AreEqual(chatRoom.Workshop.ProviderId, result.Workshop.ProviderId);
            Assert.AreEqual(chatRoom.Workshop.Title, result.Workshop.Title);
            Assert.AreEqual(chatRoom.Workshop.ProviderTitle, result.Workshop.ProviderTitle);
            Assert.AreEqual(chatRoom.ParentId, result.ParentId);
            Assert.AreEqual(chatRoom.Parent.UserId, result.Parent.UserId);
            Assert.AreEqual(chatRoom.Parent.Id, result.Parent.Id);
            Assert.AreEqual(chatRoom.Parent.User.LastName, result.Parent.LastName);
            Assert.AreEqual(chatRoom.Parent.User.MiddleName, result.Parent.MiddleName);
            Assert.AreEqual(chatRoom.Parent.User.FirstName, result.Parent.FirstName);
            Assert.AreEqual(chatRoom.Parent.User.Email, result.Parent.Email);
            Assert.AreEqual(chatRoom.Parent.User.PhoneNumber, result.Parent.PhoneNumber);
        }

        [Test]
        public void Mapping_ChatRoomWorkshopForChatList_IsCorrect()
        {
            // Arrange
            var chatRoom = new ChatRoomWorkshopForChatList()
            {
                Id = 1,
                WorkshopId = 1,
                ParentId = 1,
                LastMessage = new ChatMessageInfoForChatList()
                {
                    Id = 1,
                    ChatRoomId = 2,
                    Text = "test mess",
                    SenderRoleIsProvider = true,
                    ReadDateTime = DateTimeOffset.Parse("2021-05-25T12:15:12", new CultureInfo("uk-UA", false)),
                    CreatedDateTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                },
                NotReadByCurrentUserMessagesCount = 2,
                Parent = new ParentInfoForChatList()
                {
                    Id = 1,
                    UserId = "userId",
                    PhoneNumber = "123456734",
                    Email = "email",
                    FirstName = "Jack",
                    MiddleName = "Pirate",
                    LastName = "Sparrow",
                },
                Workshop = new WorkshopInfoForChatList()
                {
                    Id = 1,
                    Title = "workshop",
                    ProviderId = 1,
                    ProviderTitle = "provider",
                },
            };

            // Act
            var result = chatRoom.ToModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ChatRoomWorkshopDto>(result);
            Assert.IsNotNull(result.Parent);
            Assert.IsNotNull(result.Workshop);
            Assert.IsInstanceOf<ParentDtoWithContactInfo>(result.Parent);
            Assert.IsInstanceOf<WorkshopInfoForChatListDto>(result.Workshop);
            Assert.AreEqual(chatRoom.Id, result.Id);
            Assert.AreEqual(chatRoom.WorkshopId, result.WorkshopId);
            Assert.AreEqual(chatRoom.Workshop.Id, result.Workshop.Id);
            Assert.AreEqual(chatRoom.Workshop.ProviderId, result.Workshop.ProviderId);
            Assert.AreEqual(chatRoom.Workshop.Title, result.Workshop.Title);
            Assert.AreEqual(chatRoom.Workshop.ProviderTitle, result.Workshop.ProviderTitle);
            Assert.AreEqual(chatRoom.ParentId, result.ParentId);
            Assert.AreEqual(chatRoom.Parent.UserId, result.Parent.UserId);
            Assert.AreEqual(chatRoom.Parent.Id, result.Parent.Id);
            Assert.AreEqual(chatRoom.Parent.LastName, result.Parent.LastName);
            Assert.AreEqual(chatRoom.Parent.MiddleName, result.Parent.MiddleName);
            Assert.AreEqual(chatRoom.Parent.FirstName, result.Parent.FirstName);
            Assert.AreEqual(chatRoom.Parent.Email, result.Parent.Email);
            Assert.AreEqual(chatRoom.Parent.PhoneNumber, result.Parent.PhoneNumber);
            Assert.AreEqual(chatRoom.NotReadByCurrentUserMessagesCount, result.NotReadByCurrentUserMessagesCount);
            Assert.AreEqual(chatRoom.LastMessage.Text, result.LastMessage.Text);
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
