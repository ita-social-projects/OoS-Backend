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
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Extensions;
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
        private Mock<IProviderAdminService> providerAdminService;
        private Mock<IParentService> parentService;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private string userId;
        private Guid providerId;
        private Mock<HttpContext> httpContext;

        private IEnumerable<ApplicationDto> applications;
        private IEnumerable<ChildDto> children;
        private IEnumerable<WorkshopCard> workshops;
        private ParentDTO parent;
        private ProviderDto provider;
        private WorkshopDTO workshopDto;

        [SetUp]
        public void Setup()
        {
            applicationService = new Mock<IApplicationService>();
            workshopService = new Mock<IWorkshopService>();
            providerService = new Mock<IProviderService>();
            providerAdminService = new Mock<IProviderAdminService>();
            parentService = new Mock<IParentService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            userId = Guid.NewGuid().ToString();

            httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User.FindFirst("sub"))
                       .Returns(new Claim(ClaimTypes.NameIdentifier, userId));

            controller = new ApplicationController(
                applicationService.Object,
                localizer.Object,
                providerService.Object,
                providerAdminService.Object,
                parentService.Object,
                workshopService.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = httpContext.Object },
            };
            providerId = Guid.NewGuid();
            workshops = FakeWorkshopCards();
            workshopDto = FakeWorkshop();
            workshopDto.ProviderId = providerId;
            children = ChildDtoGenerator.Generate(2).WithSocial(new SocialGroupDto { Id = 1 });

            parent = ParentDtoGenerator.Generate().WithUserId(userId);
            provider = ProviderDtoGenerator.Generate();
            provider.UserId = userId;
            provider.Id = providerId;
            applications = ApplicationDTOsGenerator.Generate(2).WithWorkshopCard(workshops.First()).WithParent(parent);
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
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
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
            result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
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
            var anotherParent = ParentDtoGenerator.Generate().WithUserId(userId);
            var anotherProvider = ProviderDtoGenerator.Generate().WithUserId(userId);

            httpContext.Setup(c => c.User.IsInRole(role)).Returns(true);
            parentService.Setup(s => s.GetByUserId(It.IsAny<string>())).ReturnsAsync(anotherParent);
            providerService.Setup(s => s.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(anotherProvider);
            applicationService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(applications.SingleOrDefault(a => a.Id == applicationId));

            // Act
            var result = await controller.GetById(applicationId).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetByParentId_WhenIdIsValid_ShouldReturnOkObjectResult()
        {
            // Arrange
            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(parent);
            applicationService.Setup(s => s.GetAllByParent(parent.Id, It.IsAny<ApplicationFilter>())).ReturnsAsync(applications.Where(a => a.ParentId == parent.Id));
            var filter = new ApplicationFilter();

            // Act
            var result = await controller.GetByParentId(parent.Id, filter).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Test]
        public async Task GetByParentId_WhenParentHasNoApplications_ShouldReturnNoContent()
        {
            // Arrange
            var newParent = ParentDtoGenerator.Generate().WithUserId(userId);

            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(newParent);
            applicationService.Setup(s => s.GetAllByParent(newParent.Id, It.IsAny<ApplicationFilter>())).ReturnsAsync(applications.Where(a => a.ParentId == newParent.Id));
            var filter = new ApplicationFilter();

            // Act
            var result = await controller.GetByParentId(newParent.Id, filter).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [Test]
        public async Task GetByParentId_WhenParentHasNoRights_ShouldReturnBadRequest()
        {
            // Arrange
            var anotherParent = ParentDtoGenerator.Generate().WithUserId(userId);

            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(anotherParent);
            applicationService.Setup(s => s.GetAllByParent(parent.Id, It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(applications.Where(a => a.ParentId == parent.Id));
            var filter = new ApplicationFilter();

            // Act
            var result = await controller.GetByParentId(parent.Id, filter).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        [TestCase("provider")]
        public async Task GetByPropertyProviderId_WhenIdIsValid_ShouldReturnOkObjectResult(string property)
        {
            // Arrange
            httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);
            providerService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(provider);
            providerService.Setup(s => s.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);
            applicationService.Setup(s => s.GetAllByProvider(It.IsAny<Guid>(), It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(applications);

            // Act
            var result = await controller.GetByPropertyId(property, provider.Id, It.IsAny<ApplicationFilter>()).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Test]
        [TestCase("workshop")]
        public async Task GetByPropertyWorkshopId_WhenIdIsValid_ShouldReturnOkObjectResult(string property)
        {
            // Arrange
            httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);
            workshopService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(workshopDto);
            providerService.Setup(s => s.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);
            applicationService.Setup(s => s.GetAllByWorkshop(It.IsAny<Guid>(), It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(applications);

            // Act
            var result = await controller.GetByPropertyId(property, workshopDto.Id, It.IsAny<ApplicationFilter>()).ConfigureAwait(false) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Test]
        [TestCase("83caa2e6-902a-43b5-9744-8a9d66604666", "provider")]
        [TestCase("f25d1bfc-8ebc-4087-b1c3-8dbb7964222d", "workshop")]
        public async Task GetByPropertyId_WhenIdIsNotValid_ShouldReturnBadRequest(Guid id, string property)
        {
            // Act
            var filter = new ApplicationFilter();

            var result = await controller.GetByPropertyId(property, id, filter).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        [TestCase("83caa2e6-902a-43b5-9744-8a9d66604666", "provider")]
        [TestCase("f25d1bfc-8ebc-4087-b1c3-8dbb7964222d", "workshop")]
        public async Task GetByPropertyId_WhenProviderHasNoApplications_ShouldReturnNoContent(Guid id, string property)
        {
            // Arrange
            var filter = new ApplicationFilter ();

            var newProvider = new ProviderDto { Id = new Guid("83caa2e6-902a-43b5-9744-8a9d66604666"), UserId = userId };
            var newWorkshop = new WorkshopDTO { Id = new Guid("94b81fa7-180f-4965-8aac-908a9f3ecb8d"), ProviderId = new Guid("83caa2e6-902a-43b5-9744-8a9d66604666") };

            httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);

            providerService.Setup(s => s.GetByUserId(userId, It.IsAny<bool>())).ReturnsAsync(newProvider);
            workshopService.Setup(s => s.GetById(id)).ReturnsAsync(newWorkshop);
            providerService.Setup(s => s.GetById(id)).ReturnsAsync(newProvider);
            applicationService.Setup(s => s.GetAllByProvider(id, filter))
                .ReturnsAsync(applications.Where(a => a.Workshop.ProviderId == id));
            applicationService.Setup(s => s.GetAllByWorkshop(id, filter))
                .ReturnsAsync(applications.Where(a => a.WorkshopId == id));

            // Act
            var result = await controller.GetByPropertyId(property, id, filter).ConfigureAwait(false) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [Test]
        [TestCase("83caa2e6-902a-43b5-9744-8a9d66604666", "provider")]
        [TestCase("f25d1bfc-8ebc-4087-b1c3-8dbb7964222d", "workshop")]
        public async Task GetByPropertyId_WhenProviderHasNoRights_ShouldReturnBadRequest(Guid id, string property)
        {
            // Arrange
            var filter = new ApplicationFilter();

            var anotherProvider = new ProviderDto { Id = new Guid("83caa2e6-902a-43b5-9744-8a9d66604777"), UserId = userId };
            var anotherWorkshop = new WorkshopDTO { Id = new Guid("94b81fa7-180f-4965-8aac-908a9f3ecb8d"), ProviderId = new Guid("83caa2e6-902a-43b5-9744-8a9d66604666") };

            httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);
            providerService.Setup(s => s.GetByUserId(userId, It.IsAny<bool>())).ReturnsAsync(anotherProvider);
            providerService.Setup(s => s.GetById(id)).ReturnsAsync(anotherProvider);
            workshopService.Setup(s => s.GetById(id)).ReturnsAsync(anotherWorkshop);
            applicationService.Setup(s => s.GetAllByProvider(id, filter))
                .ReturnsAsync(applications.Where(a => a.Workshop.ProviderId == id));
            applicationService.Setup(s => s.GetAllByWorkshop(id, filter))
                .ReturnsAsync(applications.Where(a => a.WorkshopId == id));

            // Act
            var result = await controller.GetByPropertyId(property, id, filter).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        [TestCase("f25d1bfc-8ebc-4087-b1c3-8dbb7964222d", "workshop")]
        public async Task GetByPropertyId_WhenThereIsNoWorkshopWithId_ShouldReturnBadRequest(Guid id, string property)
        {
            // Arrange
            var filter = new ApplicationFilter();

            // Act
            var result = await controller.GetByPropertyId(property, id, filter).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        public async Task CreateApplication_WhenModelIsValid_ShouldReturnCreatedAtAction()
        {
            // Arrange
            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);

            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(parent);
            applicationService.Setup(s => s.Create(applications.First()))
                .ReturnsAsync(new ModelWithAdditionalData<ApplicationDto, int> { Model = applications.First(), AdditionalData = 0});

            // Act
            var result = await controller.Create(applications.First()).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status201Created);
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
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
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
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        public async Task CreateApplication_WhenParentHasNoRights_ShouldReturnBadRequest()
        {
            // Arrange
            var anotherParent = new ParentDTO { Id = new Guid("1f91783d-a68f-41fa-9ded-d879f187a94b"), UserId = userId };

            httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
            parentService.Setup(s => s.GetByUserId(userId)).ReturnsAsync(anotherParent);
            applicationService.Setup(s => s.Create(applications.First()))
                .ReturnsAsync(new ModelWithAdditionalData<ApplicationDto, int> { Model = applications.First(), AdditionalData = 0 });

            // Act
            var result = await controller.Create(applications.First()).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
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
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
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
                RejectionMessage = applications.First().RejectionMessage,
            };

            httpContext.Setup(c => c.User.IsInRole(It.IsAny<string>())).Returns(true);

            parentService.Setup(s => s.GetByUserId(It.IsAny<string>())).ReturnsAsync(parent);
            providerService.Setup(s => s.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);

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

            var anotherParent = new ParentDTO { Id = new Guid("1f91783d-a68f-41fa-9ded-d879f187a94b"), UserId = userId };
            var anotherProvider = new ProviderDto { Id = new Guid("2f91783d-a68f-41fa-9ded-d879f187a94c"), UserId = userId };

            httpContext.Setup(c => c.User.IsInRole(role)).Returns(true);
            providerService.Setup(s => s.GetByUserId(userId, It.IsAny<bool>())).ReturnsAsync(anotherProvider);
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

        private WorkshopDTO FakeWorkshop()
        {
            return new WorkshopDTO()
            {
                Id = Guid.NewGuid(),
                Title = "Title6",
                Phone = "1111111111",
                Description = "Desc6",
                Price = 6000,
                WithDisabilityOptions = true,
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
                CoverImageId = "image6",
                ProviderId = Guid.NewGuid(),
                DirectionId = 1,
                DepartmentId = 1,
                ClassId = 1,
                AddressId = 55,
                Address = new AddressDto
                {
                    Id = 55,
                    Region = "Region55",
                    District = "District55",
                    City = "Київ",
                    Street = "Street55",
                    BuildingNumber = "BuildingNumber55",
                    Latitude = 0,
                    Longitude = 0,
                },
                Teachers = new List<TeacherDTO>
                {
                    new TeacherDTO
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Alex",
                        LastName = "Brown",
                        MiddleName = "SomeMiddleName",
                        Description = "Description",
                        AvatarImageId = "Image",
                        DateOfBirth = DateTime.Parse("2000-01-01"),
                        WorkshopId = new Guid("5e519d63-0cdd-48a8-81da-6365aa5ad8c3"),
                    },
                    new TeacherDTO
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "John",
                        LastName = "Snow",
                        MiddleName = "SomeMiddleName",
                        Description = "Description",
                        AvatarImageId = "Image",
                        DateOfBirth = DateTime.Parse("1990-01-01"),
                        WorkshopId = new Guid("5e519d63-0cdd-48a8-81da-6365aa5ad8c3"),
                    },
                },
            };
        }

        private List<WorkshopDTO> FakeWorkshops()
        {
            return new List<WorkshopDTO>()
            {
                new WorkshopDTO()
                {
                    Id = Guid.NewGuid(),
                    Title = "Title1",
                    Phone = "1111111111",
                    Description = "Desc1",
                    Price = 1000,
                    WithDisabilityOptions = true,
                    Head = "Head1",
                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                    ProviderId = Guid.NewGuid(),
                    ProviderTitle = "ProviderTitle",
                    DisabilityOptionsDesc = "Desc1",
                    Website = "website1",
                    Instagram = "insta1",
                    Facebook = "facebook1",
                    Email = "email1@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    CoverImageId = "image1",
                    DirectionId = 1,
                    DepartmentId = 1,
                    ClassId = 1,
                    Address = new AddressDto
                    {
                        City = "Київ",
                    },
                },
                new WorkshopDTO()
                {
                    Id = Guid.NewGuid(),
                    Title = "Title2",
                    Phone = "1111111111",
                    Description = "Desc2",
                    Price = 2000,
                    WithDisabilityOptions = true,
                    Head = "Head2",
                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                    ProviderId = Guid.NewGuid(),
                    ProviderTitle = "ProviderTitle",
                    DisabilityOptionsDesc = "Desc2",
                    Website = "website2",
                    Instagram = "insta2",
                    Facebook = "facebook2",
                    Email = "email2@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    CoverImageId = "image2",
                    DirectionId = 1,
                    DepartmentId = 1,
                    ClassId = 1,
                    Address = new AddressDto
                    {
                        City = "Київ",
                    },
                },
                new WorkshopDTO()
                {
                    Id = Guid.NewGuid(),
                    Title = "Title3",
                    Phone = "1111111111",
                    Description = "Desc3",
                    Price = 3000,
                    WithDisabilityOptions = true,
                    Head = "Head3",
                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                    ProviderId = Guid.NewGuid(),
                    ProviderTitle = "ProviderTitleNew",
                    DisabilityOptionsDesc = "Desc3",
                    Website = "website3",
                    Instagram = "insta3",
                    Facebook = "facebook3",
                    Email = "email3@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    CoverImageId = "image3",
                    DirectionId = 1,
                    DepartmentId = 1,
                    ClassId = 1,
                },
                new WorkshopDTO()
                {
                    Id = Guid.NewGuid(),
                    Title = "Title4",
                    Phone = "1111111111",
                    Description = "Desc4",
                    Price = 4000,
                    WithDisabilityOptions = true,
                    Head = "Head4",
                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                    ProviderId = Guid.NewGuid(),
                    ProviderTitle = "ProviderTitleNew",
                    DisabilityOptionsDesc = "Desc4",
                    Website = "website4",
                    Instagram = "insta4",
                    Facebook = "facebook4",
                    Email = "email4@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    CoverImageId = "image4",
                    DirectionId = 1,
                    DepartmentId = 1,
                    ClassId = 1,
                },
                new WorkshopDTO()
                {
                    Id = Guid.NewGuid(),
                    Title = "Title5",
                    Phone = "1111111111",
                    Description = "Desc5",
                    Price = 5000,
                    WithDisabilityOptions = true,
                    Head = "Head5",
                    HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                    ProviderId = Guid.NewGuid(),
                    ProviderTitle = "ProviderTitleNew",
                    DisabilityOptionsDesc = "Desc5",
                    Website = "website5",
                    Instagram = "insta5",
                    Facebook = "facebook5",
                    Email = "email5@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    CoverImageId = "image5",
                    DirectionId = 1,
                    DepartmentId = 1,
                    ClassId = 1,
                    Address = new AddressDto
                    {
                        City = "Київ",
                    },
                },
            };
        }

        private List<WorkshopCard> FakeWorkshopCards()
        {
            var list = FakeWorkshops();
            var eSlist = new List<WorkshopCard>();
            foreach (var item in list)
            {
                eSlist.Add(item.ToESModel().ToCardDto());
            }

            return eSlist;
        }
    }
}
