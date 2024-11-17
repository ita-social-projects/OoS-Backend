using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Changes;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ChangesLogServiceTests
{
    private Mock<ILogger<ChangesLogService>> logger;
    private Mock<IMapper> mapper;
    private Mock<IChangesLogRepository> changesLogRepository;
    private Mock<IProviderRepository> providerRepository;
    private Mock<IApplicationRepository> applicationRepository;
    private Mock<IEntityRepository<long, EmployeeChangesLog>> employeeChangesLogRepository;
    private Mock<IEntityAddOnlyRepository<long, ParentBlockedByAdminLog>> parentBlockedByAdminLogRepository;
    private Mock<IValueProjector> valueProjector;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IAreaAdminService> areaAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;

    private User user;
    private Parent parent;
    private User parentUser;
    private Provider provider;
    private Workshop workshop;
    private Application application;

    [SetUp]
    public void SetUp()
    {
        user = UserGenerator.Generate();
        parent = ParentGenerator.Generate();
        parentUser = UserGenerator.Generate();
        parent.User = parentUser;
        provider = ProvidersGenerator.Generate();
        provider.Institution = InstitutionsGenerator.Generate();
        workshop = WorkshopGenerator.Generate();
        workshop.Address = AddressGenerator.Generate();
        workshop.Provider = provider;
        application = new Application()
        {
            Id = new Guid("6d4caeae-f0c3-492e-99b0-c8c105693376"),
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
            Workshop = workshop,
            CreationTime = new DateTimeOffset(2022, 01, 12, 12, 34, 15, TimeSpan.Zero),
            Status = ApplicationStatus.Pending,
            ChildId = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"),
            ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
        };

        logger = new Mock<ILogger<ChangesLogService>>();
        mapper = new Mock<IMapper>(MockBehavior.Strict);
        changesLogRepository = new Mock<IChangesLogRepository>(MockBehavior.Strict);
        providerRepository = new Mock<IProviderRepository>(MockBehavior.Strict);
        applicationRepository = new Mock<IApplicationRepository>(MockBehavior.Strict);
        employeeChangesLogRepository = new Mock<IEntityRepository<long, EmployeeChangesLog>>(MockBehavior.Strict);
        parentBlockedByAdminLogRepository = new Mock<IEntityAddOnlyRepository<long, ParentBlockedByAdminLog>>();
        valueProjector = new Mock<IValueProjector>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock= new Mock<IRegionAdminService>();
        areaAdminServiceMock = new Mock<IAreaAdminService>();
        codeficatorServiceMock= new Mock<ICodeficatorService>();
    }

    #region AddEntityChangesToDbContext
    [Test]
    public void AddEntityChangesToDbContext_WhenTrackingIsEnabledForTheEntity_AddsToContext()
    {
        // Arrange
        var options = CreateChangesLogOptions();
        changesLogRepository.Setup(repo => repo.AddChangesLogToDbContext(
                It.IsAny<Provider>(),
                It.IsAny<string>(),
                options.Value.TrackedProperties["Provider"],
                It.IsAny<Func<Type, object, string>>()))
            .Returns(new List<ChangesLog> { new ChangesLog() });
        var changesLogService = GetChangesLogService();

        // Act
        var result = changesLogService.AddEntityChangesToDbContext(new Provider(), user.Id);

        // Assert
        Assert.AreEqual(1, result);
    }

    [Test]
    public void AddEntityChangesToDbContext_WhenTrackingIsNotEnabledForTheEntity_DoesNotLogChanges()
    {
        // Arrange
        var changesLogService = GetChangesLogService();

        // Act
        var result = changesLogService.AddEntityChangesToDbContext(new Address(), user.Id);

        // Assert
        Assert.AreEqual(0, result);
    }
    #endregion

    #region AddCreatingOfEntityToDbContext
    [Test]
    public async Task AddCreatingOfEntityToDbContext_WhenTrackingIsEnabledForTheEntity_AddsToContext()
    {
        // Arrange
        var options = CreateChangesLogOptions();
        changesLogRepository.Setup(repo => repo.AddCreatingOfEntityToChangesLog(
                It.IsAny<Provider>(),
                It.IsAny<string>()))
            .ReturnsAsync(new ChangesLog());
        var changesLogService = GetChangesLogService();

        // Act
        var result = await changesLogService.AddCreatingOfEntityToDbContext(new Provider(), user.Id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(true, result);
    }

    [Test]
    public async Task AddCreatingOfEntityToDbContext_WhenTrackingIsNotEnabledForTheEntity_DoesNotLogChanges()
    {
        // Arrange
        var changesLogService = GetChangesLogService();

        // Act
        var result = await changesLogService.AddCreatingOfEntityToDbContext(new Address(), user.Id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(false, result);
    }
    #endregion

    #region GetChangesLog
    [Test]
    public async Task GetProviderChangesLog_WhenCalled_ReturnsSearchResult()
    {
        // Arange
        var changesLogService = GetChangesLogService();
        var request = new ProviderChangesLogRequest();

        var entitiesCount = 5;
        var totalAmount = 5;
        var changesMock = Enumerable.Range(1, entitiesCount)
            .Select(x => new ChangesLog { Id = x, EntityIdGuid = provider.Id, User = user })
            .AsQueryable()
            .BuildMock();
        var providersMock = new List<Provider> { provider }
            .AsQueryable()
            .BuildMock();

        mapper.Setup(m => m.Map<ChangesLogFilter>(It.IsAny<ProviderChangesLogRequest>()))
            .Returns(new ChangesLogFilter());
        mapper.Setup(m => m.Map<ShortUserDto>(user))
            .Returns(new ShortUserDto { Id = user.Id });

        changesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        changesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                string.Empty,
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(changesMock);
        providerRepository.Setup(repo => repo.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(providersMock);

        // Act
        var result = await changesLogService.GetProviderChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.Any(x => x.ProviderId == provider.Id));
        Assert.True(result.Entities.Any(x => x.ProviderTitle == provider.FullTitle));
        Assert.True(result.Entities.Any(x => x.ProviderCity == provider.LegalAddress.CATOTTG.Name));
        Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        Assert.True(result.Entities.All(x => x.UpdatedDate.Kind == DateTimeKind.Utc));
    }

    [Test]
    public async Task GetProviderChangesLog_WhenMinistryAdminCalled_ReturnsSearchResult()
    {
        // Arange
        var institutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4");
        var changesLogService = GetChangesLogService();
        var request = new ProviderChangesLogRequest();
        provider = provider.WithInstitutionId(institutionId);

        currentUserServiceMock.Setup(x => x.IsMinistryAdmin()).Returns(true);
        ministryAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<MinistryAdminDto>(new MinistryAdminDto()
            {
                InstitutionId = institutionId,
            }));

        var entitiesCount = 5;
        var totalAmount = 5;
        var changesMock = Enumerable.Range(1, entitiesCount)
            .Select(x => new ChangesLog { Id = x, EntityIdGuid = provider.Id, User = user })
            .AsQueryable()
            .BuildMock();
        var providersMock = new List<Provider> { provider }
            .AsQueryable()
            .BuildMock();

        mapper.Setup(m => m.Map<ChangesLogFilter>(It.IsAny<ProviderChangesLogRequest>()))
            .Returns(new ChangesLogFilter());
        mapper.Setup(m => m.Map<ShortUserDto>(user))
            .Returns(new ShortUserDto { Id = user.Id });

        changesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        changesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                string.Empty,
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(changesMock);
        providerRepository.Setup(repo => repo.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(providersMock);

        // Act
        var result = await changesLogService.GetProviderChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.Any(x => x.ProviderId == provider.Id));
        Assert.True(result.Entities.Any(x => x.ProviderTitle == provider.FullTitle));
        Assert.True(result.Entities.Any(x => x.ProviderCity == provider.LegalAddress.CATOTTG.Name));
        Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        Assert.True(result.Entities.All(x => x.UpdatedDate.Kind == DateTimeKind.Utc));
    }

    [Test]
    public async Task GetProviderChangesLog_WhenRegionAdminCalled_ReturnsSearchResult()
    {
        // Arange
        var institutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4");
        long catottgId = 31737;
        var changesLogService = GetChangesLogService();
        var request = new ProviderChangesLogRequest();
        provider = provider.WithInstitutionId(institutionId);

        currentUserServiceMock.Setup(x => x.IsRegionAdmin()).Returns(true);
        regionAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<RegionAdminDto>(new RegionAdminDto()
            {
                InstitutionId = institutionId,
                CATOTTGId = catottgId,
            }));

        codeficatorServiceMock
            .Setup(x => x.GetAllChildrenIdsByParentIdAsync(It.IsAny<long>()))
            .Returns(Task.FromResult((IEnumerable<long>)new List<long> { catottgId }));

        var entitiesCount = 5;
        var totalAmount = 5;
        var changesMock = Enumerable.Range(1, entitiesCount)
            .Select(x => new ChangesLog { Id = x, EntityIdGuid = provider.Id, User = user })
            .AsQueryable()
            .BuildMock();
        var providersMock = new List<Provider> { provider }
            .AsQueryable()
            .BuildMock();

        mapper.Setup(m => m.Map<ChangesLogFilter>(It.IsAny<ProviderChangesLogRequest>()))
            .Returns(new ChangesLogFilter());
        mapper.Setup(m => m.Map<ShortUserDto>(user))
            .Returns(new ShortUserDto { Id = user.Id });

        changesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        changesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                string.Empty,
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(changesMock);
        providerRepository.Setup(repo => repo.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(providersMock);

        // Act
        var result = await changesLogService.GetProviderChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.Any(x => x.ProviderId == provider.Id));
        Assert.True(result.Entities.Any(x => x.ProviderTitle == provider.FullTitle));
        Assert.True(result.Entities.Any(x => x.ProviderCity == provider.LegalAddress.CATOTTG.Name));
        Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        Assert.True(result.Entities.All(x => x.UpdatedDate.Kind == DateTimeKind.Utc));
    }

    [Test]
    public async Task GetApplicationChangesLog_WhenCalled_ReturnsSearchResult()
    {
        // Arange
        var changesLogService = GetChangesLogService();
        var request = new ApplicationChangesLogRequest();

        var entitiesCount = 5;
        var totalAmount = 5;
        var changesMock = Enumerable.Range(1, entitiesCount)
            .Select(x => new ChangesLog { Id = x, EntityIdGuid = application.Id, User = user })
            .AsQueryable()
            .BuildMock();
        var applicationsMock = new List<Application> { application }
            .AsQueryable()
            .BuildMock();

        mapper.Setup(m => m.Map<ChangesLogFilter>(It.IsAny<ApplicationChangesLogRequest>()))
            .Returns(new ChangesLogFilter());
        mapper.Setup(m => m.Map<ShortUserDto>(user))
            .Returns(new ShortUserDto { Id = user.Id });

        changesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        changesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                string.Empty,
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(changesMock);
        applicationRepository.Setup(repo => repo.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(applicationsMock);

        // Act
        var result = await changesLogService.GetApplicationChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.All(x => x.ApplicationId == application.Id));
        Assert.True(result.Entities.All(x => x.WorkshopTitle == application.Workshop.Title));
        Assert.True(result.Entities.All(x => x.WorkshopCity == application.Workshop.Address.CATOTTG.Name));
        Assert.True(result.Entities.All(x => x.ProviderTitle == application.Workshop.ProviderTitle));
        Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        Assert.True(result.Entities.All(x => x.UpdatedDate.Kind == DateTimeKind.Utc));
    }

    [Test]
    public async Task GetApplicationChangesLog_WhenMinistryAdminCalled_ReturnsSearchResult()
    {
        // Arange
        var institutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4");
        var changesLogService = GetChangesLogService();
        var request = new ApplicationChangesLogRequest();
        provider = provider.WithInstitutionId(institutionId);
        workshop = workshop.WithProvider(provider);
        application = application.WithWorkshop(workshop);

        currentUserServiceMock.Setup(x => x.IsMinistryAdmin()).Returns(true);
        ministryAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<MinistryAdminDto>(new MinistryAdminDto()
            {
                InstitutionId = institutionId,
            }));

        var entitiesCount = 5;
        var totalAmount = 5;
        var changesMock = Enumerable.Range(1, entitiesCount)
            .Select(x => new ChangesLog { Id = x, EntityIdGuid = application.Id, User = user })
            .AsQueryable()
            .BuildMock();
        var applicationsMock = new List<Application> { application }
            .AsQueryable()
            .BuildMock();

        mapper.Setup(m => m.Map<ChangesLogFilter>(It.IsAny<ApplicationChangesLogRequest>()))
            .Returns(new ChangesLogFilter());
        mapper.Setup(m => m.Map<ShortUserDto>(user))
            .Returns(new ShortUserDto { Id = user.Id });

        changesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        changesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                string.Empty,
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(changesMock);
        applicationRepository.Setup(repo => repo.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(applicationsMock);

        // Act
        var result = await changesLogService.GetApplicationChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.All(x => x.ApplicationId == application.Id));
        Assert.True(result.Entities.All(x => x.WorkshopTitle == application.Workshop.Title));
        Assert.True(result.Entities.All(x => x.WorkshopCity == application.Workshop.Address.CATOTTG.Name));
        Assert.True(result.Entities.All(x => x.ProviderTitle == application.Workshop.ProviderTitle));
        Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        Assert.True(result.Entities.All(x => x.UpdatedDate.Kind == DateTimeKind.Utc));
    }

    [Test]
    [TestCase("search-string")]
    public async Task GetEmployeeChangesLog_WhenCalled_ReturnsSearchResult(string searchString)
    {
        // Arange
        var changesLogService = GetChangesLogService();
        var request = new EmployeeChangesLogRequest();
        request.SearchString = searchString;

        var entitiesCount = 5;
        var totalAmount = 10;
        var changesMock = Enumerable.Range(1, entitiesCount)
            .Select(x => new EmployeeChangesLog
            {
                Id = x,
                ProviderId = provider.Id,
                EmployeeUser = UserGenerator.Generate(),
                User = user,
                Provider = provider,
            })
            .AsQueryable()
            .BuildMock();

        mapper.Setup(m => m.Map<ShortUserDto>(user)).Returns(new ShortUserDto { Id = user.Id });

        employeeChangesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        employeeChangesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                string.Empty,
                It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<EmployeeChangesLog, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(changesMock);

        // Act
        var result = await changesLogService.GetEmployeeChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.All(x => x.ProviderTitle == provider.FullTitle));
        Assert.True(result.Entities.All(x => x.InstitutionTitle == provider.Institution.Title));
        Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        Assert.True(result.Entities.All(x => x.OperationDate.Kind == DateTimeKind.Utc));
    }

    [Test]
    public async Task GetEmployeeChangesLog_WhenMinistryAdminCalled_ReturnsSearchResult()
    {
        // Arange
        var institutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4");
        var changesLogService = GetChangesLogService();
        var request = new EmployeeChangesLogRequest();
        provider = provider.WithInstitutionId(institutionId);

        currentUserServiceMock.Setup(x => x.IsMinistryAdmin()).Returns(true);
        ministryAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<MinistryAdminDto>(new MinistryAdminDto()
            {
                InstitutionId = institutionId,
            }));

        var entitiesCount = 5;
        var totalAmount = 5;
        var providerAdminChangesLogs = Enumerable.Range(1, entitiesCount)
            .Select(x => new EmployeeChangesLog
            {
                Id = x,
                ProviderId = provider.Id,
                EmployeeUser = UserGenerator.Generate(),
                User = user,
                Provider = provider,
            })
            .AsQueryable()
            .BuildMock();

        mapper.Setup(m => m.Map<ShortUserDto>(user))
            .Returns(new ShortUserDto { Id = user.Id });

        employeeChangesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        employeeChangesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                string.Empty,
                It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<EmployeeChangesLog, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(providerAdminChangesLogs);

        // Act
        var result = await changesLogService.GetEmployeeChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.All(x => x.ProviderTitle == provider.FullTitle));
        Assert.True(result.Entities.All(x => x.InstitutionTitle == provider.Institution.Title));
        Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        Assert.True(result.Entities.All(x => x.OperationDate.Kind == DateTimeKind.Utc));
    }

    [Test]
    public async Task GetEmployeeChangesLog_WhenAreaAdminCalled_ReturnsSearchResult()
    {
        // Arange
        var institutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4");
        long catottgId = 31737;
        var subCatottgIds = new List<long> { 31738, 31739, 31740 };
        var changesLogService = GetChangesLogService();
        var request = new EmployeeChangesLogRequest();
        provider = provider.WithInstitutionId(institutionId);

        currentUserServiceMock.Setup(x => x.IsAreaAdmin()).Returns(true);
        areaAdminServiceMock
            .Setup(x => x.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<AreaAdminDto>(new AreaAdminDto()
            {
                InstitutionId = institutionId,
                CATOTTGId = catottgId,
            }));
        codeficatorServiceMock
            .Setup(x => x.GetAllChildrenIdsByParentIdAsync(It.IsAny<long>()))
            .Returns(Task.FromResult<IEnumerable<long>>(subCatottgIds));

        var totalAmount = 3;
        var entitiesCount = 3;
        var providerAdminChangesLogs = Enumerable.Range(1, entitiesCount)
            .Select(x => new EmployeeChangesLog
            {
                Id = x,
                ProviderId = provider.Id,
                EmployeeUser = UserGenerator.Generate(),
                User = user,
                Provider = provider,
            })
            .AsQueryable()
            .BuildMock();

        employeeChangesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        mapper.Setup(m => m.Map<ShortUserDto>(user))
            .Returns(new ShortUserDto { Id = user.Id });
        employeeChangesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                string.Empty,
                It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<EmployeeChangesLog, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(providerAdminChangesLogs);

        // Act
        var result = await changesLogService.GetEmployeeChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.All(x => x.ProviderTitle == provider.FullTitle));
        Assert.True(result.Entities.All(x => x.InstitutionTitle == provider.Institution.Title));
        Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        Assert.True(result.Entities.All(x => x.OperationDate.Kind == DateTimeKind.Utc));
        currentUserServiceMock.Verify(x => x.IsRegionAdmin(), Times.Once);
    }

    [Test]
    public async Task GetParentBlockedByAdminChangesLogAsync_WithValidRequest_ReturnsExpectedResult()
    {
        // Arrange
        var changesLogService = GetChangesLogService();
        var request = new ParentBlockedByAdminChangesLogRequest
        {
            ShowParents = ShowParents.All,
            DateFrom = new DateTime(2023, 1, 1),
            DateTo = DateTime.UtcNow.AddDays(2),
            SearchString = "Test",
        };

        var fakeData = new List<ParentBlockedByAdminLog>
        {
            new()
            {
                Id = 1,
                ParentId = parent.Id,
                Parent = parent,
                User = user,
                UserId = user.Id,
                OperationDate = new DateTime(2023, 10, 1),
                Reason = "Test Reason to block",
                IsBlocked = true,
            },
            new()
            {
                Id = 2,
                ParentId = parent.Id,
                Parent = parent,
                User = user,
                UserId = user.Id,
                OperationDate = DateTime.UtcNow,
                Reason = "Test Reason to unblock",
                IsBlocked = false,
            },
        };

        mapper.Setup(m => m.Map<ShortUserDto>(user))
            .Returns(new ShortUserDto { Id = user.Id });

        parentBlockedByAdminLogRepository.Setup(x => x.Count(It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>()))
            .ReturnsAsync(fakeData.Count);

        parentBlockedByAdminLogRepository.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ParentBlockedByAdminLog, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(fakeData.AsTestAsyncEnumerableQuery());

         // Act
        var result = await changesLogService.GetParentBlockedByAdminChangesLogAsync(request);

        // Assert
        Assert.IsInstanceOf<SearchResult<ParentBlockedByAdminChangesLogDto>>(result);
        var searchResult = result;
        Assert.AreEqual(fakeData.Count, searchResult.TotalAmount);
        Assert.AreEqual(fakeData.Count, searchResult.Entities.Count);
    }

    [Test]
    public void GetParentBlockedByAdminChangesLogAsync_WithInvalidShowParentsInRequest_ThrowNotImplementedException()
    {
        // Arrange
        var changesLogService = GetChangesLogService();
        var request = new ParentBlockedByAdminChangesLogRequest
        {
            ShowParents = (ShowParents)100,
            DateFrom = new DateTime(2023, 1, 1),
            DateTo = DateTime.UtcNow.AddDays(2),
            SearchString = "Test",
        };

        // Act and Assert
        Assert.ThrowsAsync<NotImplementedException>(
            async () => await changesLogService.GetParentBlockedByAdminChangesLogAsync(request));
    }

    [Test]
    public async Task GetParentBlockedByAdminChangesLogAsync_WithDateToMaxValueInRequest_ReturnsExpectedResult()
    {
        // Arrange
        var changesLogService = GetChangesLogService();
        var request = new ParentBlockedByAdminChangesLogRequest
        {
            ShowParents = ShowParents.All,
            DateFrom = new DateTime(2023, 1, 1),
            DateTo = DateTime.MaxValue,
            SearchString = "Test",
        };

        var fakeData = new List<ParentBlockedByAdminLog>
        {
            new()
            {
                Id = 1,
                ParentId = parent.Id,
                Parent = parent,
                User = user,
                UserId = user.Id,
                OperationDate = new DateTime(2023, 10, 1),
                Reason = "Test Reason to block",
                IsBlocked = true,
            },
            new()
            {
                Id = 2,
                ParentId = parent.Id,
                Parent = parent,
                User = user,
                UserId = user.Id,
                OperationDate = DateTime.UtcNow,
                Reason = "Test Reason to unblock",
                IsBlocked = false,
            },
        };

        mapper.Setup(m => m.Map<ShortUserDto>(user))
            .Returns(new ShortUserDto { Id = user.Id });

        parentBlockedByAdminLogRepository.Setup(x => x.Count(It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>()))
            .ReturnsAsync(fakeData.Count);

        parentBlockedByAdminLogRepository.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ParentBlockedByAdminLog, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(fakeData.AsTestAsyncEnumerableQuery());

        // Act
        var result = await changesLogService.GetParentBlockedByAdminChangesLogAsync(request);

        // Assert
        Assert.IsInstanceOf<SearchResult<ParentBlockedByAdminChangesLogDto>>(result);
        var searchResult = result;
        Assert.AreEqual(fakeData.Count, searchResult.TotalAmount);
        Assert.AreEqual(fakeData.Count, searchResult.Entities.Count);
    }

    [Test]
    public async Task GetParentBlockedByAdminChangesLogAsync_WithDateFromMinValueInRequest_ReturnsExpectedResult()
    {
        // Arrange
        var changesLogService = GetChangesLogService();
        var request = new ParentBlockedByAdminChangesLogRequest
        {
            ShowParents = ShowParents.All,
            DateFrom = DateTime.MinValue,
            DateTo = DateTime.UtcNow.AddDays(2),
            SearchString = "Test",
        };

        var fakeData = new List<ParentBlockedByAdminLog>
        {
            new()
            {
                Id = 1,
                ParentId = parent.Id,
                Parent = parent,
                User = user,
                UserId = user.Id,
                OperationDate = new DateTime(2023, 10, 1),
                Reason = "Test Reason to block",
                IsBlocked = true,
            },
            new()
            {
                Id = 2,
                ParentId = parent.Id,
                Parent = parent,
                User = user,
                UserId = user.Id,
                OperationDate = DateTime.UtcNow,
                Reason = "Test Reason to unblock",
                IsBlocked = false,
            },
        };

        mapper.Setup(m => m.Map<ShortUserDto>(user))
            .Returns(new ShortUserDto { Id = user.Id });

        parentBlockedByAdminLogRepository.Setup(x => x.Count(It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>()))
            .ReturnsAsync(fakeData.Count);

        parentBlockedByAdminLogRepository.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ParentBlockedByAdminLog, dynamic>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(fakeData.AsTestAsyncEnumerableQuery());

        // Act
        var result = await changesLogService.GetParentBlockedByAdminChangesLogAsync(request);

        // Assert
        Assert.IsInstanceOf<SearchResult<ParentBlockedByAdminChangesLogDto>>(result);
        var searchResult = result;
        Assert.AreEqual(fakeData.Count, searchResult.TotalAmount);
        Assert.AreEqual(fakeData.Count, searchResult.Entities.Count);
    }
    #endregion

    private IOptions<ChangesLogConfig> CreateChangesLogOptions() =>
        Options.Create(new ChangesLogConfig
        {
            TrackedProperties = new Dictionary<string, string[]>
            {
            { "Provider", new[] { "FullTitle", "EdrpouIpn", "Director", "LegalAddress" } },
            },
        });

    private IChangesLogService GetChangesLogService()
        => new ChangesLogService(
            CreateChangesLogOptions(),
            changesLogRepository.Object,
            providerRepository.Object,
            applicationRepository.Object,
            employeeChangesLogRepository.Object,
            parentBlockedByAdminLogRepository.Object,
            logger.Object,
            mapper.Object,
            valueProjector.Object,
            currentUserServiceMock.Object,
            ministryAdminServiceMock.Object,
            regionAdminServiceMock.Object,
            areaAdminServiceMock.Object,
            codeficatorServiceMock.Object);
}