//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Elasticsearch.Net;
//using Moq;
//using Nest;
//using NUnit.Framework;
//using OutOfSchool.ElasticsearchData;
//using OutOfSchool.ElasticsearchData.Models;
//using OutOfSchool.Services.Enums;
//using OutOfSchool.WebApi.Models;
//using OutOfSchool.WebApi.Services;

//namespace OutOfSchool.WebApi.Tests.Services.Elasticsearch
//{
//    [TestFixture]
//    public class ESWorkshopServiceTests
//    {
//        private IElasticsearchService<WorkshopES, WorkshopFilterES> service;

//        private Mock<IWorkshopService> mockWorkshopService;
//        private Mock<IRatingService> mockRatingService;
//        private Mock<IElasticsearchProvider<WorkshopES, WorkshopFilterES>> mockEsProvider;

//        [SetUp]
//        public void SetUp()
//        {
//            mockWorkshopService = new Mock<IWorkshopService>();
//            mockRatingService = new Mock<IRatingService>();
//            mockEsProvider = new Mock<IElasticsearchProvider<WorkshopES, WorkshopFilterES>>();

//            service = new ESWorkshopService(mockWorkshopService.Object, mockRatingService.Object, mockEsProvider.Object);
//        }

//        #region Index
//        [Test]
//        public void Index_WhenEntityIsNull_ShouldThrowException()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.IndexEntityAsync(It.IsAny<WorkshopES>()))
//               .ReturnsAsync(Result.Created);

//            // Act and Assert
//            Assert.ThrowsAsync<ArgumentNullException>(async () => await service.Index(null));
//            mockEsProvider.Verify(x => x.IndexEntityAsync(It.IsAny<WorkshopES>()), Times.Never);
//        }

//        [Test]
//        public async Task Index_WhenDocWasIndexed_ShouldReturnTrue()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.IndexEntityAsync(It.IsAny<WorkshopES>()))
//               .ReturnsAsync(Result.Created);

//            // Act
//            var result = await service.Index(new WorkshopES() { Id = 1, Title = "Title" }).ConfigureAwait(false);

//            // Assert
//            Assert.IsTrue(result);
//            mockEsProvider.Verify(x => x.IndexEntityAsync(It.IsAny<WorkshopES>()), Times.Once);
//        }

//        [Test]
//        public async Task Index_WhenDocWasNotIndexed_ShouldReturnFalse()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.IndexEntityAsync(It.IsAny<WorkshopES>()))
//               .ReturnsAsync(Result.Error);

//            // Act
//            var result = await service.Index(new WorkshopES() { Id = 1, Title = "Title" }).ConfigureAwait(false);

//            // Assert
//            Assert.IsFalse(result);
//            mockEsProvider.Verify(x => x.IndexEntityAsync(It.IsAny<WorkshopES>()), Times.Once);
//        }

//        [Test]
//        public async Task Index_WhenElasticsearchExceptionOccures_ShouldReturnFalse()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.IndexEntityAsync(It.IsAny<WorkshopES>()))
//               .Throws(new ElasticsearchClientException("exception"));

//            // Act
//            var result = await service.Index(new WorkshopES() { Id = 1, Title = "Title" }).ConfigureAwait(false);

//            // Assert
//            Assert.IsFalse(result);
//            mockEsProvider.Verify(x => x.IndexEntityAsync(It.IsAny<WorkshopES>()), Times.Once);
//        }
//        #endregion

//        #region Update
//        [Test]
//        public void Update_WhenEntityIsNull_ShouldThrowException()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.UpdateEntityAsync(It.IsAny<WorkshopES>()))
//               .ReturnsAsync(Result.Updated);

//            // Act and Assert
//            Assert.ThrowsAsync<ArgumentNullException>(async () => await service.Update(null));
//            mockEsProvider.Verify(x => x.UpdateEntityAsync(It.IsAny<WorkshopES>()), Times.Never);
//        }

//        [Test]
//        public async Task Update_WhenDocWasUpdated_ShouldReturnTrue()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.UpdateEntityAsync(It.IsAny<WorkshopES>()))
//               .ReturnsAsync(Result.Updated);
//            mockRatingService.Setup(x => x.GetAverageRating(It.IsAny<long>(), RatingType.Workshop)).Returns(new Tuple<float, int>(3.5f, 4));

//            // Act
//            var result = await service.Update(new WorkshopES() { Id = 1, Title = "Title" }).ConfigureAwait(false);

//            // Assert
//            Assert.IsTrue(result);
//            mockEsProvider.Verify(x => x.UpdateEntityAsync(It.IsAny<WorkshopES>()), Times.Once);
//            mockRatingService.Verify(x => x.GetAverageRating(It.IsAny<long>(), RatingType.Workshop), Times.Once);
//        }

//        [Test]
//        public async Task Update_WhenDocWasNotUpdated_ShouldReturnFalse()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.UpdateEntityAsync(It.IsAny<WorkshopES>()))
//               .ReturnsAsync(Result.Error);
//            mockRatingService.Setup(x => x.GetAverageRating(It.IsAny<long>(), RatingType.Workshop)).Returns(new Tuple<float, int>(3.5f, 4));

//            // Act
//            var result = await service.Update(new WorkshopES() { Id = 1, Title = "Title" }).ConfigureAwait(false);

//            // Assert
//            Assert.IsFalse(result);
//            mockEsProvider.Verify(x => x.UpdateEntityAsync(It.IsAny<WorkshopES>()), Times.Once);
//            mockRatingService.Verify(x => x.GetAverageRating(It.IsAny<long>(), RatingType.Workshop), Times.Once);
//        }

//        [Test]
//        public async Task Update_WhenElasticsearchExceptionOccures_ShouldReturnFalse()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.UpdateEntityAsync(It.IsAny<WorkshopES>()))
//               .Throws(new ElasticsearchClientException("exception"));
//            mockRatingService.Setup(x => x.GetAverageRating(It.IsAny<long>(), RatingType.Workshop)).Returns(new Tuple<float, int>(3.5f, 4));

//            // Act
//            var result = await service.Update(new WorkshopES() { Id = 1, Title = "Title" }).ConfigureAwait(false);

//            // Assert
//            Assert.IsFalse(result);
//            mockEsProvider.Verify(x => x.UpdateEntityAsync(It.IsAny<WorkshopES>()), Times.Once);
//            mockRatingService.Verify(x => x.GetAverageRating(It.IsAny<long>(), RatingType.Workshop), Times.Once);
//        }
//        #endregion

//        #region Delete
//        [Test]
//        public async Task Delete_WhenDocWasDeleted_ShouldReturnTrue()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.DeleteEntityAsync(It.IsAny<WorkshopES>()))
//               .ReturnsAsync(Result.Deleted);

//            // Act
//            var result = await service.Delete(1).ConfigureAwait(false);

//            // Assert
//            Assert.IsTrue(result);
//            mockEsProvider.Verify(x => x.DeleteEntityAsync(It.IsAny<WorkshopES>()), Times.Once);
//        }

//        [Test]
//        public async Task Delete_WhenDocWasNotDeleted_ShouldReturnFalse()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.DeleteEntityAsync(It.IsAny<WorkshopES>()))
//               .ReturnsAsync(Result.Error);

//            // Act
//            var result = await service.Delete(1).ConfigureAwait(false);

//            // Assert
//            Assert.IsFalse(result);
//            mockEsProvider.Verify(x => x.DeleteEntityAsync(It.IsAny<WorkshopES>()), Times.Once);
//        }

//        [Test]
//        public async Task Delete_WhenElasticsearchExceptionOccures_ShouldReturnFalse()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.DeleteEntityAsync(It.IsAny<WorkshopES>()))
//               .Throws(new ElasticsearchClientException("exception"));

//            // Act
//            var result = await service.Delete(1).ConfigureAwait(false);

//            // Assert
//            Assert.IsFalse(result);
//            mockEsProvider.Verify(x => x.DeleteEntityAsync(It.IsAny<WorkshopES>()), Times.Once);
//        }
//        #endregion

//        #region ReIndex
//        [Test]
//        public async Task ReIndex_WhenDocsWereUpdated_ShouldReturnTrue()
//        {
//            // Arrange
//            var listDto = new List<WorkshopDTO>()
//            {
//                new WorkshopDTO()
//                    {
//                        Id = 1,
//                        Title = "Шаффл",
//                        Phone = "1111111111",
//                        Description = "Танці",
//                        Keywords = new List<string> { "шаффл" },
//                        Price = 0,
//                        WithDisabilityOptions = true,
//                        Head = "Head1",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "Школа танцю",
//                        DisabilityOptionsDesc = "пандус",
//                        Website = "website1",
//                        Instagram = "insta1",
//                        Facebook = "facebook1",
//                        Email = "email1@gmail.com",
//                        MaxAge = 6,
//                        MinAge = 3,
//                        Logo = "image1",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        ClassId = 1,
//                        DepartmentId = 1,
//                        AddressId = 55,
//                        Address = new AddressDto
//                        {
//                            Id = 55,
//                            Region = "Київ",
//                            District = "Київ55",
//                            City = "Київ",
//                            Street = "Street55",
//                            BuildingNumber = "BuildingNumber55",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = new List<TeacherDTO>
//                        {
//                            new TeacherDTO
//                            {
//                                Id = 1,
//                                FirstName = "Alex",
//                                LastName = "Brown",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 1,
//                            },
//                            new TeacherDTO
//                            {
//                                Id = 2,
//                                FirstName = "John",
//                                LastName = "Snow",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 1,
//                            },
//                        },
//                    },
//                new WorkshopDTO()
//                    {
//                        Id = 2,
//                        Title = "Шаффл для дорослих",
//                        Phone = "1111111111",
//                        Description = "Танці",
//                        Keywords = new List<string> { "танці", "діти", "шаффл" },
//                        Price = 50,
//                        WithDisabilityOptions = false,
//                        Head = "Head2",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "Школа танцю",
//                        DisabilityOptionsDesc = "Desc2",
//                        Website = "website2",
//                        Instagram = "insta2",
//                        Facebook = "facebook2",
//                        Email = "email2@gmail.com",
//                        MaxAge = 10,
//                        MinAge = 7,
//                        Logo = "image2",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        DepartmentId = 1,
//                        AddressId = 10,
//                        Address = new AddressDto
//                        {
//                            Id = 10,
//                            Region = "Region10",
//                            District = "District10",
//                            City = "Чернігів",
//                            Street = "Street10",
//                            BuildingNumber = "BuildingNumber10",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = new List<TeacherDTO>
//                        {
//                            new TeacherDTO
//                            {
//                                Id = 3,
//                                FirstName = "Alex",
//                                LastName = "Brown",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 2,
//                            },
//                            new TeacherDTO
//                            {
//                                Id = 4,
//                                FirstName = "John",
//                                LastName = "Snow",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 2,
//                            },
//                        },
//                    },
//            };
//            mockWorkshopService.SetupSequence(x => x.GetAll(It.IsAny<OffsetFilter>()))
//                .ReturnsAsync(new SearchResult<WorkshopDTO> { TotalAmount = listDto.Count, Entities = listDto })
//                .ReturnsAsync(new SearchResult<WorkshopDTO> { TotalAmount = listDto.Count, Entities = new List<WorkshopDTO>() });
//            mockEsProvider.Setup(x => x.ReIndexAll(It.IsAny<IEnumerable<WorkshopES>>()))
//               .ReturnsAsync(Result.Updated);
//            mockRatingService.Setup(x => x.GetAverageRating(It.IsAny<long>(), RatingType.Workshop)).Returns(new Tuple<float, int>(3.5f, 4));

//            // Act
//            var result = await service.ReIndex().ConfigureAwait(false);

//            // Assert
//            Assert.IsTrue(result);
//            mockEsProvider.Verify(x => x.ReIndexAll(It.IsAny<IEnumerable<WorkshopES>>()), Times.Once);
//            mockRatingService.Verify(x => x.GetAverageRating(It.IsAny<long>(), RatingType.Workshop), Times.AtLeastOnce);
//        }

//        [Test]
//        public async Task ReIndex_WhenDocsWereNotUpdated_ShouldReturnFalse()
//        {
//            // Arrange
//            var listDto = new List<WorkshopDTO>()
//            {
//                new WorkshopDTO()
//                    {
//                        Id = 1,
//                        Title = "Шаффл",
//                        Phone = "1111111111",
//                        Description = "Танці",
//                        Keywords = new List<string> { "шаффл" },
//                        Price = 0,
//                        WithDisabilityOptions = true,
//                        Head = "Head1",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "Школа танцю",
//                        DisabilityOptionsDesc = "пандус",
//                        Website = "website1",
//                        Instagram = "insta1",
//                        Facebook = "facebook1",
//                        Email = "email1@gmail.com",
//                        MaxAge = 6,
//                        MinAge = 3,
//                        Logo = "image1",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        ClassId = 1,
//                        DepartmentId = 1,
//                        AddressId = 55,
//                        Address = new AddressDto
//                        {
//                            Id = 55,
//                            Region = "Київ",
//                            District = "Київ55",
//                            City = "Київ",
//                            Street = "Street55",
//                            BuildingNumber = "BuildingNumber55",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = new List<TeacherDTO>
//                        {
//                            new TeacherDTO
//                            {
//                                Id = 1,
//                                FirstName = "Alex",
//                                LastName = "Brown",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 1,
//                            },
//                            new TeacherDTO
//                            {
//                                Id = 2,
//                                FirstName = "John",
//                                LastName = "Snow",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 1,
//                            },
//                        },
//                    },
//                new WorkshopDTO()
//                    {
//                        Id = 2,
//                        Title = "Шаффл для дорослих",
//                        Phone = "1111111111",
//                        Description = "Танці",
//                        Keywords = new List<string> { "танці", "діти", "шаффл" },
//                        Price = 50,
//                        WithDisabilityOptions = false,
//                        Head = "Head2",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "Школа танцю",
//                        DisabilityOptionsDesc = "Desc2",
//                        Website = "website2",
//                        Instagram = "insta2",
//                        Facebook = "facebook2",
//                        Email = "email2@gmail.com",
//                        MaxAge = 10,
//                        MinAge = 7,
//                        Logo = "image2",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        DepartmentId = 1,
//                        AddressId = 10,
//                        Address = new AddressDto
//                        {
//                            Id = 10,
//                            Region = "Region10",
//                            District = "District10",
//                            City = "Чернігів",
//                            Street = "Street10",
//                            BuildingNumber = "BuildingNumber10",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = new List<TeacherDTO>
//                        {
//                            new TeacherDTO
//                            {
//                                Id = 3,
//                                FirstName = "Alex",
//                                LastName = "Brown",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 2,
//                            },
//                            new TeacherDTO
//                            {
//                                Id = 4,
//                                FirstName = "John",
//                                LastName = "Snow",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 2,
//                            },
//                        },
//                    },
//            };
//            mockWorkshopService.SetupSequence(x => x.GetAll(It.IsAny<OffsetFilter>()))
//                .ReturnsAsync(new SearchResult<WorkshopDTO> { TotalAmount = listDto.Count, Entities = listDto })
//                .ReturnsAsync(new SearchResult<WorkshopDTO> { TotalAmount = listDto.Count, Entities = new List<WorkshopDTO>() });
//            mockEsProvider.Setup(x => x.ReIndexAll(It.IsAny<IEnumerable<WorkshopES>>()))
//               .ReturnsAsync(Result.Error);
//            mockRatingService.Setup(x => x.GetAverageRating(It.IsAny<long>(), RatingType.Workshop)).Returns(new Tuple<float, int>(3.5f, 4));

//            // Act
//            var result = await service.ReIndex().ConfigureAwait(false);

//            // Assert
//            Assert.IsFalse(result);
//            mockEsProvider.Verify(x => x.ReIndexAll(It.IsAny<IEnumerable<WorkshopES>>()), Times.Once);
//            mockRatingService.Verify(x => x.GetAverageRating(It.IsAny<long>(), RatingType.Workshop), Times.AtLeastOnce);
//        }

//        [Test]
//        public async Task ReIndex_WhenAnyExceptionOccures_ShouldReturnFalse()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.ReIndexAll(It.IsAny<IEnumerable<WorkshopES>>()))
//               .Throws(new Exception("exception"));

//            // Act
//            var result = await service.ReIndex().ConfigureAwait(false);

//            // Assert
//            Assert.IsFalse(result);
//        }
//        #endregion

//        #region Search
//        [Test]
//        public async Task Search_WhenCalled_ShouldReturnResult()
//        {
//            // Arrange
//            var listES = new List<WorkshopES>()
//            {
//                new WorkshopES()
//                    {
//                        Id = 1,
//                        Title = "Шаффл",
//                        Description = "Танці",
//                        Keywords = "шаффл",
//                        Price = 0,
//                        WithDisabilityOptions = true,
//                        ProviderTitle = "Школа танцю",
//                        MaxAge = 6,
//                        MinAge = 3,
//                        Logo = "image1",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        ClassId = 1,
//                        DepartmentId = 1,
//                        AddressId = 55,
//                        Address = new AddressES
//                        {
//                            Id = 55,
//                            Region = "Київ",
//                            District = "Київ55",
//                            City = "Київ",
//                            Street = "Street55",
//                            BuildingNumber = "BuildingNumber55",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = new List<TeacherES>
//                        {
//                            new TeacherES
//                            {
//                                Id = 1,
//                                FirstName = "Alex",
//                                LastName = "Brown",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 1,
//                            },
//                            new TeacherES
//                            {
//                                Id = 2,
//                                FirstName = "John",
//                                LastName = "Snow",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 1,
//                            },
//                        },
//                    },
//                new WorkshopES()
//                    {
//                        Id = 2,
//                        Title = "Шаффл для дорослих",
//                        Description = "Танці",
//                        Keywords = "танці",
//                        Price = 50,
//                        WithDisabilityOptions = false,
//                        ProviderTitle = "Школа танцю",
//                        MaxAge = 10,
//                        MinAge = 7,
//                        Logo = "image2",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        DepartmentId = 1,
//                        AddressId = 10,
//                        Address = new AddressES
//                        {
//                            Id = 10,
//                            Region = "Region10",
//                            District = "District10",
//                            City = "Чернігів",
//                            Street = "Street10",
//                            BuildingNumber = "BuildingNumber10",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = new List<TeacherES>
//                        {
//                            new TeacherES
//                            {
//                                Id = 3,
//                                FirstName = "Alex",
//                                LastName = "Brown",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 2,
//                            },
//                            new TeacherES
//                            {
//                                Id = 4,
//                                FirstName = "John",
//                                LastName = "Snow",
//                                MiddleName = "SomeMiddleName",
//                                Description = "Description",
//                                Image = "Image",
//                                DateOfBirth = DateTime.Parse("2000-01-01"),
//                                WorkshopId = 2,
//                            },
//                        },
//                    },
//            };
//            mockEsProvider.Setup(x => x.Search(It.IsAny<WorkshopFilterES>()))
//               .ReturnsAsync(new SearchResultES<WorkshopES>() { TotalAmount = listES.Count, Entities = listES });

//            // Act
//            var result = await service.Search(null).ConfigureAwait(false);

//            // Assert
//            Assert.IsInstanceOf<SearchResultES<WorkshopES>>(result);
//            Assert.AreEqual(2, result.TotalAmount);
//            mockEsProvider.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Once);
//        }

//        [Test]
//        public async Task Search_WhenErrorOccured_ShouldReturnEmptyResult()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.Search(It.IsAny<WorkshopFilterES>()))
//               .Throws(new Exception("exception"));

//            // Act
//            var result = await service.Search(null).ConfigureAwait(false);

//            // Assert
//            Assert.IsInstanceOf<SearchResultES<WorkshopES>>(result);
//            Assert.AreEqual(0, result.TotalAmount);
//            mockEsProvider.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Once);
//        }
//        #endregion

//        #region PingServer
//        [Test]
//        public async Task PingServer_WhenCalled_ShouldReturnResult()
//        {
//            // Arrange
//            mockEsProvider.Setup(x => x.PingServerAsync())
//               .ReturnsAsync(true);

//            // Act
//            var result = await service.PingServer().ConfigureAwait(false);

//            // Assert
//            Assert.IsTrue(result);
//            mockEsProvider.Verify(x => x.PingServerAsync(), Times.Once);
//        }
//        #endregion
//    }
//}
