using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Changes;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.IntegrationTests.ChangeLogControllerTests;

[TestFixture]
public class ChangeLogControllerTests
{
    private IMapper mapper;
    private ChangesLogController changesLogControllerWithRealService;
    private IEntityAddOnlyRepository<long, ParentBlockedByAdminLog> parentBlockedByAdminLogRepository;
    private IChangesLogService changesLogService;

    [SetUp]
    public void Setup()
    {
        var context = GetContext();
        parentBlockedByAdminLogRepository = new EntityRepository<long, ParentBlockedByAdminLog>(context);
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        changesLogService = new ChangesLogService(
            new Mock<IOptions<ChangesLogConfig>>().Object,
            new Mock<IChangesLogRepository>().Object,
            new Mock<IProviderRepository>().Object,
            new Mock<IApplicationRepository>().Object,
            new Mock<IEntityRepository<long, ProviderAdminChangesLog>>().Object,
            parentBlockedByAdminLogRepository,
            new Mock<ILogger<ChangesLogService>>().Object,
            mapper,
            new Mock<IValueProjector>().Object,
            new Mock<ICurrentUserService>().Object,
            new Mock<IMinistryAdminService>().Object,
            new Mock<IRegionAdminService>().Object,
            new Mock<IAreaAdminService>().Object,
            new Mock<ICodeficatorService>().Object);
        changesLogControllerWithRealService = new ChangesLogController(changesLogService);
    }

    [Test]
    public async Task ParentBlockedByAdmin_WhenDateToIsDateTimeMaxValue_ShouldReturnOkObjectResult()
    {
        // Arrange
        await SeedChangesLog();
        var expectedTotalAmount = 2;
        var expectedEntitiesCount = 2;

        var request = new ParentBlockedByAdminChangesLogRequest()
        {
            ShowParents = ShowParents.All,
            DateTo = DateTime.MaxValue,
        };

        // Act
        var result = await changesLogControllerWithRealService.ParentBlockedByAdmin(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<OkObjectResult>(result);
        var searchResult = (result as OkObjectResult)?.Value as SearchResult<ParentBlockedByAdminChangesLogDto>;
        Assert.IsNotNull(searchResult);
        Assert.AreEqual(expectedTotalAmount, searchResult.TotalAmount);
        Assert.AreEqual(expectedEntitiesCount, searchResult.Entities.Count);
    }

    private async Task SeedChangesLog()
    {
        OutOfSchoolDbContext context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var parent = ParentGenerator.Generate();
        var parentUser = UserGenerator.Generate();
        parent.User = parentUser;
        parent.UserId = parentUser.Id;
        var user = UserGenerator.Generate();
        context.Users.Add(parentUser);
        context.Parents.Add(parent);
        context.Users.Add(user);
        context.ParentBlockedByAdminLog.AddRange(
            new ParentBlockedByAdminLog()
            {
                Id = 1,
                ParentId = parent.Id,
                Parent = parent,
                UserId = user.Id,
                User = user,
                OperationDate = new DateTime(2023, 10, 12, 14, 10, 14),
                Reason = "Reason to block parent",
                IsBlocked = true,
            },
            new ParentBlockedByAdminLog()
            {
                Id = 2,
                ParentId = parent.Id,
                Parent = parent,
                UserId = user.Id,
                User = user,
                OperationDate = new DateTime(2023, 11, 17, 23, 28, 1),
                Reason = "Reason to unblock parent",
                IsBlocked = false,
            });

        await context.SaveChangesAsync();
    }

    private static OutOfSchoolDbContext GetContext()
    {
        return new OutOfSchoolDbContext(
            new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);
    }
}
