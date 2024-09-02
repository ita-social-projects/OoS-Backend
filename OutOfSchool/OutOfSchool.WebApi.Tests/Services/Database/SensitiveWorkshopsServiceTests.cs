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
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
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
    private Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>> dateTimeRangeRepository;
    private Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>> roomRepository;
    private Mock<ITeacherService> teacherService;
    private Mock<ILogger<WorkshopService>> logger;
    private Mock<IMapper> mapperMock;
    private Mock<IImageDependentEntityImagesInteractionService<Workshop>> workshopImagesMediator;
    private Mock<IProviderAdminRepository> providerAdminRepository;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IProviderRepository> providerRepositoryMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private string userId;

    [SetUp]
    public void SetUp()
    {
        workshopRepository = new Mock<IWorkshopRepository>();
        dateTimeRangeRepository = new Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>>();
        roomRepository = new Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>>();
        teacherService = new Mock<ITeacherService>();
        logger = new Mock<ILogger<WorkshopService>>();
        mapperMock = new Mock<IMapper>();
        workshopImagesMediator = new Mock<IImageDependentEntityImagesInteractionService<Workshop>>();
        providerAdminRepository = new Mock<IProviderAdminRepository>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        providerRepositoryMock = new Mock<IProviderRepository>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();

        sensitiveWorkshopService =
            new WorkshopService(
                workshopRepository.Object,
                dateTimeRangeRepository.Object,
                roomRepository.Object,
                teacherService.Object,
                logger.Object,
                mapperMock.Object,
                workshopImagesMediator.Object,
                providerAdminRepository.Object,
                averageRatingServiceMock.Object,
                providerRepositoryMock.Object,
                currentUserServiceMock.Object,
                ministryAdminServiceMock.Object,
                regionAdminServiceMock.Object,
                codeficatorServiceMock.Object);

        userId = Guid.NewGuid().ToString();
    }

    #region FetchByFilterForAdmins

    [Test]
    public async Task FetchByFilterForAdmins_RoleRegionAdmin_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var parentCATOTTGId = 1;
        WorkshopFilterAdministration filter = new WorkshopFilterAdministration();
        var admin = new RegionAdminDto() { Id = userId, InstitutionId = institutionId, CATOTTGId = parentCATOTTGId };
        var resultExcpected = SetupFetchByFilterForAdmins(userId, true, false, parentCATOTTGId, filter, admin);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins().ConfigureAwait(false);

        // Assert
        resultExcpected.Should().BeEquivalentTo(result);
    }

    [Test]
    public void FetchByFilterForAdmins_WhenFilterIsNullRegionAdminIsNull_ShouldReturnException()
    {
        // Arrange
        var parentCATOTTGId = 1;
        RegionAdminDto admin = null;
        SetupFetchByFilterForAdmins(userId,  true, false, parentCATOTTGId, null, admin);

        // Assert
        Func<Task<SearchResult<WorkshopDto>>> result = () => sensitiveWorkshopService.FetchByFilterForAdmins();

        // Act
        result.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage($"Region admin with the specified ID: {userId} not found");
    }

    [Test]
    public async Task FetchByFilterForAdmins_WhenFilterByCATOTTGId_ShouldReturnEntities()
    {
        // Arrange
        var parentCATOTTGId = 10;
        WorkshopFilterAdministration filter = new WorkshopFilterAdministration() { From = 3, Size = 10, CATOTTGId = 1 };
        var resultExcpected = SetupFetchByFilterForAdmins(userId, false, false, parentCATOTTGId, filter);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter).ConfigureAwait(false);

        // Assert
        resultExcpected.Should().BeEquivalentTo(result);
    }

    [Test]
    public async Task FetchByFilterForAdmins_WhenRoleMinistryAdmin_ShouldReturnEntities()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var parentCATOTTGId = 1;
        var admin = new MinistryAdminDto() { Id = userId, InstitutionId = institutionId };
        WorkshopFilterAdministration filter = new WorkshopFilterAdministration() { Size = 8, CATOTTGId = 1, From = 2, SearchString = "key word" };
        var resultExcpected = SetupFetchByFilterForAdmins(userId, false, true, parentCATOTTGId, filter, null, admin);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter).ConfigureAwait(false);

        // Assert
        resultExcpected.Should().BeEquivalentTo(result);
    }

    #endregion

    #region With

    private SearchResult<WorkshopDto> SetupFetchByFilterForAdmins(
        string userId, bool isRegionAdmin, bool isMinistryAdmin, long parentCATOTTGId, WorkshopFilterAdministration filter, RegionAdminDto adminRegion = null, MinistryAdminDto adminMinistry = null)
    {
        var workshops = WithListOfWorkshops().ToList();
        var workshopsDto = WithListOfWorkshopsDto().ToList();
        IEnumerable<long> subSettlementsIds = new List<long>() { 1, 2, 3 };
        SetUpCurrentUserService(userId, isRegionAdmin, isMinistryAdmin);
        SetupRegionAdminService(userId, adminRegion);
        SetupMinistryAdminService(userId, adminMinistry);
        SetUpCodeficatorService(subSettlementsIds, parentCATOTTGId);
        SetUpWorkshopsRepository(workshops, filter);
        SetUpMapper(workshopsDto, workshops);
        var resultExcpected = new SearchResult<WorkshopDto> { TotalAmount = workshopsDto.Count, Entities = workshopsDto };
        return resultExcpected;
    }

    private IEnumerable<Workshop> WithListOfWorkshops()
    {
        var workshops = WorkshopGenerator.Generate(5);
        return workshops;
    }

    private IEnumerable<WorkshopDto> WithListOfWorkshopsDto()
    {
        var workshops = WorkshopDtoGenerator.Generate(5);
        return workshops;
    }

    private void SetUpCurrentUserService(string userId, bool isRegionAdmin = false, bool isMinistryAdmin = false)
    {
        currentUserServiceMock.Setup(u => u.IsRegionAdmin()).Returns(isRegionAdmin);
        currentUserServiceMock.Setup(u => u.IsMinistryAdmin()).Returns(isMinistryAdmin);
        currentUserServiceMock.Setup(u => u.UserId).Returns(userId);
    }

    private void SetupRegionAdminService(string userId, RegionAdminDto admin)
    {
        regionAdminServiceMock.Setup(a => a.GetByUserId(userId)).ReturnsAsync(admin);
    }

    private void SetupMinistryAdminService(string userId, MinistryAdminDto ministryAdmin)
    {
        ministryAdminServiceMock.Setup(a => a.GetByUserId(userId)).ReturnsAsync(ministryAdmin);
    }

    private void SetUpCodeficatorService(IEnumerable<long> subSettlementsIds, long parentId)
    {
        codeficatorServiceMock.Setup(c => c.GetAllChildrenIdsByParentIdAsync(parentId)).ReturnsAsync(subSettlementsIds);
    }

    private void SetUpWorkshopsRepository(List<Workshop> workshopsReturned, WorkshopFilterAdministration filter = null)
    {
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

    private void SetUpMapper(IEnumerable<WorkshopDto> returnedDto, IEnumerable<Workshop> workshops)
    {
        mapperMock.Setup(m => m.Map<IEnumerable<WorkshopDto>>(workshops)).Returns(returnedDto);
    }

    #endregion
}