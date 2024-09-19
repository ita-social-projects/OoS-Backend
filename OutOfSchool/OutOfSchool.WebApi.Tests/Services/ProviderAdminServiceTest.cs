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
using OutOfSchool.BusinessLogic.Services.ProviderAdminOperations;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ProviderAdminServiceTest
{
    private readonly string userId = "1";
    private readonly string email = "email@gmail.com";

    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<IOptions<AuthorizationServerConfig>> identityServerConfig;
    private Mock<IOptions<ProviderAdminConfig>> providerAdminConfig;
    private Mock<IOptions<CommunicationConfig>> communicationConfig;
    private Mock<IProviderAdminRepository> providerAdminRepository;
    private Mock<IEntityRepositorySoftDeleted<string, OutOfSchool.Services.Models.User>> userRepositoryMock;
    private IMapper mapper;
    private Mock<IProviderAdminOperationsService> providerAdminOperationsService;
    private Mock<IWorkshopService> workshopService;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> apiErrorServiceUserRepositoryMock;

    private ProviderAdminService providerAdminService;
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
                    ApiErrorsTypes.ProviderAdmin.UserDontHavePermissionToCreate(userId),
                }),
        };
        badRequestApiErrorResponse = new ApiErrorResponse();
        badRequestApiErrorResponse.AddApiError(
            ApiErrorsTypes.Common.EmailAlreadyTaken("ProviderAdmin", email));
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
        providerAdminRepository = new Mock<IProviderAdminRepository>();
        userRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, OutOfSchool.Services.Models.User>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        var logger = new Mock<ILogger<ProviderAdminService>>();
        providerAdminOperationsService = new Mock<IProviderAdminOperationsService>();
        workshopService = new Mock<IWorkshopService>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        apiErrorServiceUserRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        var apiErrorServiceLogger = new Mock<ILogger<ApiErrorService>>();
        apiErrorService = new ApiErrorService(apiErrorServiceUserRepositoryMock.Object, apiErrorServiceLogger.Object);

        providerAdminService = new ProviderAdminService(
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
            apiErrorService);
    }

    [Test]
    public async Task Create_UserDontHavePermission_ReturnsErrorResponse()
    {
        // Arrange
        var expected = userDosntHavePermissionErrorResponse
            .ApiErrorResponse
            .ApiErrors[0];

        // Act
        var response = await providerAdminService.CreateProviderAdminAsync(userId, new CreateProviderAdminDto(), It.IsAny<string>()).ConfigureAwait(false);

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

        providerAdminRepository.Setup(r => r.IsExistProviderAdminDeputyWithUserIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                                   .ReturnsAsync(true);

        providerAdminRepository.Setup(r => r.IsExistProviderWithUserIdAsync(It.IsAny<string>()))
                                   .ReturnsAsync(true);

        var createProviderAdminDto = new CreateProviderAdminDto();
        createProviderAdminDto.Email = email;

        // Act
        var response = await providerAdminService.CreateProviderAdminAsync(userId, createProviderAdminDto, It.IsAny<string>()).ConfigureAwait(false);

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
