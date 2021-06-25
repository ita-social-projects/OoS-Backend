using System;
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
        private Mock<IApplicationService> applicationService;
        private Mock<IWorkshopService> workshopService;
        private Mock<IProviderService> providerService;
        private Mock<IParentService> parentService;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private string userId;
        private Mock<HttpContext> httpContext;

        private IEnumerable<ApplicationDto> applications;
        private IEnumerable<ChildDto> children;
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

            applications = FakeApplications();
            children = FakeChildren();

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
        [TestCase(1)]
        public async Task GetApplicationById_WhenIdIsValid_ShouldReturnOkObjectResult(long id)
        {
            // Arrange
            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(parent);

            applicationService.Setup(s => s.GetById(id)).ReturnsAsync(applications.SingleOrDefault(a => a.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        [TestCase(0)]
        public async Task GetApplicationById_WhenIdIsNotValid_ShouldReturnBadRequest(long id)
        {
            // Act
            var result = await controller.GetById(0).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(10)]
        public async Task GetApplicationById_WhenThereIsNoApplicationWithId_ShouldReturnNoContent(long id)
        {
            // Arrange
            applicationService.Setup(s => s.GetById(id)).ReturnsAsync(applications.SingleOrDefault(a => a.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        [Test]
        [TestCase(1, "parent")]
        [TestCase(1, "provider")]
        public async Task GetApplicationById_WhenUserHasNoRights_ShouldReturnBadRequest(long id, string role)
        {
            // Arrange
            var anotherParent = new ParentDTO { Id = 2, UserId = userId };
            var anotherProvider = new ProviderDto { Id = 2, UserId = userId };

            httpContext.Setup(c => c.User.IsInRole(role)).Returns(true);

            parentService.Setup(s => s.GetByUserId(It.IsAny<string>())).ReturnsAsync(anotherParent);
            providerService.Setup(s => s.GetByUserId(It.IsAny<string>())).ReturnsAsync(anotherProvider);
            applicationService.Setup(s => s.GetById(id)).ReturnsAsync(applications.SingleOrDefault(a => a.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
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
            httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);

            providerService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(provider);
            workshopService.Setup(s => s.GetById(id)).ReturnsAsync(FakeWorkshop());
            applicationService.Setup(s => s.GetAllByProvider(id))
                .ReturnsAsync(applications.Where(a => a.Workshop.ProviderId == id));
            applicationService.Setup(s => s.GetAllByWorkshop(id))
                .ReturnsAsync(applications.Where(a => a.WorkshopId == id));

            // Act
            var result = await controller.GetByPropertyId(property, id).ConfigureAwait(false) as OkObjectResult;

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
            var result = await controller.GetByPropertyId(property, id).ConfigureAwait(false) as BadRequestObjectResult;

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
            var newProvider = new ProviderDto { Id = 10, UserId = userId };
            var newWorkshop = new WorkshopDTO { Id = 10, ProviderId = 10 };

            httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);

            providerService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(newProvider);
            workshopService.Setup(s => s.GetById(id)).ReturnsAsync(newWorkshop);
            applicationService.Setup(s => s.GetAllByProvider(id))
                .ReturnsAsync(applications.Where(a => a.Workshop.ProviderId == id));
            applicationService.Setup(s => s.GetAllByWorkshop(id))
                .ReturnsAsync(applications.Where(a => a.WorkshopId == id));

            // Act
            var result = await controller.GetByPropertyId(property, id).ConfigureAwait(false) as NoContentResult;

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
            var anotherProvider = new ProviderDto { Id = 2, UserId = userId };

            httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);

            providerService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(anotherProvider);
            workshopService.Setup(s => s.GetById(id)).ReturnsAsync(FakeWorkshop());
            applicationService.Setup(s => s.GetAllByProvider(id))
                .ReturnsAsync(applications.Where(a => a.Workshop.ProviderId == id));
            applicationService.Setup(s => s.GetAllByWorkshop(id))
                .ReturnsAsync(applications.Where(a => a.WorkshopId == id));

            // Act
            var result = await controller.GetByPropertyId(property, id).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(0)]
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
        public async Task GetByStatus_WhenIdIsNotValid_ShouldReturnBadRequest(int status)
        {
            // Act
            var result = await controller.GetByStatus(status).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(1)]
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
            var shortApplication = new ShortApplicationDTO
            {
                Id = 1,
                Status = ApplicationStatus.Pending,
            };

            httpContext.Setup(c => c.User.IsInRole(role)).Returns(true);

            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(parent);
            providerService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(provider);

            applicationService.Setup(s => s.Update(applications.First())).ReturnsAsync(applications.First());
            applicationService.Setup(s => s.GetById(shortApplication.Id)).ReturnsAsync(applications.First());

            // Act
            var result = await controller.Update(shortApplication).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task UpdateApplication_WhenModelIsNotValid_ShouldReturnBadRequest()
        {
            // Arrange
            var shortApplication = new ShortApplicationDTO
            {
                Id = 1,
                Status = ApplicationStatus.Pending,
            };

            controller.ModelState.AddModelError("UpdateApplication", "Invalid model state.");

            // Act
            var result = await controller.Update(shortApplication).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase("parent")]
        [TestCase("provider")]
        public async Task UpdateApplication_WhenUserHasNoRights_ShouldReturnBadRequest(string role)
        {
            // Arrange
            var shortApplication = new ShortApplicationDTO
            {
                Id = 1,
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
            var result = await controller.Update(shortApplication).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteApplication_WhenIdIsValid_ShouldReturnNoContent(long id)
        {
            // Arrange
            applicationService.Setup(s => s.Delete(id));

            // Act
            var result = await controller.Delete(id).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        [Test]
        [TestCase(0)]
        public async Task DeleteApplication_WhenIdIsNotValid_ShouldReturnBadRequest(long id)
        {
            // Act
            var result = await controller.Delete(id).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteApplication_WhenThereIsNoApplicationWithId_ShouldBadRequest(long id)
        {
            // Arrange
            applicationService.Setup(s => s.Delete(id)).ThrowsAsync(new ArgumentException());

            // Act
            var result = await controller.Delete(id).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(400);
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
                    ParentId = 1,
                    WorkshopId = 1,
                    Workshop = FakeWorkshop(),
                },
                new ApplicationDto()
                {
                    Id = 2,
                    ChildId = 2,
                    Status = ApplicationStatus.Pending,
                    ParentId = 2,
                    WorkshopId = 1,
                    Workshop = FakeWorkshop(),
                },
            };
        }

        private WorkshopDTO FakeWorkshop()
        {
            return new WorkshopDTO()
            {
                Id = 1,
                Title = "w1",
                ProviderId = 1,
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
