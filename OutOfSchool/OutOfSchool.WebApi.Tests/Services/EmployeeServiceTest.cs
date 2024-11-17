using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.EmployeeOperations;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class EmployeeServiceTest
{
    private readonly string userId = "1";
    private readonly string email = "email@gmail.com";

    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<IOptions<AuthorizationServerConfig>> identityServerConfig;
    private Mock<IOptions<EmployeeConfig>> providerAdminConfig;
    private Mock<IOptions<CommunicationConfig>> communicationConfig;
    private Mock<IEmployeeRepository> providerAdminRepository;
    private Mock<IEntityRepositorySoftDeleted<string, User>> userRepositoryMock;
    private IMapper mapper;
    private Mock<IEmployeeOperationsService> providerAdminOperationsService;
    private Mock<IWorkshopService> workshopService;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> apiErrorServiceUserRepositoryMock;

    private EmployeeService employeeService;
    private ApiErrorResponse badRequestApiErrorResponse;
    private ErrorResponse userDosntHavePermissionErrorResponse;
    private ErrorResponse emailAlreadyTakenErrorResponse;
    private ApiErrorService apiErrorService;

    [SetUp]
    public void SetUp()
    {
        userDosntHavePermissionErrorResponse = new ErrorResponse
        {
            HttpStatusCode = HttpStatusCode.Forbidden,
            ApiErrorResponse = new ApiErrorResponse(new List<ApiError>()
                {
                    ApiErrorsTypes.Employee.UserDontHavePermissionToCreate(userId),
                }),
        };
        badRequestApiErrorResponse = new ApiErrorResponse();
        badRequestApiErrorResponse.AddApiError(
            ApiErrorsTypes.Common.EmailAlreadyTaken("Employee", email));
        emailAlreadyTakenErrorResponse = ErrorResponse.BadRequest(badRequestApiErrorResponse);

        httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient()
            {
                Timeout = new TimeSpan(2),
                BaseAddress = It.IsAny<Uri>(),
            });
        identityServerConfig = new Mock<IOptions<AuthorizationServerConfig>>();
        communicationConfig = new Mock<IOptions<CommunicationConfig>>();
        providerAdminConfig = new Mock<IOptions<EmployeeConfig>>();
        providerAdminConfig.Setup(x => x.Value)
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
        providerAdminRepository = new Mock<IEmployeeRepository>();
        userRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        var logger = new Mock<ILogger<EmployeeService>>();
        providerAdminOperationsService = new Mock<IEmployeeOperationsService>();
        workshopService = new Mock<IWorkshopService>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        apiErrorServiceUserRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        var apiErrorServiceLogger = new Mock<ILogger<ApiErrorService>>();
        apiErrorService = new ApiErrorService(apiErrorServiceUserRepositoryMock.Object, apiErrorServiceLogger.Object);
        var searchStringServiceMock = new Mock<ISearchStringService>();

        employeeService = new EmployeeService(
            httpClientFactory.Object,
            identityServerConfig.Object,
            providerAdminConfig.Object,
            communicationConfig.Object,
            providerAdminRepository.Object,
            userRepositoryMock.Object,
            mapper,
            logger.Object,
            providerAdminOperationsService.Object,
            workshopService.Object,
            currentUserServiceMock.Object,
            apiErrorService,
            searchStringServiceMock.Object);
    }

    [Test]
    public async Task Create_UserDontHavePermission_ReturnsErrorResponse()
    {
        // Arrange
        var expected = userDosntHavePermissionErrorResponse
            .ApiErrorResponse
            .ApiErrors[0];

        // Act
        var response = await employeeService.CreateEmployeeAsync(userId, new CreateEmployeeDto(), It.IsAny<string>()).ConfigureAwait(false);

        ErrorResponse errorResponse = default;
        response.Match<ErrorResponse>(
            actionResult => errorResponse = actionResult,
            succeed => errorResponse = new ErrorResponse());

        var result = errorResponse.ApiErrorResponse.ApiErrors[0];

        // Assert
        Assert.AreEqual(expected.Group, result.Group);
        Assert.AreEqual(expected.Code, result.Code);
        Assert.AreEqual(expected.Message, result.Message);
    }

    [Test]
    public async Task Create_EmailIsAlreadyTaken_ReturnsErrorResponse()
    {
        // Arrange
        var expected = emailAlreadyTakenErrorResponse
            .ApiErrorResponse
            .ApiErrors
            .First();
        apiErrorServiceUserRepositoryMock.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<User> { new User() });

        providerAdminRepository.Setup(r => r.IsExistEmployeeWithUserIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                                   .ReturnsAsync(true);

        providerAdminRepository.Setup(r => r.IsExistProviderWithUserIdAsync(It.IsAny<string>()))
                                   .ReturnsAsync(true);

        var createProviderAdminDto = new CreateEmployeeDto();
        createProviderAdminDto.Email = email;

        // Act
        var response = await employeeService.CreateEmployeeAsync(userId, createProviderAdminDto, It.IsAny<string>()).ConfigureAwait(false);

        ErrorResponse errorResponse = default;
        response.Match<ErrorResponse>(
            actionResult => errorResponse = actionResult,
            succeed => errorResponse = new ErrorResponse());

        var result = errorResponse.ApiErrorResponse.ApiErrors.First();

        // Assert
        Assert.AreEqual(expected.Group, result.Group);
        Assert.AreEqual(expected.Code, result.Code);
        Assert.AreEqual(expected.Message, result.Message);
    }
}
