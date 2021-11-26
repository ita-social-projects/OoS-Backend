//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;
//using OutOfSchool.ElasticsearchData.Models;
//using OutOfSchool.WebApi.Extensions;
//using OutOfSchool.WebApi.Models;
//using OutOfSchool.WebApi.Services;

//namespace OutOfSchool.WebApi.Tests.Services
//{
//    [TestFixture]
//    public class WorkshopServicesCombinerTests
//    {
//        private static WorkshopDTO workshop;
//        private static WorkshopES workshopES;
//        private static ProviderDto provider;
//        private static List<WorkshopDTO> workshops;
//        private static List<WorkshopES> workshopESs;

//        private Mock<IWorkshopService> mockDatabaseService;
//        private Mock<IElasticsearchService<WorkshopES, WorkshopFilterES>> mockElasticsearchService;
//        private Mock<ILogger<WorkshopServicesCombiner>> mockLogger;

//        private IWorkshopServicesCombiner service;

//        [OneTimeSetUp]
//        public void OneTimeSetup()
//        {
//            workshop = FakeWorkshop();
//            workshopES = workshop.ToESModel();
//            provider = FakeProvider();
//            workshops = FakeWorkshops();
//            workshopESs = FakeWorkshopESs();
//        }

//        [SetUp]
//        public void SetUp()
//        {
//            mockDatabaseService = new Mock<IWorkshopService>();
//            mockElasticsearchService = new Mock<IElasticsearchService<WorkshopES, WorkshopFilterES>>();
//            mockLogger = new Mock<ILogger<WorkshopServicesCombiner>>();

//            service = new WorkshopServicesCombiner(mockDatabaseService.Object, mockElasticsearchService.Object, mockLogger.Object);
//        }

//        [Test]
//        public async Task Create_WhenCalled_ShouldCallInnerServices()
//        {
//            // Arrange
//            mockDatabaseService.Setup(x => x.Create(workshop)).ReturnsAsync(workshop);
//            mockElasticsearchService.Setup(x => x.Index(workshopES)).ReturnsAsync(true);

//            // Act
//            var result = await service.Create(workshop).ConfigureAwait(false);

//            // Assert
//            Assert.IsInstanceOf<WorkshopDTO>(result);
//            Assert.AreEqual(workshop.Title, result.Title);

//            mockDatabaseService.Verify(x => x.Create(workshop), Times.Once);
//            mockElasticsearchService.Verify(x => x.Index(It.IsAny<WorkshopES>()), Times.Once);
//        }

//        [Test]
//        public async Task Update_WhenCalled_ShouldCallInnerServices()
//        {
//            // Arrange
//            mockDatabaseService.Setup(x => x.Update(workshop)).ReturnsAsync(workshop);
//            mockElasticsearchService.Setup(x => x.Update(workshopES)).ReturnsAsync(true);

//            // Act
//            var result = await service.Update(workshop).ConfigureAwait(false);

//            // Assert
//            Assert.IsInstanceOf<WorkshopDTO>(result);
//            Assert.AreEqual(workshop.Title, result.Title);

//            mockDatabaseService.Verify(x => x.Update(workshop), Times.Once);
//            mockElasticsearchService.Verify(x => x.Update(It.IsAny<WorkshopES>()), Times.Once);
//        }

//        [Test]
//        [TestCase(1)]
//        public async Task Delete_WhenCalled_ShouldCallInnerServices(long id)
//        {
//            // Arrange
//            mockDatabaseService.Setup(x => x.Delete(id));
//            mockElasticsearchService.Setup(x => x.Delete(id)).ReturnsAsync(true);

//            // Act
//            await service.Delete(id).ConfigureAwait(false);

//            // Assert
//            mockDatabaseService.Verify(x => x.Delete(id), Times.Once);
//            mockElasticsearchService.Verify(x => x.Delete(id), Times.Once);
//        }

//        [Test]
//        [TestCase(1)]
//        public async Task GetById_WhenCalled_ShouldCallInnerServices(long id)
//        {
//            // Arrange
//            mockDatabaseService.Setup(x => x.GetById(id)).ReturnsAsync(workshop);
//            mockElasticsearchService.Setup(x => x.Search(It.IsAny<WorkshopFilterES>())).ReturnsAsync(It.IsAny<SearchResultES<WorkshopES>>());

//            // Act
//            var result = await service.GetById(id).ConfigureAwait(false);

//            // Assert
//            Assert.IsInstanceOf<WorkshopDTO>(result);
//            Assert.AreEqual(workshop.Title, result.Title);

//            mockDatabaseService.Verify(x => x.GetById(id), Times.Once);
//            mockElasticsearchService.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Never);
//        }

//        [Test]
//        [TestCase(1)]
//        public async Task GetByProviderId_WhenCalled_ShouldCallInnerServices(long id)
//        {
//            // Arrange
//            mockDatabaseService.Setup(x => x.GetByProviderId(id)).ReturnsAsync(workshops);
//            mockElasticsearchService.Setup(x => x.Search(It.IsAny<WorkshopFilterES>())).ReturnsAsync(It.IsAny<SearchResultES<WorkshopES>>());

//            // Act
//            var result = await service.GetByProviderId(id).ConfigureAwait(false);

//            // Assert
//            Assert.IsInstanceOf<IEnumerable<WorkshopCard>>(result);

//            mockDatabaseService.Verify(x => x.GetByProviderId(id), Times.Once);
//            mockElasticsearchService.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Never);
//        }

//        [Test]
//        public async Task GetAll_WhenElasticsearchIsAvailiable_ShouldReturnElasticsearchResult()
//        {
//            // Arrange
//            var databaseResult = new SearchResult<WorkshopDTO>() { TotalAmount = workshops.Count, Entities = workshops };
//            var elasticResult = new SearchResultES<WorkshopES>() { TotalAmount = workshopESs.Count, Entities = workshopESs };
//            mockDatabaseService.Setup(x => x.GetByFilter(It.IsAny<WorkshopFilter>())).ReturnsAsync(databaseResult);
//            mockElasticsearchService.Setup(x => x.Search(It.IsAny<WorkshopFilterES>())).ReturnsAsync(elasticResult);
//            mockElasticsearchService.Setup(x => x.PingServer()).ReturnsAsync(true);

//            // Act
//            var result = await service.GetAll(new OffsetFilter()).ConfigureAwait(false);

//            // Assert
//            Assert.IsInstanceOf<SearchResult<WorkshopCard>>(result);
//            Assert.AreEqual(elasticResult.TotalAmount, result.TotalAmount);

//            mockDatabaseService.Verify(x => x.GetByFilter(It.IsAny<WorkshopFilter>()), Times.Never);
//            mockElasticsearchService.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Once);
//            mockElasticsearchService.Verify(x => x.PingServer(), Times.Never);
//        }

//        [Test]
//        public async Task GetAll_WhenElasticsearchIsUnavailiable_ShouldReturnDatabaseResult()
//        {
//            // Arrange
//            var databaseResult = new SearchResult<WorkshopDTO>() { TotalAmount = workshops.Count, Entities = workshops };
//            var elasticResult = new SearchResultES<WorkshopES>() { TotalAmount = 0, Entities = new List<WorkshopES>() };
//            mockDatabaseService.Setup(x => x.GetByFilter(It.IsAny<WorkshopFilter>())).ReturnsAsync(databaseResult);
//            mockElasticsearchService.Setup(x => x.Search(It.IsAny<WorkshopFilterES>())).ReturnsAsync(elasticResult);
//            mockElasticsearchService.Setup(x => x.PingServer()).ReturnsAsync(false);

//            // Act
//            var result = await service.GetAll(new OffsetFilter()).ConfigureAwait(false);

//            // Assert
//            Assert.IsInstanceOf<SearchResult<WorkshopCard>>(result);
//            Assert.AreEqual(databaseResult.TotalAmount, result.TotalAmount);

//            mockDatabaseService.Verify(x => x.GetByFilter(It.IsAny<WorkshopFilter>()), Times.Once);
//            mockElasticsearchService.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Once);
//            mockElasticsearchService.Verify(x => x.PingServer(), Times.Once);
//        }

//        [Test]
//        public async Task GetByFilter_WhenElasticsearchIsAvailiable_ShouldReturnElasticsearchResult()
//        {
//            // Arrange
//            var databaseResult = new SearchResult<WorkshopDTO>() { TotalAmount = workshops.Count, Entities = workshops };
//            var elasticResult = new SearchResultES<WorkshopES>() { TotalAmount = workshopESs.Count, Entities = workshopESs };
//            mockDatabaseService.Setup(x => x.GetByFilter(It.IsAny<WorkshopFilter>())).ReturnsAsync(databaseResult);
//            mockElasticsearchService.Setup(x => x.Search(It.IsAny<WorkshopFilterES>())).ReturnsAsync(elasticResult);
//            mockElasticsearchService.Setup(x => x.PingServer()).ReturnsAsync(true);

//            // Act
//            var result = await service.GetByFilter(new WorkshopFilter()).ConfigureAwait(false);

//            // Assert
//            Assert.IsInstanceOf<SearchResult<WorkshopCard>>(result);
//            Assert.AreEqual(elasticResult.TotalAmount, result.TotalAmount);

//            mockDatabaseService.Verify(x => x.GetByFilter(It.IsAny<WorkshopFilter>()), Times.Never);
//            mockElasticsearchService.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Once);
//            mockElasticsearchService.Verify(x => x.PingServer(), Times.Never);
//        }

//        [Test]
//        public async Task GetByFilter_WhenElasticsearchIsUnavailiable_ShouldReturnDatabaseResult()
//        {
//            // Arrange
//            var databaseResult = new SearchResult<WorkshopDTO>() { TotalAmount = workshops.Count, Entities = workshops };
//            var elasticResult = new SearchResultES<WorkshopES>() { TotalAmount = 0, Entities = new List<WorkshopES>() };
//            mockDatabaseService.Setup(x => x.GetByFilter(It.IsAny<WorkshopFilter>())).ReturnsAsync(databaseResult);
//            mockElasticsearchService.Setup(x => x.Search(It.IsAny<WorkshopFilterES>())).ReturnsAsync(elasticResult);
//            mockElasticsearchService.Setup(x => x.PingServer()).ReturnsAsync(false);

//            // Act
//            var result = await service.GetByFilter(new WorkshopFilter()).ConfigureAwait(false);

//            // Assert
//            Assert.IsInstanceOf<SearchResult<WorkshopCard>>(result);
//            Assert.AreEqual(databaseResult.TotalAmount, result.TotalAmount);

//            mockDatabaseService.Verify(x => x.GetByFilter(It.IsAny<WorkshopFilter>()), Times.Once);
//            mockElasticsearchService.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Once);
//            mockElasticsearchService.Verify(x => x.PingServer(), Times.Once);
//        }

//        private WorkshopDTO FakeWorkshop()
//        {
//            return new WorkshopDTO()
//            {
//                Id = 1,
//                Title = "Title",
//                Phone = "1111111111",
//                Description = "Desc6",
//                Price = 6000,
//                WithDisabilityOptions = true,
//                Head = "Head6",
//                HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                ProviderTitle = "ProviderTitle",
//                DisabilityOptionsDesc = "Desc6",
//                Website = "website6",
//                Instagram = "insta6",
//                Facebook = "facebook6",
//                Email = "email6@gmail.com",
//                MaxAge = 10,
//                MinAge = 4,
//                Logo = "image6",
//                ProviderId = 1,
//                DirectionId = 1,
//                DepartmentId = 1,
//                ClassId = 1,
//                AddressId = 55,
//                Address = new AddressDto
//                {
//                    Id = 55,
//                    Region = "Region55",
//                    District = "District55",
//                    City = "Київ",
//                    Street = "Street55",
//                    BuildingNumber = "BuildingNumber55",
//                    Latitude = 0,
//                    Longitude = 0,
//                },
//                Teachers = new List<TeacherDTO>
//                {
//                    new TeacherDTO
//                    {
//                        Id = 1,
//                        FirstName = "Alex",
//                        LastName = "Brown",
//                        MiddleName = "SomeMiddleName",
//                        Description = "Description",
//                        Image = "Image",
//                        DateOfBirth = DateTime.Parse("2000-01-01"),
//                        WorkshopId = 6,
//                    },
//                    new TeacherDTO
//                    {
//                        Id = 2,
//                        FirstName = "John",
//                        LastName = "Snow",
//                        MiddleName = "SomeMiddleName",
//                        Description = "Description",
//                        Image = "Image",
//                        DateOfBirth = DateTime.Parse("1990-01-01"),
//                        WorkshopId = 6,
//                    },
//                },
//            };
//        }

//        private ProviderDto FakeProvider()
//        {
//            return new ProviderDto()
//            {
//                Id = 1,
//                UserId = "some user Id",
//                FullTitle = "Title",
//                Description = "Description",
//            };
//        }

//        private List<WorkshopDTO> FakeWorkshops()
//        {
//            return new List<WorkshopDTO>()
//            {
//                new WorkshopDTO()
//                {
//                    Id = 1,
//                    Title = "Title1",
//                    Phone = "1111111111",
//                    Description = "Desc1",
//                    Price = 1000,
//                    WithDisabilityOptions = true,
//                    Head = "Head1",
//                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                    ProviderId = 1,
//                    ProviderTitle = "ProviderTitle",
//                    DisabilityOptionsDesc = "Desc1",
//                    Website = "website1",
//                    Instagram = "insta1",
//                    Facebook = "facebook1",
//                    Email = "email1@gmail.com",
//                    MaxAge = 10,
//                    MinAge = 4,
//                    Logo = "image1",
//                    DirectionId = 1,
//                    DepartmentId = 1,
//                    ClassId = 1,
//                    Address = new AddressDto
//                    {
//                        City = "Київ",
//                    },
//                },
//                new WorkshopDTO()
//                {
//                    Id = 2,
//                    Title = "Title2",
//                    Phone = "1111111111",
//                    Description = "Desc2",
//                    Price = 2000,
//                    WithDisabilityOptions = true,
//                    Head = "Head2",
//                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                    ProviderId = 1,
//                    ProviderTitle = "ProviderTitle",
//                    DisabilityOptionsDesc = "Desc2",
//                    Website = "website2",
//                    Instagram = "insta2",
//                    Facebook = "facebook2",
//                    Email = "email2@gmail.com",
//                    MaxAge = 10,
//                    MinAge = 4,
//                    Logo = "image2",
//                    DirectionId = 1,
//                    DepartmentId = 1,
//                    ClassId = 1,
//                    Address = new AddressDto
//                    {
//                        City = "Київ",
//                    },
//                },
//                new WorkshopDTO()
//                {
//                    Id = 3,
//                    Title = "Title3",
//                    Phone = "1111111111",
//                    Description = "Desc3",
//                    Price = 3000,
//                    WithDisabilityOptions = true,
//                    Head = "Head3",
//                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                    ProviderId = 2,
//                    ProviderTitle = "ProviderTitleNew",
//                    DisabilityOptionsDesc = "Desc3",
//                    Website = "website3",
//                    Instagram = "insta3",
//                    Facebook = "facebook3",
//                    Email = "email3@gmail.com",
//                    MaxAge = 10,
//                    MinAge = 4,
//                    Logo = "image3",
//                    DirectionId = 1,
//                    DepartmentId = 1,
//                    ClassId = 1,
//                },
//                new WorkshopDTO()
//                {
//                    Id = 4,
//                    Title = "Title4",
//                    Phone = "1111111111",
//                    Description = "Desc4",
//                    Price = 4000,
//                    WithDisabilityOptions = true,
//                    Head = "Head4",
//                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                    ProviderId = 2,
//                    ProviderTitle = "ProviderTitleNew",
//                    DisabilityOptionsDesc = "Desc4",
//                    Website = "website4",
//                    Instagram = "insta4",
//                    Facebook = "facebook4",
//                    Email = "email4@gmail.com",
//                    MaxAge = 10,
//                    MinAge = 4,
//                    Logo = "image4",
//                    DirectionId = 1,
//                    DepartmentId = 1,
//                    ClassId = 1,
//                },
//                new WorkshopDTO()
//                {
//                    Id = 5,
//                    Title = "Title5",
//                    Phone = "1111111111",
//                    Description = "Desc5",
//                    Price = 5000,
//                    WithDisabilityOptions = true,
//                    Head = "Head5",
//                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                    ProviderId = 2,
//                    ProviderTitle = "ProviderTitleNew",
//                    DisabilityOptionsDesc = "Desc5",
//                    Website = "website5",
//                    Instagram = "insta5",
//                    Facebook = "facebook5",
//                    Email = "email5@gmail.com",
//                    MaxAge = 10,
//                    MinAge = 4,
//                    Logo = "image5",
//                    DirectionId = 1,
//                    DepartmentId = 1,
//                    ClassId = 1,
//                    Address = new AddressDto
//                    {
//                        City = "Київ",
//                    },
//                },
//            };
//        }

//        private List<WorkshopES> FakeWorkshopESs()
//        {
//            var list = FakeWorkshops();
//            var eSlist = new List<WorkshopES>();
//            foreach (var item in list)
//            {
//                eSlist.Add(item.ToESModel());
//            }

//            return eSlist;
//        }
//    }
//}
