using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class SensitiveWorkshopsServiceTests
{
    private readonly string includingPropertiesForMappingDtoModel = $"{nameof(Workshop.Address)},{nameof(Workshop.Teachers)}," +
        $"{nameof(Workshop.DateTimeRanges)},{nameof(Workshop.InstitutionHierarchy)}";

    private ISensitiveWorkshopsService sensitiveWorkshopService;
    private Mock<IWorkshopRepository> workshopRepository;
    private Mock<IMapper> mapperMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<ITagService> tagServiceMock;
    private Mock<ISearchStringService> searchStringServiceMock;
    private Mock<IEntityRepository<long, Tag>> tagRepository;

    [SetUp]
    public void SetUp()
    {
        workshopRepository = new Mock<IWorkshopRepository>();
        mapperMock = new Mock<IMapper>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        tagServiceMock = new Mock<ITagService>();
        searchStringServiceMock = new Mock<ISearchStringService>();
        tagRepository = new Mock<IEntityRepository<long, Tag>>();

        sensitiveWorkshopService =
            new WorkshopService(
                workshopRepository.Object,
                tagRepository.Object,
                new Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>>().Object,
                new Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>>().Object,
                new Mock<ITeacherService>().Object,
                new Mock<ILogger<WorkshopService>>().Object,
                mapperMock.Object,
                new Mock<IImageDependentEntityImagesInteractionService<Workshop>>().Object,
                new Mock<IEmployeeRepository>().Object,
                new Mock<IAverageRatingService>().Object,
                new Mock<IProviderRepository>().Object,
                currentUserServiceMock.Object,
                ministryAdminServiceMock.Object,
                regionAdminServiceMock.Object,
                codeficatorServiceMock.Object,
                tagServiceMock.Object,
                searchStringServiceMock.Object);
    }

    #region FetchByFilterForAdmins

    [Test]
    public async Task FetchByFilterForAdmins_RoleRegionAdmin_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var userId = Guid.Parse("67a6c5b2-73b9-4c5d-b541-e59fb4a43ef6").ToString();
        var parentCATOTTGid = 11;
        var subSettlementsIds = new List<long>() { parentCATOTTGid, 12, 13 };
        var filter = new WorkshopFilterAdministration();
        var admin = new RegionAdminDto() { Id = userId, InstitutionId = institutionId, CATOTTGId = parentCATOTTGid };
        var resultExpected = SetupFetchByFilterForAdmins(userId, true, false, parentCATOTTGid, filter, subSettlementsIds, admin);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins()
            .ConfigureAwait(false);

        // Assert
        resultExpected.Should()
            .BeEquivalentTo(result);

        codeficatorServiceMock.Verify(
            s => s.GetAllChildrenIdsByParentIdAsync(It.Is<long>(s => s == parentCATOTTGid)), Times.Once);

        regionAdminServiceMock.Verify(
            r => r.GetByUserId(It.Is<string>(id => id == userId)));
    }

    [Test]
    public void FetchByFilterForAdmins_WhenFilterIsNullRegionAdminIsNull_ShouldReturnException()
    {
        // Arrange
        var codeficatorId = 10;
        var userId = Guid.Parse("f9d79c19-17f0-4cbe-8a2f-6e299983bdc7").ToString();
        var subSettlementsIds = new List<long>() { codeficatorId };
        RegionAdminDto admin = null;
        SetupFetchByFilterForAdmins(userId, true, false, codeficatorId, null, subSettlementsIds, admin);

        // Assert
        Func<Task<SearchResult<WorkshopDto>>> result = () => sensitiveWorkshopService.FetchByFilterForAdmins();

        // Act
        result.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Region admin with the specified ID: {userId} not found");
    }

    [Test]
    public async Task FetchByFilterForAdmins_ByFilterСriteria_ShouldReturnEntities()
    {
        // Arrange
        var codeficatorId = 10;
        var subSettlementsIds = new List<long>() { codeficatorId };
        var userId = Guid.Parse("d112e6b9-8cb3-4b62-9d7e-2dff7588f83f").ToString();
        var filter = new WorkshopFilterAdministration()
        {
            From = 2,
            Size = 2,
            CATOTTGId = codeficatorId,
            SearchString = "   ",
        };
        var resultExpected = SetupFetchByFilterForAdmins(userId, false, false, codeficatorId, filter, subSettlementsIds);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter)
            .ConfigureAwait(false);

        // Assert
        resultExpected.Should()
            .BeEquivalentTo(result);
    }

    [Test]
    public async Task FetchByFilterForAdmins_WhenRoleMinistryAdmin_ShouldReturnEntities()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var parentCATOTTGId = 11;
        var userId = Guid.Parse("2c8a3a36-53c8-4a2d-9a73-b223b611d469").ToString();
        var subSettlementsIds = new List<long>() { parentCATOTTGId, 40, 30 };
        var admin = new MinistryAdminDto() { Id = userId, InstitutionId = institutionId };
        var filterWorkshop = new WorkshopFilterAdministration()
        {
            Size = 8,
            CATOTTGId = 30,
            SearchString = null,
        };
        var resultExpected = SetupFetchByFilterForAdmins(userId, false, true, parentCATOTTGId, filterWorkshop, subSettlementsIds, null, admin);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filterWorkshop)
            .ConfigureAwait(false);

        // Assert
        resultExpected.Should()
            .BeEquivalentTo(result);

        ministryAdminServiceMock.Verify(m =>
        m.GetByUserId(It.Is<string>(id => id == userId)));

        codeficatorServiceMock.Verify(
            s => s.GetAllChildrenIdsByParentIdAsync(It.Is<long>(s => s == filterWorkshop.CATOTTGId)), Times.Once);
    }

    [Test]
    public async Task FetchByFilterForAdmins_FilteredBySearchString_ShouldReturnEntities()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var filterWorkshop = new WorkshopFilterAdministration()
        {
            SearchString = "Шахмати для початківців",
        };

        var resultExpected = SetupFetchByFilterForAdmins(
            userId: userId,
            isRegionAdmin: false,
            isMinistryAdmin: false,
            parentCATOTTGId: 0,
            filter: filterWorkshop,
            subSettlementsIds: null,
            adminRegion: null,
            adminMinistry: null,
            searchWords: ["Шахмати", "для", "початківців"]);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filterWorkshop)
            .ConfigureAwait(false);

        // Assert
        result.Should()
            .BeEquivalentTo(resultExpected);

        searchStringServiceMock.VerifyAll();
        workshopRepository.VerifyAll();
    }
    #endregion

    #region With

    private SearchResult<WorkshopDto> SetupFetchByFilterForAdmins(
        string userId = null,
        bool isRegionAdmin = false,
        bool isMinistryAdmin = false,
        long parentCATOTTGId = 0,
        WorkshopFilterAdministration filter = null,
        IEnumerable<long> subSettlementsIds = null,
        RegionAdminDto adminRegion = null,
        MinistryAdminDto adminMinistry = null,
        string[] searchWords = null)
    {
        var workshops = WorkshopGenerator.Generate(5).ToList();
        var workshopsDto = WorkshopDtoGenerator.Generate(5).ToList();

        SetUpCurrentUserService(userId, isRegionAdmin, isMinistryAdmin);
        SetUpWorkshopsRepository(workshops, filter);

        regionAdminServiceMock.Setup(a => a.GetByUserId(userId))
            .ReturnsAsync(adminRegion);

        ministryAdminServiceMock.Setup(a => a.GetByUserId(userId))
            .ReturnsAsync(adminMinistry);

        codeficatorServiceMock.Setup(c => c.GetAllChildrenIdsByParentIdAsync(parentCATOTTGId))
            .ReturnsAsync(subSettlementsIds);

        mapperMock.Setup(m => m.Map<IEnumerable<WorkshopDto>>(workshops))
            .Returns(workshopsDto);

        searchStringServiceMock.Setup(s => s.SplitSearchString(It.Is<string>(x => x == filter.SearchString)))
            .Returns(searchWords);

        var resultExpected = new SearchResult<WorkshopDto>()
        {
            TotalAmount = workshopsDto.Count,
            Entities = workshopsDto,
        };

        return resultExpected;
    }

    private void SetUpCurrentUserService(string userId, bool isRegionAdmin = false, bool isMinistryAdmin = false)
    {
        currentUserServiceMock.Setup(u => u.IsRegionAdmin()).Returns(isRegionAdmin);
        currentUserServiceMock.Setup(u => u.IsMinistryAdmin()).Returns(isMinistryAdmin);
        currentUserServiceMock.Setup(u => u.UserId).Returns(userId);
    }

    private void SetUpWorkshopsRepository(List<Workshop> workshopsReturned, WorkshopFilterAdministration filter = null)
    {
        workshopRepository.Setup(
            x => x.Count(It.IsAny<Expression<Func<Workshop, bool>>>()))
            .ReturnsAsync(workshopsReturned.Count);

        workshopRepository.Setup(
                w => w.Get(
                    It.Is<int>(x => x == filter.From),
                    It.Is<int>(x => x == filter.Size),
                    It.Is<string>(x => x.Equals(includingPropertiesForMappingDtoModel)),
                    It.IsAny<Expression<Func<Workshop, bool>>>(),
                    It.Is<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>(x => x == null),
                    It.Is<bool>(x => x.Equals(true))))
            .Returns(workshopsReturned.AsTestAsyncEnumerableQuery());
    }

    #endregion
}