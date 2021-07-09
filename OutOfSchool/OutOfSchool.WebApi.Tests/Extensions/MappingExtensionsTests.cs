using System;
using System.Collections.Generic;
using NUnit.Framework;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Extensions.Tests
{
    [TestFixture]
    public class MappingExtensionsTests
    {
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
    }
}
