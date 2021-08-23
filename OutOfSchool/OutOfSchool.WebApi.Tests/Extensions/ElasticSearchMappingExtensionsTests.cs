using System;
using System.Collections.Generic;
using NUnit.Framework;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Tests.Extensions
{
    [TestFixture]
    public class ElasticSearchMappingExtensionsTests
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
            Assert.AreEqual(workshopDto.Description, result.Description);
        }

        [Test]
        public void Mapping_WorkshopFilterDto_ToESModel_IsCorrect()
        {
            // Arrange
            var filter = new WorkshopFilter()
            {
                Ids = new List<long>() { 1, 2 },
                MinAge = 0,
                MaxAge = 5,
                WithDisabilityOptions = true,
                City = "City",
                DirectionIds = new List<long>() { 1, 2 },
                IsFree = false,
                MinPrice = 0,
                MaxPrice = 20,
                OrderByField = Enums.OrderBy.PriceDesc.ToString(),
                SearchText = "Text",
                From = 13,
                Size = 12,
            };

            // Act
            var result = filter.ToESModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<WorkshopFilterES>(result);
            Assert.AreEqual(filter.Ids, result.Ids);
            Assert.AreEqual(filter.DirectionIds, result.DirectionIds);
            Assert.AreEqual(filter.IsFree, result.IsFree);
            Assert.AreEqual(filter.MinAge, result.MinAge);
            Assert.AreEqual(filter.MaxAge, result.MaxAge);
            Assert.AreEqual(filter.WithDisabilityOptions, result.WithDisabilityOptions);
            Assert.AreEqual(filter.City, result.City);
            Assert.AreEqual(filter.MinPrice, result.MinPrice);
            Assert.AreEqual(filter.MaxPrice, result.MaxPrice);
            Assert.AreEqual(filter.OrderByField.ToString(), result.OrderByField.ToString());
            Assert.AreEqual(filter.SearchText, result.SearchText);
            Assert.AreEqual(filter.From, result.From);
            Assert.AreEqual(filter.Size, result.Size);
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
                Description = "Some description",
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

        [Test]
        public void Mapping_SearchResultES_ToSearchResult_IsCorrect()
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
            var searchResultES = new SearchResultES<WorkshopES>()
            {
                TotalAmount = 1,
                Entities = new List<WorkshopES>() { workshopES },
            };

            // Act
            var result = searchResultES.ToSearchResult();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<SearchResult<WorkshopCard>>(result);
            Assert.IsNotNull(result.Entities);
            Assert.IsInstanceOf<IReadOnlyCollection<WorkshopCard>>(result.Entities);
            Assert.AreEqual(searchResultES.TotalAmount, result.TotalAmount);
            Assert.AreEqual(searchResultES.Entities.Count, result.Entities.Count);
        }
    }
}
