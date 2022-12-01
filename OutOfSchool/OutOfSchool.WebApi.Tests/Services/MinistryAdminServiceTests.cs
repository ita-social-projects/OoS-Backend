using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class MinistryAdminServiceTests
{
    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<IOptions<IdentityServerConfig>> identityServerConfig;
    private Mock<IOptions<CommunicationConfig>> communicationConfig;
    private Mock<IInstitutionAdminRepository> institutionAdminRepositoryMock;
    private Mock<IEntityRepository<string, User>> userRepositoryMock;
    private IMapper mapper;

    private MinistryAdminService ministryAdminService;
    private InstitutionAdmin institutionAdmin;
    private List<InstitutionAdmin> institutionAdmins;

    [SetUp]
    public void SetUp()
    {
        institutionAdmin = AdminGenerator.GenerateInstitutionAdmin().WithUserAndInstitution();
        institutionAdmins = AdminGenerator.GenerateInstitutionAdmins(5).WithUserAndInstitution();

        httpClientFactory = new Mock<IHttpClientFactory>();
        identityServerConfig = new Mock<IOptions<IdentityServerConfig>>();
        communicationConfig = new Mock<IOptions<CommunicationConfig>>();
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

        institutionAdminRepositoryMock = new Mock<IInstitutionAdminRepository>();
        var logger = new Mock<ILogger<MinistryAdminService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        userRepositoryMock = new Mock<IEntityRepository<string, User>>();

        ministryAdminService = new MinistryAdminService(
            httpClientFactory.Object,
            identityServerConfig.Object,
            communicationConfig.Object,
            institutionAdminRepositoryMock.Object,
            logger.Object,
            userRepositoryMock.Object,
            mapper);
    }

    [Test]
    public async Task GetById_WhenCalled_ReturnsEntity()
    {
        // Arrange
        var expected = institutionAdmin;
        institutionAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(institutionAdmin);

        // Act
        var result = await ministryAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        institutionAdminRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        TestHelper.AssertDtosAreEqual(mapper.Map<MinistryAdminDto>(expected), result);
    }

    [Test]
    public async Task GetById_WhenNoRecordsInDbWithSuchId_ReturnsNullAsync()
    {
        // Act
        var result = await ministryAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByFilter_WhenCalled_ReturnsEntities()
    {
        // Arrange
        var expected = institutionAdmins
            .Select(p => mapper.Map<MinistryAdminDto>(p))
            .ToList();

        var filter = new MinistryAdminFilter()
        {
            From = 1,
            Size = 5,
        };

        var institutionAdminsMock = institutionAdmins.AsQueryable().BuildMock();
        institutionAdminRepositoryMock.Setup(repo => repo.Count(It.IsAny<Expression<Func<InstitutionAdmin, bool>>>()))
            .ReturnsAsync(5);
        institutionAdminRepositoryMock
            .Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<string>(),
                It.IsAny<Expression<Func<InstitutionAdmin, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<InstitutionAdmin, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(institutionAdminsMock);

        // Act
        var result = await ministryAdminService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        institutionAdminRepositoryMock.VerifyAll();
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result.Entities);
        Assert.AreEqual(expected.Count, result.TotalAmount);
    }

    [Test]
    public void GetByFilter_WhenFilterIsInvalid_ReturnsException()
    {
        // Act
        ministryAdminService.Invoking(x => x.GetByFilter(new MinistryAdminFilter())).Should()
            .ThrowAsync<ArgumentNullException>();
    }
}
