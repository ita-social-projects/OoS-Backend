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
using OutOfSchool.BusinessLogic.Services.EmployeeOperations;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Config;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class EmployeeServiceDBTest
{
    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<IOptions<AuthorizationServerConfig>> identityServerConfig;
    private Mock<IOptions<EmployeeConfig>> employeeConfig;
    private Mock<IOptions<CommunicationConfig>> communicationConfig;
    private IEntityRepositorySoftDeleted<string, User> userRepository;
    private IMapper mapper;
    private Mock<IEmployeeOperationsService> employeeOperationsService;
    private Mock<IWorkshopService> workshopService;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IApiErrorService> apiErrorService;

    private EmployeeService employeeService;

    private Provider provider;
    private User providerUser;
    private Employee employee;

    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private TestOutOfSchoolDbContext dbContext;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB_ProviderAdmin2")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        dbContext = new TestOutOfSchoolDbContext(dbContextOptions);
        var providerAdminRepository = new EmployeeRepository(dbContext);

        httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient()
            {
                Timeout = new TimeSpan(2),
                BaseAddress = It.IsAny<Uri>(),
            });
        identityServerConfig = new Mock<IOptions<AuthorizationServerConfig>>();
        communicationConfig = new Mock<IOptions<CommunicationConfig>>();
        employeeConfig = new Mock<IOptions<EmployeeConfig>>();
        employeeConfig.Setup(x => x.Value)
            .Returns(new EmployeeConfig()
            {
                MaxNumberEmployees = 1,
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
        var logger = new Mock<ILogger<EmployeeService>>();
        employeeOperationsService = new Mock<IEmployeeOperationsService>();
        workshopService = new Mock<IWorkshopService>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        apiErrorService = new Mock<IApiErrorService>();
        var searchStringServiceMock = new Mock<ISearchStringService>();

        employeeService = new EmployeeService(
            httpClientFactory.Object,
            identityServerConfig.Object,
            employeeConfig.Object,
            communicationConfig.Object,
            providerAdminRepository,
            userRepository,
            mapper,
            logger.Object,
            employeeOperationsService.Object,
            workshopService.Object,
            currentUserServiceMock.Object,
            apiErrorService.Object,
            searchStringServiceMock.Object);

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
        var filter = new EmployeeSearchFilter();

        // Act
        var result = await employeeService.GetFilteredRelatedProviderAdmins(providerUser.Id, filter).ConfigureAwait(false);
        var providerAdmins = dbContext.Employees.Where(x => x.ProviderId == provider.Id).ToList();
        List<EmployeeDto> dtos = new();

        foreach (var pa in providerAdmins)
        {
            var user = dbContext.Users.Single(u => u.Id == pa.UserId);
            var dto = mapper.Map<EmployeeDto>(user);
            dto.AccountStatus = AccountStatusExtensions.Convert(user);

            dtos.Add(dto);
        }

        dtos = dtos.OrderBy(x => x.AccountStatus).ThenBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.MiddleName).ToList();

        // Assert
        TestHelper.AssertTwoCollectionsEqualByValues(dtos, result.Entities);
    }

    private IEmployeeRepository GetProviderAdminRepository(OutOfSchoolDbContext dbContext)
        => new EmployeeRepository(dbContext);

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

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

        employee = EmployeesGenerator.Generate();
        employee.UserId = user.Id;
        employee.ProviderId = provider.Id;

        dbContext.Add(employee);

        // 2
        user = UserGenerator.Generate();
        user.LastName = "2" + user.LastName;
        user.IsBlocked = false;
        user.LastLogin = DateTimeOffset.Now;
        dbContext.Add(user);

        employee = EmployeesGenerator.Generate();
        employee.UserId = user.Id;
        employee.ProviderId = provider.Id;

        dbContext.Add(employee);

        // 3
        user = UserGenerator.Generate();
        user.IsBlocked = false;
        user.LastLogin = DateTimeOffset.MinValue;
        dbContext.Add(user);

        employee = EmployeesGenerator.Generate();
        employee.UserId = user.Id;
        employee.ProviderId = provider.Id;

        dbContext.Add(employee);

        // 4
        user = UserGenerator.Generate();
        user.LastName = "1" + user.LastName;
        user.IsBlocked = false;
        user.LastLogin = DateTimeOffset.Now;
        dbContext.Add(user);

        employee = EmployeesGenerator.Generate();
        employee.UserId = user.Id;
        employee.ProviderId = provider.Id;

        dbContext.Add(employee);

        await dbContext.SaveChangesAsync();
    }
}
