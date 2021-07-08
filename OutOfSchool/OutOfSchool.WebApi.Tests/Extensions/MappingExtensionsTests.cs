using System;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using NUnit.Framework;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Extensions.Tests
{
    [TestFixture]
    public class MappingExtensionsTests
    {
        #region ElasticsearchMapping
        [Test]
        public void Mapping_WorkshopDto_ToESModel_IsCorrect()
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
            var result = workshopDto.ToESModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<WorkshopES>(result);
            Assert.AreEqual(workshopDto.Id, result.Id);
            Assert.AreEqual(workshopDto.Title, result.Title);
            Assert.AreEqual(workshopDto.Price, result.Price);
            Assert.AreEqual(workshopDto.IsPerMonth, result.IsPerMonth);
            Assert.AreEqual(workshopDto.WithDisabilityOptions, result.WithDisabilityOptions);
            Assert.AreEqual(workshopDto.ProviderId, result.ProviderId);
            Assert.AreEqual(workshopDto.ProviderTitle, result.ProviderTitle);
            Assert.AreEqual(workshopDto.MinAge, result.MinAge);
            Assert.AreEqual(workshopDto.MaxAge, result.MaxAge);
            Assert.AreEqual(workshopDto.Logo, result.Logo);
            Assert.AreEqual(workshopDto.DirectionId, result.DirectionId);
            Assert.AreEqual(workshopDto.Direction, result.Direction);
            Assert.AreEqual(workshopDto.DepartmentId, result.DepartmentId);
            Assert.AreEqual(workshopDto.ClassId, result.ClassId);
            Assert.AreEqual(workshopDto.AddressId, result.AddressId);
            Assert.IsNotNull(result.Address);
            Assert.IsInstanceOf<AddressES>(result.Address);
            Assert.AreEqual(workshopDto.Address.Latitude, result.Address.Latitude);
            Assert.AreEqual(workshopDto.Rating, result.Rating);
            Assert.IsNotNull(result.Teachers);
            Assert.IsInstanceOf<IEnumerable<TeacherES>>(result.Teachers);
            Assert.AreEqual("dance¤twist", result.Keywords);
        }

        [Test]
        public void Mapping_WorkshopFilterDto_ToESModel_IsCorrect()
        {
            // Arrange
            var filter = new WorkshopFilterDto()
            {
                Ids = new List<long>() { 1, 2 },
                Ages = new List<AgeRange>() { new AgeRange() { MinAge = 0, MaxAge = 5 } },
                City = "City",
                DirectionIds = new List<long>() { 1, 2 },
                MinPrice = 0,
                MaxPrice = 20,
                OrderByField = Enums.OrderBy.Price,
                SearchText = "Text",
            };

            // Act
            var result = filter.ToESModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<WorkshopFilterES>(result);
            Assert.AreEqual(filter.Ids, result.Ids);
            Assert.AreEqual(filter.DirectionIds, result.DirectionIds);
            Assert.IsNotNull(result.Ages);
            Assert.IsInstanceOf<List<AgeRangeES>>(result.Ages);
            Assert.AreEqual(filter.City, result.City);
            Assert.AreEqual(filter.MinPrice, result.MinPrice);
            Assert.AreEqual(filter.MaxPrice, result.MaxPrice);
            Assert.AreEqual(filter.OrderByField.ToString(), result.OrderByField.ToString());
            Assert.AreEqual(filter.SearchText, result.SearchText);
        }

        [Test]
        public void Mapping_WorkshopES_ToCardDto_IsCorrect()
        {
            // Arrange
            var workshopES = new WorkshopES()
            {
                Id = 5,
                Title = "Title5",
                Price = 5000,
                IsPerMonth = true,
                WithDisabilityOptions = true,
                ProviderTitle = "ProviderTitle",
                MaxAge = 10,
                MinAge = 4,
                Logo = "image5",
                ProviderId = 5,
                DirectionId = 1,
                Direction = "Some title of direction",
                DepartmentId = 1,
                ClassId = 1,
                AddressId = 17,
                Address = new AddressES
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
                Teachers = new List<TeacherES>()
            {
                            new TeacherES
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
                            new TeacherES
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
                Keywords = "dance¤twist",
                Rating = 23.12314f,
            };

            // Act
            var result = workshopES.ToCardDto();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<WorkshopCard>(result);
            Assert.AreEqual(workshopES.Id, result.WorkshopId);
            Assert.AreEqual(workshopES.Title, result.Title);
            Assert.AreEqual(workshopES.Price, result.Price);
            Assert.AreEqual(workshopES.IsPerMonth, result.IsPerMonth);
            Assert.AreEqual(workshopES.ProviderId, result.ProviderId);
            Assert.AreEqual(workshopES.ProviderTitle, result.ProviderTitle);
            Assert.AreEqual(workshopES.MinAge, result.MinAge);
            Assert.AreEqual(workshopES.MaxAge, result.MaxAge);
            Assert.AreEqual(workshopES.Logo, result.Photo);
            Assert.AreEqual(workshopES.Direction, result.Direction);
            Assert.IsNotNull(result.Address);
            Assert.IsInstanceOf<AddressDto>(result.Address);
            Assert.AreEqual(workshopES.Address.Latitude, result.Address.Latitude);
            Assert.AreEqual(workshopES.Rating, result.Rating);
        }
        #endregion

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
                NotReadMessagesCount = 4,
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
            Assert.Zero(result.NotReadMessagesCount);
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
            Assert.Zero(result.NotReadMessagesCount);
        }
    }
}
