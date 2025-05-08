using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Official;
using OutOfSchool.BusinessLogic.Models.Position;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class DirectorManagementServiceTests
{
    private Mock<IOfficialRepository> _officialRepositoryMock;
    private Mock<IPositionRepository> _positionRepositoryMock;
    private Mock<IPositionService> _positionServiceMock;
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<IOfficialChangesLogService> _officialChangesLogServiceMock;
    private Mock<ILogger<DirectorManagementService>> _loggerMock;
    private Mock<ITransactionManagerService> _transactionManagerServiceMock;

    private DirectorManagementService _service;

    private Guid _providerId;

    [SetUp]
    public void SetUp()
    {
        _officialRepositoryMock = new Mock<IOfficialRepository>();
        _positionRepositoryMock = new Mock<IPositionRepository>();
        _positionServiceMock = new Mock<IPositionService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<DirectorManagementService>>();
        _transactionManagerServiceMock = new Mock<ITransactionManagerService>();

        _providerId = Guid.NewGuid();
        SetupTransactionManagerMock();
        SetupOfficialChangesLogServiceMock();
        _service = new DirectorManagementService(
            _officialRepositoryMock.Object,
            _officialChangesLogServiceMock.Object,
            _positionRepositoryMock.Object,
            _positionServiceMock.Object,
            _currentUserServiceMock.Object,
            _transactionManagerServiceMock.Object,
            _loggerMock.Object
            );
    }


    #region PromoteEmployee
    [Test]
    public async Task Promote_Should_Fail_When_Official_Not_Found()
    {
        var official = SetupOfficial();
        var requestDto = CreatePromoteRequestDto(official.Id);
        SetupUserHasRightsAsDeputy();
        _officialRepositoryMock
           .Setup(x => x.GetById(requestDto.OfficialId))
           .ReturnsAsync((Official)null!);

        var result =  await _service.PromoteEmployeeToDirector(_providerId, requestDto);

        result.Succeeded.Should().BeFalse();
        result.OperationResult.Errors.Should().ContainSingle(e =>
            e.Code == "OfficialNotFound" &&
            e.Description == $"Official with ID {requestDto.OfficialId} not found");
    }

    [Test]
    public async Task Promote_Should_Fail_When_Director_Already_Exists()
    {
        var official = SetupOfficial();
        var requestDto = CreatePromoteRequestDto(official.Id);

        SetupUserHasRightsAsDeputy();
        SetupExistingDirectorForСurrentProvider();

        var result = await _service.PromoteEmployeeToDirector(_providerId, requestDto);

        result.Succeeded.Should().BeFalse();
        result.OperationResult.Errors.Should().ContainSingle(e =>
            e.Code == "DirectorAlreadyExists" &&
            e.Description == $"Director already exists for provider with ID: {_providerId}");
    }

    [Test]
    public async Task Promote_Should_Throw_UnauthorizedAccessException_When_User_Has_No_Access_To_Provider()
    {
        var official = SetupOfficial();
        var requestDto = CreatePromoteRequestDto(official.Id);

        SetupUserHasNoRights();

        // Act
        Func<Task> act = async () => await _service.PromoteEmployeeToDirector(_providerId, requestDto);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User has no rights to perform operation");
    }

    [Test]
    public async Task Promote_Should_Fail_When_Initiator_Is_Not_Deputy()
    {
        // Arrange
        var official = SetupOfficial(); // canditate for promotion
        var requestDto = CreatePromoteRequestDto(official.Id);
        SetupNoDirectorForProvider();
        SetupUserHasRightsButNotDeputy();

        // Act
        var result = await _service.PromoteEmployeeToDirector(_providerId, requestDto);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.OperationResult.Errors.Should().ContainSingle(e =>
            e.Code == "Unauthorized" &&
            e.Description == "Only Director or Deputy of this provider can promote.");
    }

    [Test]
    public async Task Promote_Should_Fail_When_Official_Not_From_This_Provider()
    {
        // Arrange
        var wrongProviderId = Guid.NewGuid(); // wrong providerId
        var official = SetupOfficial(providerId: wrongProviderId); // create official with wrong provider id
        var requestDto = CreatePromoteRequestDto(official.Id);

        SetupUserHasRightsAsDeputy();

        SetupNoDirectorForProvider(wrongProviderId);

        // Act
        var result = await _service.PromoteEmployeeToDirector(_providerId, requestDto);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.OperationResult.Errors.Should().ContainSingle(e =>
            e.Code == "WrongProvider" &&
            e.Description == "Official does not belong to the specified provider.");
    }

    [Test]
    public async Task Promote_Should_Call_ExecuteInTransactionAsync()
    {
        // Arrange
        var official = SetupOfficial();
        var requestDto = CreatePromoteRequestDto(official.Id);

        SetupUserHasRightsAsDeputy();
        SetupNoDirectorForProvider();
        SetupInitiatorAsDeputy(_currentUserServiceMock.Object.UserId, _providerId);

        var createdPositionId = Guid.NewGuid();
        var newPositionDto = new PositionDto
        {
            Id = createdPositionId,
            ProviderId = _providerId,
            PositionType = PositionType.Director,
            FullName = "Director",
            ActiveFrom = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _positionServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<PositionCreateUpdateDto>(), _providerId))
            .ReturnsAsync(newPositionDto);

        _officialRepositoryMock
            .Setup(x => x.Update(It.Is<Official>(o => o.PositionId == createdPositionId)))
            .ReturnsAsync((Official o) => o);

        // Act
        await _service.PromoteEmployeeToDirector(_providerId, requestDto);

        // Assert
        _transactionManagerServiceMock.Verify(x =>
            x.ExecuteInTransactionAsync(It.IsAny<Func<Task<PromoteToDirectorResponseDto>>>()),
            Times.Once);
    }

    [Test]
    public async Task Promote_Should_Succeed_When_All_Valid()
    {
        // Arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var official = SetupOfficial();

        var userId = Guid.NewGuid().ToString();

        SetupInitiatorAsDeputy(userId, _providerId);

        var requestDto = CreatePromoteRequestDto(official.Id);

        SetupUserHasRightsAsDeputy();
        SetupNoDirectorForProvider();

        var createdPositionId = Guid.NewGuid();
        var newPositionDto = new PositionDto
        {
            Id = createdPositionId,
            ProviderId = _providerId,
            PositionType = PositionType.Director,
            FullName = "Director",
            ActiveFrom = now
        };

        _positionServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<PositionCreateUpdateDto>(), _providerId))
            .ReturnsAsync(newPositionDto);

        _officialRepositoryMock
            .Setup(x => x.Update(It.Is<Official>(o => o.PositionId == createdPositionId)))
            .ReturnsAsync((Official o) => o);

        // Act
        var result = await _service.PromoteEmployeeToDirector(_providerId, requestDto);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.OfficialId.Should().Be(official.Id);
        result.Value.PositionId.Should().Be(createdPositionId);
        result.Value.PositionType.Should().Be(PositionType.Director);
        result.Value.ActiveFrom.Should().Be(now);

        _positionServiceMock.Verify(x => x.CreateAsync(It.IsAny<PositionCreateUpdateDto>(), _providerId), Times.Once);
        _officialRepositoryMock.Verify(x => x.Update(It.Is<Official>(o => o.PositionId == createdPositionId)), Times.Once);
    }



    [Test]
    public async Task Promote_Should_Still_Call_ExecuteInTransactionAsync_When_Exception_Occurs()
    {
        // Arrange
        var official = SetupOfficial();
        var requestDto = CreatePromoteRequestDto(official.Id);

        SetupUserHasRightsAsDeputy();
        SetupNoDirectorForProvider();
        SetupInitiatorAsDeputy(_currentUserServiceMock.Object.UserId, _providerId);

        _positionServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<PositionCreateUpdateDto>(), _providerId))
            .ThrowsAsync(new Exception("Something went wrong"));

        // Act
        var result = await _service.PromoteEmployeeToDirector(_providerId, requestDto);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.OperationResult.Errors.Should().ContainSingle(e => e.Description.Contains("unexpected error"));

        _transactionManagerServiceMock.Verify(x =>
             x.ExecuteInTransactionAsync(It.IsAny<Func<Task<PromoteToDirectorResponseDto>>>()),
             Times.Once);
    }

    [Test]
    public async Task Promote_Should_LogError_When_Exception_Occurs_In_Transaction()
    {
        // Arrange
        var official = SetupOfficial();
        var requestDto = CreatePromoteRequestDto(official.Id);

        SetupUserHasRightsAsDeputy();
        SetupNoDirectorForProvider();
        SetupInitiatorAsDeputy(_currentUserServiceMock.Object.UserId, _providerId);

        _positionServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<PositionCreateUpdateDto>(), _providerId))
            .ThrowsAsync(new Exception("Transaction failed"));

        // Act
        var result =  await _service.PromoteEmployeeToDirector(_providerId, requestDto);

        result.Succeeded.Should().BeFalse();
        result.OperationResult.Errors.Should().ContainSingle(e => e.Description.Contains("unexpected error"));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
               LogLevel.Error,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to promote employee to Director")),
               It.Is<Exception>(ex => ex.Message == "Transaction failed"),
               It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
    }
    #endregion PromoteEmployee

    #region TransferDirectorPosition
    [Test]
    public async Task TransferDirectorPosition_ShouldTransferPositions_WhenAllValid()
    {
        // Arrange
        var fromOfficialId = Guid.NewGuid();
        var toOfficialId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var fromPositionId = Guid.NewGuid();
        var toPositionId = Guid.NewGuid();

        var fromOfficial = new Official
        {
            Id = fromOfficialId,
            PositionId = fromPositionId,
            Position = new Position { Id = fromPositionId, PositionType = PositionType.Director, ProviderId = _providerId },
            IndividualId = Guid.NewGuid(),
            Individual = new Individual { FirstName = "Ivan", LastName = "Ivanov", UserId = userId.ToString() }
        };

        var toOfficial = new Official
        {
            Id = toOfficialId,
            PositionId = toPositionId,
            Position = new Position { Id = toPositionId, PositionType = PositionType.DeputyDirector, ProviderId = _providerId },
            IndividualId = Guid.NewGuid(),
            Individual = new Individual { FirstName = "Petro", LastName = "Petrov" }
        };

        var request = new TransferDirectorRequestDto
        {
            FromOfficialId = fromOfficialId,
            ToOfficialId = toOfficialId
        };

        SetupRepositoryMocksForTransfer(fromOfficial, toOfficial);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        // Act
        var result = await _service.TransferDirectorPosition(_providerId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Succeeded);

        var value = result.Value;
        Assert.AreEqual(_providerId, value.ProviderId);
        Assert.AreEqual(fromOfficial.IndividualId, value.PreviousDirectorIndividualId);
        Assert.AreEqual("Ivanov Ivan", value.PreviousDirectorFullName);
        Assert.AreEqual(toOfficial.IndividualId, value.NewDirectorIndividualId);
        Assert.AreEqual("Petrov Petro", value.NewDirectorFullName);

        _officialRepositoryMock.Verify(repo => repo.Update(It.IsAny<Official>()), Times.Exactly(2));
    }
    [Test]
    public async Task TransferDirectorPosition_ShouldReturnFailedResult_WhenOfficialNotFound()
    {
        // Arrange
        var request =
        new TransferDirectorRequestDto
        {
            FromOfficialId = Guid.NewGuid(),
            ToOfficialId = Guid.NewGuid()
        };

        _officialRepositoryMock
            .Setup(repo => repo.RunInTransaction(It.IsAny<Func<Task<Result<TransferDirectorResponseDto>>>>()))
            .Returns<Func<Task<Result<TransferDirectorResponseDto>>>>(func => func());

        _officialRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(It.IsAny<Guid>(), String.Empty, It.IsAny<Func<IQueryable<Official>, IQueryable<Official>>>()))
            .ReturnsAsync((Official)null);

        // Act
        var result = await _service.TransferDirectorPosition(_providerId, request);

        //Assert
        Assert.IsFalse(result.Succeeded);
        Assert.IsTrue(result.OperationResult.Errors.Any());
        Assert.AreEqual("One or both officials were not found.", result.OperationResult.Errors.First().Description); // або адаптуйте до вашого ValidationError
    }

    [Test]
    public async Task TransferDirectorPosition_ShouldReturnFailedResult_WhenOfficialsHaveDifferentProviders()
    {
        // Arrange
        var request = new TransferDirectorRequestDto
        {
            FromOfficialId = Guid.NewGuid(),
            ToOfficialId = Guid.NewGuid()
        };

        var fromPositionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherProvideId = Guid.NewGuid();

        var fromOfficial = new Official
        {
            Id = request.FromOfficialId,
            PositionId = fromPositionId,
            Position = new Position { Id = fromPositionId, PositionType = PositionType.Director, ProviderId = _providerId },
            IndividualId = Guid.NewGuid(),
            Individual = new Individual { FirstName = "Ivan", LastName = "Ivanov", UserId = userId.ToString() }
        };

        var toOfficial = new Official
        {
            Id = request.ToOfficialId,
            PositionId = fromPositionId,
            Position = new Position { Id = fromPositionId, PositionType = PositionType.Director, ProviderId = otherProvideId },
            IndividualId = Guid.NewGuid(),
            Individual = new Individual { FirstName = "Petro", LastName = "Petrov", UserId = Guid.NewGuid().ToString() }
        };

        SetupRepositoryMocksForTransfer(fromOfficial, toOfficial);

        // Act
        var result = await _service.TransferDirectorPosition(_providerId, request);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("Officials must belong to the same provider.", (result.OperationResult.Errors).First().Description); 
    }

    [Test]
    public async Task TransferDirectorPosition_ShouldReturnFailedResult_WhenCurrentUserIsNotDirector()
    {
        // Arrange
        var fromOfficialId = Guid.NewGuid();
        var toOfficialId = Guid.NewGuid();

        var fromPositionId = Guid.NewGuid();
        var toPositionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherProvideId = Guid.NewGuid();

        var fromOfficial = new Official
        {
            Id = fromOfficialId,
            PositionId = fromPositionId,
            Position = new Position { Id = fromPositionId, PositionType = PositionType.DeputyDirector, ProviderId = _providerId },
            IndividualId = Guid.NewGuid(),
            Individual = new Individual { FirstName = "Ivan", LastName = "Ivanov", UserId = userId.ToString() }
        };

        var toOfficial = new Official
        {
            Id = toOfficialId,
            PositionId = toPositionId,
            Position = new Position { Id = toPositionId, PositionType = PositionType.Employee, ProviderId = _providerId },
            IndividualId = Guid.NewGuid(),
            Individual = new Individual { FirstName = "Petro", LastName = "Petrov", UserId = Guid.NewGuid().ToString() }
        };

        var request = new TransferDirectorRequestDto
        {
            FromOfficialId = fromOfficialId,
            ToOfficialId = toOfficialId
        };

        SetupRepositoryMocksForTransfer(fromOfficial, toOfficial);
        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(Guid.NewGuid().ToString()); // does not match with fromOfficial.UserId

        // Act
        var result=  await _service.TransferDirectorPosition(_providerId, request);
        
        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.That(result.OperationResult.Errors, Has.One.Matches<OperationError>(e =>
            e.Code == "Unauthorized" || e.Description.Contains("not authorized", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public async Task TransferDirectorPosition_ShouldReturnFailedResult_AndLogError_WhenExceptionThrown()
    {
        // Arrange
        var request = new TransferDirectorRequestDto
        {
            FromOfficialId = Guid.NewGuid(),
            ToOfficialId = Guid.NewGuid()
        };

        _officialRepositoryMock
            .Setup(repo => repo.RunInTransaction(It.IsAny<Func<Task<Result<TransferDirectorResponseDto>>>>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _service.TransferDirectorPosition(_providerId, request);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.That(result.OperationResult.Errors, Has.One.Matches<OperationError>(e =>
            e.Code == "TransferFailed" &&
            e.Description.Contains("unexpected error", StringComparison.OrdinalIgnoreCase)));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected error during director transfer")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    #endregion TransferDirectorPosition
    private void SetupTransactionManagerMock()
    {
        _transactionManagerServiceMock = new Mock<ITransactionManagerService>();

        _transactionManagerServiceMock
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async func => await func());

        _transactionManagerServiceMock
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task<PromoteToDirectorResponseDto>>>()))
            .Returns<Func<Task<PromoteToDirectorResponseDto>>>(async func => await func());
    }
    private void SetupOfficialChangesLogServiceMock()
    {
        _officialChangesLogServiceMock = new Mock<IOfficialChangesLogService>();

        _officialChangesLogServiceMock
            .Setup(x => x.SaveChangesLogAsync(
                It.IsAny<Official>(),
                It.IsAny<string>(),
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.FromResult(1));
    }
    private void SetupUserHasRightsAsDeputy(Guid? providerId = null)
    {
        var deputyId = Guid.NewGuid();
        var provId = providerId ?? _providerId;

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(deputyId.ToString());

        var deputy = SetupOfficial(provId, deputyId, PositionType.DeputyDirector);

        _officialRepositoryMock
            .Setup(x => x.GetByUserIdAsync(deputyId.ToString()))
            .ReturnsAsync(deputy);

        _positionRepositoryMock
            .Setup(x => x.Update(deputy.Position))
            .ReturnsAsync(deputy.Position);

        _currentUserServiceMock
            .Setup(x => x.UserHasRights(It.IsAny<DeputyDirectorRights>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupUserHasRightsButNotDeputy()
    {
        var userId = Guid.NewGuid().ToString();

        // mock userid
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        // user have provider rights
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        // user have provider rights
        _currentUserServiceMock
            .Setup(x => x.UserHasRights(It.IsAny<ProviderRights>()))
            .Returns(Task.CompletedTask);

        // create of official with the same id as userId which is not deputy
        var initiator = SetupOfficial(providerId: _providerId, officialId: Guid.Parse(userId), positionType: PositionType.Employee);

        _officialRepositoryMock
            .Setup(x => x.GetById(Guid.Parse(userId)))
            .ReturnsAsync(initiator);
    }
    private void SetupUserHasNoRights()
    {
        _currentUserServiceMock
            .Setup(x => x.UserHasRights(It.IsAny<DeputyDirectorRights>()))
            .ThrowsAsync(new UnauthorizedAccessException("User has no rights to perform operation"));
    }
    private void SetupExistingDirectorForСurrentProvider()
    {
        _positionRepositoryMock
            .Setup(x => x.DirectorExistsAsync(_providerId))
            .ReturnsAsync(true);
    }
    private void SetupNoDirectorForProvider(Guid? providerId = null)
    {
        _positionRepositoryMock
            .Setup(x => x.DirectorExistsAsync(providerId ?? _providerId))
            .ReturnsAsync(false);
    }
    private void SetupInitiatorAsDeputy(string userId, Guid providerId)
    {
        var initiator = SetupOfficial(providerId);
        initiator.Position.PositionType = PositionType.DeputyDirector;
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        _officialRepositoryMock
            .Setup(x => x.GetById(initiator.Id))
            .ReturnsAsync(initiator);

        _positionRepositoryMock
            .Setup(x => x.Update(initiator.Position))
            .ReturnsAsync(initiator.Position);
    }

    private Official SetupOfficial(Guid? providerId = null, Guid? officialId = null, PositionType? positionType = null)
    {
        var official = new Official
        {
            Id = officialId ?? Guid.NewGuid(),
            EmploymentType = EmploymentType.Main,
            Position = new Position
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId ?? _providerId,
                FullName = "Test Position",
                PositionType = positionType ?? PositionType.Employee,
            },
            Individual = new Individual
            {
                FirstName = "Test",
                LastName = "User",
                MiddleName = "Middle",
                Rnokpp = "1234567890",
            },
        };

        _officialRepositoryMock
            .Setup(x => x.GetById(official.Id))
            .ReturnsAsync(official);

        return official;
    }

    private PromoteToDirectorRequestDto CreatePromoteRequestDto(Guid officialId)
    {
        return new PromoteToDirectorRequestDto
        {
            OfficialId = officialId
        };
    }

    private TransferDirectorRequestDto CreateTransferRequestDto(Guid fromOfficialId, Guid toOfficialId) // need review TK
    {
        return new TransferDirectorRequestDto
        {
            FromOfficialId = fromOfficialId,
            ToOfficialId = toOfficialId
        };
    }
    private void SetupRepositoryMocksForTransfer(Official fromOfficial, Official toOfficial)
    {
        _officialRepositoryMock
            .Setup(repo => repo.RunInTransaction(It.IsAny<Func<Task<Result<TransferDirectorResponseDto>>>>()))
            .Returns<Func<Task<Result<TransferDirectorResponseDto>>>>(func => func());

        _officialRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(fromOfficial.Id, It.IsAny<string>(), It.IsAny<Func<IQueryable<Official>, IQueryable<Official>>>()))
            .ReturnsAsync(fromOfficial);

        _officialRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(toOfficial.Id, It.IsAny<string>(), It.IsAny<Func<IQueryable<Official>, IQueryable<Official>>>()))
            .ReturnsAsync(toOfficial);
    }

}
