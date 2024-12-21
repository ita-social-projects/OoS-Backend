using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.WebApi.Controllers.V1;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using System.Threading.Tasks;
using System;
using OutOfSchool.BusinessLogic.Models.Position;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Enums;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using OutOfSchool.BusinessLogic.Models.Providers;

namespace OutOfSchool.WebApi.Tests.Controllers;
[TestFixture]
public class PositionControllerTests
{
    private PositionController controller;
    private Mock<IPositionService> positionService;
    private Mock<ICurrentUserService> currentUserService;
    private Mock<IProviderService> providerService;
    private Mock<IUserService> userService;

    private PositionDto positionDto;
    private PositionCreateDto positionCreateDto;
    private PositionUpdateDto positionUpdateDto;

    private Guid providerId;

    [SetUp]
    public void SetUp()
    {
        positionService = new Mock<IPositionService>();
        currentUserService = new Mock<ICurrentUserService>();
        providerService = new Mock<IProviderService>();
        userService = new Mock<IUserService>();

        controller = new PositionController(
            positionService.Object,
            currentUserService.Object,
            userService.Object,
            providerService.Object
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
        var result = await controller.Create(null).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<BadRequestResult>(result.Result);
    }

    [Test]
    public async Task CreatePosition_WithValidPositionCreateDto_ShouldReturnCreatedAtAction()
    {
        // Arrange                        
        // current service should return the id we created
        currentUserService.Setup(s => s.UserId).Returns(providerId.ToString());
        currentUserService.Setup(s => s.IsInRole(Role.Provider)).Returns(true);

        // provider service should return the provider with id we created
        providerService.Setup(s => s.GetById(It.IsAny<Guid>()))
        .ReturnsAsync(new ProviderDto{ Id = providerId });

        positionService.Setup(s => s.CreateAsync(positionCreateDto, providerId))
       .ReturnsAsync(positionDto);

        // Act
        var result = await controller.Create(positionCreateDto);

        // Assert
        Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
    }

    [Test]
    public async Task GetPositionOfCurrentProvider_WithValidInput_ReturnsPositionList()
    {
        // Arrange
        currentUserService.Setup(s => s.UserId).Returns(providerId.ToString());
        currentUserService.Setup(s => s.IsInRole(Role.Provider)).Returns(true);

        // Act
        var result = await controller.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Controller_ShouldHaveAuthorizeAttribute()
    {
        // Arrange
        var controllerType = typeof(PositionController);

        // Act
        var authorizeAttribute = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true);

        // Assert
        Assert.IsNotNull(authorizeAttribute);
        Assert.IsTrue(authorizeAttribute.Any());
    }

    [Test]
    public async Task UpdatePosition_WithValidInput_ShouldReturnUpdatedPosition()
    {
        // Arrange        
        var positionId = positionDto.Id;
        currentUserService.Setup(s => s.UserId).Returns(providerId.ToString());
        currentUserService.Setup(s => s.IsInRole(Role.Provider)).Returns(true);

        providerService.Setup(s => s.GetById(It.IsAny<Guid>()))
                .ReturnsAsync(new ProviderDto { Id = providerId });

        positionService.Setup(s => s.UpdateAsync(positionId, positionUpdateDto, providerId))
            .ReturnsAsync(positionDto);        

        // Act
        var result = await controller.Update(positionId, positionUpdateDto);

        // Assert
        Assert.IsInstanceOf<ActionResult<PositionDto>>(result); // Ensure it's ActionResult<PositionDto>
        var okResult = result.Result as OkObjectResult;         // Extract the OkObjectResult
        Assert.IsNotNull(okResult);                             // Ensure the result is not null
        Assert.AreEqual(200, okResult.StatusCode);              // Check that the status code is 200 (OK)
        Assert.IsInstanceOf<PositionDto>(okResult.Value);       // Ensure the returned value is PositionDto
        var updatedPosition = okResult.Value as PositionDto;
        Assert.IsNotNull(updatedPosition);                      // Ensure the updatedPosition is not null
        Assert.AreEqual(positionDto.Id, updatedPosition.Id);    // Validate the position ID
        Assert.AreEqual(positionDto.FullName, updatedPosition.FullName);
    }

    private PositionUpdateDto FakePositionUpdateDto(Guid providerId, PositionDto oldPosition)
    {        
        return new PositionUpdateDto
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

    private PositionDto FakePositionDto(Guid providerId, PositionCreateDto positionCreateDto)
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

    private PositionCreateDto FakePositionCreateDto()
    {
        return new PositionCreateDto()
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

    private IEnumerable<PositionDto> FakePositions()
    {        
        return new List<PositionDto>() { positionDto, positionDto, positionDto };
    }
}
