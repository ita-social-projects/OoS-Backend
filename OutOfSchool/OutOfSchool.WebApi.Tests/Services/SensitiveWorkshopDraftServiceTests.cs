using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config.Images;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class SensitiveWorkshopDraftServiceTests
{
    private ISensitiveWorkshopDraftService service;
    private Mock<IWorkshopDraftRepository> workshopDraftRepoMock;

    private Mock<IProviderService> providerServiceMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IEntityRepository<long, Tag>> tagRepositoryMock;
    private Mock<IWorkshopServicesCombinerV2> workshopServiceCombinerV2Mock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<ISearchStringService> searchStringServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IInstitutionHierarchyRepository> institutionHierarchyRepositoryMock;
    private Mock<ICodeficatorRepository> codeficatorRepository;

    private string userId;

    [SetUp]
    public void SetUp()
    {
        workshopDraftRepoMock = new Mock<IWorkshopDraftRepository>();

        currentUserServiceMock = new Mock<ICurrentUserService>();
        providerServiceMock = new Mock<IProviderService>();
        tagRepositoryMock = new Mock<IEntityRepository<long, Tag>>();
        workshopServiceCombinerV2Mock = new Mock<IWorkshopServicesCombinerV2>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        searchStringServiceMock = new Mock<ISearchStringService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        institutionHierarchyRepositoryMock = new Mock<IInstitutionHierarchyRepository>();
        codeficatorRepository = new Mock<ICodeficatorRepository>();

        var options = new Mock<IOptions<UploadConcurrencySettings>>();
        var settings = new UploadConcurrencySettings();
        options.Setup(o => o.Value).Returns(settings);

        var logger = new Mock<ILogger<WorkshopDraftService>>();
        var workshopDraftImagesService = new Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>>();
        var teacherDraftImagesService = new Mock<IEntityCoverImageInteractionService<TeacherDraft>>();

        userId = "someUserId";

        service = new WorkshopDraftService(
                   logger.Object,
                   workshopDraftRepoMock.Object,
                   workshopDraftImagesService.Object,
                   providerServiceMock.Object,
                   currentUserServiceMock.Object,
                   teacherDraftImagesService.Object,
                   tagRepositoryMock.Object,
                   options.Object,
                   workshopServiceCombinerV2Mock.Object,
                   regionAdminServiceMock.Object,
                   ministryAdminServiceMock.Object,
                   codeficatorServiceMock.Object,
                   searchStringServiceMock.Object,
                   institutionHierarchyRepositoryMock.Object,
                   codeficatorRepository.Object);
    }

    #region FetchByFilterForAdmins    
    [Test]
    public async Task FetchByFilterForAdmins_RoleRegionAdmin_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var parentCATOTTGid = 11;
        var subSettlementsIds = new List<long>() { parentCATOTTGid, 12, 13 };
        var filter = new WorkshopDraftFilterAdministration();
        var admin = new RegionAdminDto() { Id = userId, InstitutionId = institutionId, CATOTTGId = parentCATOTTGid };
        var resultExpected = SetupFetchByFilterForAdmins(userId, true, false, parentCATOTTGid, filter, subSettlementsIds, admin);

        // Act
        var result = await service.FetchByFilterForAdmins()
            .ConfigureAwait(false);

        // Assert
        result.Should()
            .BeEquivalentTo(resultExpected);

        codeficatorServiceMock.Verify(
            s => s.GetAllChildrenIdsByParentIdAsync(It.Is<long>(s => s == parentCATOTTGid)), Times.Once);

        regionAdminServiceMock.Verify(
            r => r.GetByUserId(It.Is<string>(id => id == userId)));
    }

    [Test]
    public async Task FetchByFilterForAdmins_FilteredBySearchString_ShouldReturnEntities()
    {
        // Arrange        
        var filterWorkshop = new WorkshopDraftFilterAdministration()
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
        var result = await service.FetchByFilterForAdmins(filterWorkshop)
            .ConfigureAwait(false);

        // Assert
        result.Should()
            .BeEquivalentTo(resultExpected);

        searchStringServiceMock.VerifyAll();
        workshopDraftRepoMock.VerifyAll();
    }
    #endregion

    private SearchResult<WorkshopDraftResponseDto> SetupFetchByFilterForAdmins(
        string userId = null,
        bool isRegionAdmin = false,
        bool isMinistryAdmin = false,
        long parentCATOTTGId = 0,
        WorkshopDraftFilterAdministration filter = null,
        IEnumerable<long> subSettlementsIds = null,
        RegionAdminDto adminRegion = null,
        MinistryAdminDto adminMinistry = null,
        string[] searchWords = null)
    {
        var workshops = WorkshopV2DtoGenerator.Generate(5).ToList();
        var workshopDrafts = workshops.ToDraft();
        var workshopDraftDtos = workshopDrafts.ToResponseDto();

        SetUpCurrentUserService(userId, isRegionAdmin, isMinistryAdmin);
        SetUpWorkshopsRepository(workshopDrafts, filter);

        regionAdminServiceMock.Setup(a => a.GetByUserId(userId))
            .ReturnsAsync(adminRegion);

        ministryAdminServiceMock.Setup(a => a.GetByUserId(userId))
            .ReturnsAsync(adminMinistry);

        codeficatorServiceMock.Setup(c => c.GetAllChildrenIdsByParentIdAsync(parentCATOTTGId))
            .ReturnsAsync(subSettlementsIds);

        searchStringServiceMock.Setup(s => s.SplitSearchString(It.Is<string>(x => x == filter.SearchString)))
            .Returns(searchWords);

        institutionHierarchyRepositoryMock.Setup(
            x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<InstitutionHierarchy, object>>, SortDirection>>()))
            .Returns(new List<InstitutionHierarchy>().AsTestAsyncEnumerableQuery());

        codeficatorRepository.Setup(
            x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<CATOTTG, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsTestAsyncEnumerableQuery());

        return new SearchResult<WorkshopDraftResponseDto>()
        {
            TotalAmount = workshopDraftDtos.Count,
            Entities = workshopDraftDtos,
        };
    }

    private void SetUpCurrentUserService(string userId, bool isRegionAdmin = false, bool isMinistryAdmin = false)
    {
        currentUserServiceMock.Setup(u => u.IsRegionAdmin()).Returns(isRegionAdmin);
        currentUserServiceMock.Setup(u => u.IsMinistryAdmin()).Returns(isMinistryAdmin);
        currentUserServiceMock.Setup(u => u.UserId).Returns(userId);
    }

    private void SetUpWorkshopsRepository(List<WorkshopDraft> workshopDraftsReturned, WorkshopDraftFilterAdministration filter = null)
    {
        workshopDraftRepoMock.Setup(
            x => x.Count(It.IsAny<Expression<Func<WorkshopDraft, bool>>>()))
            .ReturnsAsync(workshopDraftsReturned.Count);

        workshopDraftRepoMock.Setup(
                w => w.Get(
                    It.Is<int>(x => x == filter.From),
                    It.Is<int>(x => x == filter.Size),
                    It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                    It.Is<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>(x => x == null)))
            .Returns(workshopDraftsReturned.AsTestAsyncEnumerableQuery());
    }
}
