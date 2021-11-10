using System;
using System.Collections.Generic;
using System.Globalization;

using AutoMapper;

using Moq;

using NUnit.Framework;

using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util;

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
                Id = Guid.NewGuid(),
                UserId = "test",
                ChatRoomId = Guid.NewGuid(),
                Text = "test mess",
                IsRead = true,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
            };

            // Act
            var result = chatMessageDTO.ToDomain();

            // Assert
            Assert.Multiple(() =>
            {
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
            });
        }

        [Test]
        public void Mapping_ChatMessageToModel_IsCorrect()
        {
            // Arrange
            ChatMessage chatMessage = new ChatMessage()
            {
                Id = Guid.NewGuid(),
                UserId = "test",
                ChatRoomId = Guid.NewGuid(),
                Text = "test mess",
                IsRead = true,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                User = Mock.Of<User>(),
                ChatRoom = Mock.Of<ChatRoom>(),
            };

            // Act
            var result = chatMessage.ToModel();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<ChatMessageDto>(result);
                Assert.IsNotNull(result.Text);
                Assert.AreEqual(chatMessage.Id, result.Id);
                Assert.AreEqual(chatMessage.UserId, result.UserId);
                Assert.AreEqual(chatMessage.ChatRoomId, result.ChatRoomId);
                Assert.AreEqual(chatMessage.Text, result.Text);
                Assert.AreEqual(chatMessage.CreatedTime, result.CreatedTime);
                Assert.AreEqual(chatMessage.IsRead, result.IsRead);
            });
        }

        [Test]
        public void Mapping_ChatRoomDtoToDomain_IsCorrect()
        {
            // Arrange
            var user1 = new UserDto() { Id = "test" };
            var user2 = new UserDto() { Id = "test2" };

            var chatMessage1 = new ChatMessageDto()
            {
                Id = Guid.NewGuid(),
                UserId = "test",
                ChatRoomId = Guid.NewGuid(),
                Text = "test mess",
                IsRead = true,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
            };
            var chatMessage2 = new ChatMessageDto()
            {
                Id = Guid.NewGuid(),
                UserId = "test2",
                ChatRoomId = Guid.NewGuid(),
                Text = "test mess",
                IsRead = false,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:20", new CultureInfo("uk-UA", false)),
            };

            var listOfUsers = new List<UserDto>() { user1, user2 };

            var listOfMessages = new List<ChatMessageDto>() { chatMessage1, chatMessage2 };

            var chatRoomDto = new ChatRoomDto()
            {
                Id = Guid.NewGuid(),
                WorkshopId = Guid.NewGuid(),
                ChatMessages = listOfMessages,
                Users = listOfUsers,
                NotReadMessagesCount = 4,
            };

            // Act
            var result = chatRoomDto.ToDomain();

            // Assert
            Assert.Multiple(() =>
            {
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
            });
        }

        [Test]
        public void Mapping_ChatRoomToModel_IsCorrect()
        {
            // Arrange
            var user1 = new User() { Id = "test" };
            var user2 = new User() { Id = "test2" };

            var chatMessage1 = new ChatMessage()
            {
                Id = Guid.NewGuid(),
                UserId = "test",
                ChatRoomId = Guid.NewGuid(),
                Text = "test mess",
                IsRead = true,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                User = Mock.Of<User>(),
                ChatRoom = Mock.Of<ChatRoom>(),
            };
            var chatMessage2 = new ChatMessage()
            {
                Id = Guid.NewGuid(),
                UserId = "test2",
                ChatRoomId = Guid.NewGuid(),
                Text = "test mess",
                IsRead = false,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:20", new CultureInfo("uk-UA", false)),
                User = Mock.Of<User>(),
                ChatRoom = Mock.Of<ChatRoom>(),
            };

            var listOfUsers = new List<User>() { user1, user2 };

            var listOfMessages = new List<ChatMessage>() { chatMessage1, chatMessage2 };

            var chatRoom = new ChatRoom()
            {
                Id = Guid.NewGuid(),
                WorkshopId = Guid.NewGuid(),
                Workshop = Mock.Of<Workshop>(),
                ChatMessages = listOfMessages,
                Users = listOfUsers,
                ChatRoomUsers = new List<ChatRoomUser>(),
            };

            // Act
            var result = chatRoom.ToModel();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<ChatRoomDto>(result);
                Assert.IsNotNull(result.ChatMessages);
                Assert.IsInstanceOf<IEnumerable<ChatMessageDto>>(result.ChatMessages);
                Assert.IsNotNull(result.Users);
                Assert.IsInstanceOf<IEnumerable<UserDto>>(result.Users);
                Assert.AreEqual(chatRoom.Id, result.Id);
                Assert.AreEqual(chatRoom.WorkshopId, result.WorkshopId);
                Assert.Zero(result.NotReadMessagesCount);
                foreach (var el in result.ChatMessages)
                {
                    Assert.AreEqual(chatMessage1.Text, el.Text);
                }
            });
        }

        [Test]
        public void Mapping_ChatRoomToModelWithoutCHatMessages_IsCorrect()
        {
            // Arrange
            var user1 = new User() { Id = "test" };
            var user2 = new User() { Id = "test2" };

            var chatMessage1 = new ChatMessage()
            {
                Id = Guid.NewGuid(),
                UserId = "test",
                ChatRoomId = Guid.NewGuid(),
                Text = "test mess",
                IsRead = true,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:12", new CultureInfo("uk-UA", false)),
                User = Mock.Of<User>(),
                ChatRoom = Mock.Of<ChatRoom>(),
            };
            var chatMessage2 = new ChatMessage()
            {
                Id = Guid.NewGuid(),
                UserId = "test2",
                ChatRoomId = Guid.NewGuid(),
                Text = "test mess",
                IsRead = false,
                CreatedTime = DateTimeOffset.Parse("2021-05-24T12:15:20", new CultureInfo("uk-UA", false)),
                User = Mock.Of<User>(),
                ChatRoom = Mock.Of<ChatRoom>(),
            };

            var listOfUsers = new List<User>() { user1, user2 };

            var listOfMessages = new List<ChatMessage>() { chatMessage1, chatMessage2 };

            var chatRoom = new ChatRoom()
            {
                Id = Guid.NewGuid(),
                WorkshopId = Guid.NewGuid(),
                Workshop = Mock.Of<Workshop>(),
                ChatMessages = listOfMessages,
                Users = listOfUsers,
                ChatRoomUsers = new List<ChatRoomUser>(),
            };

            // Act
            var result = chatRoom.ToModelWithoutChatMessages();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<ChatRoomDto>(result);
                Assert.IsNull(result.ChatMessages);
                Assert.IsNotNull(result.Users);
                Assert.IsInstanceOf<IEnumerable<UserDto>>(result.Users);
                Assert.AreEqual(chatRoom.Id, result.Id);
                Assert.AreEqual(chatRoom.WorkshopId, result.WorkshopId);
                Assert.Zero(result.NotReadMessagesCount);
            });
        }

        [Test]
        public void Mapping_WorkshopDTO_ToCardDto_IsCorrect()
        {
            // Arrange
            var workshopDto = new WorkshopDTO()
            {
                Id = Guid.NewGuid(),
                Title = "Title5",
                Phone = "1111111111",
                Description = "Desc5",
                Price = 5000,
                IsPerMonth = true,
                WithDisabilityOptions = true,
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
                ProviderId = Guid.NewGuid(),
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
                                Id = Guid.NewGuid(),
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("1990-01-01"),
                                WorkshopId = new Guid("3a217d24-4945-477b-9381-e9ee8dc1f338"),
                            },
                            new TeacherDTO
                            {
                                Id = Guid.NewGuid(),
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("1990-01-01"),
                                WorkshopId = new Guid("3a217d24-4945-477b-9381-e9ee8dc1f338"),
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
            Assert.Multiple(() =>
            {
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
                Assert.AreEqual(workshopDto.DirectionId, result.DirectionId);
                Assert.AreEqual(workshopDto.Address?.Id, result.Address.Id);
                Assert.IsNotNull(result.Address);
                Assert.AreEqual(workshopDto.Address.Latitude, result.Address.Latitude);
                Assert.AreEqual(workshopDto.Rating, result.Rating);
            });
        }

        // TODO: fix mapper configuration
        [Test]
        public void Mapping_MappingProfile_ConfigurationIsCorrect()
        {
            // arrange
            var profile = new MappingProfile();

            // act
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));

            // assert
            configuration.AssertConfigurationIsValid();
        }

        [Test]
        public void Mapping_ProviderDto_ToProvider_IsCorrect()
        {
            var providerDto = ProviderDtoGenerator.Generate();
            var provider = providerDto.ToDomain();

            EnsureProviderAndProviderDtoAreEqual(providerDto, provider);
        }

        [Test]
        public void Mapping_Provider_ToProviderDto_IsCorrect()
        {
            var provider = ProvidersGenerator.Generate();
            var providerDto = provider.ToModel();

            EnsureProviderAndProviderDtoAreEqual(providerDto, provider);
        }

        private static void EnsureProviderAndProviderDtoAreEqual(ProviderDto providerDto, Provider provider)
        {
            Assert.Multiple(() =>
            {
                Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
                Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
                Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
                Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
                Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
                Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
                Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
                Assert.That(providerDto.FullTitle, Is.EqualTo(provider.FullTitle));
                Assert.That(providerDto.ShortTitle, Is.EqualTo(provider.ShortTitle));
                Assert.That(providerDto.Website, Is.EqualTo(provider.Website));
                Assert.That(providerDto.Email, Is.EqualTo(provider.Email));
                Assert.That(providerDto.Facebook, Is.EqualTo(provider.Facebook));
                Assert.That(providerDto.Instagram, Is.EqualTo(provider.Instagram));
                Assert.That(providerDto.Description, Is.EqualTo(provider.Description));
                Assert.That(providerDto.EdrpouIpn, Is.EqualTo(provider.EdrpouIpn.ToString()));
                Assert.That(providerDto.Director, Is.EqualTo(provider.Director));
                Assert.That(providerDto.DirectorDateOfBirth, Is.EqualTo(provider.DirectorDateOfBirth));
                Assert.That(providerDto.PhoneNumber, Is.EqualTo(provider.PhoneNumber));
                Assert.That(providerDto.Founder, Is.EqualTo(provider.Founder));
                Assert.That(providerDto.Ownership, Is.EqualTo(provider.Ownership));
                Assert.That(providerDto.Type, Is.EqualTo(provider.Type));
                Assert.That(providerDto.Status, Is.EqualTo(provider.Status));
                //Assert.That(providerDto.Rating, Is.EqualTo(provider.Rating));
                //Assert.That(providerDto.NumberOfRatings, Is.EqualTo(provider.NumberOfRatings));
                Assert.That(providerDto.UserId, Is.EqualTo(provider.UserId));
                // TODO: add address comparers
                //Assert.That(providerDto.LegalAddress, Is.EqualTo(provider.LegalAddress));
                //Assert.That(providerDto.ActualAddress, Is.EqualTo(provider.ActualAddress));
                Assert.That(providerDto.InstitutionStatusId, Is.EqualTo(provider.InstitutionStatusId));
            });
        }
    }
}
