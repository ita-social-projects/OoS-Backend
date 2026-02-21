using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.Services;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Repository.CompetitiveEventDraftRepository;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class CompetitiveEventDraftRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;

    private IReadOnlyCollection<CompetitiveEventDraft> competitiveEventDrafts;

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
        // Arrange
        var context = new TestOutOfSchoolDbContext(dbContextOptions);
        var repository = new CompetitiveEventDraftRepository(context);

        var competitiveEventDraft = await context.CompetitiveEventDrafts.FirstAsync();

        // Act
        await repository.Delete(competitiveEventDraft);

        // Assert
        Assert.IsNull(await repository.GetById(competitiveEventDraft.Id));
    }

    [Test]
    public async Task Update_WithValidEntity_UpdatesEntity()
    {
        // Arrange
        var context = new TestOutOfSchoolDbContext(dbContextOptions);
        var repository = new CompetitiveEventDraftRepository(context);

        var updatedTitle = "Updated Title";

        var competitiveEventDraft = await context.CompetitiveEventDrafts.FirstAsync();
        competitiveEventDraft.CompetitiveEventDraftContent.Title = updatedTitle;

        // Act
        var result = await repository.Update(competitiveEventDraft).ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.CompetitiveEventDraftContent.Title, updatedTitle);
    }

    private async Task Seed()
    {
        using var context = new TestOutOfSchoolDbContext(dbContextOptions);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var dtos = CompetitiveEventV2DtoGenerator.Generate(3);

        competitiveEventDrafts = dtos.ToDraft();

        context.AddRange(competitiveEventDrafts);
        await context.SaveChangesAsync();
    }
}
