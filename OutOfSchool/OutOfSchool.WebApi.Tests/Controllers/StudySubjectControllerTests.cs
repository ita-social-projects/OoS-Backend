using NUnit.Framework;
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
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Common;

namespace OutOfSchool.WebApi.Tests.Controllers;
[TestFixture]
public class StudySubjectControllerTests
{
    private StudySubjectController controller;
    private Guid providerId;
    private Mock<IStudySubjectService> studySubjectService;

    [SetUp]
    public void SetUp()
    {
        studySubjectService = new Mock<IStudySubjectService>();
        providerId = Guid.NewGuid();

        controller = new StudySubjectController(studySubjectService.Object);
    }

    #region Get

    [Test]
    public async Task Get_ReturnsNoContent_WhenListIsEmpty()
    {
        // Arrange
        var emptySearchResult = new SearchResult<StudySubjectDto>();
        studySubjectService.Setup(s => s.GetByFilter(providerId ,null)).ReturnsAsync(emptySearchResult);

        // Act
        var result = await controller.Get(providerId).ConfigureAwait(false) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public async Task Get_ReturnsOk_WhenListIsNotEmpty()
    {
        // Arrange
        var searchResult = new SearchResult<StudySubjectDto>()
        {
            Entities = new List<StudySubjectDto>
            {
                FakeStudySubjectDto(),
                FakeStudySubjectDto(),
                FakeStudySubjectDto()
            },
            TotalAmount = 3
        };
        studySubjectService.Setup(s => s.GetByFilter(providerId, null)).ReturnsAsync(searchResult);

        // Act
        var result = await controller.Get(providerId).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));

        var returnedList = result.Value as SearchResult<StudySubjectDto>;
        Assert.That(returnedList, Is.Not.Null);
    }

    #endregion
    
    #region GetById

    [Test]
    public async Task GetById_ReturnsOk_WhenValid()
    {
        // Arrange
        var dto = FakeStudySubjectDto();
        studySubjectService.Setup(s => s.GetById(dto.Id, providerId)).ReturnsAsync(dto);

        // Act
        var result = await controller.GetById(dto.Id, providerId).ConfigureAwait(false) as OkObjectResult;

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
        StudySubjectDto dto = null;
        studySubjectService.Setup(s => s.GetById(id, providerId)).ReturnsAsync(dto);

        // Act
        var result = await controller.GetById(id, providerId).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

    #endregion
    
    #region Create

    [Test]
    public async Task Create_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Arrange
        StudySubjectCreateUpdateDto dto = null;

        // Act
        var result = await controller.Create(dto, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var dto = FakeInvalidSubjectCreateUpdateDto();

        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(dto, validationContext, validationResults, true);

        foreach (var validationResult in validationResults)
        {
            controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
        }

        // Act
        var result = await controller.Create(dto, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_ReturnsCreatedAtAction_WhenUserHaveRights()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var dtoToReturn = FakeStudySubjectDto();

        studySubjectService.Setup(s => s.Create(dto, providerId)).ReturnsAsync(dtoToReturn);

        // Act
        var result = await controller.Create(dto, providerId).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(201));

        var returnedValue = result.Value as StudySubjectDto;
        Assert.That(returnedValue, Is.Not.Null);
    }
    
    [Test]
    public async Task Create_ReturnsBadRequest_WhenArgumentExceptionWasCatched()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();

        studySubjectService.Setup(s => s.Create(dto, providerId)).ThrowsAsync(new ArgumentException());

        // Act
        var result = await controller.Create(dto, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_ReturnsForbidden_WhenUnauthorizedAccessExceptionWasCatched()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();

        studySubjectService.Setup(s => s.Create(dto, providerId)).ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await controller.Create(dto, providerId).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Create_ReturnsBadRequest_WhenExceptionWasCatched()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();

        studySubjectService.Setup(s => s.Create(dto, providerId)).ThrowsAsync(new Exception());

        // Act
        var result = await controller.Create(dto, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void Create_ReturnsValidationError_WhenLanguageIdIsLessThanOrEqualToZero()
    {
        // Arrange
        var dto = new StudySubjectCreateUpdateDto()
        {
            Id = Guid.NewGuid(),
            NameInUkrainian = "тест",
            NameInInstructionLanguage = "тест",
            IsLanguageUkrainian = true,
            Language = new LanguageDto { Id = 0, Code = "Ua", Name = "Українська" }
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(dto);
        bool isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

        // Assert
        Assert.IsFalse(isValid);
        Assert.AreEqual(1, validationResults.Count);
        Assert.AreEqual("Language ID must be greater than 0.", validationResults[0].ErrorMessage);
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Arrange
        StudySubjectCreateUpdateDto dto = null;

        // Act
        var result = await controller.Update(dto, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var dto = FakeInvalidSubjectCreateUpdateDto();

        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(dto, validationContext, validationResults, true);

        foreach (var validationResult in validationResults)
        {
            controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
        }

        // Act
        var result = await controller.Update(dto, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Update_ReturnsOk_WhenEntityUpdated()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var resultToReturn = Result<StudySubjectDto>.Success(FakeStudySubjectDto());

        studySubjectService.Setup(s => s.Update(dto, providerId)).ReturnsAsync(resultToReturn);

        // Act
        var result = await controller.Update(dto, providerId).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Update_ReturnsNotFound_WhenEntityToUpdateNotFound()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var resultToReturn = Result<StudySubjectDto>.Failed(new OperationError
        {
            Code = "404",
            Description = $"There are no recors in StudySubjects table with such id"
        });

        studySubjectService.Setup(s => s.Update(dto, providerId)).ReturnsAsync(resultToReturn);

        // Act
        var result = await controller.Update(dto, providerId).ConfigureAwait(false) as NotFoundObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
        
    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenUpdatingFailed()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();
        var resultToReturn = Result<StudySubjectDto>.Failed(new OperationError
        {
            Code = "400",
            Description = $"Updating failed."
        });

        studySubjectService.Setup(s => s.Update(dto, providerId)).ReturnsAsync(resultToReturn);

        // Act
        var result = await controller.Update(dto, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));

    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenArgumentExceptionWasCatched()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();

        studySubjectService.Setup(s => s.Update(dto, providerId)).ThrowsAsync(new ArgumentException());

        // Act
        var result = await controller.Update(dto, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Update_ReturnsForbidden_WhenUnauthorizedAccessExceptionWasCatched()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();

        studySubjectService.Setup(s => s.Update(dto, providerId)).ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await controller.Update(dto, providerId).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenExceptionWasCatched()
    {
        // Arrange
        var dto = FakeStudySubjectCreateUpdateDto();

        studySubjectService.Setup(s => s.Update(dto, providerId)).ThrowsAsync(new Exception());

        // Act
        var result = await controller.Update(dto, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_ReturnsNotFound_WhenEntityNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        StudySubjectDto dto = null;
        studySubjectService.Setup(s => s.GetById(id, providerId)).ReturnsAsync(dto);

        // Act
        var result = await controller.Delete(id, providerId).ConfigureAwait(false) as NotFoundObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }
    

    [Test]
    public async Task Delete_ReturnsOk_WhenEntityIsDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = FakeStudySubjectDto();
        var resultToReturn = Result<StudySubjectDto>.Success(dto);

        studySubjectService.Setup(s => s.GetById(id, providerId)).ReturnsAsync(dto);
        studySubjectService.Setup(s => s.Delete(id, providerId)).ReturnsAsync(resultToReturn);

        // Act
        var result = await controller.Delete(id, providerId).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Delete_ReturnsNotFound_WhenEntityToDeleteNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = FakeStudySubjectDto();
        var resultToReturn = Result<StudySubjectDto>.Failed(new OperationError
        {
            Code = "404",
            Description = $"There are no recors in StudySubjects table with such id"
        });

        studySubjectService.Setup(s => s.GetById(id, providerId)).ReturnsAsync(dto);
        studySubjectService.Setup(s => s.Delete(id, providerId)).ReturnsAsync(resultToReturn);

        // Act
        var result = await controller.Delete(id, providerId).ConfigureAwait(false) as NotFoundObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));

    }

    [Test]
    public async Task Delete_ReturnsBadRequest_WhenDeletingFailed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = FakeStudySubjectDto();
        var resultToReturn = Result<StudySubjectDto>.Failed(new OperationError
        {
            Code = "400",
            Description = $"Deleting failed."
        });

        studySubjectService.Setup(s => s.GetById(id, providerId)).ReturnsAsync(dto);
        studySubjectService.Setup(s => s.Delete(id, providerId)).ReturnsAsync(resultToReturn);

        // Act
        var result = await controller.Delete(id, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));

    }

    [Test]
    public async Task Delete_ReturnsBadRequest_WhenArgumentExceptionWasCatched()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = FakeStudySubjectCreateUpdateDto();

        studySubjectService.Setup(s => s.GetById(id, providerId)).ThrowsAsync(new ArgumentException());

        // Act
        var result = await controller.Delete(id, providerId).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Delete_ReturnsForbidden_WhenUnauthorizedAccessExceptionWasCatched()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = FakeStudySubjectCreateUpdateDto();

        studySubjectService.Setup(s => s.GetById(id, providerId)).ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await controller.Delete(id, providerId).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Delete_ReturnsBadRequest_WhenExceptionWasCatched()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = FakeStudySubjectCreateUpdateDto();

        studySubjectService.Setup(s => s.GetById(id, providerId)).ThrowsAsync(new Exception());

        // Act
        var result = await controller.Delete(id, providerId).ConfigureAwait(false) as BadRequestObjectResult;

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
            Id = Guid.NewGuid(),
            IsLanguageUkrainian = true,
            Language = new LanguageDto()
            {
                Id = 2,
                Name = "English",
                Code = "en"
            },
            NameInInstructionLanguage = "тест",
            NameInUkrainian = "тест",
            LanguageId = 1,
            WorkshopId = Guid.NewGuid()
        };
    }

    private StudySubjectCreateUpdateDto FakeStudySubjectCreateUpdateDto()
    {
        return new StudySubjectCreateUpdateDto()
        {
            Id = Guid.NewGuid(),
            IsLanguageUkrainian = true,
            Language = new LanguageDto()
            {
                Id = 2,
                Name = "English",
                Code = "en"
            },
            NameInInstructionLanguage = "тест",
            NameInUkrainian = "тест"
        };
    }

    private StudySubjectCreateUpdateDto FakeInvalidSubjectCreateUpdateDto()
    {
        return new StudySubjectCreateUpdateDto()
        {
            Id = Guid.NewGuid(),
            IsLanguageUkrainian = true,
            Language = new LanguageDto()
            {
                Id = 2,
                Name = "English",
                Code = "en"
            },
            NameInInstructionLanguage = null,
            NameInUkrainian = null
        };
    }
}