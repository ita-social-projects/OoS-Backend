using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ApplicationControllerTests
    {
        private ApplicationController controller;
        private Mock<IApplicationService> service;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private IEnumerable<ApplicationDTO> applications;

        [SetUp]
        public void Setup()
        {
            service = new Mock<IApplicationService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            controller = new ApplicationController(service.Object, localizer.Object);

            applications = FakeApplications();
        }

        [Test]
        public async Task GetApplications_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(s => s.GetAll()).ReturnsAsync(applications);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        public async Task GetApplications_WhenCollectionIsEmpty_ShouldReturnNoContent()
        {
            // Arrange
            service.Setup(s => s.GetAll()).ReturnsAsync(new List<ApplicationDTO>());

            // Act
            var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 204);
        }

        [Test]
        [TestCase(1)]
        public async Task GetApplicationById_WhenIdIsValid_ShouldReturnOkResultObject(long id)
        {
            // Arrange
            service.Setup(s => s.GetById(id)).ReturnsAsync(applications.SingleOrDefault(a => a.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        [TestCase(0)]
        public void GetApplicationById_WhenIdIsNotValid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Assert
            Assert.That(
                async () => await controller.GetById(id),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase(10)]
        public async Task GetApplicationById_WhenIdIsNotValid_ShouldReturnNull(long id)
        {
            // Arrange
            service.Setup(s => s.GetById(id)).ReturnsAsync(applications.SingleOrDefault(a => a.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
            Assert.That(result.Value, Is.Null);
        }

        [Test]
        [TestCase("de909VV5-5eb7-4b7a-bda8-40a5bfda96a6")]
        public async Task GetApplicationsByUserId_WhenIdIsValid_ShouldReturnOkObjectResult(string id)
        {
            // Arrange
            service.Setup(s => s.GetAllByUser(id)).ReturnsAsync(applications.Where(a => a.UserId.Equals(id)));

            // Act
            var result = await controller.GetByUserId(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        [TestCase("string")]
        public async Task GetAppicationsByUserId_WhenIdIsNotValid_ShouldReturnBadRequest(string id)
        {
            // Arrange
            service.Setup(s => s.GetAllByUser(id)).ThrowsAsync(new ArgumentException());

            // Act
            var result = await controller.GetByUserId(id).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        [TestCase(1)]
        public async Task GetApplicationsByWorkshopId_WhenIdIsValid_ShouldReturnOkObjectResult(long id)
        {
            // Arrange
            service.Setup(s => s.GetAllByWorkshop(id)).ReturnsAsync(applications.Where(a => a.WorkshopId == id));

            // Act
            var result = await controller.GetByWorkshopId(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        [TestCase(0)]
        public void GetApplicationByWorkshopId_WhenIdIsNotValid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Assert
            Assert.That(
                async () => await controller.GetByWorkshopId(id),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase("10")]
        public async Task GetApplicationByWorkshopId_WhenIdIsNotValid_ShouldReturnBadRequest(long id)
        {
            // Arrange
            service.Setup(s => s.GetAllByWorkshop(id)).ThrowsAsync(new ArgumentException());

            // Act
            var result = await controller.GetByWorkshopId(id).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task CreateApplication_WhenModelIsNotValid_ShoulReturnBadRequest()
        {
            // Arrange
            controller.ModelState.AddModelError("CreateApplication", "Invalid model state.");

            // Act
            var result = await controller.Create(applications.First()).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateApplication_WhenModelIsValid_ShouldReturnOkObjectResult()
        {
            // Arrange
            var application = applications.First();
            service.Setup(s => s.Update(application)).ReturnsAsync(application);

            // Act
            var result = await controller.Update(application).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [Test]
        public async Task UpdateApplication_WhenModelIsNotValid_ShouldReturnBadRequest()
        {
            // Arrange
            controller.ModelState.AddModelError("UpdateApplication", "Invalid model state.");

            // Act
            var result = await controller.Update(applications.First()).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteApplication_WhenIdIsValid_ShouldReturnNoContent(long id)
        {
            // Arrange
            service.Setup(s => s.Delete(id));

            // Act
            var result = await controller.Delete(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, 204);
        }

        [Test]
        [TestCase(0)]
        public void DeleteApplication_WhenIdIsNotValid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Assert
            Assert.That(
                async () => await controller.Delete(id),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteApplication_WhenIdIsNotValid_ShouldReturnNull(long id)
        {
            // Arrange
            service.Setup(s => s.Delete(id));

            // Act
            var result = await controller.Delete(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        private IEnumerable<ApplicationDTO> FakeApplications()
        {
            return new List<ApplicationDTO>()
            {
                new ApplicationDTO()
                {
                    Id = 1,
                    ChildId = 1,
                    Status = ApplicationStatus.Pending,
                    UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6",
                    WorkshopId = 1,
                },
                new ApplicationDTO()
                {
                    Id = 2,
                    ChildId = 1,
                    Status = ApplicationStatus.Pending,
                    UserId = "de909VV5-5eb7-4b7a-bda8-40a5bfda96a6",
                    WorkshopId = 1,
                },
            };
        }
    }
}
