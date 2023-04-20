//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Localization;
//using Moq;
//using NUnit.Framework;
//using OutOfSchool.ElasticsearchData.Models;
//using OutOfSchool.Services.Enums;
//using OutOfSchool.WebApi.Controllers.V1;
//using OutOfSchool.WebApi.Models;
//using OutOfSchool.WebApi.Services;

//namespace OutOfSchool.WebApi.Tests.Controllers
//{
//    [TestFixture]
//    public class RatingControllerTests
//    {
//        private const int OkStatusCode = 200;
//        private const int NoContentStatusCode = 204;
//        private const int CreateStatusCode = 201;
//        private const int BadRequestStatusCode = 400;
//        private RatingController controller;
//        private Mock<IRatingService> service;
//        private Mock<IElasticsearchService<WorkshopES, WorkshopFilterES>> eSWorkshopservice;
//        private Mock<IStringLocalizer<SharedResource>> localizer;

//        private IEnumerable<RatingDto> ratings;
//        private RatingDto rating;

//        [SetUp]
//        public void Setup()
//        {
//            service = new Mock<IRatingService>();
//            localizer = new Mock<IStringLocalizer<SharedResource>>();
//            eSWorkshopservice = new Mock<IElasticsearchService<WorkshopES, WorkshopFilterES>>();

//            controller = new RatingController(service.Object, localizer.Object, eSWorkshopservice.Object);

//            ratings = FakeRatings();
//            rating = FakeRating();
//        }

//        [Test]
//        public async Task GetRatings_WhenCalled_ReturnsOkResultObject()
//        {
//            // Arrange
//            service.Setup(x => x.GetAll()).ReturnsAsync(ratings);

//            // Act
//            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.AreEqual(OkStatusCode, result.StatusCode);
//        }

//        [Test]
//        public async Task GetRatings_WhenEmptyCollection_ReturnsNoContentResult()
//        {
//            // Arrange
//            service.Setup(x => x.GetAll()).ReturnsAsync(new List<RatingDto>());

//            // Act
//            var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.AreEqual(NoContentStatusCode, result.StatusCode);
//        }

//        [Test]
//        [TestCase(1)]
//        [TestCase(3)]
//        public async Task GetRatingById_WhenIdIsValid_ReturnOkResultObject(long id)
//        {
//            // Arrange
//            service.Setup(x => x.GetById(id)).ReturnsAsync(ratings.SingleOrDefault(x => x.Id == id));

//            // Act
//            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.AreEqual(OkStatusCode, result.StatusCode);
//        }

//        [Test]
//        [TestCase(0)]
//        [TestCase(-50)]
//        public void GetRatingById_WhenIdIsInvalid_ReturnsArgumentOutOfRangeException(long id)
//        {
//            // Arrange
//            service.Setup(x => x.GetById(id)).ReturnsAsync(ratings.SingleOrDefault(x => x.Id == id));

//            // Act and Assert
//            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
//                async () => await controller.GetById(id).ConfigureAwait(false));
//        }

//        [Test]
//        [TestCase(10)]
//        [TestCase(100)]
//        public async Task GetRatingById_WhenIdIsNotValid_ReturnsEmptyObject(long id)
//        {
//            // Arrange
//            service.Setup(x => x.GetById(id)).ReturnsAsync(ratings.SingleOrDefault(x => x.Id == id));

//            // Act
//            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.That(result.Value, Is.Null);
//            Assert.AreEqual(OkStatusCode, result.StatusCode);
//        }

//        [Test]
//        [TestCase(1, "provider")]
//        public async Task GetByEntityId_WhenDataIsValid_ReturnsOkResultObject(long entityId, string entityType)
//        {
//            // Arrange
//            RatingType type = entityType == "provider" ? RatingType.Provider : RatingType.Workshop;
//            service.Setup(x => x.GetAllByEntityId(entityId, type)).ReturnsAsync(ratings.Where(r => r.EntityId == entityId && r.Type == type));

//            // Act
//            var result = await controller.GetByEntityId(entityType, entityId).ConfigureAwait(false) as OkObjectResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.AreEqual(OkStatusCode, result.StatusCode);
//        }

//        [Test]
//        [TestCase(2, "provider")]
//        public async Task GetByEntityId_WhenEmptyCollection_ReturnsNoContentResult(long entityId, string entityType)
//        {
//            // Arrange
//            RatingType type = entityType == "provider" ? RatingType.Provider : RatingType.Workshop;
//            service.Setup(x => x.GetAllByEntityId(entityId, type)).ReturnsAsync(new List<RatingDto>());

//            // Act
//            var result = await controller.GetByEntityId(entityType, entityId).ConfigureAwait(false) as NoContentResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.AreEqual(NoContentStatusCode, result.StatusCode);
//        }

//        [Test]
//        [TestCase("provider", 1, 1)]
//        public async Task GetParentRating_WhenDataIsValid_ReturnOkResultObject(string entityType, long parentId, long entityId)
//        {
//            // Arrange
//            service.Setup(x => x.GetParentRating(parentId, entityId, RatingType.Provider))
//                .ReturnsAsync(ratings.SingleOrDefault(x => x.ParentId == parentId
//                                                        && x.EntityId == entityId
//                                                        && x.Type == RatingType.Provider));

//            // Act
//            var result = await controller.GetParentRating(entityType, parentId, entityId).ConfigureAwait(false) as OkObjectResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.AreEqual(OkStatusCode, result.StatusCode);
//        }

//        [Test]
//        [TestCase("provider", 10, 10)]
//        [TestCase("provider", 4, 2)]
//        public async Task GetParentRating_WhenDataIsNotValid_ReturnsNoContentResult(string entityType, long parentId, long entityId)
//        {
//            // Arrange
//            service.Setup(x => x.GetParentRating(parentId, entityId, RatingType.Provider))
//                .ReturnsAsync(ratings.SingleOrDefault(x => x.ParentId == parentId
//                                                        && x.EntityId == entityId
//                                                        && x.Type == RatingType.Provider));

//            // Act
//            var result = await controller.GetParentRating(entityType, parentId, entityId).ConfigureAwait(false) as NoContentResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.AreEqual(NoContentStatusCode, result.StatusCode);
//        }

//        [Test]
//        public async Task CreateRating_WhenModelIsValid_ReturnsCreatedAtActionResult()
//        {
//            // Arrange
//            service.Setup(x => x.Create(rating)).ReturnsAsync(rating);

//            // Act
//            var result = await controller.Create(rating).ConfigureAwait(false) as CreatedAtActionResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.AreEqual(CreateStatusCode, result.StatusCode);
//        }

//        [Test]
//        public async Task CreateRating_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
//        {
//            // Arrange
//            controller.ModelState.AddModelError("CreateRating", "Invalid model state.");

//            // Act
//            var result = await controller.Create(rating).ConfigureAwait(false);

//            // Assert
//            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
//            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(BadRequestStatusCode));
//        }

//        [Test]
//        public async Task CreateRating_WhenCalled_CallsElasticsearchServiceToUpdateWorkshopRating()
//        {
//            // Arrange
//            var ratingWorkshop = new RatingDto()
//            {
//                Id = 1,
//                Rate = 5,
//                ParentId = 1,
//                EntityId = 1,
//                Type = RatingType.Workshop,
//            };

//            var fakeWorkshop = new WorkshopES() { Id = 1 };
//            service.Setup(x => x.Create(ratingWorkshop)).ReturnsAsync(rating);
//            eSWorkshopservice.Setup(x => x.Search(It.IsAny<WorkshopFilterES>())).ReturnsAsync(new SearchResultES<WorkshopES>() { Entities = new List<WorkshopES> { fakeWorkshop } });
//            eSWorkshopservice.Setup(x => x.Update(It.IsAny<WorkshopES>())).ReturnsAsync(true);

//            // Act
//            await controller.Create(ratingWorkshop).ConfigureAwait(false);

//            // Assert
//            eSWorkshopservice.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Once);
//            eSWorkshopservice.Verify(x => x.Update(It.IsAny<WorkshopES>()), Times.Once);
//        }

//        [Test]
//        public async Task UpdateRating_WhenModelIsValid_ReturnsOkObjectResult()
//        {
//            // Arrange
//            service.Setup(x => x.Update(rating)).ReturnsAsync(rating);

//            // Act
//            var result = await controller.Update(rating).ConfigureAwait(false) as OkObjectResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.AreEqual(OkStatusCode, result.StatusCode);
//        }

//        [Test]
//        public async Task UpdateRating_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
//        {
//            // Arrange
//            controller.ModelState.AddModelError("UpdateRating", "Invalid model state.");

//            // Act
//            var result = await controller.Update(rating).ConfigureAwait(false);

//            // Assert
//            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
//            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(BadRequestStatusCode));
//        }

//        [Test]
//        public async Task UpdateRating_WhenCalled_CallsElasticsearchServiceToUpdateWorkshopRating()
//        {
//            // Arrange
//            var ratingWorkshop = new RatingDto()
//            {
//                Id = 1,
//                Rate = 5,
//                ParentId = 1,
//                EntityId = 1,
//                Type = RatingType.Workshop,
//            };

//            var fakeWorkshop = new WorkshopES() { Id = 1 };
//            service.Setup(x => x.Update(ratingWorkshop)).ReturnsAsync(rating);
//            eSWorkshopservice.Setup(x => x.Search(It.IsAny<WorkshopFilterES>())).ReturnsAsync(new SearchResultES<WorkshopES>() { Entities = new List<WorkshopES> { fakeWorkshop } });
//            eSWorkshopservice.Setup(x => x.Update(It.IsAny<WorkshopES>())).ReturnsAsync(true);

//            // Act
//            await controller.Update(ratingWorkshop).ConfigureAwait(false);

//            // Assert
//            eSWorkshopservice.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Once);
//            eSWorkshopservice.Verify(x => x.Update(It.IsAny<WorkshopES>()), Times.Once);
//        }

//        [Test]
//        [TestCase(1)]
//        public async Task DeleteRating_WhenIdIsValid_ReturnsNoContentResult(long id)
//        {
//            // Arrange
//            service.Setup(x => x.Delete(id));

//            // Act
//            var result = await controller.Delete(id) as NoContentResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.AreEqual(NoContentStatusCode, result.StatusCode);
//        }

//        [Test]
//        [TestCase(0)]
//        [TestCase(-50)]
//        public void DeleteRating_WhenIdIsInvalid_ReturnsArgumentOutOfRangeException(long id)
//        {
//            // Arrange
//            service.Setup(x => x.Delete(id));

//            // Act and Assert
//            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
//                async () => await controller.Delete(id).ConfigureAwait(false));
//        }

//        [Test]
//        [TestCase(10)]
//        public async Task DeleteRating_WhenIdIsInvalid_ReturnsNull(long id)
//        {
//            // Arrange
//            service.Setup(x => x.Delete(id));

//            // Act
//            var result = await controller.Delete(id) as OkObjectResult;

//            // Assert
//            Assert.That(result, Is.Null);
//        }

//        [Test]
//        [TestCase(1)]
//        public async Task DeleteRating_WhenCalled_CallsElasticsearchServiceToUpdateWorkshopRating(long id)
//        {
//            // Arrange
//            var ratingWorkshop = new RatingDto()
//            {
//                Id = 1,
//                Rate = 5,
//                ParentId = 1,
//                EntityId = 1,
//                Type = RatingType.Workshop,
//            };

//            var fakeWorkshop = new WorkshopES() { Id = 1 };

//            service.Setup(x => x.GetById(id)).ReturnsAsync(ratingWorkshop);
//            service.Setup(x => x.Delete(id));

//            eSWorkshopservice.Setup(x => x.Search(It.IsAny<WorkshopFilterES>())).ReturnsAsync(new SearchResultES<WorkshopES>() { Entities = new List<WorkshopES> { fakeWorkshop } });
//            eSWorkshopservice.Setup(x => x.Update(It.IsAny<WorkshopES>())).ReturnsAsync(true);

//            // Act
//            await controller.Delete(id).ConfigureAwait(false);

//            // Assert
//            eSWorkshopservice.Verify(x => x.Search(It.IsAny<WorkshopFilterES>()), Times.Once);
//            eSWorkshopservice.Verify(x => x.Update(It.IsAny<WorkshopES>()), Times.Once);
//            service.Verify(x => x.Delete(id), Times.Once);
//        }

//        private RatingDto FakeRating()
//        {
//            return new RatingDto()
//            {
//                Id = 1,
//                Rate = 5,
//                ParentId = 1,
//                EntityId = 1,
//                Type = RatingType.Provider,
//            };
//        }

//        private IEnumerable<RatingDto> FakeRatings()
//        {
//            return new List<RatingDto>()
//            {
//                new RatingDto()
//                {
//                Id = 1,
//                Rate = 5,
//                ParentId = 1,
//                EntityId = 1,
//                Type = RatingType.Provider,
//                },
//                new RatingDto()
//                {
//                Id = 2,
//                Rate = 4,
//                ParentId = 2,
//                EntityId = 2,
//                Type = RatingType.Provider,
//                },
//                new RatingDto()
//                {
//                Id = 3,
//                Rate = 3,
//                ParentId = 3,
//                EntityId = 3,
//                Type = RatingType.Provider,
//                },
//                new RatingDto()
//                {
//                Id = 4,
//                Rate = 2,
//                ParentId = 3,
//                EntityId = 1,
//                Type = RatingType.Workshop,
//                },
//            };
//        }
//    }
//}
