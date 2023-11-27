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
public class AreaAdminServiceTests
{
    private Mock<ICodeficatorRepository> codeficatorRepositoryMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<IOptions<AuthorizationServerConfig>> identityServerConfig;
    private Mock<IOptions<CommunicationConfig>> communicationConfig;
    private Mock<IAreaAdminRepository> areaAdminRepositoryMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> userRepositoryMock;
    private IMapper mapper;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> institutionAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;

    private AreaAdminService areaAdminService;
    private AreaAdmin areaAdmin;
    private List<AreaAdmin> areaAdmins;
    private AreaAdminDto areaAdminDto;
    private List<AreaAdminDto> areaAdminsDtos;

    [SetUp]
    public void SetUp()
    {
        areaAdmin = AdminGenerator.GenerateAreaAdmin().WithUserAndInstitution();
        areaAdmins = AdminGenerator.GenerateAreaAdmins(5).WithUserAndInstitution();
        areaAdminDto = AdminGenerator.GenerateAreaAdminDto();
        areaAdminsDtos = AdminGenerator.GenerateAreaAdminsDtos(5);

        codeficatorRepositoryMock = new Mock<ICodeficatorRepository>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        httpClientFactory = new Mock<IHttpClientFactory>();
        identityServerConfig = new Mock<IOptions<AuthorizationServerConfig>>();
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

        areaAdminRepositoryMock = new Mock<IAreaAdminRepository>();
        var logger = new Mock<ILogger<AreaAdminService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        userRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        institutionAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();

        areaAdminService = new AreaAdminService(
            codeficatorRepositoryMock.Object,
            codeficatorServiceMock.Object,
            httpClientFactory.Object,
            identityServerConfig.Object,
            communicationConfig.Object,
            areaAdminRepositoryMock.Object,
            logger.Object,
            userRepositoryMock.Object,
            mapper,
            currentUserServiceMock.Object,
            institutionAdminServiceMock.Object,
            regionAdminServiceMock.Object);
    }

    [Test]
    public async Task GetById_WhenCalled_ReturnsEntity()
    {
        // Arrange
        var expected = mapper.Map<AreaAdminDto>(areaAdmin);
        areaAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(areaAdmin);

        // Act
        var result = await areaAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        areaAdminRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public async Task GetById_WhenNoRecordsInDbWithSuchId_ReturnsNullAsync()
    {
        // Act
        var result = await areaAdminService.GetByIdAsync(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByUserId_WhenCalled_ReturnsEntity()
    {
        // Arrange
        var expected = mapper.Map<AreaAdminDto>(areaAdmin);
        areaAdminRepositoryMock
            .Setup(x => x
                .GetByFilter(It.IsAny<Expression<Func<AreaAdmin, bool>>>(), It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<AreaAdmin>>(new List<AreaAdmin> { areaAdmin }));

        // Act
        var result = await areaAdminService.GetByUserId(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        areaAdminRepositoryMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public void GetByUserId_WhenCalled_ReturnsException()
    {
        // Arrange
        areaAdminRepositoryMock
            .Setup(x => x
                .GetByFilter(It.IsAny<Expression<Func<AreaAdmin, bool>>>(), It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<AreaAdmin>>(new List<AreaAdmin>()));

        // Act, Assert
        Assert.That(() => areaAdminService.GetByUserId(It.IsAny<string>()), Throws.ArgumentException);
    }

    [Test]
    public async Task GetByFilter_WhenCalled_ReturnsEntities()
    {
        // Arrange
        var expected = areaAdmins
            .Select(p => mapper.Map<AreaAdminDto>(p))
            .ToList();

        var filter = new AreaAdminFilter()
        {
            From = 1,
            Size = 5,
        };

        var areaAdminsMock = areaAdmins.AsQueryable().BuildMock();
        areaAdminRepositoryMock.Setup(repo => repo.Count(It.IsAny<Expression<Func<AreaAdmin, bool>>>()))
            .ReturnsAsync(5);
        areaAdminRepositoryMock
            .Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<string>(),
                It.IsAny<Expression<Func<AreaAdmin, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<AreaAdmin, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(areaAdminsMock);

        // Act
        var result = await areaAdminService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        areaAdminRepositoryMock.VerifyAll();
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result.Entities);
        Assert.That(result.TotalAmount, Is.EqualTo(expected.Count));
    }

    [Test]
    public void GetByFilter_WhenFilterIsInvalid_ReturnsException()
    {
        // Act
        areaAdminService.Invoking(x => x.GetByFilter(new AreaAdminFilter())).Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public void Update_WhenNullModel_ReturnsException()
    {
        // Act
        areaAdminService
            .Invoking(x => x
                .UpdateAreaAdminAsync(It.IsAny<string>(), It.IsAny<AreaAdminDto>(), It.IsAny<string>()))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public void Subordinate_WhenNullIds_ReturnsException()
    {
        // Act
        areaAdminService
            .Invoking(x => x
                .IsAreaAdminSubordinateMinistryAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Subordinate_WhenIsSubordinate_ReturnsTrue()
    {
        // Arrange
        MinistryAdminDto institutionAdmin = AdminGenerator.GenerateMinistryAdminDto();
        institutionAdmin.InstitutionId = areaAdmin.InstitutionId;
        institutionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(institutionAdmin));
        areaAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(areaAdmin));

        // Act
        var result = await areaAdminService.IsAreaAdminSubordinateMinistryAsync(institutionAdmin.Id, areaAdmin.UserId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Subordinate_WhenIsNotSubordinate_ReturnsFalse()
    {
        // Arrange
        MinistryAdminDto institutionAdmin = AdminGenerator.GenerateMinistryAdminDto();
        institutionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(institutionAdmin));
        areaAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(areaAdmin));

        // Act
        var result = await areaAdminService.IsAreaAdminSubordinateMinistryAsync(institutionAdmin.Id, areaAdmin.UserId);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Subordinate_WhenIsSubordinateToRegionAdmin_ReturnsTrue()
    {
        // Arrange
        RegionAdminDto regionAdmin = AdminGenerator.GenerateRegionAdminDto();
        regionAdmin.InstitutionId = areaAdmin.InstitutionId;
        regionAdmin.CATOTTGId = long.MinValue;
        areaAdmin.CATOTTG = new CATOTTG { Parent = new CATOTTG { Parent = new CATOTTG { Id = long.MinValue } } };
        regionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(regionAdmin));
        areaAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(areaAdmin));

        // Act
        var result = await areaAdminService.IsAreaAdminSubordinateRegionAsync(regionAdmin.Id, areaAdmin.UserId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Subordinate_WhenIsNotSubordinateToRegionAdmin_ReturnsFalse()
    {
        // Arrange
        RegionAdminDto regionAdmin = AdminGenerator.GenerateRegionAdminDto();
        regionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(regionAdmin));
        areaAdminRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(areaAdmin));

        // Act
        var result = await areaAdminService.IsAreaAdminSubordinateRegionAsync(regionAdmin.Id, areaAdmin.UserId);

        // Assert
        Assert.That(result, Is.False);
    }
}