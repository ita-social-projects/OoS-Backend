using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MockQueryable.Moq;
using System.Linq.Expressions;
using OutOfSchool.BusinessLogic.Models.Position;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Tests.Common;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.Services.Enums;
using OutOfSchool.Common.Models;
namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class PositionServiceTests
{
    private Mock<IEntityRepository<Guid, Position>> _mockRepository;
    private Mock<IProviderService> _mockProviderService;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private IMapper _mapper;
    private Mock<ILogger<Position>> _mockLogger;
    private PositionService _service;   

    private readonly Guid providerId = Guid.NewGuid();
    private readonly Guid positionId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {        
        _mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();

        _mockRepository = new Mock<IEntityRepository<Guid, Position>>();
        _mockProviderService = new Mock<IProviderService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();        
        _mockLogger = new Mock<ILogger<Position>>();
       
        _service = new PositionService(
            _mockLogger.Object,
            _mapper,
            _mockRepository.Object,
            _mockProviderService.Object,
            _mockCurrentUserService.Object);
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
            OrderByFullName = true,
            OrderByCreatedAt = true
        };

        _mockRepository.Setup(r => r.Count(It.IsAny<Expression<Func<Position, bool>>>()))
            .ReturnsAsync(mockData.Count());

        _mockRepository.Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Position, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Position, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(mockData);
         
        _mockProviderService.Setup(s => s.HasProviderRights(providerId)).Returns(Task.CompletedTask);

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
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Position, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Position, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
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
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Position, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Position, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
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
    public async Task GetById_WithInvalidPosition_ReturnsMessage()
    {
        // Arrange 
        var data = Positions().AsQueryable().BuildMock();
        var nonExistingPositionId = Guid.NewGuid();
      
        _mockRepository.Setup(r => r.GetByFilter(
            It.IsAny<Expression<Func<Position, bool>>>(),
            It.IsAny<string>()))
        .ReturnsAsync(new List<Position>());

        // Act & Assert
        var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _service.GetByIdAsync(nonExistingPositionId, providerId).ConfigureAwait(false));

        Assert.That(exception.Message, Is.EqualTo($"Position with positionId {nonExistingPositionId} not found or it was deleted."));
    }

    [Test]
    public async Task GetById_Valid_ReturnsEntity()
    {
        // Arrange
        var data = Positions().AsQueryable().BuildMock(); 
        var existingPosition = data.First();
                        
        _mockRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<string>()))
        .ReturnsAsync((Expression<Func<Position, bool>> predicate, string includeProperties) =>
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

        // Create a mock ProviderDto to return from the GetByUserId method
        var mockProviderDto = new ProviderDto { Id = providerId };

        _mockCurrentUserService.Setup(a => a.UserId).Returns(userId.ToString());

        // Mock the ProviderService's GetByUserId with isEmployee = false (default)
        _mockProviderService.Setup(s => s.GetByUserId(It.IsAny<string>(), false))
            .ReturnsAsync(mockProviderDto);

        _mockRepository.Setup(a => a.Create(It.IsAny<Position>()))
       .ReturnsAsync((Position position) => position);

        // Act
        var result = await _service.CreateAsync(dto, providerId);

        // Assert
        Assert.IsNotNull(result);         

        // Verify that the method was called with the expected arguments
        _mockProviderService.Verify(s => s.GetByUserId(It.IsAny<string>(), false), Times.Once);
    }

    [Test]
    public async Task CreatePosition_ProviderNotFound_ThrowsException()
    {
        // Arrange
        var createDto = new PositionCreateUpdateDto
        {
            FullName = "Test Position",
            Description = "Description for test position"
        };

        _mockCurrentUserService
            .Setup(r => r.UserId).Returns(providerId.ToString());
       
        _mockProviderService
            .Setup(s => s.HasProviderRights(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _mockProviderService
            .Setup(s => s.GetByUserId(It.IsAny<string>(), false))
            .ThrowsAsync(new Exception("Provider not found"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () =>
        {
            await _service.CreateAsync(createDto, providerId);
        });

        Assert.AreEqual("Provider not found", exception.Message);
        _mockProviderService.Verify(s => s.HasProviderRights(It.IsAny<Guid>()), Times.Once);
        _mockProviderService.Verify(s => s.GetByUserId(It.IsAny<string>(), false), Times.Once);
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

        _mockProviderService
            .Setup(s => s.HasProviderRights(It.IsAny<Guid>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have the necessary rights"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _service.CreateAsync(createDto, providerId);
        });

        Assert.AreEqual("User does not have the necessary rights", exception.Message);
        _mockProviderService.Verify(s => s.HasProviderRights(It.IsAny<Guid>()), Times.Once);
    }
    #endregion

    #region Delete
    [Test]
    public async Task DeletePosition_ValidInput()
    {
        // Arrange
        var data = Positions().AsQueryable().BuildMock(); 
        var existingPosition = data.First();        
        
        _mockRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<string>()))
        .ReturnsAsync((Expression<Func<Position, bool>> predicate, string includeProperties) =>
        {
            var mockData = data;
            return mockData.AsQueryable().Where(predicate.Compile()).ToList();
        });
        
        _mockRepository.Setup(r => r.Delete(It.IsAny<Position>())).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(existingPosition.Id, providerId);

        // Assert        
        
        _mockProviderService.Verify(p => p.HasProviderRights(providerId), Times.Once);        
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

        _mockRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<string>()))
        .ReturnsAsync((Expression<Func<Position, bool>> predicate, string includeProperties) =>
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
        _mockRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Position> { existingPosition });

        // Set up the update call to return the updated position
        _mockRepository.Setup(r => r.Update(It.IsAny<Position>()))
            .ReturnsAsync((Position position) => position); // Mock the update to return the same position               

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
        _mockRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Position>()); // Simulate that the position doesn't exist
        
        // Act & Assert
        var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _service.UpdateAsync(nonExistingPositionId, updateDto, providerId);
        });

        // Verify the exception message is correct
        Assert.AreEqual($"Position with positionId {nonExistingPositionId} not found or it was deleted.", exception.Message);

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
        _mockProviderService.Setup(s => s.HasProviderRights(providerId))
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
            IsForRuralAreas = false
        };
    }
}