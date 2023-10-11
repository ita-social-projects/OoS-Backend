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
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Application;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Models.SocialGroup;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ApplicationControllerTests
{
    private ApplicationController controller;
    private Mock<IApplicationService> applicationService;
    private Mock<IWorkshopService> workshopService;
    private Mock<IProviderService> providerService;
    private Mock<IProviderAdminService> providerAdminService;
    private Mock<IUserService> userService;

    private string userId;
    private Guid providerId;
    private Mock<HttpContext> httpContext;

    private List<ApplicationDto> applications;
    private IEnumerable<ChildDto> children;
    private IEnumerable<WorkshopCard> workshops;
    private ParentDTO parent;
    private ProviderDto provider;
    private WorkshopV2Dto workshopDto;

    [SetUp]
    public void Setup()
    {
        applicationService = new Mock<IApplicationService>();
        workshopService = new Mock<IWorkshopService>();
        providerService = new Mock<IProviderService>();
        providerAdminService = new Mock<IProviderAdminService>();
        userService = new Mock<IUserService>();

        userId = Guid.NewGuid().ToString();

        httpContext = new Mock<HttpContext>();
        httpContext.Setup(c => c.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));

        controller = new ApplicationController(
            applicationService.Object,
            providerService.Object,
            providerAdminService.Object,
            workshopService.Object,
            userService.Object)
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
    public async Task GetApplications_WhenCalledByAdmin_ShouldReturnOkResultObject()
    {
        // Arrange
        applicationService.Setup(s => s.GetAll(It.IsAny<ApplicationFilter>())).ReturnsAsync(new SearchResult<ApplicationDto>
        {
            Entities = applications,
            TotalAmount = applications.Count,
        });

        // Act
        var result = await controller.Get(new ApplicationFilter()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Test]
    public void GetApplications_WhenCalledParentOrProvider_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        applicationService.Setup(s => s.GetAll(It.IsAny<ApplicationFilter>())).ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await controller.Get(new ApplicationFilter()));
    }

    [Test]
    public async Task GetApplications_WhenCollectionIsEmpty_ShouldReturnNoContent()
    {
        // Arrange
        applicationService.Setup(s => s.GetAll(It.IsAny<ApplicationFilter>())).ReturnsAsync(new SearchResult<ApplicationDto>()
        {
            Entities = new List<ApplicationDto>(),
            TotalAmount = 0,
        });

        // Act
        var result = await controller.Get(new ApplicationFilter()).ConfigureAwait(false) as NoContentResult;

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

    [Test]
    public void GetApplicationById_WhenUserHasNoRights_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        var applicationId = applications.First().Id;
        applicationService.Setup(s => s.GetById(It.IsAny<Guid>())).ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await controller.GetById(applicationId));
    }

    [Test]
    public async Task GetByParentId_WhenIdIsValid_ShouldReturnOkObjectResult()
    {
        // Arrange
        httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
        List<ApplicationDto> app = applications.Where(a => a.ParentId == parent.Id).ToList();
        applicationService.Setup(s => s.GetAllByParent(parent.Id, It.IsAny<ApplicationFilter>())).ReturnsAsync(new SearchResult<ApplicationDto>() { TotalAmount = app.Count(), Entities = app });
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
        List<ApplicationDto> app = applications.Where(a => a.ParentId == newParent.Id).ToList();
        applicationService.Setup(s => s.GetAllByParent(newParent.Id, It.IsAny<ApplicationFilter>())).ReturnsAsync(new SearchResult<ApplicationDto>() { TotalAmount = app.Count(), Entities = app });
        var filter = new ApplicationFilter();

        // Act
        var result = await controller.GetByParentId(newParent.Id, filter).ConfigureAwait(false) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Test]
    public void GetByParentId_WhenParentHasNoRights_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
        List<ApplicationDto> app = applications.Where(a => a.ParentId == parent.Id).ToList();
        applicationService.Setup(s => s.GetAllByParent(parent.Id, It.IsAny<ApplicationFilter>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        var filter = new ApplicationFilter();

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await controller.GetByParentId(parent.Id, filter));
    }

    [Test]
    public async Task GetByProviderId_WhenIdIsValid_ShouldReturnOkObjectResult()
    {
        // Arrange
        httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);
        providerService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(provider);
        providerService.Setup(s => s.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);
        List<ApplicationDto> app = applications.ToList();
        applicationService.Setup(s => s.GetAllByProvider(It.IsAny<Guid>(), It.IsAny<ApplicationFilter>()))
            .ReturnsAsync(new SearchResult<ApplicationDto>() { TotalAmount = app.Count(), Entities = app });

        // Act
        var result = await controller.GetByProviderId(provider.Id, It.IsAny<ApplicationFilter>()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GetByWorkshopId_WhenIdIsValid_ShouldReturnOkObjectResult()
    {
        // Arrange
        httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);
        workshopService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(workshopDto);
        providerService.Setup(s => s.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);
        List<ApplicationDto> app = applications.ToList();
        applicationService.Setup(s => s.GetAllByWorkshop(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ApplicationFilter>()))
            .ReturnsAsync(new SearchResult<ApplicationDto>() { TotalAmount = app.Count(), Entities = app });

        // Act
        var result = await controller.GetByWorkshopId(workshopDto.Id, It.IsAny<ApplicationFilter>()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GetByWorkshopId_WhenIdIsNotValid_ShouldReturnBadRequest()
    {
        // Act
        var filter = new ApplicationFilter();

        var result = await controller.GetByWorkshopId(Guid.Parse("f25d1bfc-8ebc-4087-b1c3-8dbb7964222d"), filter).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task GetByProviderId_WhenIdIsNotValid_ShouldReturnBadRequest()
    {
        // Act
        var filter = new ApplicationFilter();

        var result = await controller.GetByProviderId(Guid.Parse("83caa2e6-902a-43b5-9744-8a9d66604666"), filter).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task GetByWorkshopId_WhenProviderHasNoApplications_ShouldReturnNoContent()
    {
        // Arrange
        var id = Guid.Parse("f25d1bfc-8ebc-4087-b1c3-8dbb7964222d");
        var filter = new ApplicationFilter ();

        var newProvider = new ProviderDto { Id = new Guid("83caa2e6-902a-43b5-9744-8a9d66604666"), UserId = userId };
        var newWorkshop = new WorkshopDto { Id = new Guid("94b81fa7-180f-4965-8aac-908a9f3ecb8d"), ProviderId = new Guid("83caa2e6-902a-43b5-9744-8a9d66604666") };

        httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);

        providerService.Setup(s => s.GetByUserId(userId, It.IsAny<bool>())).ReturnsAsync(newProvider);
        workshopService.Setup(s => s.GetById(id)).ReturnsAsync(newWorkshop);
        providerService.Setup(s => s.GetById(id)).ReturnsAsync(newProvider);
        List<ApplicationDto> app1 = applications.Where(a => a.Workshop.ProviderId == id).ToList();
        applicationService.Setup(s => s.GetAllByProvider(id, filter))
            .ReturnsAsync(new SearchResult<ApplicationDto>() { TotalAmount = app1.Count(), Entities = app1 });
        List<ApplicationDto> app2 = applications.Where(a => a.WorkshopId == id).ToList();
        applicationService.Setup(s => s.GetAllByWorkshop(id, new Guid("83caa2e6-902a-43b5-9744-8a9d66604666"), filter))
            .ReturnsAsync(new SearchResult<ApplicationDto>() { TotalAmount = app2.Count(), Entities = app2 });

        // Act
        var result = await controller.GetByWorkshopId(id, filter).ConfigureAwait(false) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Test]
    public async Task GetByProviderId_WhenProviderHasNoApplications_ShouldReturnNoContent()
    {
        // Arrange
        var id = Guid.Parse("83caa2e6-902a-43b5-9744-8a9d66604666");
        var filter = new ApplicationFilter ();

        var newProvider = new ProviderDto { Id = new Guid("83caa2e6-902a-43b5-9744-8a9d66604666"), UserId = userId };
        var newWorkshop = new WorkshopDto { Id = new Guid("94b81fa7-180f-4965-8aac-908a9f3ecb8d"), ProviderId = new Guid("83caa2e6-902a-43b5-9744-8a9d66604666") };

        httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);

        providerService.Setup(s => s.GetByUserId(userId, It.IsAny<bool>())).ReturnsAsync(newProvider);
        workshopService.Setup(s => s.GetById(id)).ReturnsAsync(newWorkshop);
        providerService.Setup(s => s.GetById(id)).ReturnsAsync(newProvider);
        List<ApplicationDto> app1 = applications.Where(a => a.Workshop.ProviderId == id).ToList();
        applicationService.Setup(s => s.GetAllByProvider(id, filter))
            .ReturnsAsync(new SearchResult<ApplicationDto>() { TotalAmount = app1.Count(), Entities = app1 });
        List<ApplicationDto> app2 = applications.Where(a => a.WorkshopId == id).ToList();
        applicationService.Setup(s => s.GetAllByWorkshop(id, new Guid("83caa2e6-902a-43b5-9744-8a9d66604666"), filter))
            .ReturnsAsync(new SearchResult<ApplicationDto>() { TotalAmount = app2.Count(), Entities = app2 });

        // Act
        var result = await controller.GetByProviderId(id, filter).ConfigureAwait(false) as NoContentResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Test]
    public void GetByWorkshopId_WhenProviderHasNoRights_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        var id = Guid.Parse("f25d1bfc-8ebc-4087-b1c3-8dbb7964222d");
        var filter = new ApplicationFilter();

        httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);

        workshopService.Setup(s => s.GetById(id)).ReturnsAsync(new WorkshopDto());
        applicationService.Setup(s => s.GetAllByWorkshop(id, It.IsAny<Guid>(), filter))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await controller.GetByWorkshopId(id, filter));
    }

    [Test]
    public void GetByProviderId_WhenProviderHasNoRights_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        var id = Guid.Parse("83caa2e6-902a-43b5-9744-8a9d66604666");
        var filter = new ApplicationFilter();
        httpContext.Setup(c => c.User.IsInRole("provider")).Returns(true);
        providerService.Setup(s => s.GetById(id)).ReturnsAsync(new ProviderDto());
        applicationService.Setup(s => s.GetAllByProvider(id, filter))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await controller.GetByProviderId(id, filter));
    }

    [Test]
    public async Task GetByWorkshopId_WhenThereIsNoWorkshopWithId_ShouldReturnBadRequest()
    {
        // Arrange
        var filter = new ApplicationFilter();

        // Act
        var result = await controller.GetByWorkshopId(Guid.Parse("f25d1bfc-8ebc-4087-b1c3-8dbb7964222d"), filter).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task CreateApplication_WhenModelIsValid_ShouldReturnCreatedAtAction()
    {
        // Arrange
        httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
        var app = new ApplicationCreate()
        {
            ChildId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
            ParentId = new Guid("1f91783d-a68f-41fa-9ded-d879f187a94b"),
            WorkshopId = new Guid("5e519d63-0cdd-48a8-81da-6365aa5ad8c3"),
        };

        applicationService.Setup(s => s.Create(app))
            .ReturnsAsync(new ModelWithAdditionalData<ApplicationDto, int>
                {Model = applications.First(), AdditionalData = 0});

        // Act
        var result = await controller.Create(app).ConfigureAwait(false) as CreatedAtActionResult;

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
        var result = await controller.Create(new ApplicationCreate()).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task CreateApplication_WhenModelIsNull_ShouldReturnBadRequest()
    {
        // Arrange
        ApplicationCreate application = null;

        // Act
        var result = await controller.Create(application).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public void CreateApplication_WhenParentHasNoRights_ShouldThrowUnauthorizedAccess()
    {
        // Arrange
        var anotherParent = new ParentDTO {Id = new Guid("1f91783d-a68f-41fa-9ded-d879f187a94b"), UserId = userId};

        httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);

        applicationService.Setup(s => s.Create(It.IsAny<ApplicationCreate>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await controller.Create(new ApplicationCreate()));
    }

    [Test]
    public async Task CreateApplication_WhenParametersAreNotValid_ShouldReturnBadRequest()
    {
        // Arrange
        httpContext.Setup(c => c.User.IsInRole("parent")).Returns(true);
        applicationService.Setup(s => s.Create(It.IsAny<ApplicationCreate>())).ThrowsAsync(new ArgumentException());

        // Act
        var result = await controller.Create(new ApplicationCreate()).ConfigureAwait(false) as BadRequestObjectResult;

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
        var shortApplication = new ApplicationUpdate()
        {
            Id = applications.First().Id,
            Status = ApplicationStatus.Pending,
            RejectionMessage = applications.First().RejectionMessage,
        };

        applicationService.Setup(s => s.Update(It.IsAny<ApplicationUpdate>(), It.IsAny<Guid>())).ReturnsAsync(Result<ApplicationDto>.Success(applications.First()));
        workshopService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync(new WorkshopDto());

        // Act
        var result = await controller.Update(shortApplication).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateApplication_WhenModelIsNotValid_ShouldReturnBadRequest()
    {
        // Arrange
        var shortApplication = new ApplicationUpdate()
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
    public void UpdateApplication_WhenUserHasNoRights_ShouldThrowUnauthorizedAccess(string role)
    {
        // Arrange
        var shortApplication = new ApplicationUpdate()
        {
            Id = applications.First().Id,
            Status = ApplicationStatus.Pending,
            WorkshopId = Guid.NewGuid(),
        };
        httpContext.Setup(c => c.User.IsInRole(role)).Returns(true);

        applicationService.Setup(s => s.Update(shortApplication, It.IsAny<Guid>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        workshopService.Setup(s => s.GetById(shortApplication.WorkshopId)).ReturnsAsync(new WorkshopDto());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await controller.Update(shortApplication));
    }

    [Test]
    public async Task UpdateApplication_WhenThereIsNoApplicationWithId_ShouldReturnBadRequest()
    {
        // Arrange
        var shortApplication = new ApplicationUpdate()
        {
            Id = Guid.NewGuid(),
            Status = ApplicationStatus.Pending,
        };

        applicationService.Setup(s => s.GetById(shortApplication.Id))
            .ReturnsAsync(applications.FirstOrDefault(a => a.Id == shortApplication.Id));

        // Act
        var result = await controller.Update(shortApplication).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    private WorkshopV2Dto FakeWorkshop()
    {
        return new WorkshopV2Dto()
        {
            Id = Guid.NewGuid(),
            Title = "Title6",
            Phone = "1111111111",
            WorkshopDescriptionItems = new[]
            {
                FakeWorkshopDescriptionItem(),
                FakeWorkshopDescriptionItem(),
                FakeWorkshopDescriptionItem(),
            },
            Price = 6000,
            WithDisabilityOptions = true,
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
            InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
            AddressId = 55,
            Address = new AddressDto
            {
                Id = 55,
                CATOTTGId = 4970,
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
                    CoverImageId = "Image",
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
                    CoverImageId = "Image",
                    DateOfBirth = DateTime.Parse("1990-01-01"),
                    WorkshopId = new Guid("5e519d63-0cdd-48a8-81da-6365aa5ad8c3"),
                },
            },
        };
    }

    private List<WorkshopV2Dto> FakeWorkshops()
    {
        return new List<WorkshopV2Dto>()
        {
            new WorkshopV2Dto()
            {
                Id = Guid.NewGuid(),
                Title = "Title1",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    FakeWorkshopDescriptionItem(),
                    FakeWorkshopDescriptionItem(),
                },
                Price = 1000,
                WithDisabilityOptions = true,
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
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                Address = new AddressDto
                {
                    CATOTTGId = 4970,
                },
            },
            new WorkshopV2Dto()
            {
                Id = Guid.NewGuid(),
                Title = "Title2",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    FakeWorkshopDescriptionItem(),
                },
                Price = 2000,
                WithDisabilityOptions = true,
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
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                Address = new AddressDto
                {
                    CATOTTGId = 4970,
                },
            },
            new WorkshopV2Dto()
            {
                Id = Guid.NewGuid(),
                Title = "Title3",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    FakeWorkshopDescriptionItem(),
                    FakeWorkshopDescriptionItem(),
                    FakeWorkshopDescriptionItem(),
                },
                Price = 3000,
                WithDisabilityOptions = true,
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
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
            },
            new WorkshopV2Dto()
            {
                Id = Guid.NewGuid(),
                Title = "Title4",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    FakeWorkshopDescriptionItem(),
                    FakeWorkshopDescriptionItem(),
                },
                Price = 4000,
                WithDisabilityOptions = true,
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
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
            },
            new WorkshopV2Dto()
            {
                Id = Guid.NewGuid(),
                Title = "Title5",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    FakeWorkshopDescriptionItem(),
                },
                Price = 5000,
                WithDisabilityOptions = true,
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
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                Address = new AddressDto
                {
                    CATOTTGId = 4970,
                },
            },
        };
    }

    private List<WorkshopCard> FakeWorkshopCards()
    {
        return FakeWorkshops().Select(w => new WorkshopCard
        {
            WorkshopId = w.Id,
            ProviderTitle = w.ProviderTitle,
            ProviderOwnership = w.ProviderOwnership,
            Title = w.Title,
            PayRate = (OutOfSchool.Common.Enums.PayRateType)w.PayRate,
            CoverImageId = w.CoverImageId,
            MinAge = w.MinAge,
            MaxAge = w.MaxAge,
            Price = (decimal)w.Price,
            DirectionIds = w.DirectionIds,
            ProviderId = w.ProviderId,
            Address = w.Address,
            WithDisabilityOptions = w.WithDisabilityOptions,
            Rating = w.Rating,
            ProviderLicenseStatus = w.ProviderLicenseStatus,
            InstitutionHierarchyId = w.InstitutionHierarchyId,
            InstitutionId = w.InstitutionId,
            Institution = w.Institution,
            AvailableSeats = w.AvailableSeats,
            TakenSeats = w.TakenSeats,
        }).ToList();
    }

    private WorkshopDescriptionItemDto FakeWorkshopDescriptionItem()
    {
        var id = Guid.NewGuid();
        return new WorkshopDescriptionItemDto
        {
            Id = id,
            SectionName = "test heading",
            Description = $"test description text sentence for id: {id.ToString()}",
        };
    }
}