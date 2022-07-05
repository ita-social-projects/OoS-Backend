using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.Enums;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class WorkshopControllerTests
    {
        private const int Ok = 200;
        private const int NoContent = 204;
        private const int Create = 201;
        private const int BadRequest = 400;
        private const int Forbidden = 403;

        private static List<WorkshopDTO> workshops;
        private static List<WorkshopCard> workshopCards;
        private static WorkshopDTO workshop;
        private static ProviderDto provider;
        private static ProviderAdminDto providerAdmin;
        private static Mock<IOptions<AppDefaultsConfig>> options;

        private WorkshopController controller;
        private Mock<IWorkshopServicesCombiner> workshopServiceMoq;
        private Mock<IProviderService> providerServiceMoq;
        private Mock<IProviderAdminService> providerAdminService;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private string userId;
        private Mock<HttpContext> httpContextMoq;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            userId = "someUserId";
            httpContextMoq = new Mock<HttpContext>();
            httpContextMoq.Setup(x => x.User.FindFirst("sub"))
                .Returns(new Claim(ClaimTypes.NameIdentifier, userId));
            httpContextMoq.Setup(x => x.User.IsInRole("provider"))
                .Returns(true);

            workshops = WithWorkshops();
            workshop = WithWorkshop();
            provider = WithProvider();
            workshopCards = WithWorkshopCards();

            var config = new AppDefaultsConfig();
            config.City = "Київ";
            options = new Mock<IOptions<AppDefaultsConfig>>();
            options.Setup(x => x.Value).Returns(config);
        }

        [SetUp]
        public void Setup()
        {
            workshopServiceMoq = new Mock<IWorkshopServicesCombiner>();
            providerServiceMoq = new Mock<IProviderService>();
            providerAdminService = new Mock<IProviderAdminService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            providerAdminService = new Mock<IProviderAdminService>();

            controller = new WorkshopController(workshopServiceMoq.Object, providerServiceMoq.Object, providerAdminService.Object, localizer.Object, options.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
            };
        }

        #region GetWorkshopById
        //[Test]
        //[TestCase(1)]
        //public async Task GetWorkshopById_WhenIdIsValid_ShouldReturnOkResultObject(long id)
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(workshops.SingleOrDefault(x => x.Id == id));

        //    // Act
        //    var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.AreEqual(Ok, result.StatusCode);
        //}

        //[Test]
        //[TestCase(0)]
        //public void GetWorkshopById_WhenIdIsInvalid_ShouldThrowArgumentOutOfRangeException(long id)
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(workshops.SingleOrDefault(x => x.Id == id));

        //    // Assert
        //    Assert.That(
        //        async () => await controller.GetById(id),
        //        Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        //}

        //[Test]
        //[TestCase(10)]
        //public async Task GetWorkshopById_WhenThereIsNoWorkshopWithId_ShouldReturnNoContent(long id)
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(workshops.SingleOrDefault(x => x.Id == id));

        //    // Act
        //    var result = await controller.GetById(id).ConfigureAwait(false) as NoContentResult;

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.AreEqual(NoContent, result.StatusCode);
        //}
        #endregion

        #region GetByProviderId
        //[Test]
        //[TestCase(0)]
        //public void GetByProviderId_WhenIdIsInvalid_ShouldThrowArgumentOutOfRangeException(long id)
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.GetByProviderId(id)).ReturnsAsync(workshopCards.Where(x => x.ProviderId == id).ToList());

        //    // Assert
        //    Assert.That(
        //        async () => await controller.GetByProviderId(id),
        //        Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        //}

        //[Test]
        //[TestCase(-1)]
        //public void GetByProviderId_WhenIdIsLessThanZero_ShouldThrowArgumentOutOfRangeException(long id)
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.GetByProviderId(id)).ReturnsAsync(workshopCards.Where(x => x.ProviderId == id).ToList());

        //    // Assert
        //    Assert.That(
        //        async () => await controller.GetByProviderId(id),
        //        Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        //}

        //[Test]
        //[TestCase(long.MaxValue)]
        //public async Task GetByProviderId_WhenIdMaxValue_ShouldReturnNoConterntResult(long id)
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.GetByProviderId(id)).ReturnsAsync(workshopCards.Where(x => x.ProviderId == id).ToList());

        //    // Act
        //    var result = await controller.GetByProviderId(id).ConfigureAwait(false);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result, Is.InstanceOf<NoContentResult>());
        //}

        //[Test]
        //[TestCase(1)]
        //public async Task GetByProviderId_WhenThereAreWorkshops_ShouldReturnOkResultObject(long id)
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.GetByProviderId(id)).ReturnsAsync(workshopCards.Where(x => x.ProviderId == id).ToList());

        //    // Act
        //    var result = await controller.GetByProviderId(id).ConfigureAwait(false);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result, Is.InstanceOf<OkObjectResult>());
        //    Assert.AreEqual(Ok, (result as OkObjectResult).StatusCode);
        //    Assert.AreEqual(2, ((result as OkObjectResult).Value as List<WorkshopCard>).Count());
        //}

        //[Test]
        //public async Task GetWorkshops_WhenThereIsNoWorkshop_ShouldReturnNoConterntResult([Random(uint.MinValue, uint.MaxValue, 1)] long randomNumber)
        //{
        //    // Arrange
        //    var emptyList = new List<WorkshopCard>();
        //    var id = workshops.Select(w => w.Id).Max() + randomNumber + 1;
        //    workshopServiceMoq.Setup(x => x.GetByProviderId(id)).ReturnsAsync(emptyList);

        //    // Act
        //    var result = await controller.GetByProviderId(id).ConfigureAwait(false);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result, Is.InstanceOf<NoContentResult>());
        //}
        #endregion

        #region GetWorkshopsByFilter
        [Test]
        public async Task GetWorkshopByFilter_WhenThereAreWorkshops_ShouldReturnOkResultObject()
        {
            // Arrange
            var searchResult = new SearchResult<WorkshopCard>() { TotalAmount = 5, Entities = workshopCards };
            workshopServiceMoq.Setup(x => x.GetByFilter(It.IsAny<WorkshopFilter>())).ReturnsAsync(searchResult);

            // Act
            var result = await controller.GetByFilter(new WorkshopFilter()).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(Ok, result.StatusCode);
            Assert.IsInstanceOf<SearchResult<WorkshopCard>>(result.Value);
        }

        [Test]
        public async Task GetWorkshopByFilter_WhenThereIsNoAnyWorkshop_ShouldReturnNoConterntResult()
        {
            // Arrange
            var searchResult = new SearchResult<WorkshopCard>() { TotalAmount = 0, Entities = new List<WorkshopCard>() };
            workshopServiceMoq.Setup(x => x.GetByFilter(It.IsAny<WorkshopFilter>())).ReturnsAsync(searchResult);

            // Act
            var result = await controller.GetByFilter(new WorkshopFilter()).ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(NoContent, result.StatusCode);
        }
        #endregion

        //#region CreateWorkshop
        //[Test]
        //public async Task CreateWorkshop_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
        //{
        //    // Arrange
        //    providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider);
        //    workshopServiceMoq.Setup(x => x.Create(workshop)).ReturnsAsync(workshop);

        //    // Act
        //    var result = await controller.Create(workshop).ConfigureAwait(false) as CreatedAtActionResult;

        //    // Assert
        //    workshopServiceMoq.Verify(x => x.Create(workshop), Times.Once);
        //    Assert.That(result, Is.Not.Null);
        //    Assert.AreEqual(Create, result.StatusCode);
        //}

        //[Test]
        //public async Task CreateWorkshop_WhenModelIsInvalid_ShouldReturnBadRequestObjectResult()
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.Create(workshop)).ReturnsAsync(workshop);
        //    controller.ModelState.AddModelError("CreateWorkshop", "Invalid model state.");

        //    // Act
        //    var result = await controller.Create(workshop).ConfigureAwait(false) as BadRequestObjectResult;

        //    // Assert
        //    workshopServiceMoq.Verify(x => x.Create(workshop), Times.Never);
        //    Assert.That(result, Is.Not.Null);
        //    Assert.AreEqual(BadRequest, result.StatusCode);
        //}

        //[Test]
        //public async Task CreateWorkshop_WhenProviderHasNoRights_ShouldReturn403ObjectResult()
        //{
        //    // Arrange
        //    var notAuthorProvider = new ProviderDto() { Id = 2, UserId = userId };
        //    providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(notAuthorProvider);

        //    // Act
        //    var result = await controller.Create(workshop) as ObjectResult;

        //    // Assert
        //    workshopServiceMoq.Verify(x => x.Create(workshop), Times.Never);
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(Forbidden, result.StatusCode);
        //}
        //#endregion

        #region UpdateWorkshop
        [Test]
        public async Task UpdateWorkshop_WhenModelIsValid_ShouldReturnOkObjectResult()
        {
            // Arrange
            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);
            workshopServiceMoq.Setup(x => x.Update(workshop)).ReturnsAsync(workshop);

            // Act
            var result = await controller.Update(workshop).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(Ok, result.StatusCode);
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
            Assert.AreEqual(BadRequest, result.StatusCode);
        }

        //[Test]
        //public async Task UpdateWorkshop_WhenIdProviderHasNoRights_ShouldReturn403ObjectResult()
        //{
        //    // Arrange
        //    var notAuthorProvider = new ProviderDto() { Id = 2, UserId = userId };
        //    providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(notAuthorProvider);

        //    // Act
        //    var result = await controller.Update(workshop).ConfigureAwait(false) as ObjectResult;

        //    // Assert
        //    workshopServiceMoq.Verify(x => x.Update(It.IsAny<WorkshopDTO>()), Times.Never);
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(Forbidden, result.StatusCode);
        //}
        #endregion

        #region UpdateStatus
        [Test]
        public async Task UpdateStatus_WhenModelIsValid_ShouldReturnOkObjectResult()
        {
            // Arrange
            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);

            var updateRequest = WithWorkshopStatusDto(workshop.Id, WorkshopStatus.Open);

            workshopServiceMoq.Setup(x => x.GetById(updateRequest.WorkshopId))
                .ReturnsAsync(workshop);
            workshopServiceMoq.Setup(x => x.UpdateStatus(updateRequest))
                .ReturnsAsync(updateRequest);

            // Act
            var result = await controller.UpdateStatus(updateRequest).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(Ok, result.StatusCode);
        }

        [Test]
        public async Task UpdateStatus_WhenIdDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var expected = new NotFoundObjectResult($"There is no Workshop in DB with Id - {nonExistentId}");

            var updateRequest = WithWorkshopStatusDto(nonExistentId, WorkshopStatus.Open);

            workshopServiceMoq.Setup(x => x.GetById(updateRequest.WorkshopId))
                .ReturnsAsync(null as WorkshopDTO);

            // Act
            var result = await controller.UpdateStatus(updateRequest).ConfigureAwait(false) as NotFoundObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(expected.Value, result.Value);
        }

        [Test]
        public async Task UpdateStatus_WhenModelIsInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            var updateRequest = WithWorkshopStatusDto(workshop.Id, WorkshopStatus.Closed);

            workshop.ProviderOwnership = OwnershipType.Common;

            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);
            workshopServiceMoq.Setup(x => x.GetById(updateRequest.WorkshopId))
                .ReturnsAsync(workshop);
            workshopServiceMoq.Setup(x => x.UpdateStatus(updateRequest)).
                ThrowsAsync(new ArgumentException(It.IsAny<string>()));

            // Act
            var result = await controller.UpdateStatus(updateRequest).ConfigureAwait(false) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(BadRequest, result.StatusCode);
        }

        [Test]
        public async Task UpdateStatus_WhenProviderHasNoRights_ShouldReturn403ObjectResult()
        {
            // Arrange
            var workShopStatusDto = WithWorkshopStatusDto(workshop.Id, WorkshopStatus.Open);
            var notAuthorProvider = new ProviderDto() { Id = It.IsAny<Guid>(), UserId = userId };
            workshopServiceMoq.Setup(x => x.GetById(workShopStatusDto.WorkshopId))
                .ReturnsAsync(workshop);
            providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(notAuthorProvider);

            // Act
            var result = await controller.UpdateStatus(workShopStatusDto) as ObjectResult;

            // Assert
            workshopServiceMoq.Verify(x => x.Create(workshop), Times.Never);
            Assert.IsNotNull(result);
            Assert.AreEqual(Forbidden, result.StatusCode);
        }

        #endregion

        #region DeleteWorkshop
        //[Test]
        //[TestCase(1)]
        //public async Task DeleteWorkshop_WhenIdIsValid_ShouldReturnNoContentResult(long id)
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(workshop);
        //    providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider);
        //    workshopServiceMoq.Setup(x => x.Delete(id)).Returns(Task.CompletedTask);

        //    // Act
        //    var result = await controller.Delete(id) as NoContentResult;

        //    // Assert
        //    workshopServiceMoq.Verify(x => x.Delete(It.IsAny<long>()), Times.Once);
        //    Assert.That(result, Is.Not.Null);
        //    Assert.AreEqual(NoContent, result.StatusCode);
        //}

        //[Test]
        //[TestCase(0)]
        //public void DeleteWorkshop_WhenIdIsInvalid_ThrowArgumentOutOfRangeException(long id)
        //{
        //    // Assert
        //    Assert.That(
        //        async () => await controller.Delete(id),
        //        Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        //    workshopServiceMoq.Verify(x => x.Delete(It.IsAny<long>()), Times.Never);
        //}

        //[Test]
        //[TestCase(10)]
        //public async Task DeleteWorkshop_WhenThereIsNoWorkshopWithId_ShouldNoContentResult(long id)
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(() => null);
        //    providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(provider);
        //    workshopServiceMoq.Setup(x => x.Delete(id)).Returns(Task.CompletedTask);

        //    // Act
        //    var result = await controller.Delete(id) as NoContentResult;

        //    // Assert
        //    workshopServiceMoq.Verify(x => x.Delete(It.IsAny<long>()), Times.Never);
        //    Assert.That(result, Is.Not.Null);
        //    Assert.AreEqual(NoContent, result.StatusCode);
        //}

        //[Test]
        //[TestCase(1)]
        //public async Task DeleteWorkshop_WhenIdProviderHasNoRights_ShouldReturn403ObjectResult(long id)
        //{
        //    // Arrange
        //    workshopServiceMoq.Setup(x => x.GetById(id)).ReturnsAsync(workshop);
        //    var notAuthorProvider = new ProviderDto() { Id = 2, UserId = userId };
        //    providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>())).ReturnsAsync(notAuthorProvider);

        //    // Act
        //    var result = await controller.Delete(id) as ObjectResult;

        //    // Assert
        //    workshopServiceMoq.Verify(x => x.Delete(It.IsAny<long>()), Times.Never);
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(Forbidden, result.StatusCode);
        //}
        #endregion

        private WorkshopDTO WithWorkshop()
        {
            return new WorkshopDTO()
            {
                Id = Guid.NewGuid(),
                Title = "Title6",
                Phone = "1111111111",
                WorkshopDescriptionItems = new[]
                {
                    WithWorkshopDescriptionItem(),
                    WithWorkshopDescriptionItem(),
                },
                Price = 6000,
                WithDisabilityOptions = true,
                Status = WorkshopStatus.Open,
                ProviderTitle = "ProviderTitle",
                DisabilityOptionsDesc = "Desc6",
                Website = "website6",
                Instagram = "insta6",
                Facebook = "facebook6",
                Email = "email6@gmail.com",
                MaxAge = 10,
                MinAge = 4,
                CoverImageId = "image6",
                ProviderId = new Guid("5e519d63-0cdd-48a8-81da-6365aa5ad8c3"),
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
                        //Image = "Image",
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
                        //Image = "Image",
                        DateOfBirth = DateTime.Parse("1990-01-01"),
                        WorkshopId = new Guid("5e519d63-0cdd-48a8-81da-6365aa5ad8c3"),
                    },
                },
            };
        }

        private List<WorkshopDTO> WithWorkshops()
        {
            return new List<WorkshopDTO>()
            {
                new WorkshopDTO()
                {
                    Id = Guid.NewGuid(),
                    Title = "Title1",
                    Phone = "1111111111",
                    WorkshopDescriptionItems = new[]
                    {
                        WithWorkshopDescriptionItem(),
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
                    WorkshopDescriptionItems = new[]
                    {
                        WithWorkshopDescriptionItem(),
                        WithWorkshopDescriptionItem(),
                        WithWorkshopDescriptionItem(),
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
                    WorkshopDescriptionItems = new[]
                    {
                        WithWorkshopDescriptionItem(),
                        WithWorkshopDescriptionItem(),
                        WithWorkshopDescriptionItem(),
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
                    DirectionId = 1,
                    DepartmentId = 1,
                    ClassId = 1,
                },
                new WorkshopDTO()
                {
                    Id = Guid.NewGuid(),
                    Title = "Title4",
                    Phone = "1111111111",
                    WorkshopDescriptionItems = new[]
                    {
                        WithWorkshopDescriptionItem(),
                        WithWorkshopDescriptionItem(),
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
                    DirectionId = 1,
                    DepartmentId = 1,
                    ClassId = 1,
                },
                new WorkshopDTO()
                {
                    Id = Guid.NewGuid(),
                    Title = "Title5",
                    Phone = "1111111111",
                    WorkshopDescriptionItems = new[]
                    {
                        WithWorkshopDescriptionItem(),
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

        private ProviderDto WithProvider()
        {
            return new ProviderDto()
            {
                UserId = userId,
                Id = new Guid("5e519d63-0cdd-48a8-81da-6365aa5ad8c3"),
            };
        }

        private List<WorkshopCard> WithWorkshopCards()
        {
            var list = WithWorkshops();
            var eSlist = new List<WorkshopCard>();
            foreach (var item in list)
            {
                eSlist.Add(item.ToESModel().ToCardDto());
            }

            return eSlist;
        }

        private WorkshopDescriptionItemDto WithWorkshopDescriptionItem()
        {
            var id = Guid.NewGuid();
            return new WorkshopDescriptionItemDto
            {
                Id = id,
                SectionName = "test heading",
                Description = $"test description text sentence for id: {id.ToString()}",
            };
        }

        private WorkshopStatusDto WithWorkshopStatusDto(Guid workshopDtoId, WorkshopStatus workshopStatus)
        {
            return new WorkshopStatusDto()
            {
                WorkshopId = workshopDtoId,
                Status = workshopStatus,
            };
        }
    }
}