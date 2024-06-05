using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;
using OutOfSchool.Common;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

public class WorkshopServicesCombinerV2Tests
{
    private Mock<IWorkshopService> workshopService;
    private Mock<IElasticsearchSynchronizationService> elasticsearchSynchronizationService;

    private IWorkshopServicesCombinerV2 service;

    [SetUp]
    public void SetUp()
    {
        workshopService = new Mock<IWorkshopService>();
        elasticsearchSynchronizationService = new Mock<IElasticsearchSynchronizationService>();
        var notificationService = new Mock<INotificationService>();
        var favoriteRepository = new Mock<IEntityRepositorySoftDeleted<long, Favorite>>();
        var applicationRepository = new Mock<IApplicationRepository>();
        var workshopStrategy = new Mock<IWorkshopStrategy>();
        var currentUserServicse = new Mock<ICurrentUserService>();
        var ministryAdminService = new Mock<IMinistryAdminService>();
        var regionAdminService = new Mock<IRegionAdminService>();
        var codeficatorService = new Mock<ICodeficatorService>();
        var esProvider = new Mock<IElasticsearchProvider<WorkshopES, WorkshopFilterES>>();
        var mapper = new Mock<IMapper>();

        service = new WorkshopServicesCombinerV2(
            workshopService.Object,
            elasticsearchSynchronizationService.Object,
            notificationService.Object,
            favoriteRepository.Object,
            applicationRepository.Object,
            workshopStrategy.Object,
            currentUserServicse.Object,
            ministryAdminService.Object,
            regionAdminService.Object,
            codeficatorService.Object,
            esProvider.Object,
            mapper.Object);
    }

    [Test]
    public async Task Update_WithValidDto_ShouldReturnSucceededResult()
    {
        // Arrange
        var currentWorkshopDto = WorkshopDtoGenerator.Generate();
        currentWorkshopDto.TakenSeats = 4;
        var newWorkshopV2Dto = WorkshopV2DtoGenerator.Generate();
        newWorkshopV2Dto.Id = currentWorkshopDto.Id;
        newWorkshopV2Dto.AvailableSeats = 8;
        var workshopResultDto = new WorkshopResultDto()
        {
            Workshop = newWorkshopV2Dto,
        };

        workshopService.Setup(x => x.GetById(newWorkshopV2Dto.Id, true))
            .ReturnsAsync(currentWorkshopDto);
        workshopService.Setup(x => x.UpdateV2(newWorkshopV2Dto))
            .ReturnsAsync(workshopResultDto);
        elasticsearchSynchronizationService.Setup(
            x => x.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                workshopResultDto.Workshop.Id,
                ElasticsearchSyncOperation.Update))
            .Returns(Task.CompletedTask);

        // Act
        var result = await service.Update(newWorkshopV2Dto).ConfigureAwait(false);

        // Assert
        workshopService.VerifyAll();
        elasticsearchSynchronizationService.VerifyAll();
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(workshopResultDto, result.Value);
    }

    [Test]
    public async Task Update_WithNotExistWorkshop_ShouldReturnBadRequestResult()
    {
        // Arrange
        var currentWorkshopDto = null as WorkshopDto;
        var newWorkshopV2Dto = WorkshopV2DtoGenerator.Generate();

        workshopService.Setup(x => x.GetById(newWorkshopV2Dto.Id, true))
            .ReturnsAsync(currentWorkshopDto);

        // Act
        var result = await service.Update(newWorkshopV2Dto).ConfigureAwait(false);
        var firstError = result.OperationResult.Errors.FirstOrDefault();

        // Assert
        workshopService.VerifyAll();
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Succeeded);
        Assert.IsNotNull(firstError, "Expected an error, but no errors were found.");
        Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), firstError.Code);
        Assert.AreEqual(Constants.WorkshopNotFoundErrorMessage, firstError.Description);
    }

    [Test]
    public async Task Update_WithInvalidAvailableSeats_ShouldReturnBadRequestResult()
    {
        // Arrange
        var currentWorkshopDto = WorkshopDtoGenerator.Generate();
        currentWorkshopDto.TakenSeats = 7;
        var newWorkshopV2Dto = WorkshopV2DtoGenerator.Generate();
        newWorkshopV2Dto.Id = currentWorkshopDto.Id;
        newWorkshopV2Dto.AvailableSeats = 5;

        workshopService.Setup(x => x.GetById(newWorkshopV2Dto.Id, true))
            .ReturnsAsync(currentWorkshopDto);

        // Act
        var result = await service.Update(newWorkshopV2Dto).ConfigureAwait(false);
        var firstError = result.OperationResult.Errors.FirstOrDefault();

        // Assert
        workshopService.VerifyAll();
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Succeeded);
        Assert.IsNotNull(firstError, "Expected an error, but no errors were found.");
        Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), firstError.Code);
        Assert.AreEqual(Constants.InvalidAvailableSeatsForWorkshopErrorMessage, firstError.Description);
    }
}
