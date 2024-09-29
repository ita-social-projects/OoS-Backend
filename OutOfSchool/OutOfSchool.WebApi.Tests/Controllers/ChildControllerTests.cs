using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models.SocialGroup;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ChildControllerTests
{
    private ChildController controller;
    private Mock<IChildService> service;
    private Mock<IProviderService> providerService;
    private Mock<IEmployeeService> providerAdminService;
    private Mock<IWorkshopServicesCombiner> workshopService;
    private List<ChildDto> children;
    private ChildDto child;
    private ChildCreateDto childCreateDto;
    private List<ChildCreateDto> childrenCreateDto;
    private string currentUserId;

    private ParentDtoWithContactInfo existingParent;

    [SetUp]
    public void Setup()
    {
        service = new Mock<IChildService>();
        providerService = new Mock<IProviderService>();
        providerAdminService = new Mock<IEmployeeService>();
        workshopService = new Mock<IWorkshopServicesCombiner>();

        controller = new ChildController(service.Object, providerService.Object, providerAdminService.Object, workshopService.Object);

        // TODO: find out why it is a string but not a GUID
        currentUserId = Guid.NewGuid().ToString();

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[] { new Claim(IdentityResourceClaimsTypes.Sub, currentUserId) }, "sub"));

        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        existingParent = ParentDtoWithContactInfoGenerator.Generate();
        existingParent.UserId = currentUserId;

        var parent2 = ParentDtoWithContactInfoGenerator.Generate();

        children =
            ChildDtoGenerator.Generate(2)
                .WithParent(existingParent)
                .WithSocial(new SocialGroupDto { Id = 1 })
                .Concat(ChildDtoGenerator.Generate(2)
                    .WithParent(parent2)
                    .WithSocial(new SocialGroupDto { Id = 2 }))
                .ToList();
        child = ChildDtoGenerator.Generate();
        childCreateDto = ChildCreateDtoGenerator.Generate();
        childrenCreateDto = ChildCreateDtoGenerator.Generate(2);
    }

    [Test]
    public void ChildController_WhenServicesIsNull_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new ChildController(null, providerService.Object, providerAdminService.Object, workshopService.Object));
        Assert.Throws<ArgumentNullException>(() => new ChildController(service.Object, null, providerAdminService.Object, workshopService.Object));
        Assert.Throws<ArgumentNullException>(() => new ChildController(service.Object, providerService.Object, null, workshopService.Object));
        Assert.Throws<ArgumentNullException>(() => new ChildController(service.Object, providerService.Object, providerAdminService.Object, null));
    }

    [Test]
    public async Task GetChildren_WhenThereAreChildren_ShouldReturnOkResultObject()
    {
        // Arrange
        var filter = new ChildSearchFilter();
        service.Setup(x => x.GetByFilter(filter)).ReturnsAsync(new SearchResult<ChildDto>() { TotalAmount = children.Count(), Entities = children });

        // Act
        var result = await controller.GetAllForAdmin(filter).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetChildren_WhenThereIsNoChild_ShouldReturnOkObjectResult()
    {
        // Arrange
        var filter = new ChildSearchFilter();
        service.Setup(x => x.GetByFilter(filter)).ReturnsAsync(new SearchResult<ChildDto>() { Entities = new List<ChildDto>() });

        // Act
        var result = await controller.GetAllForAdmin(filter).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    public async Task GetUsersChilById_WhenChildWasFound_ShouldReturnOkResultObject()
    {
        // Arrange
        var existingChildId = children.RandomItem().Id;
        service.Setup(x => x.GetByIdAndUserId(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(children.First(x => x.Id.Equals(existingChildId)));

        // Act
        var result = await controller.GetUsersChildById(existingChildId).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetUsersChildById_WhenChildWasNotFound_ShouldReturnOkObjectResult()
    {
        // Arrange
        var noneExistingChildId = Guid.NewGuid();
        service.Setup(x => x.GetByIdAndUserId(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(default, new TimeSpan(1));

        // Act
        var result = await controller.GetUsersChildById(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    public async Task GetByParentId_WhenThereAreChildren_ShouldReturnOkResultObject()
    {
        // Arrange
        var filter = new OffsetFilter();
        service.Setup(x => x.GetByParentIdOrderedByFirstName(existingParent.Id, filter))
            .ReturnsAsync(
                new SearchResult<ChildDto>()
                {
                    TotalAmount = children.Where(p => p.ParentId == existingParent.Id).Count(),
                    Entities = children.Where(p => p.ParentId == existingParent.Id).ToList(),
                });

        // Act
        var result = await controller.GetByParentIdForAdmin(existingParent.Id, filter).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [TestCase(10)]
    public async Task GetByParentId_WhenThereIsNoChild_ShouldReturnOkObjectResult(long id)
    {
        // Arrange
        var noneExistingParentId = Guid.NewGuid();
        var filter = new OffsetFilter();
        service.Setup(x => x.GetByParentIdOrderedByFirstName(noneExistingParentId, filter)).ReturnsAsync(new SearchResult<ChildDto>() { Entities = children.Where(p => p.ParentId == noneExistingParentId).ToList() });

        // Act
        var result = await controller.GetByParentIdForAdmin(noneExistingParentId, filter).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task CreateChild_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
    {
        // Arrange
        service.Setup(x => x.CreateChildForUser(childCreateDto, currentUserId)).ReturnsAsync(child);

        // Act
        var result = await controller.Create(childCreateDto).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<CreatedAtActionResult>(result);
    }

    [Test]
    public async Task CreateChildren_WhenModelIsValid_ShouldReturnOkObjectResult()
    {
        // Arrange
        service.Setup(x => x.CreateChildrenForUser(childrenCreateDto, currentUserId))
            .ReturnsAsync(new ChildrenCreationResultDto());

        // Act
        var result = await controller.CreateChildren(childrenCreateDto).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task UpdateChild_WhenModelIsValid_ShouldReturnOkObjectResult()
    {
        // Arrange
        var childToUpdate = ChildUpdateDtoGenerator.Generate();
        var updatedChild = ChildDtoGenerator.Generate();
        var childId = children.RandomItem().Id;
        updatedChild.Id = childId;

        service.Setup(x => x.UpdateChildCheckingItsUserIdProperty(childToUpdate, childId, It.IsAny<string>())).ReturnsAsync(updatedChild);

        // Act
        var result = await controller.Update(childToUpdate, childId).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task DeleteChild_WhenChildWithIdExistsAndUserIsParent_ShouldReturnNoContentResult()
    {
        // Arrange
        var childToDelete = children.RandomItem();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[] { new(IdentityResourceClaimsTypes.Sub, currentUserId), new(IdentityResourceClaimsTypes.Role, Role.TechAdmin.ToString()) }, "sub"));

        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        // Act
        var result = await controller.Delete(childToDelete.Id);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public async Task DeleteChild_WhenChildWithIdExistsAndUserIsTechAdmin_ShouldReturnNoContentResult()
    {
        // Arrange
        var childToDelete = children.RandomItem();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[] { new(IdentityResourceClaimsTypes.Sub, currentUserId), new(IdentityResourceClaimsTypes.Role, Role.Parent.ToString()) }, "sub"));

        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        // Act
        var result = await controller.Delete(childToDelete.Id);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public async Task DeleteChild_WhenIdIsNotValid_ShouldReturnNull()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[] { new(IdentityResourceClaimsTypes.Sub, currentUserId), new(IdentityResourceClaimsTypes.Role, Role.TechAdmin.ToString()) }, "sub"));

        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        // Act
        var result = await controller.Delete(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public async Task DeleteChild_WhenWorkshopIdIsNotValid_ShouldReturnNotFound()
    {
        // Arrange
        var filter = new OffsetFilter();
        workshopService.Setup(s => s.Exists(It.IsAny<Guid>())).ReturnsAsync(() => false);

        // Act
        var result = await controller.GetApprovedByWorkshopId(Guid.NewGuid(), filter).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
    }

    [Test]
    public async Task GetApprovedByWorkshopId_WhenThereAreChildren_ShouldReturnOkResultObject()
    {
        // Arrange
        ChildDto child = children.First();
        WorkshopV2Dto existingWorkshop = new WorkshopV2Dto()
        {
            Id = Guid.NewGuid(),
            Title = "Title1",
            Phone = "1111111111",
            WorkshopDescriptionItems = new[]
                {
                    new WorkshopDescriptionItemDto
                    {
                        Id = Guid.NewGuid(),
                        SectionName = "test heading",
                        Description = "test description",
                    },
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
            Address = new AddressDto
            {
                CATOTTGId = 4970,
            },
        };

        ProviderDto existingProvider = ProviderDtoGenerator.Generate().WithUserId(existingWorkshop.ProviderId.ToString());

        ApplicationDto existingApplication = ApplicationDTOsGenerator
            .Generate()
            .WithWorkshopCard(new WorkshopCard
            {
                WorkshopId = existingWorkshop.Id,
                ProviderTitle = existingWorkshop.ProviderTitle,
                ProviderOwnership = existingWorkshop.ProviderOwnership,
                Title = existingWorkshop.Title,
                PayRate = (PayRateType)existingWorkshop.PayRate,
                CoverImageId = existingWorkshop.CoverImageId,
                MinAge = existingWorkshop.MinAge,
                MaxAge = existingWorkshop.MaxAge,
                Price = (decimal)existingWorkshop.Price,
                DirectionIds = existingWorkshop.DirectionIds,
                ProviderId = existingWorkshop.ProviderId,
                Address = existingWorkshop.Address,
                WithDisabilityOptions = existingWorkshop.WithDisabilityOptions,
                Rating = existingWorkshop.Rating,
                ProviderLicenseStatus = existingWorkshop.ProviderLicenseStatus,
                InstitutionHierarchyId = existingWorkshop.InstitutionHierarchyId,
                InstitutionId = existingWorkshop.InstitutionId,
                Institution = existingWorkshop.Institution,
                AvailableSeats = (uint)existingWorkshop.AvailableSeats,
                TakenSeats = existingWorkshop.TakenSeats,
            })
            .WithChild(child);

        workshopService.Setup(s => s.Exists(existingWorkshop.Id)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(existingWorkshop.Id)).ReturnsAsync(existingProvider.Id);
        providerService.Setup(s => s.GetByUserId(It.IsAny<string>(), false)).ReturnsAsync(existingProvider);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.Provider).ToLower()),
                new Claim("sub", currentUserId),
            }));

        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        var filter = new OffsetFilter();
        service.Setup(x => x.GetApprovedByWorkshopId(existingWorkshop.Id, filter))
            .ReturnsAsync(
            new SearchResult<ChildDto>()
            {
                TotalAmount = children.Where(p => p.Id == child.Id).Count(),
                Entities = children.Where(p => p.Id == child.Id).ToList(),
            });

        // Act
        var result = await controller.GetApprovedByWorkshopId(existingWorkshop.Id, filter).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }
}
