﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Services.ProviderAdminOperations;
using OutOfSchool.Common;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Tests.ProviderAdminOperations;

[TestFixture]
public class ProviderAdminOperationsRESTServiceTests
{
    private Mock<ProviderAdminOperationsRESTService> providerAdminOperationsRESTService;

    [SetUp]
    public void SetUp()
    {
        var logger = new Mock<ILogger<ProviderAdminOperationsRESTService>>();
        var authConfig = new Mock<IOptions<AuthorizationServerConfig>>();
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var communicationConfig = new Mock<IOptions<CommunicationConfig>>();

        communicationConfig.Setup(x => x.Value)
            .Returns(new CommunicationConfig()
            {
                ClientName = "test",
                TimeoutInSeconds = 2,
                MaxNumberOfRetries = 7,
            });
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient()
            {
                Timeout = new TimeSpan(2),
                BaseAddress = It.IsAny<Uri>(),
            });

        authConfig.Setup(x => x.Value).Returns(new AuthorizationServerConfig()
        {
            Authority = new Uri("https://www.test.com"),
        });

        providerAdminOperationsRESTService = new Mock<ProviderAdminOperationsRESTService>(
            logger.Object,
            authConfig.Object,
            httpClientFactory.Object,
            communicationConfig.Object)
        {
            CallBase = true,
        };
    }

    [Test]
    public async Task CreateProviderAdminAsync_WhenModelValid_ShouldReturnCreatedEntity()
    {
        // Arrange
        var providerAdminDto = new CreateProviderAdminDto();

        providerAdminOperationsRESTService.Setup(x => x.SendRequest<ResponseDto>(It.IsAny<Request>()))
            .ReturnsAsync(new ResponseDto()
            {
                HttpStatusCode = HttpStatusCode.Created,
            });

        // Act
        var result = await providerAdminOperationsRESTService.Object.CreateProviderAdminAsync(It.IsAny<string>(), providerAdminDto, It.IsAny<string>());

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.Match(left => HttpStatusCode.BadRequest, right => HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateProviderAdminAsync_WhenModelInValid_ShouldReturnBadRequest()
    {
        // Arrange
        var providerAdminDto = new CreateProviderAdminDto();

        providerAdminOperationsRESTService.Setup(x => x.SendRequest<ResponseDto>(It.IsAny<Request>()))
            .ReturnsAsync(new ResponseDto()
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
            });

        // Act
        var result = await providerAdminOperationsRESTService.Object.CreateProviderAdminAsync(It.IsAny<string>(), providerAdminDto, It.IsAny<string>());

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.Match(left => HttpStatusCode.BadRequest, right => HttpStatusCode.OK));
    }
}
