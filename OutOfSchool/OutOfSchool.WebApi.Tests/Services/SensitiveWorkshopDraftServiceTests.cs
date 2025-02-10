using AutoMapper;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Tests.Common.TestDataGenerators;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Config.Images;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.Services.Models;
using OutOfSchool.Common.Extensions;
using OutOfSchool.BusinessLogic.Models;
using System.Linq;
using OutOfSchool.Tests.Common;
using OutOfSchool.Services.Enums;
using System.Linq.Expressions;
using FluentAssertions;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class SensitiveWorkshopDraftServiceTests
{
    private ISensitiveWorkshopDraftService service;
    private Mock<IWorkshopDraftRepository> workshopDraftRepoMock;
    private IMapper mapper;

    private Mock<IProviderService> providerServiceMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IEntityRepository<long, Tag>> tagRepositoryMock;
    private Mock<IWorkshopServicesCombinerV2> workshopServiceCombinerV2Mock;
    private Mock<ICodeficatorService> codeficatorServiceMock; 
    private Mock<ISearchStringService> searchStringServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;

    private string userId;

    [SetUp]
    public void SetUp()
    {
        workshopDraftRepoMock = new Mock<IWorkshopDraftRepository>();

        var config = new MapperConfiguration(cfg =>
            cfg.UseProfile<CommonProfile>()
               .UseProfile<MappingProfile>()
               .UseProfile<WorkshopDraftMappingProfile>());

        mapper = config.CreateMapper();

        currentUserServiceMock = new Mock<ICurrentUserService>();
        providerServiceMock = new Mock<IProviderService>();
        tagRepositoryMock = new Mock<IEntityRepository<long, Tag>>();
        workshopServiceCombinerV2Mock = new Mock<IWorkshopServicesCombinerV2>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        searchStringServiceMock = new Mock<ISearchStringService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();

        var options = new Mock<IOptions<UploadConcurrencySettings>>();
        var settings = new UploadConcurrencySettings();
        options.Setup(o => o.Value).Returns(settings);

        var logger = new Mock<ILogger<WorkshopDraftService>>();
        var workshopDraftImagesService = new Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>>();
        var teacherDraftImagesService = new Mock<IEntityCoverImageInteractionService<TeacherDraft>>();
        var employeeService = new Mock<IEmployeeService>();     

        userId = "someUserId";

        service = new WorkshopDraftService(
                   logger.Object,
                   workshopDraftRepoMock.Object,
                   mapper,
                   workshopDraftImagesService.Object,
                   providerServiceMock.Object,
                   currentUserServiceMock.Object,
                   teacherDraftImagesService.Object,
                   tagRepositoryMock.Object,
                   options.Object,
                   employeeService.Object,
                   workshopServiceCombinerV2Mock.Object,
                   regionAdminServiceMock.Object,
                   ministryAdminServiceMock.Object,
                   codeficatorServiceMock.Object,
                   searchStringServiceMock.Object);
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

    private SearchResult<WorkshopV2Dto> SetupFetchByFilterForAdmins(
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
        var workshops = WorkshopGenerator.Generate(5).ToList();
        var workshopV2Dtos = mapper.Map<List<WorkshopV2Dto>>(workshops);
        var workshopDrafts = mapper.Map<List<WorkshopDraft>>(workshopV2Dtos);

        var ExpectedResult = mapper.Map<List<WorkshopV2Dto>>(workshopDrafts);

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

        return new SearchResult<WorkshopV2Dto>()
        {
            TotalAmount = ExpectedResult.Count,
            Entities = ExpectedResult,
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
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                    It.Is<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>(x => x == null),
                    It.Is<bool>(x => x.Equals(true))))
            .Returns(workshopDraftsReturned.AsTestAsyncEnumerableQuery());
    }
}
