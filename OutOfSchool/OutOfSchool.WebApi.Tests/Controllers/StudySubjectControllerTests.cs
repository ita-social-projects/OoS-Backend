using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.WebApi.Controllers.V1;
using Moq;
using System.Threading.Tasks;
using OutOfSchool.BusinessLogic.Models.StudySubjects;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Tests.Common;
using OutOfSchool.Services.Enums;
using OutOfSchool.BusinessLogic.Models.Providers;

namespace OutOfSchool.WebApi.Tests.Controllers;
[TestFixture]
public class StudySubjectControllerTests
{
    private StudySubjectController controller;
    private Mock<IStudySubjectService> studySubjectService;
    private Mock<IProviderService> providerService;
    private Mock<IWorkshopService> workshopService;
    private Mock<IEmployeeService> employeeService;

    [SetUp]
    public void SetUp()
    {
        studySubjectService = new Mock<IStudySubjectService>();
        providerService = new Mock<IProviderService>();
        workshopService = new Mock<IWorkshopService>();
        employeeService = new Mock<IEmployeeService>();

        controller = new StudySubjectController(studySubjectService.Object, providerService.Object,
            workshopService.Object, employeeService.Object);
    }

    #region Get

    [Test]
    public async Task Get_ReturnsNoContent_WhenListIsEmpty()
    {
        // Arrange
        var emptyList = new List<StudySubjectDto>();
        studySubjectService.Setup(s => s.GetByFilter(null)).ReturnsAsync(emptyList);

        // Act
        var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public async Task Get_ReturnsOk_WhenListIsNotEmpty()
    {
        // Arrange
        var list = new List<StudySubjectDto>()
        {
            FakeStudySubjectDto(),
            FakeStudySubjectDto(),
            FakeStudySubjectDto()
        };
        studySubjectService.Setup(s => s.GetByFilter(null)).ReturnsAsync(list);

        // Act
        var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));

        var returnedList = result.Value as IEnumerable<StudySubjectDto>;
        Assert.That(returnedList, Is.Not.Null);
        Assert.That(returnedList, Is.EqualTo(list));
    }

    #endregion

    #region GetById

    [Test]
    public async Task GetById_ReturnsOk_WhenValid()
    {
        // Arrange
        var dto = FakeStudySubjectDto();
        studySubjectService.Setup(s => s.GetById(dto.Id)).ReturnsAsync(dto);

        // Act
        var result = await controller.GetById(dto.Id).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));

        var returnedValue = result.Value as StudySubjectDto;
        Assert.That(returnedValue, Is.Not.Null);
        Assert.That(returnedValue, Is.EqualTo(dto));
    }

    [Test]
    public async Task GetById_ReturnsBadRequest_WhenNotValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        studySubjectService.Setup(s => s.GetById(id)).ThrowsAsync(new ArgumentException());

        // Act
        var result = await controller.GetById(id).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    #endregion

    #region Create

    [Test]
    public async Task Create_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Arrange
        StudySubjectCreateUpdateDto dto = null;

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_ReturnsNotFound_WhenWorkshopIsNotValid()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(false);

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as NotFoundObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task Create_ReturnsForbidden_WhenProviderIsBlocked()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var providerId = Guid.NewGuid();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(true);

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Create_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var dto = FakeInvalidSubjectCreateUpdateDto();
        var providerId = Guid.NewGuid();

        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(dto, validationContext, validationResults, true);

        foreach (var validationResult in validationResults)
        {
            controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
        }

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_ReturnsForbidden_WhenUserDontHaveRights()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var providerId = Guid.NewGuid();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Parent);

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Create_ReturnsCreatedAtAction_WhenUserHaveRights()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Provider, userId);

        workshopService.Setup(s => s.GetWorkshopProviderOwnerIdAsync(dto.WorkshopId)).ReturnsAsync(providerId);
        employeeService.Setup(s => s.CheckUserIsRelatedEmployee(userId, providerId, dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetByUserId(userId, false)).ReturnsAsync(new ProviderDto { Id = providerId });
        studySubjectService.Setup(s => s.Create(dto)).ReturnsAsync(dto);

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(201));

        var returnedValue = result.Value as StudySubjectCreateUpdateDto;
        Assert.That(returnedValue, Is.Not.Null);
        Assert.That(returnedValue, Is.EqualTo(dto));
    }

    [Test]
    public async Task Create_ReturnsBadRequest_WhenArgumentExceptionWasCatched()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Provider, userId);

        workshopService.Setup(s => s.GetWorkshopProviderOwnerIdAsync(dto.WorkshopId)).ReturnsAsync(providerId);
        employeeService.Setup(s => s.CheckUserIsRelatedEmployee(userId, providerId, dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetByUserId(userId, false)).ReturnsAsync(new ProviderDto { Id = providerId });
        studySubjectService.Setup(s => s.Create(dto)).ThrowsAsync(new ArgumentException());

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Arrange
        StudySubjectCreateUpdateDto dto = null;

        // Act
        var result = await controller.Update(dto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Update_ReturnsNotFound_WhenWorkshopIsNotValid()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(false);

        // Act
        var result = await controller.Update(dto).ConfigureAwait(false) as NotFoundObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task Update_ReturnsForbidden_WhenProviderIsBlocked()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var providerId = Guid.NewGuid();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(true);

        // Act
        var result = await controller.Update(dto).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var dto = FakeInvalidSubjectCreateUpdateDto();
        var providerId = Guid.NewGuid();

        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(dto, validationContext, validationResults, true);

        foreach (var validationResult in validationResults)
        {
            controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
        }

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        // Act
        var result = await controller.Update(dto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Update_ReturnsForbidden_WhenUserDontHaveRights()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var providerId = Guid.NewGuid();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Parent);

        // Act
        var result = await controller.Update(dto).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Update_ReturnsOk_WhenUserHaveRights()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Provider, userId);

        workshopService.Setup(s => s.GetWorkshopProviderOwnerIdAsync(dto.WorkshopId)).ReturnsAsync(providerId);
        employeeService.Setup(s => s.CheckUserIsRelatedEmployee(userId, providerId, dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetByUserId(userId, false)).ReturnsAsync(new ProviderDto { Id = providerId });
        studySubjectService.Setup(s => s.Update(dto)).ReturnsAsync(dto);

        // Act
        var result = await controller.Update(dto).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_ReturnsBadRequest_WhenArgumentExceptionWasCatchedWhileGettingEntityById()
    {
        // Arrange
        var id = Guid.NewGuid();
        studySubjectService.Setup(s => s.GetById(id)).ThrowsAsync(new ArgumentException());

        // Act
        var result = await controller.Delete(id).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Delete_ReturnsForbidden_WhenProviderIsBlocked()
    {
        // Arrange
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var dto = FakeStudySubjectDto();

        studySubjectService.Setup(s => s.GetById(id)).ReturnsAsync(dto);
        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(true);

        // Act
        var result = await controller.Delete(id).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Delete_ReturnsForbidden_WhenUserDontHaveRights()
    {
        // Arrange
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var dto = FakeStudySubjectDto();

        studySubjectService.Setup(s => s.GetById(id)).ReturnsAsync(dto);
        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Parent);

        // Act
        var result = await controller.Delete(id).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Delete_ReturnsNoContent_WhenEntityIsDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var dto = FakeStudySubjectDto();

        studySubjectService.Setup(s => s.GetById(id)).ReturnsAsync(dto);
        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Provider, userId);

        workshopService.Setup(s => s.GetWorkshopProviderOwnerIdAsync(dto.WorkshopId)).ReturnsAsync(providerId);
        employeeService.Setup(s => s.CheckUserIsRelatedEmployee(userId, providerId, dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetByUserId(userId, false)).ReturnsAsync(new ProviderDto { Id = providerId });
        studySubjectService.Setup(s => s.Delete(id));

        // Act
        var result = await controller.Delete(id).ConfigureAwait(false) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public async Task Delete_ReturnsBadRequest_WhenArgumentExceptionWasCatchedWhileDeletingEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var dto = FakeStudySubjectDto();

        studySubjectService.Setup(s => s.GetById(id)).ReturnsAsync(dto);
        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Provider, userId);

        workshopService.Setup(s => s.GetWorkshopProviderOwnerIdAsync(dto.WorkshopId)).ReturnsAsync(providerId);
        employeeService.Setup(s => s.CheckUserIsRelatedEmployee(userId, providerId, dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetByUserId(userId, false)).ReturnsAsync(new ProviderDto { Id = providerId });
        studySubjectService.Setup(s => s.Delete(id)).ThrowsAsync(new ArgumentException());

        // Act
        var result = await controller.Delete(id).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    #endregion

    private StudySubjectDto FakeStudySubjectDto()
    {
        return new StudySubjectDto()
        {
            ActiveFrom = DateOnly.FromDateTime(DateTime.Now),
            ActiveTo = DateOnly.FromDateTime(DateTime.Now),
            CreatedAt = DateTime.Now,
            Id = Guid.NewGuid(),
            IsPrimaryLanguageUkrainian = true,
            LanguageIds = new List<long> { 1, 2 },
            NameInInstructionLanguage = "тест",
            NameInUkrainian = "тест",
            PrimaryLanguageId = 1,
            UpdatedAt = DateTime.Now,
            WorkshopId = Guid.NewGuid()
        };
    }

    private StudySubjectCreateUpdateDto FakeStudySubjectCreateUpdateDto()
    {
        return new StudySubjectCreateUpdateDto()
        {
            Id = Guid.NewGuid(),
            IsPrimaryLanguageUkrainian = true,
            Languages = new List<long> { 1, 2 },
            NameInInstructionLanguage = "тест",
            NameInUkrainian = "тест",
            PrimaryLanguageId = 1,
            WorkshopId = Guid.NewGuid()
        };
    }

    private StudySubjectCreateUpdateDto FakeInvalidSubjectCreateUpdateDto()
    {
        return new StudySubjectCreateUpdateDto()
        {
            Id = Guid.NewGuid(),
            IsPrimaryLanguageUkrainian = true,
            Languages = new List<long> { 1, 2 },
            NameInInstructionLanguage = null,
            NameInUkrainian = null,
            PrimaryLanguageId = 1,
            WorkshopId = Guid.NewGuid()
        };
    }
}
