using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Models;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.AuthorizationServer.Services;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Models.ExternalAuth;
using OutOfSchool.Tests.Common;
using static OutOfSchool.Tests.Common.HttpClientTestHelper;

namespace OutOfSchool.AuthServer.Tests.Services;

[TestFixture]
public class GovCommunicationServiceTests
{
    private Mock<IOptions<CommunicationConfig>> communicationOptions;
    private Mock<IOptions<AuthorizationServerConfig>> authServerOptions;
    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<ILogger<GovIdentityCommunicationService>> logger;
    private Mock<HttpMessageHandler> handler;
    private HttpClient client;
    private readonly Uri eUSignServiceUri = new("https://sign.com");
    private readonly Uri idServerUri = new("https://id.com");
    private IGovIdentityCommunicationService communicationService;

    [SetUp]
    public void SetUp()
    {
        handler = new Mock<HttpMessageHandler>();
        client = new HttpClient(handler.Object);
        var communicationConfig = new CommunicationConfig
        {
            ClientName = "test",
            MaxNumberOfRetries = 1,
            TimeoutInSeconds = 1,
        };
        var authServerConfig = new AuthorizationServerConfig
        {
            ExternalLogin = new ExternalLogin
            {
                EUSignServiceUri = eUSignServiceUri,
                IdServerUri = idServerUri,
                IdServerPaths = new IdServerPaths
                {
                    UserInfo = "/userinfo"
                },
                EUSignServicePaths = new EUSignServicePaths
                {
                    Certificate = "api/v1/certificate",
                    Decrypt = "api/v1/decrypt",
                },
                Parameters = new Parameters
                {
                    UserInfoFields = new UserInfoFields
                    {
                        Key = "key",
                        PersonalInfo = ["name"],
                        BusinessInfo = ["work"]
                    }
                }
            },
        };
        communicationOptions = new Mock<IOptions<CommunicationConfig>>();
        communicationOptions.Setup(x => x.Value).Returns(communicationConfig);
        authServerOptions = new Mock<IOptions<AuthorizationServerConfig>>();
        authServerOptions.Setup(x => x.Value).Returns(authServerConfig);
        httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);
        logger = new Mock<ILogger<GovIdentityCommunicationService>>();
        communicationService =
            new GovIdentityCommunicationService(httpClientFactory.Object, communicationOptions.Object, logger.Object,
                authServerOptions.Object);
    }

    [Test]
    public async Task GetUserInfo_WithCorrectRequest_ReturnsOkResponse()
    {
        // Arrange
        var remoteUserId = "123";
        var remoteToken = "secret";
        var expectedEmail = "test@example.com";

        var expected = new UserInfoResponse
        {
            Email = expectedEmail,
        };

        var certResponse = new CertificateResponse
        {
            CertBase64 = "cert",
        };
        handler.SetupSendAsync(HttpMethod.Get,
            new Uri(eUSignServiceUri, "api/v1/certificate").ToString())
            .ReturnsHttpResponseAsync(certResponse, HttpStatusCode.OK);

        var infoResponse = new EnvelopedUserInfoResponse
        {
            EncryptedUserInfo = "serialized_info",
        };

        handler.SetupSendAsync(HttpMethod.Get, new Uri(idServerUri, "/userinfo").ToString(), true)
            .ReturnsHttpResponseAsync(infoResponse, HttpStatusCode.OK);

        handler.SetupSendAsync(HttpMethod.Post, new Uri(eUSignServiceUri, "api/v1/decrypt").ToString())
            .ReturnsHttpResponseAsync(expected, HttpStatusCode.OK);

        // Act
        var userInfo = await communicationService.GetUserInfo(remoteUserId, remoteToken);
        
        // Assert
        userInfo.AssertRight(u => Assert.AreEqual(expectedEmail, u.Email));
    }
    
    [Test]
    public async Task GetUserInfo_WithEUSignCertError_ReturnsErrorResponse()
    {
        // Arrange
        var remoteUserId = "123";
        var remoteToken = "secret";
        handler.SetupSendAsync(HttpMethod.Get,
            new Uri(eUSignServiceUri, "api/v1/certificate").ToString())
            .ReturnsHttpResponseAsync(null, HttpStatusCode.InternalServerError);

        // Act
        var userInfo = await communicationService.GetUserInfo(remoteUserId, remoteToken);
        
        // Assert
        userInfo.AssertLeft(e =>
        {
            Assert.IsInstanceOf<ExternalAuthError>(e);
            
            var error = (ExternalAuthError) e;
            Assert.AreEqual(ExternalAuthErrorGroup.Encryption, error.ErrorGroup);
        });
    }
    
    [Test]
    public async Task GetUserInfo_WithGovError_ReturnsErrorResponse()
    {
        // Arrange
        var remoteUserId = "123";
        var remoteToken = "secret";
        var certResponse = new CertificateResponse
        {
            CertBase64 = "cert",
        };
        handler.SetupSendAsync(HttpMethod.Get,
            new Uri(eUSignServiceUri, "api/v1/certificate").ToString())
            .ReturnsHttpResponseAsync(certResponse, HttpStatusCode.OK);

        var errorResponse = new IdGovErrorResponse
        {
            Error = 1,
        };
        
        handler.SetupSendAsync(HttpMethod.Get, new Uri(idServerUri, "/userinfo").ToString(), true)
            .ReturnsHttpResponseAsync(errorResponse, HttpStatusCode.Unauthorized);

        // Act
        var userInfo = await communicationService.GetUserInfo(remoteUserId, remoteToken);
        
        // Assert
        userInfo.AssertLeft(e =>
        {
            Assert.IsInstanceOf<ExternalAuthError>(e);
            
            var error = (ExternalAuthError) e;
            Assert.AreEqual(ExternalAuthErrorGroup.IdGovUa, error.ErrorGroup);
            Assert.AreEqual(error.HttpStatusCode, HttpStatusCode.Unauthorized);
            Assert.AreEqual(error.Message, "1");
        });
    }
    
    [Test]
    public async Task GetUserInfo_WithEUSignDecryptError_ReturnsErrorResponse()
    {
        // Arrange
        var remoteUserId = "123";
        var remoteToken = "secret";

        var certResponse = new CertificateResponse
        {
            CertBase64 = "cert",
        };
        handler.SetupSendAsync(HttpMethod.Get,
            new Uri(eUSignServiceUri, "api/v1/certificate").ToString())
            .ReturnsHttpResponseAsync(certResponse, HttpStatusCode.OK);

        var infoResponse = new EnvelopedUserInfoResponse
        {
            EncryptedUserInfo = "serialized_info",
        };

        handler.SetupSendAsync(HttpMethod.Get, new Uri(idServerUri, "/userinfo").ToString(), true)
            .ReturnsHttpResponseAsync(infoResponse, HttpStatusCode.OK);

        handler.SetupSendAsync(HttpMethod.Post, new Uri(eUSignServiceUri, "api/v1/decrypt").ToString())
            .ReturnsHttpResponseAsync(null, HttpStatusCode.InternalServerError);

        // Act
        var userInfo = await communicationService.GetUserInfo(remoteUserId, remoteToken);
        
        // Assert
        userInfo.AssertLeft(e =>
        {
            Assert.IsInstanceOf<ExternalAuthError>(e);
            
            var error = (ExternalAuthError) e;
            Assert.AreEqual(ExternalAuthErrorGroup.Encryption, error.ErrorGroup);
        });
    }
}