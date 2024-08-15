﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class WorkshopServiceDBTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private OutOfSchoolDbContext dbContext;

    private IWorkshopService workshopService;
    private IWorkshopRepository workshopRepository;
    private Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>> dateTimeRangeRepository;
    private Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>> roomRepository;
    private Mock<ITeacherService> teacherService;
    private Mock<ILogger<WorkshopService>> logger;
    private IMapper mapper;
    private Mock<IImageDependentEntityImagesInteractionService<Workshop>> workshopImagesMediator;
    private Mock<IProviderAdminRepository> providerAdminRepository;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IProviderRepository> providerRepositoryMock;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        dbContext = new OutOfSchoolDbContext(dbContextOptions);

        workshopRepository = new WorkshopRepository(dbContext);
        dateTimeRangeRepository = new Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>>();
        roomRepository = new Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>>();
        teacherService = new Mock<ITeacherService>();
        logger = new Mock<ILogger<WorkshopService>>();
        workshopImagesMediator = new Mock<IImageDependentEntityImagesInteractionService<Workshop>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        providerAdminRepository = new Mock<IProviderAdminRepository>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        providerRepositoryMock = new Mock<IProviderRepository>();

        workshopService =
            new WorkshopService(
                workshopRepository,
                dateTimeRangeRepository.Object,
                roomRepository.Object,
                teacherService.Object,
                logger.Object,
                mapper,
                workshopImagesMediator.Object,
                providerAdminRepository.Object,
                averageRatingServiceMock.Object,
                providerRepositoryMock.Object);

        Seed();
    }

    [TearDown]
    public void Dispose()
    {
        dbContext.Dispose();
    }

    [Test]
    public async Task GetByFilter_WhenSetFormOfLearning_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        await SeedFormOfLearningWorkshops();

        var filter = new WorkshopFilter()
        {
            FormOfLearning = new List<FormOfLearning>()
            {
                FormOfLearning.Offline,
            },
        };

        // Act
        var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(2, result.TotalAmount);
    }

    [Test]
    public async Task GetByFilter_WhenSetFormOfLearning_ReturnNothing()
    {
        // Arrange
        await SeedFormOfLearningWorkshops();

        var filter = new WorkshopFilter()
        {
            FormOfLearning = new List<FormOfLearning>()
            {
                FormOfLearning.Mixed,
            },
        };

        // Act
        var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(0, result.TotalAmount);
    }

    [Test]
    public async Task GetByFilter_WhenNotSetFormOfLearning_ReturnAll()
    {
        // Arrange
        await SeedFormOfLearningWorkshops();

        var filter = new WorkshopFilter();

        // Act
        var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(5, result.TotalAmount);
    }

    [Test]
    public async Task Exists_WhenSendWrongId_ReturnFalse()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate();
        dbContext.Add(workshop);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await workshopService.Exists(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(false, result);
    }

    [Test]
    public async Task Exists_WhenSendGoodId_ReturnTrue()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate();
        dbContext.Add(workshop);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await workshopService.Exists(workshop.Id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(true, result);
    }

    #region private

    private OutOfSchoolDbContext GetContext() => new OutOfSchoolDbContext(dbContextOptions);

    private IWorkshopRepository GetWorkshopRepository(OutOfSchoolDbContext dbContext)
        => new WorkshopRepository(dbContext);

    private void Seed()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    private Task SeedFormOfLearningWorkshops()
    {
        var workshops = new List<Workshop>();

        var workshop = WorkshopGenerator.Generate().WithProvider();
        workshop.FormOfLearning = FormOfLearning.Offline;
        workshop.Provider.Status = ProviderStatus.Approved;
        workshops.Add(workshop);

        workshop = WorkshopGenerator.Generate().WithProvider();
        workshop.FormOfLearning = FormOfLearning.Offline;
        workshop.Provider.Status = ProviderStatus.Approved;
        workshops.Add(workshop);

        workshop = WorkshopGenerator.Generate().WithProvider();
        workshop.FormOfLearning = FormOfLearning.Online;
        workshop.Provider.Status = ProviderStatus.Approved;
        workshops.Add(workshop);

        workshop = WorkshopGenerator.Generate().WithProvider();
        workshop.FormOfLearning = FormOfLearning.Online;
        workshop.Provider.Status = ProviderStatus.Approved;
        workshops.Add(workshop);

        workshop = WorkshopGenerator.Generate().WithProvider();
        workshop.FormOfLearning = FormOfLearning.Online;
        workshop.Provider.Status = ProviderStatus.Approved;
        workshops.Add(workshop);

        dbContext.AddRange(workshops);
        return dbContext.SaveChangesAsync();
    }

    #endregion
}
