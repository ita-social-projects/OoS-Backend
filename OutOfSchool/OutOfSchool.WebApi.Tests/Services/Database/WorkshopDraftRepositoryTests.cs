using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.WorkshopDraftRepository;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class WorkshopDraftRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;

    private IReadOnlyCollection<WorkshopDraft> workshopDrafts;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        await Seed();
    }

    [Test]
    public async Task Delete_WithValidEntity_DeletesEntity()
    {
        //Arrange
        var context = GetContext();
        var repository = GetWorkshopDraftRepository(context);

        var workshopDraft = await context.WorkshopDrafts.FirstAsync();

        //Act
        await repository.Delete(workshopDraft);

        //Assert
        Assert.IsNull(await repository.GetById(workshopDraft.Id));
    }

    [Test]
    public async Task Update_WithValidEntity_UpdatesEntity()
    {
        //Arrange
        var context = GetContext();
        var repository = GetWorkshopDraftRepository(context);

        var updatedTitle = "Updated Title";

        var workshopDraft = await context.WorkshopDrafts.FirstAsync();   
        workshopDraft.WorkshopDraftContent.Title = updatedTitle;

        //Act
        var result = await repository.Update(workshopDraft);

        //Assert
        Assert.NotNull(result);
        Assert.AreEqual(result.WorkshopDraftContent.Title, updatedTitle);
    }

    [Test]
    public async Task GetByProviderId_WithValidId_ReturnsEntities()
    {
        //Arrange
        var context = GetContext();
        var repository = GetWorkshopDraftRepository(context);

        var providerId = Guid.NewGuid();

        var workshopDraft = await context.WorkshopDrafts.FirstAsync();
        workshopDraft.ProviderId = providerId;
        await context.SaveChangesAsync();

        //Act
        var result = await repository.GetByProviderIdAsync(providerId);

        //Assert
        Assert.NotNull(result);
        foreach (var item in result)
        {
            Assert.NotNull(item);
            Assert.AreEqual(item.ProviderId, providerId);
        }        
    }

    #region private
    private static IWorkshopDraftRepository GetWorkshopDraftRepository(TestOutOfSchoolDbContext dbContext)
        => new WorkshopDraftRepository(dbContext);

    private TestOutOfSchoolDbContext GetContext() 
        => new TestOutOfSchoolDbContext(dbContextOptions);

    private async Task Seed()
    {
        using var context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var worshopV2Dtos = WorkshopV2DtoGenerator.Generate(3);

        workshopDrafts = worshopV2Dtos.ToDraft(false);

        context.AddRange(workshopDrafts);
        await context.SaveChangesAsync();
    }
    #endregion
}

