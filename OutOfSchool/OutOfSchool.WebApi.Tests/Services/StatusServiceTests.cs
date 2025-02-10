using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class StatusServiceTests
{
    private IStatusService service;
    private TestOutOfSchoolDbContext context;
    private IEntityRepositorySoftDeleted<long, InstitutionStatus> repository;
    private DbContextOptions<OutOfSchoolDbContext> options;
    private IMapper mapper;

    [SetUp]
    public void Setup()
    {
        options = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        context = new TestOutOfSchoolDbContext(options);
        repository = new EntityRepositorySoftDeleted<long, InstitutionStatus>(context);
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        var logger = new Mock<ILogger<StatusService>>();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        service = new StatusService(repository, logger.Object, localizer.Object, mapper);

        SeedDatabase();
    }

    [TearDown]
    public void Dispose()
    {
        context.Dispose();
    }

    [Test]
    public async Task GetAll_WhenCalled_ReturnsAllInstitutionStatuses()
    {
        // Arrange
        var expected = (await repository.GetAll()).Select(s => mapper.Map<InstitutionStatusDTO>(s));

        // Act
        var result = await service.GetAll().ConfigureAwait(false);

        // Assert
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result);
    }

    [Test]
    public async Task GetById_WhenIdIsValid_ReturnsInstitutionStatus()
    {
        // Arrange
        var collection = await repository.GetAll() as ICollection<InstitutionStatus>;
        var existingId = TestDataHelper.RandomItem(collection).Id;
        var expected = mapper.Map<InstitutionStatusDTO>(await repository.GetById(existingId));

        // Act
        var result = await service.GetById(existingId).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public async Task GetById_WhenIdDoesntExists_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var lastIndex = (await repository.GetAll()).Last().Id;
        var notExistingId = TestDataHelper.GetPositiveInt((int)lastIndex + 1, int.MaxValue);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.GetById(notExistingId).ConfigureAwait(false));
    }

    // research why test is failing from time to time
    [Test]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        var lastIndex = (await repository.GetAll()).Last().Id;
        var entityToCreate = new InstitutionStatus() { Name = TestDataHelper.GetRandomWords() };
        var expected = mapper.Map<InstitutionStatusDTO>(entityToCreate);
        expected.Id = lastIndex + 1;

        // Act
        var result = await service.Create(mapper.Map<InstitutionStatusDTO>(entityToCreate)).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
    {
        // Arrange
        var entityToUpdate = new InstitutionStatus()
        {
            Id = 1,
            Name = TestDataHelper.GetRandomWords(),
        };

        var expected = mapper.Map<InstitutionStatusDTO>(entityToUpdate);

        // Act
        var result = await service.Update(mapper.Map<InstitutionStatusDTO>(entityToUpdate)).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var institutionStatus = InstitutionStatusGenerator.Generate();
        institutionStatus.Id = 100;

        var changedEntity = mapper.Map<InstitutionStatusDTO>(institutionStatus);

        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(changedEntity).ConfigureAwait(false));
    }

    [Test]
    public async Task Delete_WhenIdIsValid_DeletesEntityFromRepository()
    {
        // Arrange
        var collection = await repository.GetAll() as ICollection<InstitutionStatus>;
        var existingId = TestDataHelper.RandomItem(collection).Id;
        var expectedCollection = (await repository.GetByFilter(x => x.Id != existingId)).ToList();

        // Act
        await service.Delete(existingId).ConfigureAwait(false);
        var actualCollection = await repository.GetAll();

        // Assert
        TestHelper.AssertTwoCollectionsEqualByValues(expectedCollection, actualCollection);
    }

    [Test]
    public async Task Delete_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var notExistingId = TestDataHelper.GetPositiveInt(
            (int)(await repository.GetAll()).Last().Id,
            int.MaxValue);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.Delete(notExistingId).ConfigureAwait(false));
    }

    private void SeedDatabase()
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}