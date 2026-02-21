using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class CodeficatorRepositoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .Options;

        await Seed();
    }

    [Test]
    public async Task GetFullAddressesByPartOfName_WithExistingNamePartAndCategory_ShouldReturnDtos()
    {
        // Arrange
        using var context = new TestOutOfSchoolDbContext(dbContextOptions);
        var repository = new CodeficatorRepository(context);
        var namePart = "Кам*";
        var categories = "MTCXKB";
        var expectedResultsQty = 2;
        var expectedSettlements = new List<string> { "Кам’янець-Подільський", "Кам’янки" };

        // Act
        var result = await repository.GetFullAddressesByPartOfName(namePart, categories);
        var actualSettlements = result.Select(r => r.Settlement).ToList();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedResultsQty, result.Count);
        CollectionAssert.AreEqual(expectedSettlements, actualSettlements);
    }

    private async Task Seed()
    {
        using var context = new TestOutOfSchoolDbContext(dbContextOptions);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.AddRange(CATOTTGs());
        await context.SaveChangesAsync();
    }

    private List<CATOTTG> CATOTTGs()
    {
        return new List<CATOTTG>
        {
            new()
            {
                Id = 5,
                Code = "UA05040230070020555",
                ParentId = null,
                Category = "K",
                Name = "Кам’янець-Подільський",
                Order = 10,
                Parent = null,
            },
            new()
            {
                Id = 4,
                Code = "UA05040230070020999",
                ParentId = 3,
                Category = "C",
                Name = "Кам’янки",
                Order = 50,
                Parent = new()
                {
                    Id = 3,
                    Code = "UA05040230000017028",
                    ParentId = 2,
                    Category = "H",
                    Name = "Теплицька",
                    Order = 70,
                    Parent = new()
                    {
                        Id = 2,
                        Code = "UA05040000000050292",
                        ParentId = 1,
                        Category = "P",
                        Name = "Гайсинський",
                        Order = 90,
                        Parent = new()
                        {
                            Id = 1,
                            Code = "UA05000000000010236",
                            ParentId = null,
                            Category = "O",
                            Name = "Вінницька",
                            Order = 110,
                            Parent = null,
                        },
                    },
                },
            },
        };
    }
}