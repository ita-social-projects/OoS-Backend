﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.ApiModels;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ApplicationControllerTests
    {
        private ApplicationController controller;
        private Mock<IApplicationService> applicationService;
        private Mock<IWorkshopService> workshopService;
        private Mock<IProviderService> providerService;
        private Mock<IParentService> parentService;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private string userId;
        private Mock<HttpContext> httpContext;

        private IEnumerable<ApplicationDto> applications;
        private IEnumerable<ChildDto> children;
        private IEnumerable<WorkshopDTO> workshops;
        private ParentDTO parent;
        private ProviderDto provider;

        [SetUp]
        public void Setup()
        {
            applicationService = new Mock<IApplicationService>();
            workshopService = new Mock<IWorkshopService>();
            providerService = new Mock<IProviderService>();
            parentService = new Mock<IParentService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            userId = "User1Id";

            httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.FindFirst("sub"))
                       .Returns(new Claim(ClaimTypes.NameIdentifier, userId));

            controller = new ApplicationController(
                applicationService.Object,
                localizer.Object,
                providerService.Object,
                parentService.Object,
                workshopService.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = httpContext.Object},
            };

            workshops = FakeWorkshops();
            applications = ApplicationDTOsGenerator.Generate(2).WithWorkshopDto(workshops.First());
            children = ChildDtoGenerator.Generate(2).WithSocial(new SocialGroupDto { Id = 1 });

            parent = new ParentDTO { Id = 1, UserId = userId };
            provider = new ProviderDto { Id = 1, UserId = userId };
        }

        [Test]
        public async Task GetApplications_WhenCalled_ShouldReturnOkResultObject()
        {
            // Arrange
            applicationService.Setup(s => s.GetAll()).ReturnsAsync(applications);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task GetApplications_WhenCollectionIsEmpty_ShouldReturnNoContent()
        {
            // Arrange
            applicationService.Setup(s => s.GetAll()).ReturnsAsync(new List<ApplicationDto>());

            // Act
            var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        [Test]
        public async Task GetApplicationById_WhenIdIsValid_ShouldReturnOkObjectResult()
        {
            // Arrange
            var existingApplicationId = applications.First().Id;
            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(parent);

            applicationService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(applications.SingleOrDefault(a => a.Id == existingApplicationId));

            // Act
            var result = await controller.GetById(existingApplicationId).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetApplicationById_WhenThereIsNoApplicationWithId_ShouldReturnNoContent()
        {
            // Arrange
            var noneExistingApplicationId = Guid.NewGuid();
            applicationService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(applications.SingleOrDefault(a => a.Id == noneExistingApplicationId));

            // Act
            var result = await controller.GetById(noneExistingApplicationId).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [TestCase("parent")]
        [TestCase("provider")]
        public async Task GetApplicationById_WhenUserHasNoRights_ShouldReturnBadRequest(string role)
        {
            // Arrange
            var applicationId = applications.First().Id;
            var anotherParent = new ParentDTO { Id = 2, UserId = userId };
            var anotherProvider = new ProviderDto { Id = 2, UserId = userId };

            httpContext.Setup(c => c.User.IsInRole(role)).Returns(true);

            parentService.Setup(s => s.GetByUserId(It.IsAny<string>())).ReturnsAsync(anotherParent);
            providerService.Setup(s => s.GetByUserId(It.IsAny<string>())).ReturnsAsync(anotherProvider);
            applicationService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(applications.SingleOrDefault(a => a.Id == applicationId));

            // Act
            var result = await controller.GetById(applicationId).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        [TestCase(1)]
        public async Task GetByParentId_WhenIdIsValid_ShouldReturnOkObjectResult(long id)
        {
            // Arrange
            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);

            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(parent);
            applicationService.Setup(s => s.GetAllByParent(id)).ReturnsAsync(applications.Where(a => a.ParentId == id));

            // Act
            var result = await controller.GetByParentId(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        [TestCase(0)]
        public async Task GetByParentId_WhenIdIsNotValid_ShouldReturnBadRequest(long id)
        {
            // Act
            var result = await controller.GetByParentId(id).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(10)]
        public async Task GetByParentId_WhenParentHasNoApplications_ShouldReturnNoContent(long id)
        {
            // Arrange
            var newParent = new ParentDTO { Id = 10, UserId = userId };

            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(newParent);
            applicationService.Setup(s => s.GetAllByParent(id)).ReturnsAsync(applications.Where(a => a.ParentId == id));

            // Act
            var result = await controller.GetByParentId(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        [Test]
        [TestCase(1)]
        public async Task GetByParentId_WhenParentHasNoRights_ShouldReturnBadRequest(long id)
        {
            // Arrange
            var anotherParent = new ParentDTO { Id = 2, UserId = userId };

            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(anotherParent);
            applicationService.Setup(s => s.GetAllByParent(id)).ReturnsAsync(applications.Where(a => a.ParentId == id));

            // Act
            var result = await controller.GetByParentId(id).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(1, "provider")]
        [TestCase(1, "workshop")]
        public async Task GetByPropertyId_WhenIdIsValid_ShouldReturnOkObjectResult(long id, string property)
        {
            // Arrange
            var filter = new ApplicationFilter{ Status = 1 };

            httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);

            providerService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(provider);
            workshopService.Setup(s => s.GetById(id)).ReturnsAsync(workshops.First());
            applicationService.Setup(s => s.GetAllByProvider(id, filter))
                .ReturnsAsync(applications.Where(a => a.Workshop.ProviderId == id));
            applicationService.Setup(s => s.GetAllByWorkshop(id, filter))
                .ReturnsAsync(applications.Where(a => a.WorkshopId == id));

            // Act
            var result = await controller.GetByPropertyId(property, id, filter).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        [TestCase(0, "provider")]
        [TestCase(0, "workshop")]
        public async Task GetByPropertyId_WhenIdIsNotValid_ShouldReturnBadRequest(long id, string property)
        {
            // Act
            var filter = new ApplicationFilter { Status = 1 };

            var result = await controller.GetByPropertyId(property, id, filter).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(10, "provider")]
        [TestCase(10, "workshop")]
        public async Task GetByPropertyId_WhenProviderHasNoApplications_ShouldReturnNoContent(long id, string property)
        {
            // Arrange
            var filter = new ApplicationFilter { Status = 1 };

            var newProvider = new ProviderDto { Id = 10, UserId = userId };
            var newWorkshop = new WorkshopDTO { Id = 10, ProviderId = 10 };

            httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);

            providerService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(newProvider);
            workshopService.Setup(s => s.GetById(id)).ReturnsAsync(newWorkshop);
            applicationService.Setup(s => s.GetAllByProvider(id, filter))
                .ReturnsAsync(applications.Where(a => a.Workshop.ProviderId == id));
            applicationService.Setup(s => s.GetAllByWorkshop(id, filter))
                .ReturnsAsync(applications.Where(a => a.WorkshopId == id));

            // Act
            var result = await controller.GetByPropertyId(property, id, filter).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        [Test]
        [TestCase(1, "provider")]
        [TestCase(1, "workshop")]
        public async Task GetByPropertyId_WhenProviderHasNoRights_ShouldReturnNoContent(long id, string property)
        {
            // Arrange
            var filter = new ApplicationFilter { Status = 1 };

            var anotherProvider = new ProviderDto { Id = 2, UserId = userId };

            httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);

            providerService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(anotherProvider);
            workshopService.Setup(s => s.GetById(id)).ReturnsAsync(workshops.First());
            applicationService.Setup(s => s.GetAllByProvider(id, filter))
                .ReturnsAsync(applications.Where(a => a.Workshop.ProviderId == id));
            applicationService.Setup(s => s.GetAllByWorkshop(id, filter))
                .ReturnsAsync(applications.Where(a => a.WorkshopId == id));

            // Act
            var result = await controller.GetByPropertyId(property, id, filter).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(10, "workshop")]
        public async Task GetByPropertyId_WhenThereIsNoWorkshopWithId_ShouldReturnBadRequest(long id, string property)
        {
            // Arrange
            var filter = new ApplicationFilter { Status = 1 };

            workshopService.Setup(s => s.GetById(id)).ReturnsAsync(workshops.Where(w => w.Id == id).FirstOrDefault());

            // Act
            var result = await controller.GetByPropertyId(property, id, filter).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(1)]
        public async Task GetByStatus_WhenStatusIsValid_ShouldReturnOkObjectResult(int status)
        {
            // Arrange
            applicationService.Setup(s => s.GetAllByStatus(status))
                .ReturnsAsync(applications.Where(a => (int)a.Status == status));

            // Act
            var result = await controller.GetByStatus(status).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        [TestCase(10)]
        [TestCase(-1)]
        public async Task GetByStatus_WhenIdIsNotValid_ShouldReturnBadRequest(int status)
        {
            // Act
            var result = await controller.GetByStatus(status).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(0)]
        public async Task GetByStatus_WhenThereIsNoApplicationsWithStatus_ShoulReturnNoContent(int status)
        {
            // Arrange
            applicationService.Setup(s => s.GetAllByStatus(status))
                .ReturnsAsync(applications.Where(a => (int)a.Status == status));

            // Act
            var result = await controller.GetByStatus(status).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        [Test]
        public async Task CreateMultiple_WhenModelIsValid_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var applicationApiModel = new ApplicationApiModel()
            {
                WorkshopId = 1,
                Children = children,
            };

            applicationService.Setup(s => s.Create(It.IsAny<IEnumerable<ApplicationDto>>())).ReturnsAsync(applications);

            // Act
            var result = await controller.Create(applicationApiModel)
                                         .ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
        }

        [Test]
        public async Task CreateMultiple_WhenModelIsNotValid_ShoulReturnBadRequest()
        {
            // Arrange
            var applicationApiModel = new ApplicationApiModel()
            {
                WorkshopId = 1,
                Children = children,
            };

            controller.ModelState.AddModelError("CreateApplication", "Invalid model state.");

            // Act
            var result = await controller.Create(applicationApiModel).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task CreateMultiple_WhenParametersAreNotValid_ShouldReturnBadRequest()
        {
            // Arrange
            var applicationApiModel = new ApplicationApiModel()
            {
                WorkshopId = 1,
                Children = children,
            };

            applicationService.Setup(s => s.Create(It.IsAny<IEnumerable<ApplicationDto>>())).ThrowsAsync(new ArgumentException());

            // Act
            var result = await controller.Create(applicationApiModel).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task CreateApplication_WhenModelIsValid_ShouldReturnCreatedAtAction()
        {
            // Arrange
            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);

            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(parent);
            applicationService.Setup(s => s.Create(applications.First())).ReturnsAsync(applications.First());

            // Act
            var result = await controller.Create(applications.First()).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
        }

        [Test]
        public async Task CreateApplication_WhenModelIsNotValid_ShouldReturnBadRequest()
        {
            // Arrange
            controller.ModelState.AddModelError("CreateApplication", "Invalid model state.");

            // Act
            var result = await controller.Create(applications.First()).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task CreateApplication_WhenModelIsNull_ShouldReturnBadRequest()
        {
            // Arrange
            ApplicationDto application = null;

            // Act
            var result = await controller.Create(application).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task CreateApplication_WhenParentHasNoRights_ShouldReturnBadRequest()
        {
            // Arrange
            var anotherParent = new ParentDTO { Id = 2, UserId = userId };

            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);

            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(anotherParent);
            applicationService.Setup(s => s.Create(applications.First())).ReturnsAsync(applications.First());

            // Act
            var result = await controller.Create(applications.First()).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task CreateApplication_WhenParametersAreNotValid_ShouldReturnBadRequest()
        {
            // Arrange
            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);

            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(parent);
            applicationService.Setup(s => s.Create(applications.First())).ThrowsAsync(new ArgumentException());

            // Act
            var result = await controller.Create(applications.First()).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase("provider")]
        [TestCase("parent")]
        public async Task UpdateApplication_WhenModelIsValid_ShouldReturnOkObjectResult(string role)
        {
            // Arrange
            var shortApplication = new ShortApplicationDto
            {
                Id = applications.First().Id,
                Status = ApplicationStatus.Pending,
            };

            httpContext.Setup(c => c.User.IsInRole(It.IsAny<string>())).Returns(true);

            parentService.Setup(s => s.GetByUserId(It.IsAny<string>())).ReturnsAsync(parent);
            providerService.Setup(s => s.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider);

            applicationService.Setup(s => s.Update(It.IsAny<ApplicationDto>())).ReturnsAsync(applications.First());
            applicationService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(applications.First());

            // Act
            var result = await controller.Update(shortApplication).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateApplication_WhenModelIsNotValid_ShouldReturnBadRequest()
        {
            // Arrange
            var shortApplication = new ShortApplicationDto
            {
                Id = Guid.NewGuid(),
                Status = ApplicationStatus.Pending,
            };

            controller.ModelState.AddModelError("UpdateApplication", "Invalid model state.");

            // Act
            var result = await controller.Update(shortApplication).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [TestCase("parent")]
        [TestCase("provider")]
        public async Task UpdateApplication_WhenUserHasNoRights_ShouldReturnBadRequest(string role)
        {
            // Arrange
            var shortApplication = new ShortApplicationDto
            {
                Id = applications.First().Id,
                Status = ApplicationStatus.Pending,
            };

            var anotherParent = new ParentDTO { Id = 2, UserId = userId };
            var anotherProvider = new ProviderDto { Id = 2, UserId = userId };

            httpContext.Setup(c => c.User.IsInRole(role)).Returns(true);

            providerService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(anotherProvider);
            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(anotherParent);

            applicationService.Setup(s => s.Update(applications.First())).ReturnsAsync(applications.First());
            applicationService.Setup(s => s.GetById(shortApplication.Id)).ReturnsAsync(applications.First());

            // Act
            var result = await controller.Update(shortApplication).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateApplication_WhenThereIsNoApplicationWithId_ShouldReturnBadRequest()
        {
            // Arrange
            var shortApplication = new ShortApplicationDto
            {
                Id = Guid.NewGuid(),
                Status = ApplicationStatus.Pending,
            };

            applicationService.Setup(s => s.GetById(shortApplication.Id))
                .ReturnsAsync(applications.Where(a => a.Id == shortApplication.Id).FirstOrDefault());

            // Act
            var result = await controller.Update(shortApplication).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task DeleteApplication_WhenIdIsValid_ShouldReturnNoContent()
        {
            // Arrange
            var applicationId = applications.First().Id;
            applicationService.Setup(s => s.Delete(applicationId));

            // Act
            var result = await controller.Delete(applicationId).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        private IEnumerable<WorkshopDTO> FakeWorkshops()
        {
            return new List<WorkshopDTO>()
            {
                new WorkshopDTO()
                {
                    Id = 1,
                    Title = "w1",
                    ProviderId = 1,
                },
            };
        }
    }
}
