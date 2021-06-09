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
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.ApiModels;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ApplicationControllerTests
    {
        private ApplicationController controller;
        private Mock<IApplicationService> service;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private ClaimsPrincipal user;

        private IEnumerable<ApplicationDto> applications;
        private IEnumerable<ChildDto> children;

        [SetUp]
        public void Setup()
        {
            service = new Mock<IApplicationService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            controller = new ApplicationController(service.Object, localizer.Object);

            user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            applications = FakeApplications();
            children = FakeChildren();
        }

        [Test]
        public async Task GetApplications_WhenCalled_ShouldReturnOkResultObject()
        {
            // Arrange
            service.Setup(s => s.GetAll()).ReturnsAsync(applications);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task GetApplications_WhenCollectionIsEmpty_ShouldReturnNoContent()
        {
            // Arrange
            service.Setup(s => s.GetAll()).ReturnsAsync(new List<ApplicationDto>());

            // Act
            var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
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
            Assert.AreEqual(200, result.StatusCode);
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
            Assert.AreEqual(200, result.StatusCode);
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
            Assert.AreEqual(200, result.StatusCode);
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
            Assert.AreEqual(400, (result as BadRequestObjectResult).StatusCode);
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
            Assert.AreEqual(200, result.StatusCode);
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
            Assert.AreEqual(400, (result as BadRequestObjectResult).StatusCode);
        }

        [Test]
        public async Task CreateApplication_WhenModelIsValid_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var applicationApiModel = new ApplicationApiModel()
            {
                WorkshopId = 1,
                Children = children,
            };

            service.Setup(s => s.Create(It.IsAny<IEnumerable<ApplicationDto>>())).ReturnsAsync(applications);

            // Act
            var result = await controller.Create(applicationApiModel)
                                         .ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(201, result.StatusCode);
        }

        [Test]
        public async Task CreateApplication_WhenModelIsNotValid_ShoulReturnBadRequest()
        {
            // Arrange
            var applicationApiModel = new ApplicationApiModel()
            {
                WorkshopId = 1,
                Children = children,
            };

            controller.ModelState.AddModelError("CreateApplication", "Invalid model state.");

            // Act
            var result = await controller.Create(applicationApiModel).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.AreEqual(400, (result as BadRequestObjectResult).StatusCode);
        }

        [Test]
        public async Task CreateApplication_WhenParametersAreNotValid_ShouldReturnBadRequest()
        {
            // Arrange
            var applicationApiModel = new ApplicationApiModel()
            {
                WorkshopId = 1,
                Children = children,
            };

            service.Setup(s => s.Create(It.IsAny<IEnumerable<ApplicationDto>>())).ThrowsAsync(new ArgumentException());

            // Act
            var result = await controller.Create(applicationApiModel).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.AreEqual(400, (result as BadRequestObjectResult).StatusCode);
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
            Assert.AreEqual(200, result.StatusCode);
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
            Assert.AreEqual(400, (result as BadRequestObjectResult).StatusCode);
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
            Assert.AreEqual(204, result.StatusCode);
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

        private IEnumerable<ApplicationDto> FakeApplications()
        {
            return new List<ApplicationDto>()
            {
                new ApplicationDto()
                {
                    Id = 1,
                    ChildId = 1,
                    Status = ApplicationStatus.Pending,
                    UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6",
                    WorkshopId = 1,
                },
                new ApplicationDto()
                {
                    Id = 2,
                    ChildId = 2,
                    Status = ApplicationStatus.Pending,
                    UserId = "de909VV5-5eb7-4b7a-bda8-40a5bfda96a6",
                    WorkshopId = 1,
                },
            };
        }

        private IEnumerable<ChildDto> FakeChildren()
        {
            return new List<ChildDto>()
            {
                new ChildDto()
                {
                    Id = 1,
                    FirstName = "fn1",
                    LastName = "ln1",
                    MiddleName = "mn1",
                    DateOfBirth = new DateTime(2003, 11, 9),
                    Gender = Gender.Male,
                    ParentId = 1,
                    SocialGroupId = 2,
                },
                new ChildDto()
                {
                    Id = 2,
                    FirstName = "fn2",
                    LastName = "ln2",
                    MiddleName = "mn2",
                    DateOfBirth = new DateTime(2004, 11, 8),
                    Gender = Gender.Female,
                    ParentId = 2,
                    SocialGroupId = 1,
                },
            };
        }
    }
}
