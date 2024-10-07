using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using GrpcServiceServer;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.AuthCommon.Util;
using OutOfSchool.AuthServer.Tests.Util;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Tests.Common;

namespace OutOfSchool.AuthServer.Tests.Services;

[TestFixture]
public class ProviderAdminServiceGrpcTests
{
    private ProviderAdminServiceGrpc providerAdminServiceGrpc;
    private Mock<IEmployeeService> providerAdminServiceMock;
    private TestServerCallContext serverCallContextMock;
    private Mock<HttpContext> httpContextMock;
    private string userId;

    [SetUp]
    public void SetUp()
    {
        userId = Guid.NewGuid().ToString();
        httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.User.FindFirst(IdentityResourceClaimsTypes.Sub))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));

        providerAdminServiceMock = new Mock<IEmployeeService>();
        serverCallContextMock = new TestServerCallContext(new Dictionary<object, object>()
        {
            {"__HttpContext", httpContextMock.Object}
        });
        var mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        providerAdminServiceGrpc = new ProviderAdminServiceGrpc(providerAdminServiceMock.Object, mapper);
    }

    #region CreateProviderAdmin

    [Test]
    public async Task CreateProviderAdmin_WhenEntityValid_ShouldCreateNewProviderAdmin()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        var request = MockRequest(providerId);

        var response = MockResponse(true, providerId);

        providerAdminServiceMock.Setup(
                p => p.CreateEmployeeAsync(It.IsAny<CreateEmployeeDto>(), null, userId))
            .ReturnsAsync(response);

        // Act
        var result = await providerAdminServiceGrpc.CreateProviderAdmin(request, serverCallContextMock);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateProviderAdmin_WhenEntityInvalid_ShouldReturnSuccessFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        var request = MockRequest(providerId);

        var response = MockResponse(false, providerId);

        providerAdminServiceMock.Setup(
                p => p.CreateEmployeeAsync(It.IsAny<CreateEmployeeDto>(), null, userId))
            .ReturnsAsync(response);

        // Act
        var result = await providerAdminServiceGrpc.CreateProviderAdmin(request, serverCallContextMock);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    private static CreateProviderAdminRequest MockRequest(Guid providerId)
    {
        return new CreateProviderAdminRequest()
        {
            FirstName = "Example",
            MiddleName = "Example",
            LastName = "Example",
            Email = "test@test.com",
            IsDeputy = true,
            CreatingTime = new Timestamp(),
            PhoneNumber = "+380671234567",
            ProviderId = providerId.ToString(),
            RequestId = Guid.NewGuid().ToString()
        };
    }

    private static ResponseDto MockResponse(bool isSuccess, Guid providerId)
    {
        return new ResponseDto()
        {
            IsSuccess = isSuccess,
            Result = new CreateEmployeeDto()
            {
                FirstName = "Example",
                MiddleName = "Example",
                LastName = "Example",
                Email = "test@test.com",
                PhoneNumber = "+380671234567",
                ProviderId = providerId,
                UserId = Guid.NewGuid().ToString(),
                ReturnUrl = "example.com",
                ManagedWorkshopIds = new List<Guid>(),
            },
        };
    }
}