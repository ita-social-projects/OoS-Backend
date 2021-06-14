using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class WorkshopControllerTests
    {
        private WorkshopController controller;
        private Mock<IWorkshopService> workshopServiceMoq;
        private Mock<IProviderService> providerServiceMoq;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<WorkshopDTO> workshops;
        private WorkshopDTO workshop;
        private ProviderDto provider;

        private string userId;
        private Mock<HttpContext> httpContextMoq;

        [SetUp]
        public void Setup()
        {
            workshopServiceMoq = new Mock<IWorkshopService>();
            providerServiceMoq = new Mock<IProviderService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            userId = "someUserId";
            httpContextMoq = new Mock<HttpContext>();
            httpContextMoq.Setup(x => x.User.FindFirst("sub"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userId));
            httpContextMoq.Setup(x => x.User.IsInRole("provider"))
                .Returns(true);

            controller = new WorkshopController(workshopServiceMoq.Object, providerServiceMoq.Object, localizer.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = httpContextMoq.Object;

            workshops = FakeWorkshops();
            workshop = FakeWorkshop();
            provider = FakeProvider();
        }

#pragma warning disable SA1124 // Do not use regions

        #region GetWorkshops
        [Test]
        public async Task GetWorkshops_WhenThereAreWOrkshops_ShouldReturnOkResultObject()
        {
            // Arrange
            workshopServiceMoq.Setup(x => x.GetAll()).ReturnsAsync(workshops);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task GetWorkshops_WhenThereIsNoTAnyWorkshop_ShouldReturnNoConterntResult()
        {
            // Arrange
            var emptyList = new List<WorkshopDTO>();
            workshopServiceMoq.Setup(x => x.GetAll()).ReturnsAsync(emptyList);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }
        #endregion

        #region GetWorkshopById
        [Test]
        [TestCase(1)]
        public async Task GetWorkshopById_WhenIdIsValid_ShouldReturnOkResultObject(long id)
        {
            // Arrange
            workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(workshops.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(0)]
        public void GetWorkshopById_WhenIdIsInvalid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Arrange
            workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(workshops.SingleOrDefault(x => x.Id == id));

            // Assert
            Assert.That(
                async () => await controller.GetById(id),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase(10)]
        public async Task GetWorkshopById_WhenThereIsNoWorkshopWithId_ShouldReturnNoContent(long id)
        {
            // Arrange
            workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(workshops.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }
        #endregion

        #region CreateWorkshop
        [Test]
        public async Task CreateWorkshop_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider);
            workshopServiceMoq.Setup(x => x.Create(workshop)).ReturnsAsync(workshop);

            // Act
            var result = await controller.Create(workshop).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            workshopServiceMoq.Verify(x => x.Create(workshop), Times.Once);
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(201, result.StatusCode);
        }

        [Test]
        public async Task CreateWorkshop_WhenModelIsInvalid_ShouldReturnBadRequestObjectResult()
        {
            // Arrange
            workshopServiceMoq.Setup(x => x.Create(workshop)).ReturnsAsync(workshop);
            controller.ModelState.AddModelError("CreateWorkshop", "Invalid model state.");

            // Act
            var result = await controller.Create(workshop).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            workshopServiceMoq.Verify(x => x.Create(workshop), Times.Never);
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        public async Task CreateWorkshop_WhenProviderHasNoRights_ShouldReturnForbidResult()
        {
            // Arrange
            var notAuthorProvider = new ProviderDto() { Id = 2, UserId = userId };
            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(notAuthorProvider);

            // Act
            var result = await controller.Create(workshop);

            // Assert
            workshopServiceMoq.Verify(x => x.Create(workshop), Times.Never);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ForbidResult>(result);
        }
        #endregion

        #region UpdateWorkshop
        [Test]
        public async Task UpdateWorkshop_WhenModelIsValid_ShouldReturnOkObjectResult()
        {
            // Arrange
            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider);
            workshopServiceMoq.Setup(x => x.Update(workshop)).ReturnsAsync(workshop);

            // Act
            var result = await controller.Update(workshop).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task UpdateWorkshop_WhenModelIsInvalid_ShouldReturnBadRequestObjectResult()
        {
            // Arrange
            workshopServiceMoq.Setup(x => x.Update(workshop)).ReturnsAsync(workshop);
            controller.ModelState.AddModelError("CreateWorkshop", "Invalid model state.");

            // Act
            var result = await controller.Update(workshop).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            workshopServiceMoq.Verify(x => x.Update(It.IsAny<WorkshopDTO>()), Times.Never);
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        public async Task UpdateWorkshop_WhenIdProviderHasNoRights_ShouldReturnForbidResult()
        {
            // Arrange
            var notAuthorProvider = new ProviderDto() { Id = 2, UserId = userId };
            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(notAuthorProvider);

            // Act
            var result = await controller.Update(workshop).ConfigureAwait(false);

            // Assert
            workshopServiceMoq.Verify(x => x.Update(It.IsAny<WorkshopDTO>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ForbidResult>(result);
        }
        #endregion

        #region DeleteWorkshop
        [Test]
        [TestCase(1)]
        public async Task DeleteWorkshop_WhenIdIsValid_ShouldReturnNoContentResult(long id)
        {
            // Arrange
            workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(workshop);
            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider);
            workshopServiceMoq.Setup(x => x.Delete(id)).Returns(Task.CompletedTask);

            // Act
            var result = await controller.Delete(id) as NoContentResult;

            // Assert
            workshopServiceMoq.Verify(x => x.Delete(It.IsAny<long>()), Times.Once);
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(0)]
        public void DeleteWorkshop_WhenIdIsInvalid_ThrowArgumentOutOfRangeException(long id)
        {
            // Assert
            Assert.That(
                async () => await controller.Delete(id),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            workshopServiceMoq.Verify(x => x.Delete(It.IsAny<long>()), Times.Never);
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteWorkshop_WhenThereIsNoWorkshopWithId_ShouldNoContentResult(long id)
        {
            // Arrange
            workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(() => null);
            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider);
            workshopServiceMoq.Setup(x => x.Delete(id)).Returns(Task.CompletedTask);

            // Act
            var result = await controller.Delete(id) as NoContentResult;

            // Assert
            workshopServiceMoq.Verify(x => x.Delete(It.IsAny<long>()), Times.Never);
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteWorkshop_WhenIdProviderHasNoRights_ShouldReturnForbidResult(long id)
        {
            // Arrange
            workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(workshop);
            var notAuthorProvider = new ProviderDto() { Id = 2, UserId = userId };
            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(notAuthorProvider);

            // Act
            var result = await controller.Delete(id);

            // Assert
            workshopServiceMoq.Verify(x => x.Delete(It.IsAny<long>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ForbidResult>(result);
        }
        #endregion

#pragma warning restore SA1124 // Do not use regions

        private WorkshopDTO FakeWorkshop()
        {
            return new WorkshopDTO()
            {
                Id = 6,
                Title = "Title6",
                Phone = "1111111111",
                Description = "Desc6",
                Price = 6000,
                WithDisabilityOptions = true,
                DaysPerWeek = 6,
                Head = "Head6",
                HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                ProviderTitle = "ProviderTitle",
                DisabilityOptionsDesc = "Desc6",
                Website = "website6",
                Instagram = "insta6",
                Facebook = "facebook6",
                Email = "email6@gmail.com",
                MaxAge = 10,
                MinAge = 4,
                Logo = "image6",
                ProviderId = 1,
                CategoryId = 1,
                SubcategoryId = 1,
                AddressId = 55,
                Address = new AddressDto
                {
                    Id = 55,
                    Region = "Region55",
                    District = "District55",
                    City = "City55",
                    Street = "Street55",
                    BuildingNumber = "BuildingNumber55",
                    Latitude = 0,
                    Longitude = 0,
                },
                Teachers = new List<TeacherDTO>
                {
                    new TeacherDTO
                    {
                        Id = 1,
                        FirstName = "Alex",
                        LastName = "Brown",
                        MiddleName = "SomeMiddleName",
                        Description = "Description",
                        Image = "Image",
                        DateOfBirth = DateTime.Parse("2000-01-01"),
                        WorkshopId = 6,
                    },
                    new TeacherDTO
                    {
                        Id = 2,
                        FirstName = "John",
                        LastName = "Snow",
                        MiddleName = "SomeMiddleName",
                        Description = "Description",
                        Image = "Image",
                        DateOfBirth = DateTime.Parse("1990-01-01"),
                        WorkshopId = 6,
                    },
                },
            };
        }

        private IEnumerable<WorkshopDTO> FakeWorkshops()
        {
            return new List<WorkshopDTO>()
            {
                new WorkshopDTO()
                {
                    Id = 1,
                    Title = "Title1",
                    Phone = "1111111111",
                    Description = "Desc1",
                    Price = 1000,
                    WithDisabilityOptions = true,
                    DaysPerWeek = 1,
                    Head = "Head1",
                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                    ProviderTitle = "ProviderTitle",
                    DisabilityOptionsDesc = "Desc1",
                    Website = "website1",
                    Instagram = "insta1",
                    Facebook = "facebook1",
                    Email = "email1@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    Logo = "image1",
                },
                new WorkshopDTO()
                {
                    Id = 2,
                    Title = "Title2",
                    Phone = "1111111111",
                    Description = "Desc2",
                    Price = 2000,
                    WithDisabilityOptions = true,
                    DaysPerWeek = 2,
                    Head = "Head2",
                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                    DisabilityOptionsDesc = "Desc2",
                    Website = "website2",
                    Instagram = "insta2",
                    Facebook = "facebook2",
                    Email = "email2@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    Logo = "image2",
                },
                new WorkshopDTO()
                {
                    Id = 3,
                    Title = "Title3",
                    Phone = "1111111111",
                    Description = "Desc3",
                    Price = 3000,
                    WithDisabilityOptions = true,
                    DaysPerWeek = 3,
                    Head = "Head3",
                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                    ProviderTitle = "ProviderTitle",
                    DisabilityOptionsDesc = "Desc3",
                    Website = "website3",
                    Instagram = "insta3",
                    Facebook = "facebook3",
                    Email = "email3@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    Logo = "image3",
                },
                new WorkshopDTO()
                {
                    Id = 4,
                    Title = "Title4",
                    Phone = "1111111111",
                    Description = "Desc4",
                    Price = 4000,
                    WithDisabilityOptions = true,
                    DaysPerWeek = 4,
                    Head = "Head4",
                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                    ProviderTitle = "ProviderTitle",
                    DisabilityOptionsDesc = "Desc4",
                    Website = "website4",
                    Instagram = "insta4",
                    Facebook = "facebook4",
                    Email = "email4@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    Logo = "image4",
                },
                new WorkshopDTO()
                {
                    Id = 5,
                    Title = "Title5",
                    Phone = "1111111111",
                    Description = "Desc5",
                    Price = 5000,
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
                },
            };
        }

        private ProviderDto FakeProvider()
        {
            return new ProviderDto()
            {
                UserId = userId,
                Id = 1,
            };
        }
    }
}