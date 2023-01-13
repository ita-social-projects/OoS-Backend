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
public class RegionAdminServiceTests
{
    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<IOptions<IdentityServerConfig>> identityServerConfig;
    private Mock<IOptions<CommunicationConfig>> communicationConfig;
    private Mock<IRegionAdminRepository> regionAdminRepositoryMock;
    private Mock<IEntityRepository<string, User>> userRepositoryMock;
    private IMapper mapper;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;

    private RegionAdminService regionAdminService;
    private RegionAdmin regionAdmin;
    private List<RegionAdmin> regionAdmins;
    private RegionAdminDto regionAdminDto;
    private List<RegionAdminDto> regionAdminsDtos;

    [SetUp]
    public void SetUp()
    {
        regionAdmin = AdminGenerator.GenerateRegionAdmin().WithUserAndInstitution();
        regionAdmins = AdminGenerator.GenerateRegionAdmins(5).WithUserAndInstitution();
        regionAdminDto = AdminGenerator.GenerateRegionAdminDto();
        regionAdminsDtos = AdminGenerator.GenerateRegionAdminsDtos(5);

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

        regionAdminRepositoryMock = new Mock<IRegionAdminRepository>();
        var logger = new Mock<ILogger<RegionAdminService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        userRepositoryMock = new Mock<IEntityRepository<string, User>>();
        ministryAdminServiceMock= new Mock<IMinistryAdminService>();

        regionAdminService = new RegionAdminService(
            httpClientFactory.Object,
            identityServerConfig.Object,
            communicationConfig.Object,
            regionAdminRepositoryMock.Object,
            logger.Object,
            userRepositoryMock.Object,
            mapper,
            ministryAdminServiceMock.Object);
    }

    [Test]
    public async Task GetById_WhenCalled_ReturnsEntity()
    {
        // Arrange
        var expected = mapper.Map<RegionAdminDto>(regionAdmin);
        regionAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(regionAdmin);

        // Act
        var result = await regionAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        regionAdminRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public async Task GetById_WhenNoRecordsInDbWithSuchId_ReturnsNullAsync()
    {
        // Act
        var result = await regionAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByFilter_WhenCalled_ReturnsEntities()
    {
        // Arrange
        var expected = regionAdmins
            .Select(p => mapper.Map<RegionAdminDto>(p))
            .ToList();

        var filter = new RegionAdminFilter()
        {
            From = 1,
            Size = 5,
        };

        var regionAdminsMock = regionAdmins.AsQueryable().BuildMock();
        regionAdminRepositoryMock.Setup(repo => repo.Count(It.IsAny<Expression<Func<RegionAdmin, bool>>>()))
            .ReturnsAsync(5);
        regionAdminRepositoryMock
            .Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<string>(),
                It.IsAny<Expression<Func<RegionAdmin, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<RegionAdmin, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(regionAdminsMock);

        // Act
        var result = await regionAdminService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        regionAdminRepositoryMock.VerifyAll();
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result.Entities);
        Assert.That(result.TotalAmount, Is.EqualTo(expected.Count));
    }

    [Test]
    public void GetByFilter_WhenFilterIsInvalid_ReturnsException()
    {
        // Act
        regionAdminService.Invoking(x => x.GetByFilter(new RegionAdminFilter())).Should()
            .ThrowAsync<ArgumentNullException>();
    }
}
