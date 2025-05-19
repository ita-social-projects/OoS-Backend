using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Position;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class PositionServiceTests
{
    private Mock<IPositionRepository> _mockRepository;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private Mock<ILogger<PositionService>> _mockLogger;
    private PositionService _service;

    private readonly Guid providerId = Guid.NewGuid();
    private readonly Guid positionId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IPositionRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockLogger = new Mock<ILogger<PositionService>>();
       
        _service = new PositionService(
            _mockRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }
    #region GetByFilter
    [Test]
    public async Task GetByFilter_ValidFilter_ReturnsFilteredPositions()
    {
        // Arrange
        var positions = Positions(); // Use predefined mock positions
        var mockData = positions.AsQueryable().BuildMock();
        var filter = new PositionsFilter
        {
            SearchString = "jdcdkc", // Matches FullName of one position
            From = 0,
            Size = 10,            
        };

        _mockRepository.Setup(r => r.Count(It.IsAny<Expression<Func<Position, bool>>>()))
            .ReturnsAsync(mockData.Count());

        _mockRepository.Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<Expression<Func<Position, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Position, dynamic>>, SortDirection>>()))
            .Returns(mockData);

        // Act
        var result = await _service.GetByFilter(providerId, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(positions.Count, result.TotalAmount);
        Assert.AreEqual(1, result.Entities.Count); // Only one position matches the filter
        Assert.AreEqual("jdcdkc", result.Entities.First().FullName);
    }

    [Test]
    public async Task GetByFilter_NoResultsMatchFilter_ReturnsEmptyResult()
    {
        // Arrange
        var filter = new PositionsFilter
        {
            SearchString = "NonExistent",
            From = 0,
            Size = 5
        };

        List<Position> positionsEmpty = new List<Position>();

        _mockCurrentUserService
            .Setup(s => s.UserHasRights(It.IsAny<ProviderRights>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(r => r.Count(It.IsAny<Expression<Func<Position, bool>>>()))
            .ReturnsAsync(0);

        _mockRepository.Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<Expression<Func<Position, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Position, dynamic>>, SortDirection>>()))
            .Returns(positionsEmpty.AsQueryable().BuildMock());

        // Act
        var result = await _service.GetByFilter(providerId, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.IsEmpty(result.Entities);
    }

    [Test]
    public async Task GetByFilter_FilterByProviderId_ReturnsOnlyMatchingResults()
    {
        // Arrange
        var filter = new PositionsFilter();

        var mockPositions = Positions().Where(p => p.ProviderId == providerId).ToList();

        _mockCurrentUserService
            .Setup(s => s.UserHasRights(It.IsAny<ProviderRights>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(r => r.Count(It.IsAny<Expression<Func<Position, bool>>>()))
            .ReturnsAsync(mockPositions.Count);

        _mockRepository.Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<Expression<Func<Position, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Position, dynamic>>, SortDirection>>()))
            .Returns(mockPositions.AsQueryable().BuildMock());

        // Act
        var result = await _service.GetByFilter(providerId, filter);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(mockPositions.Count, result.TotalAmount);
        Assert.IsTrue(result.Entities.All(e => e.ProviderId == providerId));
    }
    #endregion

    #region GetById
    [Test]
    public async Task GetById_WithInvalidPosition_ReturnsNull()
    {
        // Arrange 
        var data = Positions().AsQueryable().BuildMock();
        var nonExistingPositionId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByFilter(
            It.IsAny<Expression<Func<Position, bool>>>(),
            string.Empty,
            It.IsAny<Func<IQueryable<Position>, IQueryable<Position>>>()))
        .ReturnsAsync(new List<Position>());

        // Act
        var result = await _service.GetByIdAsync(nonExistingPositionId, providerId).ConfigureAwait(false);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task GetById_Valid_ReturnsEntity()
    {
        // Arrange
        var data = Positions().AsQueryable().BuildMock();
        var existingPosition = data.First();

        _mockRepository.Setup(r => r.GetByFilter(
            It.IsAny<Expression<Func<Position, bool>>>(),
            string.Empty,
            It.IsAny<Func<IQueryable<Position>, IQueryable<Position>>>()))
        .ReturnsAsync((Expression<Func<Position, bool>> predicate, string includeProperties, Func<IQueryable<Position>, IQueryable<Position>> includeExpression) =>
        {
            var mockData = data; // List of mock positions
            return mockData.AsQueryable().Where(predicate.Compile()).ToList();
        });

        // Act
        var actualPositionDto = await _service.GetByIdAsync(existingPosition.Id, providerId);

        // Assert
        Assert.IsNotNull(actualPositionDto);
    }
    #endregion

    #region Create
    [Test]
    public async Task CreatePosition_WithValidDto_ReturnsPosition()
    {
        // Arrange        
        var dto = FakePositionUpdateDto();
        var userId = Guid.NewGuid();

        _mockCurrentUserService.Setup(a => a.UserId).Returns(userId.ToString());

        _mockRepository.Setup(a => a.Create(It.IsAny<Position>())).ReturnsAsync((Position position) => position);

        // Act
        var result = await _service.CreateAsync(dto, providerId);

        // Assert
        Assert.IsNotNull(result);         
    }

    [Test]
    public async Task CreateAsync_UnauthorizedAccess_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var createDto = new PositionCreateUpdateDto
        {
            // Populate the create DTO with test data
            FullName = "Test Position",
            Description = "Description for test position"
        };

        _mockCurrentUserService
            .Setup(s => s.UserHasRights(It.IsAny<ProviderRights>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have the necessary rights"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _service.CreateAsync(createDto, providerId);
        });

        Assert.AreEqual("User does not have the necessary rights", exception.Message);
        _mockCurrentUserService.Verify(s => s.UserHasRights(It.IsAny<ProviderRights>()), Times.Once);
    }
    #endregion

    #region Delete
    [Test]
    public async Task DeletePosition_ValidInput()
    {
        // Arrange
        var data = Positions().AsQueryable().BuildMock();
        var existingPosition = data.First();

        _mockRepository.Setup(r => r.GetByFilter(
            It.IsAny<Expression<Func<Position, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<Position>, IQueryable<Position>>>()))
        .ReturnsAsync((Expression<Func<Position, bool>> predicate, string includeProperties, Func<IQueryable<Position>, IQueryable<Position>> includeExpression) =>
        {
            var mockData = data;
            return mockData.AsQueryable().Where(predicate.Compile()).ToList();
        });

        _mockRepository.Setup(r => r.Delete(It.IsAny<Position>())).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(existingPosition.Id, providerId);

        // Assert        
        
        _mockCurrentUserService.Verify(s => s.UserHasRights(It.IsAny<ProviderRights>()), Times.Once);
        _mockRepository.Verify(r => r.Delete(It.Is<Position>(p => p.Id == positionId)), Times.Once);
    }

    [Test]
    public async Task DeletePosition_WhenIsDeletedTrue_ReturnsExceptionMessage()
    {
        // Arrange
        var data = Positions().AsQueryable().BuildMock();
        var existingPosition = data.First();
        existingPosition.IsDeleted = true;

        _mockRepository.Setup(r => r.Any(It.IsAny<Expression<Func<Position, bool>>>())).ReturnsAsync(true);

        _mockRepository.Setup(r => r.GetByFilter(
            It.IsAny<Expression<Func<Position, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<Position>, IQueryable<Position>>>()))
        .ReturnsAsync((Expression<Func<Position, bool>> predicate, string includeProperties, Func<IQueryable<Position>, IQueryable<Position>> includeExpression) =>
        {
            throw new KeyNotFoundException($"Position with positionId {existingPosition.Id} not found or it was deleted.");
        });

        _mockRepository.Setup(r => r.Delete(It.IsAny<Position>())).Returns(Task.CompletedTask);

        // Act
        var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _service.DeleteAsync(existingPosition.Id, providerId);
        });

        // Verify that the exception message is correct
        Assert.AreEqual($"Position with positionId {existingPosition.Id} not found or it was deleted.", exception.Message);

        // Verify that the Delete method was not called because the position was not found
        _mockRepository.Verify(r => r.Delete(It.IsAny<Position>()), Times.Never);
    }
    #endregion

    #region Update
    [Test]
    public async Task UpdatePosition_ValidInput_ReturnsUpdatedPosition()
    {
        // Arrange
        var existingPosition = Positions().First();
        var updateDto = FakePositionUpdateDto();

        // Set up the mock repository to return the existing position
        _mockRepository.Setup(r => r.GetByFilter(
            It.IsAny<Expression<Func<Position, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<Position>, IQueryable<Position>>>()))
            .ReturnsAsync(new List<Position> { existingPosition });

        // Set up the update call to return the updated position
        _mockRepository.Setup(r => r.Update(It.IsAny<Position>()))
            .ReturnsAsync((Position position) => position);

        // Act
        var updatedPositionDto = await _service.UpdateAsync(existingPosition.Id, updateDto, providerId);

        // Assert
        Assert.IsNotNull(updatedPositionDto);
        Assert.AreEqual(existingPosition.Id, updatedPositionDto.Id);
        Assert.AreEqual(updateDto.FullName, updatedPositionDto.FullName);
        Assert.AreEqual(updateDto.Description, updatedPositionDto.Description);
        Assert.AreEqual(updateDto.Department, updatedPositionDto.Department);
        Assert.AreEqual(updateDto.SeatsAmount, updatedPositionDto.SeatsAmount);
        Assert.AreEqual(updateDto.GenitiveName, updatedPositionDto.GenitiveName);
        Assert.AreEqual(updateDto.IsTeachingPosition, updatedPositionDto.IsTeachingPosition);
        Assert.AreEqual(updateDto.Rate, updatedPositionDto.Rate);
        Assert.AreEqual(updateDto.Tariff, updatedPositionDto.Tariff);
        Assert.AreEqual(updateDto.ClassifierType, updatedPositionDto.ClassifierType);
        Assert.AreEqual(updateDto.IsForRuralAreas, updatedPositionDto.IsForRuralAreas);

        // Verify that the Update method was called on the repository with the correct position
        _mockRepository.Verify(r => r.Update(It.Is<Position>(p => p.Id == existingPosition.Id)), Times.Once);
    }

    [Test]
    public async Task UpdatePosition_PositionDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var updateDto = FakePositionUpdateDto();
        var nonExistingPositionId = Guid.NewGuid(); // A non-existing positionId

        // Set up the mock repository to return an empty list for the non-existing position
        _mockRepository.Setup(r => r.GetByFilter(
            It.IsAny<Expression<Func<Position, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<Position>, IQueryable<Position>>>()))
            .ReturnsAsync(new List<Position>()); // Simulate that the position doesn't exist

        // Act & Assert
        var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _service.UpdateAsync(nonExistingPositionId, updateDto, providerId);
        });

        // Verify the exception message exists
        Assert.IsNotEmpty(exception.Message);

        // Verify that the Update method was never called on the repository because the position does not exist
        _mockRepository.Verify(r => r.Update(It.IsAny<Position>()), Times.Never);
    }

    [Test]
    public async Task UpdatePosition_UnauthorizedAccessException_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var updateDto = FakePositionUpdateDto();
        var existingPositionId = positionId; // Use the existing positionId from the setup

        // Set up the mock for HasProviderRights to throw UnauthorizedAccessException
        _mockCurrentUserService.Setup(s => s.UserHasRights(It.IsAny<ProviderRights>()))
            .ThrowsAsync(new UnauthorizedAccessException("Provider does not have the necessary rights"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _service.UpdateAsync(existingPositionId, updateDto, providerId);
        });

        // Verify that the exception message is correct
        Assert.AreEqual("Provider does not have the necessary rights", exception.Message);

        // Verify that the Update method was never called on the repository because the rights check failed
        _mockRepository.Verify(r => r.Update(It.IsAny<Position>()), Times.Never);
    }
    #endregion

    private List<Position> Positions()
    {
        return new List<Position>()
        {
            new Position()
            {
                Id = positionId,
                ProviderId = providerId,
                FullName = "jdcdkc",
                ShortName = "shhhs",
                GenitiveName = "llll",
                Language = "aaa",
                Department = "ppp",
                Description = "uuu",
                IsForRuralAreas = true,
                SeatsAmount = 20,
                Rate = 42,
                Tariff = 55,
                ClassifierType = "type",
                ContactId = Guid.Empty,
                IsDeleted = false,
                IsTeachingPosition = true,
                PositionType = PositionType.Employee
            }
        };
    }

    private PositionCreateUpdateDto FakePositionUpdateDto()
    {
        return new PositionCreateUpdateDto
        {
            FullName = "Hello",
            Language = "ffff",
            Description = "ffff",
            Department = "ffff",
            SeatsAmount = 20,
            GenitiveName = "ffff",
            IsTeachingPosition = true,
            Rate = 20,
            Tariff = 10,
            ClassifierType = "ffff",
            IsForRuralAreas = false,
            PositionType = PositionType.Employee
        };
    }
}