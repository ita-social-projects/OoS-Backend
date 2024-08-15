using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Extensions;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.ProviderAdminOperations;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class ProviderAdminServiceDBTest
{
    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<IOptions<AuthorizationServerConfig>> identityServerConfig;
    private Mock<IOptions<ProviderAdminConfig>> providerAdminConfig;
    private Mock<IOptions<CommunicationConfig>> communicationConfig;
    private IEntityRepositorySoftDeleted<string, User> userRepository;
    private IMapper mapper;
    private Mock<IProviderAdminOperationsService> providerAdminOperationsService;
    private Mock<IWorkshopService> workshopService;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IApiErrorService> apiErrorService;

    private ProviderAdminService providerAdminService;

    private Provider provider;
    private User providerUser;
    private ProviderAdmin providerAdmin;

    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private OutOfSchoolDbContext dbContext;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB_ProviderAdmin2")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        dbContext = new OutOfSchoolDbContext(dbContextOptions);
        var providerAdminRepository = new ProviderAdminRepository(dbContext);

        httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient()
            {
                Timeout = new TimeSpan(2),
                BaseAddress = It.IsAny<Uri>(),
            });
        identityServerConfig = new Mock<IOptions<AuthorizationServerConfig>>();
        communicationConfig = new Mock<IOptions<CommunicationConfig>>();
        providerAdminConfig = new Mock<IOptions<ProviderAdminConfig>>();
        providerAdminConfig.Setup(x => x.Value)
            .Returns(new ProviderAdminConfig()
            {
                MaxNumberAdmins = 1,
            });
        communicationConfig.Setup(x => x.Value)
            .Returns(new CommunicationConfig()
            {
                ClientName = "test",
                TimeoutInSeconds = 2,
                MaxNumberOfRetries = 7,
            });

        userRepository = new EntityRepositorySoftDeleted<string, User>(dbContext);
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        var logger = new Mock<ILogger<ProviderAdminService>>();
        providerAdminOperationsService = new Mock<IProviderAdminOperationsService>();
        workshopService = new Mock<IWorkshopService>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        apiErrorService = new Mock<IApiErrorService>();

        providerAdminService = new ProviderAdminService(
            httpClientFactory.Object,
            identityServerConfig.Object,
            providerAdminConfig.Object,
            communicationConfig.Object,
            providerAdminRepository,
            userRepository,
            mapper,
            logger.Object,
            providerAdminOperationsService.Object,
            workshopService.Object,
            currentUserServiceMock.Object,
            apiErrorService.Object);

        await Seed();
    }

    [TearDown]
    public void Dispose()
    {
        dbContext.Dispose();
    }

    [Test]
    public async Task GetFilteredRelatedProviderAdmins_WhenCalled_ReturnsEntities()
    {
        var filter = new ProviderAdminSearchFilter();

        // Act
        var result = await providerAdminService.GetFilteredRelatedProviderAdmins(providerUser.Id, filter).ConfigureAwait(false);
        var providerAdmins = dbContext.ProviderAdmins.Where(x => x.ProviderId == provider.Id).ToList();
        List<ProviderAdminDto> dtos = new();

        foreach (var pa in providerAdmins)
        {
            var user = dbContext.Users.Where(u => u.Id == pa.UserId).Single();
            var dto = mapper.Map<ProviderAdminDto>(user);
            dto.AccountStatus = AccountStatusExtensions.Convert(user);

            dtos.Add(dto);
        }

        dtos = dtos.OrderBy(x => x.AccountStatus).ThenBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.MiddleName).ToList();

        // Assert
        TestHelper.AssertTwoCollectionsEqualByValues(dtos, result.Entities);
    }

    private IProviderAdminRepository GetProviderAdminRepository(OutOfSchoolDbContext dbContext)
        => new ProviderAdminRepository(dbContext);

    private OutOfSchoolDbContext GetContext() => new OutOfSchoolDbContext(dbContextOptions);

    private async Task Seed()
    {
        providerUser = UserGenerator.Generate();

        provider = ProvidersGenerator.Generate();
        provider.UserId = providerUser.Id;

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        dbContext.Add(providerUser);
        dbContext.Add(provider);

        // 1
        var user = UserGenerator.Generate();
        user.IsBlocked = true;
        dbContext.Add(user);

        providerAdmin = ProviderAdminsGenerator.Generate();
        providerAdmin.UserId = user.Id;
        providerAdmin.ProviderId = provider.Id;

        dbContext.Add(providerAdmin);

        // 2
        user = UserGenerator.Generate();
        user.LastName = "2" + user.LastName;
        user.IsBlocked = false;
        user.LastLogin = DateTimeOffset.Now;
        dbContext.Add(user);

        providerAdmin = ProviderAdminsGenerator.Generate();
        providerAdmin.UserId = user.Id;
        providerAdmin.ProviderId = provider.Id;

        dbContext.Add(providerAdmin);

        // 3
        user = UserGenerator.Generate();
        user.IsBlocked = false;
        user.LastLogin = DateTimeOffset.MinValue;
        dbContext.Add(user);

        providerAdmin = ProviderAdminsGenerator.Generate();
        providerAdmin.UserId = user.Id;
        providerAdmin.ProviderId = provider.Id;

        dbContext.Add(providerAdmin);

        // 4
        user = UserGenerator.Generate();
        user.LastName = "1" + user.LastName;
        user.IsBlocked = false;
        user.LastLogin = DateTimeOffset.Now;
        dbContext.Add(user);

        providerAdmin = ProviderAdminsGenerator.Generate();
        providerAdmin.UserId = user.Id;
        providerAdmin.ProviderId = provider.Id;

        dbContext.Add(providerAdmin);

        await dbContext.SaveChangesAsync();
    }
}
