using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class StatusServiceTests
{
    private IStatusService service;
    private OutOfSchoolDbContext context;
    private IEntityRepository<InstitutionStatus> repository;
    private DbContextOptions<OutOfSchoolDbContext> options;

    [SetUp]
    public void Setup()
    {
        var builder = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new OutOfSchoolDbContext(options);
        repository = new EntityRepository<InstitutionStatus>(context);
        var logger = new Mock<ILogger<StatusService>>();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        service = new StatusService(repository, logger.Object, localizer.Object);
        SeedDatabase();
    }

    [Test]
    public async Task GetAll_WhenCalled_ReturnsAllInstitutionStatuses()
    {
        // Arrange
        var expected = (await repository.GetAll()).Select(s => s.ToModel());

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
        var expected = (await repository.GetById(existingId)).ToModel();

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
        var expected = entityToCreate.ToModel();
        expected.Id = lastIndex + 1;

        // Act
        var result = await service.Create(entityToCreate.ToModel()).ConfigureAwait(false);

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

        var expected = entityToUpdate.ToModel();

        // Act
        var result = await service.Update(entityToUpdate.ToModel()).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var changedEntity = InstitutionStatusGenerator.Generate().ToModel();

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
        using var context = new OutOfSchoolDbContext(options);
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var institutionStatuses = InstitutionStatusGenerator.Generate(5);
            context.InstitutionStatuses.AddRange(institutionStatuses);

            context.SaveChanges();
        }
    }
}