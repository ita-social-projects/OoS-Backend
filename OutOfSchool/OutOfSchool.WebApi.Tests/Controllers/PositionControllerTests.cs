using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.WebApi.Controllers.V1;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using System.Threading.Tasks;
using System;
using OutOfSchool.BusinessLogic.Models.Position;
using System.Collections.Generic;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.WebApi.Tests.Controllers;
[TestFixture]
public class PositionControllerTests
{
    private PositionController controller;
    private Mock<IPositionService> positionService;
    private Mock<ICurrentUserService> currentUserService;
    private Mock<IProviderService> providerService;     

    private PositionDto positionDto;
    private PositionCreateUpdateDto positionCreateDto;
    private PositionCreateUpdateDto positionUpdateDto;

    private Guid providerId;

    [SetUp]
    public void SetUp()
    {
        positionService = new Mock<IPositionService>();
        currentUserService = new Mock<ICurrentUserService>();
        providerService = new Mock<IProviderService>();        

        controller = new PositionController(
            positionService.Object   
        );
        
        providerId = Guid.NewGuid();
        positionCreateDto = FakePositionCreateDto();
        positionDto = FakePositionDto(providerId, positionCreateDto);
        positionUpdateDto = FakePositionUpdateDto(providerId, positionDto);       
    }

    [Test]
    public async Task CreatePosition_WhenPositionCreateDtoIsNull_ShouldReturnBadRequest()
    {
        // Act
        var result = await controller.Create(providerId, null).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<BadRequestResult>(result);
    }

    [Test]
    public async Task Create_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        PositionCreateUpdateDto createDto = positionCreateDto;
        createDto.FullName = null;
        controller.ModelState.AddModelError("FullName", "FullName is required");

        // Act
        var result = await controller.Create(providerId, createDto).ConfigureAwait(false);
        
        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>(), "Result should be a BadRequestObjectResult.");
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null, "BadRequestObjectResult should not be null.");

        // Assert that the error message in the ModelState matches expectations
        var modelState = badRequestResult.Value as SerializableError;
        Assert.That(modelState, Is.Not.Null, "ModelState should be serializable.");

        // Check the error message for "FullName"
        Assert.That(modelState.ContainsKey("FullName"), "ModelState should contain an error for 'FullName'.");
        var fullNameErrors = modelState["FullName"] as string[];
        Assert.That(fullNameErrors, Is.Not.Null, "Errors for 'FullName' should not be null.");
        Assert.That(fullNameErrors[0], Is.EqualTo("FullName is required"), "Error message should match expected value.");
    }

    [Test]
    public async Task CreatePosition_WithValidPositionCreateDto_ShouldReturnCreatedAtAction()
    {
        // Arrange                                
        currentUserService.Setup(s => s.UserId).Returns(providerId.ToString());        
        
        providerService.Setup(s => s.GetById(It.IsAny<Guid>()))
        .ReturnsAsync(new ProviderDto{ Id = providerId });

        positionService.Setup(s => s.CreateAsync(positionCreateDto, providerId))
       .ReturnsAsync(positionDto);

        // Act
        var result = await controller.Create(providerId, positionCreateDto);

        // Assert
        Assert.IsInstanceOf<CreatedAtActionResult>(result);
    }

    [Test]
    public async Task GetPosition_WithValidInput_ReturnsPositionList()
    {
        // Arrange
        currentUserService.Setup(s => s.UserId).Returns(providerId.ToString());        
        positionService.Setup(a => a.GetByFilter(providerId, It.IsAny<PositionsFilter>())).ReturnsAsync(SearchResult());

        // Act
        var result = await controller.GetByFilter(providerId, It.IsAny<PositionsFilter>()).ConfigureAwait(false) as OkObjectResult; ;
        var resultValue = result.Value as SearchResult<PositionDto>;
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(10, resultValue.TotalAmount);
        Assert.AreEqual(6, resultValue.Entities.Count);
    }

    [Test]
    public async Task GetPositionByFilter_WithWrongSearchString_ShouldReturnMessage()
    {
        // Arrange        
        string searchString = "hello";
        currentUserService.Setup(s => s.UserId).Returns(providerId.ToString());
        PositionsFilter filter = new PositionsFilter();
        filter.SearchString = searchString;
        
        positionService
        .Setup(a => a.GetByFilter(providerId, filter))
        .ReturnsAsync(new SearchResult<PositionDto>
        {
            TotalAmount = 0,
            Entities = new List<PositionDto>()
        });

        // Act
        var result = await controller.GetByFilter(providerId, filter);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>(), "Result should be OkObjectResult.");
        var okResult = result as OkObjectResult;

        Assert.That(okResult.Value, Is.EqualTo("There is no records for given provider"));
    }

    [Test]
    public async Task GetPositionsByFilter_WhenNoRecords_ReturnsMessage()
    {
        // Arrange
        Guid providerWithNoRecordId = new Guid();
        currentUserService.Setup(s => s.UserId).Returns(providerWithNoRecordId.ToString());        
        
        positionService.Setup(a => a.GetByFilter(providerWithNoRecordId, It.IsAny<PositionsFilter>()))
            .ReturnsAsync(new SearchResult<PositionDto> { TotalAmount = 0, Entities = new List<PositionDto>() });

        // Act
        var result = await controller.GetByFilter(providerWithNoRecordId, It.IsAny<PositionsFilter>()).ConfigureAwait(false) as OkObjectResult; ;
        var resultValue = result.Value as SearchResult<PositionDto>;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual("There is no records for given provider", result.Value);
    }

    [Test]
    public async Task GetPositionById_WithDeletedPositionId_ReturnsMessage()
    {  
        // Arrange        
        var deletedPosition = positionDto;
        deletedPosition.IsDeleted = true;

        currentUserService.Setup(s => s.UserId).Returns(providerId.ToString());
        positionService.Setup(a => a.GetByIdAsync(deletedPosition.Id, providerId))
            .ThrowsAsync(new KeyNotFoundException($"Position with positionId {deletedPosition.Id} not found or it was deleted."));;

        // Act
        var result = await controller.GetById(providerId, deletedPosition.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.AreEqual($"Position with positionId {deletedPosition.Id} not found or it was deleted.", notFoundResult.Value);
    }
    
    [Test]
    public async Task UpdatePosition_WithValidInput_ShouldReturnUpdatedPosition()
    {
        // Arrange        
        var positionId = positionDto.Id;
        currentUserService.Setup(s => s.UserId).Returns(providerId.ToString());        

        providerService.Setup(s => s.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(new ProviderDto { Id = providerId });

        positionService.Setup(s => s.UpdateAsync(positionId, positionUpdateDto, providerId))
            .ReturnsAsync(positionDto);        

        // Act
        var result = await controller.Update(positionUpdateDto, providerId, positionId);

        // Assert
        Assert.IsInstanceOf<IActionResult>(result);             // Ensure it's IActionResult
        var okResult = result as OkObjectResult;                // Extract the OkObjectResult
        Assert.IsNotNull(okResult);                             // Ensure the result is not null
        Assert.AreEqual(200, okResult.StatusCode);              // Check that the status code is 200 (OK)
        Assert.IsInstanceOf<PositionDto>(okResult.Value);       // Ensure the returned value is PositionDto
        var updatedPosition = okResult.Value as PositionDto;
        Assert.IsNotNull(updatedPosition);                      // Ensure the updatedPosition is not null
        Assert.AreEqual(positionDto.Id, updatedPosition.Id);    // Validate the position ID
        Assert.AreEqual(positionDto.FullName, updatedPosition.FullName);
    }

    [Test]
    public async Task DeletePosition_WhichDeleted_ShouldReturnBadRequest()
    {
        // Arrange
        currentUserService.Setup(s => s.UserId).Returns(providerId.ToString());

        var deletedPosition = positionDto;
        deletedPosition.IsDeleted = true;

        positionService
            .Setup(s => s.DeleteAsync(deletedPosition.Id, providerId))
            .ThrowsAsync(new KeyNotFoundException($"Position with ID {deletedPosition.Id} not found or it was deleted."));

        // Act
        var result = await controller.Delete(providerId, deletedPosition.Id);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult, "Expected NotFoundObjectResult.");
        Assert.AreEqual($"Position with ID {deletedPosition.Id} not found or it was deleted.", notFoundResult.Value);
    }

    [Test]
    public async Task DeletePosition_WithValidInput_ShoudReturnNoContent()
    {        
        // Act 
        var result = await controller.Delete(positionDto.Id, providerId);
        
        // Act
        Assert.IsInstanceOf<NoContentResult>(result); // Verify the result type
    }

    private static SearchResult<PositionDto> SearchResult()
    {
        return new SearchResult<PositionDto>
        {
            TotalAmount = 10,
            Entities = new List<PositionDto>()
            {
                new PositionDto(),
                new PositionDto(),
                new PositionDto(),
                new PositionDto(),
                new PositionDto(),
                new PositionDto(),
            },
        };
    }

    private PositionCreateUpdateDto FakePositionUpdateDto(Guid providerId, PositionDto oldPosition)
    {        
        return new PositionCreateUpdateDto
        {             
            FullName = "Hello",
            Language = oldPosition.Language,
            Description = oldPosition.Description,
            Department = oldPosition.Department,
            SeatsAmount = oldPosition.SeatsAmount,
            GenitiveName = oldPosition.GenitiveName,
            IsTeachingPosition = oldPosition.IsTeachingPosition,
            Rate = oldPosition.Rate,
            Tariff = oldPosition.Tariff,
            ClassifierType = oldPosition.ClassifierType,
            IsForRuralAreas = oldPosition.IsForRuralAreas            
        };
    }

    private PositionDto FakePositionDto(Guid providerId, PositionCreateUpdateDto positionCreateDto)
    {
        return new PositionDto()
        {
            Id = Guid.NewGuid(),
            FullName = positionCreateDto.FullName,
            Language = positionCreateDto.Language,
            Description = positionCreateDto.Description,
            Department = positionCreateDto.Department,
            SeatsAmount = positionCreateDto.SeatsAmount,
            GenitiveName = positionCreateDto.GenitiveName,
            IsTeachingPosition = positionCreateDto.IsTeachingPosition,
            Rate = positionCreateDto.Rate,
            Tariff = positionCreateDto.Tariff,
            ClassifierType = positionCreateDto.ClassifierType,
            IsForRuralAreas = positionCreateDto.IsForRuralAreas,
            ProviderId = providerId,
            ContactId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };
    }

    private PositionCreateUpdateDto FakePositionCreateDto()
    {
        return new PositionCreateUpdateDto()
        {
            Language = "AnyLanguage",
            Description = "Description",
            Department = "Department",
            SeatsAmount = 1,
            FullName = "Name",
            ShortName = "Name",
            GenitiveName = "Name",
            IsTeachingPosition = true,
            IsForRuralAreas = true,
            Rate = 2.0f,
            Tariff = 2.0f,
            ClassifierType = "type",            
        };
    }    
}