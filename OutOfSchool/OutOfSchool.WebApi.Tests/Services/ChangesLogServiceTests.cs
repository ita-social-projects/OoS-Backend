using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
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
using OutOfSchool.BusinessLogic.Services.Logging;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ChangesLogServiceTests
{
    private Mock<ILogger<ChangesLogService>> logger;
    private Mock<IChangesLogRepository> changesLogRepository;
    private Mock<IProviderRepository> providerRepository;
    private Mock<IApplicationRepository> applicationRepository;
    private Mock<IWorkshopDraftRepository> workshopDraftRepositoryMock;
    private Mock<IEntityRepository<long, EmployeeChangesLog>> employeeChangesLogRepository;
    private Mock<IEntityAddOnlyRepository<long, ParentBlockedByAdminLog>> parentBlockedByAdminLogRepository;
    private Mock<IWorkshopRepository> workshopRepository;
    private Mock<IValueProjector> valueProjector;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IAreaAdminService> areaAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<INestedObjectChangeLogger> nestedObjectChangeLoggerMock;
    private Mock<ICollectionChangeLogger> collectionChangeLoggerMock;

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
        workshop.Contacts = [new ()
        {
            Title = "Test",
            IsDefault = true,
            Address = ContactsAddressGenerator.Generate()
        }];
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
        changesLogRepository = new Mock<IChangesLogRepository>(MockBehavior.Strict);
        providerRepository = new Mock<IProviderRepository>(MockBehavior.Strict);
        applicationRepository = new Mock<IApplicationRepository>(MockBehavior.Strict);
        workshopDraftRepositoryMock = new Mock<IWorkshopDraftRepository>(MockBehavior.Strict);
        employeeChangesLogRepository = new Mock<IEntityRepository<long, EmployeeChangesLog>>(MockBehavior.Strict);
        parentBlockedByAdminLogRepository = new Mock<IEntityAddOnlyRepository<long, ParentBlockedByAdminLog>>();
        workshopRepository = new Mock<IWorkshopRepository>(MockBehavior.Strict);
        valueProjector = new Mock<IValueProjector>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        areaAdminServiceMock = new Mock<IAreaAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        nestedObjectChangeLoggerMock = new Mock<INestedObjectChangeLogger>();
        collectionChangeLoggerMock = new Mock<ICollectionChangeLogger>();
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

        changesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        changesLogRepository
            .Setup(repo => repo.Get(0, 0,
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>()))
            .Returns(changesMock);
        providerRepository.Setup(repo => repo.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, object>>, SortDirection>>()))
            .Returns(providersMock);

        // Act
        var result = await changesLogService.GetProviderChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.Any(x => x.ProviderId == provider.Id));
        Assert.True(result.Entities.Any(x => x.ProviderTitle == provider.FullTitle));
        Assert.True(result.Entities.Any(x => x.ProviderCity == provider.Contacts.Single(c => c.IsDefault).Address.CATOTTG.Name));
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

        changesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        changesLogRepository
            .Setup(repo => repo.Get(0, 0,
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>()))
            .Returns(changesMock);
        providerRepository.Setup(repo => repo.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, object>>, SortDirection>>()))
            .Returns(providersMock);

        // Act
        var result = await changesLogService.GetProviderChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.Any(x => x.ProviderId == provider.Id));
        Assert.True(result.Entities.Any(x => x.ProviderTitle == provider.FullTitle));
        Assert.True(result.Entities.Any(x => x.ProviderCity == provider.Contacts.Single(c => c.IsDefault).Address.CATOTTG.Name));
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

        changesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        changesLogRepository
            .Setup(repo => repo.Get(0, 0,
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>()))
            .Returns(changesMock);
        providerRepository.Setup(repo => repo.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, object>>, SortDirection>>()))
            .Returns(providersMock);

        // Act
        var result = await changesLogService.GetProviderChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.Any(x => x.ProviderId == provider.Id));
        Assert.True(result.Entities.Any(x => x.ProviderTitle == provider.FullTitle));
        Assert.True(result.Entities.Any(x => x.ProviderCity == provider.Contacts.Single(c => c.IsDefault).Address.CATOTTG.Name));
        Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        Assert.True(result.Entities.All(x => x.UpdatedDate.Kind == DateTimeKind.Utc));
    }

    [Test]
    public async Task GetApplicationChangesLog_WhenCalled_ReturnsSearchResult()
    {
        // Arange
        var changesLogService = GetChangesLogService();
        var request = new ApplicationChangesLogRequest();

        currentUserServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var entitiesCount = 5;
        var totalAmount = 5;
        var changesMock = Enumerable.Range(1, entitiesCount)
            .Select(x => new ChangesLog { Id = x, EntityIdGuid = application.Id, User = user })
            .AsQueryable()
            .BuildMock();
        var applicationsMock = new List<Application> { application }
            .AsQueryable()
            .BuildMock();

        changesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        changesLogRepository
            .Setup(repo => repo.Get(0, 0,
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>()))
            .Returns(changesMock);
        applicationRepository.Setup(repo => repo.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>()))
            .Returns(applicationsMock);

        // Act
        var result = await changesLogService.GetApplicationChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.All(x => x.ApplicationId == application.Id));
        Assert.True(result.Entities.All(x => x.WorkshopTitle == application.Workshop.Title));
        Assert.True(result.Entities.All(x => application.Workshop.Contacts.Any(c => c.IsDefault && c.Address.CATOTTG.Name == x.WorkshopCity)));
        Assert.True(result.Entities.All(x => x.ProviderTitle == application.Workshop.Provider.FullTitle));
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

        currentUserServiceMock.Setup(x => x.IsAdmin()).Returns(true);
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

        changesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        changesLogRepository
            .Setup(repo => repo.Get(0, 0,
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection>>()))
            .Returns(changesMock);
        applicationRepository.Setup(repo => repo.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>()))
            .Returns(applicationsMock);

        // Act
        var result = await changesLogService.GetApplicationChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.All(x => x.ApplicationId == application.Id));
        Assert.True(result.Entities.All(x => x.WorkshopTitle == application.Workshop.Title));
        Assert.True(result.Entities.All(x => application.Workshop.Contacts.Any(c => c.IsDefault && c.Address.CATOTTG.Name == x.WorkshopCity)));
        Assert.True(result.Entities.All(x => x.ProviderTitle == application.Workshop.Provider.FullTitle));
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
        currentUserServiceMock.Setup(x => x.IsAdmin()).Returns(true);
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

        employeeChangesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        employeeChangesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<EmployeeChangesLog, object>>, SortDirection>>()))
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

        currentUserServiceMock.Setup(x => x.IsAdmin()).Returns(true);
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

        employeeChangesLogRepository
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>()))
            .Returns(Task.FromResult(totalAmount));
        employeeChangesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<EmployeeChangesLog, object>>, SortDirection>>()))
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

        currentUserServiceMock.Setup(x => x.IsAdmin()).Returns(true);
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
        employeeChangesLogRepository
            .Setup(repo => repo.Get(
                request.From,
                request.Size,
                It.IsAny<Expression<Func<EmployeeChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<EmployeeChangesLog, object>>, SortDirection>>()))
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

        parentBlockedByAdminLogRepository.Setup(x => x.Count(It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>()))
            .ReturnsAsync(fakeData.Count);

        parentBlockedByAdminLogRepository.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ParentBlockedByAdminLog, dynamic>>, SortDirection>>()))
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

        parentBlockedByAdminLogRepository.Setup(x => x.Count(It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>()))
            .ReturnsAsync(fakeData.Count);

        parentBlockedByAdminLogRepository.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ParentBlockedByAdminLog, dynamic>>, SortDirection>>()))
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

        parentBlockedByAdminLogRepository.Setup(x => x.Count(It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>()))
            .ReturnsAsync(fakeData.Count);

        parentBlockedByAdminLogRepository.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ParentBlockedByAdminLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ParentBlockedByAdminLog, dynamic>>, SortDirection>>()))
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
    public async Task GetWorkshopChangesLogAsync_WithValidRequest_ReturnsExpectedResult()
    {
        // Arrange
        var changesLogService = GetChangesLogService();
        var request = new WorkshopChangesLogRequest
        {
            From = 0,
            Size = 5
        };

        var entitiesCount = 5;
        var totalAmount = 10;
        var changesMock = Enumerable.Range(1, totalAmount)
            .Select(x => new ChangesLog
            {
                Id = x,
                EntityIdGuid = workshop.Id,
                PropertyName = "TestProperty",
                OldValue = "OldValue",
                NewValue = "NewValue",
                UpdatedDate = DateTime.UtcNow,
                User = user
            })
            .AsQueryable()
            .BuildMock();

        var workshopsMock = new List<Workshop> { workshop }
            .AsQueryable()
            .BuildMock();

        changesLogRepository
            .Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<Expression<Func<ChangesLog, bool>>>(), It.IsAny<Dictionary<Expression<Func<ChangesLog, Object>>, SortDirection>>()))
            .Returns(changesMock);
        workshopRepository.Setup(repo => repo.Get(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<Dictionary<Expression<Func<Workshop, Object>>, SortDirection>>()))
            .Returns(workshopsMock);

        // Act  
        var result = await changesLogService.GetWorkshopChangesLogAsync(request);

        // Assert
        Assert.AreEqual(totalAmount, result.TotalAmount);
        Assert.AreEqual(entitiesCount, result.Entities.Count);
        Assert.True(result.Entities.All(x => x.WorkshopId == workshop.Id));
        Assert.True(result.Entities.All(x => x.FieldName == "TestProperty"));
        Assert.True(result.Entities.All(x => x.OldValue == "OldValue"));
        Assert.True(result.Entities.All(x => x.NewValue == "NewValue"));
        Assert.True(result.Entities.All(x => x.User.Id == user.Id));
        Assert.True(result.Entities.All(x => x.UpdatedDate.Kind == DateTimeKind.Utc));
    }

    #endregion

    #region GetWorkshopDraftChangesLogAsync

    [Test]
    public async Task GetWorkshopDraftChangesLogAsync_ValidRequest_ReturnsExpectedResults()
    {
        // Arrange
        var request = new WorkshopDraftChangesLogRequest
        {
            From = 0,
            Size = 10
        };

        var changesLogs = new List<ChangesLog>
        {
            new()
            {
                Id = 1,
                EntityType = "WorkshopDraft",
                EntityIdGuid = Guid.NewGuid(),
                PropertyName = "Title",
                OldValue = "Old Title",
                NewValue = "New Title",
                UpdatedDate = DateTime.UtcNow,
                UserId = "user-id",
                User = user
            },
            new()
            {
                Id = 2,
                EntityType = "WorkshopDraft",
                EntityIdGuid = Guid.NewGuid(),
                PropertyName = "Description",
                OldValue = "Old Description",
                NewValue = "New Description",
                UpdatedDate = DateTime.UtcNow,
                UserId = "user-id",
                User = user
            }
        };

        var workshopDrafts = new List<WorkshopDraft>
        {
            new()
            {
                Id = changesLogs[0].EntityIdGuid.Value,
                ProviderId = provider.Id,
                Provider = provider,
                WorkshopDraftContent = new WorkshopDraftContent { Title = "Workshop Title 1" }
            },
            new()
            {
                Id = changesLogs[1].EntityIdGuid.Value,
                ProviderId = provider.Id,
                Provider = provider,
                WorkshopDraftContent = new WorkshopDraftContent { Title = "Workshop Title 2" }
            }
        };

        var changeLogsQueryable = changesLogs.AsQueryable().BuildMock();
        var workshopDraftsQueryable = workshopDrafts.AsQueryable().BuildMock();

        changesLogRepository.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, object>>, SortDirection>>()))
            .Returns(changeLogsQueryable);

        changesLogRepository
           .Setup(r => r.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
           .ReturnsAsync(changesLogs.Count);

        workshopDraftRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>()))
            .Returns(workshopDraftsQueryable);

        // Setup current user as non-admin to simplify predicate
        currentUserServiceMock.Setup(s => s.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(s => s.IsRegionAdmin()).Returns(false);
        currentUserServiceMock.Setup(s => s.IsAreaAdmin()).Returns(false);

        // Mock extension methods
        var service = GetChangesLogService();

        // Act
        var result = await service.GetWorkshopDraftChangesLogAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.TotalAmount);
        Assert.AreEqual(2, result.Entities.Count);
        Assert.AreEqual(changesLogs[0].EntityIdGuid, result.Entities.ElementAt(0).WorkshopDraftId);
        Assert.AreEqual(changesLogs[1].EntityIdGuid, result.Entities.ElementAt(1).WorkshopDraftId);
        Assert.AreEqual("Workshop Title 1", result.Entities.ElementAt(0).WorkshopTitle);
        Assert.AreEqual("Workshop Title 2", result.Entities.ElementAt(1).WorkshopTitle);
    }

    [Test]
    public async Task GetWorkshopDraftChangesLogAsync_EmptyResult_ReturnsEmptyCollection()
    {
        // Arrange
        var request = new WorkshopDraftChangesLogRequest
        {
            From = 0,
            Size = 10
        };

        var changesLogs = new List<ChangesLog>();
        var workshopDrafts = new List<WorkshopDraft>();

        var changeLogsQueryable = changesLogs.AsQueryable().BuildMock();
        var workshopDraftsQueryable = workshopDrafts.AsQueryable().BuildMock();

        changesLogRepository.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ChangesLog, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<ChangesLog, object>>, SortDirection>>()))
            .Returns(changeLogsQueryable);

        changesLogRepository
           .Setup(r => r.Count(It.IsAny<Expression<Func<ChangesLog, bool>>>()))
           .ReturnsAsync(changesLogs.Count);

        workshopDraftRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>()))
            .Returns(workshopDraftsQueryable);

        // Setup current user as non-admin to simplify predicate
        currentUserServiceMock.Setup(s => s.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(s => s.IsRegionAdmin()).Returns(false);
        currentUserServiceMock.Setup(s => s.IsAreaAdmin()).Returns(false);

        var service = GetChangesLogService();

        // Act
        var result = await service.GetWorkshopDraftChangesLogAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.IsEmpty(result.Entities);
    }

    [Test]
    public void GetWorkshopDraftChangesLogAsync_InvalidRequest_ThrowsArgumentException()
    {
        // Arrange
        var request = new WorkshopDraftChangesLogRequest
        {
            From = -1, // Invalid value
            Size = 10
        };

        var service = GetChangesLogService();

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => service.GetWorkshopDraftChangesLogAsync(request));
    }

    #endregion

    #region LogWorkshopDraftChanges

    [Test]
    public void LogWorkshopDraftChanges_WithChanges_LogsSuccessfully()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var userId = "test-user-id";

        var oldContent = new WorkshopDraftContent
        {
            Title = "Old Title",
            WorkshopDescriptionItems = new List<WorkshopDescriptionItemDraft>
            {
                new() { SectionName = "Section 1", Description = "Old Description" }
            }
        };

        var newContent = new WorkshopDraftContent
        {
            Title = "New Title",
            WorkshopDescriptionItems = new List<WorkshopDescriptionItemDraft>
            {
                new() { SectionName = "Section 1", Description = "New Description" },
                new() { SectionName = "Section 2", Description = "Added Section" }
            }
        };

        var nestedChangeLogs = new List<ChangesLog>
        {
            new()
            {
                EntityType = "WorkshopDraft",
                EntityIdGuid = draftId,
                PropertyName = "Title",
                OldValue = "Old Title",
                NewValue = "New Title",
                UserId = userId
            }
        };

        var collectionChangeLogs = new List<ChangesLog>
        {
            new()
            {
                EntityType = "WorkshopDraft",
                EntityIdGuid = draftId,
                PropertyName = "WorkshopDescriptionItems[Section 1].Description",
                OldValue = "Old Description",
                NewValue = "New Description",
                UserId = userId
            },
            new()
            {
                EntityType = "WorkshopDraft",
                EntityIdGuid = draftId,
                PropertyName = "WorkshopDescriptionItems.Added",
                OldValue = null,
                NewValue = "Section 2",
                UserId = userId
            }
        };

        nestedObjectChangeLoggerMock.Setup(l => l.CompareAndLogChanges(
                oldContent,
                newContent,
                draftId,
                "WorkshopDraft",
                userId,
                It.IsAny<IEnumerable<string>>(),
                valueProjector.Object))
            .Returns(nestedChangeLogs);

        collectionChangeLoggerMock.Setup(l => l.CompareCollections(
                oldContent.WorkshopDescriptionItems ?? new List<WorkshopDescriptionItemDraft>(),
                newContent.WorkshopDescriptionItems ?? new List<WorkshopDescriptionItemDraft>(),
                It.IsAny<Func<WorkshopDescriptionItemDraft, string>>(),
                draftId,
                "WorkshopDraft",
                userId,
                nameof(WorkshopDraftContent.WorkshopDescriptionItems),
                valueProjector.Object,
                true,
                null))
            .Returns(collectionChangeLogs);

        changesLogRepository.Setup(r => r.AddChangeLogsToDbContext(It.IsAny<IEnumerable<ChangesLog>>()))
            .Verifiable();

        var service = GetChangesLogService();

        // Act
        service.LogWorkshopDraftChanges(oldContent, newContent, draftId, userId);

        // Assert
        changesLogRepository.Verify(r => r.AddChangeLogsToDbContext(
            It.Is<IEnumerable<ChangesLog>>(logs => logs.Count() == nestedChangeLogs.Count + collectionChangeLogs.Count)),
            Times.Once);
    }

    [Test]
    public void LogWorkshopDraftChanges_WithoutChanges_DoesNotLogAnything()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var userId = "test-user-id";

        var oldContent = new WorkshopDraftContent
        {
            Title = "Same Title",
            WorkshopDescriptionItems = new List<WorkshopDescriptionItemDraft>()
        };

        var newContent = new WorkshopDraftContent
        {
            Title = "Same Title",
            WorkshopDescriptionItems = new List<WorkshopDescriptionItemDraft>()
        };

        nestedObjectChangeLoggerMock.Setup(l => l.CompareAndLogChanges(
                oldContent,
                newContent,
                draftId,
                "WorkshopDraft",
                userId,
                It.IsAny<IEnumerable<string>>(),
                valueProjector.Object))
            .Returns(new List<ChangesLog>());

        // Also need to setup the collectionChangeLogger
        collectionChangeLoggerMock.Setup(l => l.CompareCollections(
                It.IsAny<IEnumerable<WorkshopDescriptionItemDraft>>(),
                It.IsAny<IEnumerable<WorkshopDescriptionItemDraft>>(),
                It.IsAny<Func<WorkshopDescriptionItemDraft, string>>(),
                draftId,
                "WorkshopDraft",
                userId,
                nameof(WorkshopDraftContent.WorkshopDescriptionItems),
                valueProjector.Object,
                true,
                null))
            .Returns(new List<ChangesLog>());

        var service = GetChangesLogService();

        // Act
        service.LogWorkshopDraftChanges(oldContent, newContent, draftId, userId);

        // Assert
        changesLogRepository.Verify(r => r.AddChangeLogsToDbContext(It.IsAny<IEnumerable<ChangesLog>>()), Times.Never);
    }

    #endregion

    #region LogContactCollections Tests

    [Test]
    public void LogContactCollections_WithAddedContacts_LogsCorrectly()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var userId = "test-user-id";
        var trackedProperties = new[] { "Contacts.Phones", "Contacts.Emails" };

        var oldContacts = new List<Contacts>();
        var newContacts = new List<Contacts>
    {
        new()
        {
            Title = "Main Office",
            Phones =
            [
                new() { Number = "+1234567890", Type = "Work" }
            ],
            Emails =
            [
                new() { Address = "test@example.com", Type = "Work" }
            ]
        }
    };

        var expectedLogs = new List<ChangesLog>
    {
        new()
        {
            EntityType = "WorkshopDraft",
            EntityIdGuid = draftId,
            PropertyName = "Contacts[Main Office].Phones.Added[0].Number",
            OldValue = null,
            NewValue = "+1234567890",
            UserId = userId
        },
        new()
        {
            EntityType = "WorkshopDraft",
            EntityIdGuid = draftId,
            PropertyName = "Contacts[Main Office].Emails.Added[0].Address",
            OldValue = null,
            NewValue = "test@example.com",
            UserId = userId
        }
    };

        collectionChangeLoggerMock.Setup(l => l.CompareCollections(
                It.IsAny<IEnumerable<PhoneNumber>>(),
                It.IsAny<IEnumerable<PhoneNumber>>(),
                It.IsAny<Func<PhoneNumber, string>>(),
                draftId,
                "WorkshopDraft",
                userId,
                "Contacts[Main Office].Phones",
                valueProjector.Object,
                true,
                null))
            .Returns(expectedLogs.Where(l => l.PropertyName.Contains("Phones")).ToList());

        collectionChangeLoggerMock.Setup(l => l.CompareCollections(
                It.IsAny<IEnumerable<Email>>(),
                It.IsAny<IEnumerable<Email>>(),
                It.IsAny<Func<Email, string>>(),
                draftId,
                "WorkshopDraft",
                userId,
                "Contacts[Main Office].Emails",
                valueProjector.Object,
                true,
                null))
            .Returns(expectedLogs.Where(l => l.PropertyName.Contains("Emails")).ToList());

        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("LogContactCollections",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<ChangesLog>)methodInfo.Invoke(service,
        [
        oldContacts, newContacts, trackedProperties, draftId, userId
        ]);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.True(result.Any(l => l.PropertyName.Contains("Phones")));
        Assert.True(result.Any(l => l.PropertyName.Contains("Emails")));
    }

    [Test]
    public void LogContactCollections_WithRemovedContacts_LogsCorrectly()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var userId = "test-user-id";
        var trackedProperties = new[] { "Contacts.Phones" };

        var oldContacts = new List<Contacts>
    {
        new()
        {
            Title = "Old Office",
            Phones = new List<PhoneNumber>
            {
                new() { Number = "+0987654321", Type = "Work" }
            }
        }
    };
        var newContacts = new List<Contacts>();

        var expectedLogs = new List<ChangesLog>
    {
        new()
        {
            EntityType = "WorkshopDraft",
            EntityIdGuid = draftId,
            PropertyName = "Contacts[Old Office].Phones.Removed[0].Number",
            OldValue = "+0987654321",
            NewValue = null,
            UserId = userId
        }
    };

        collectionChangeLoggerMock.Setup(l => l.CompareCollections(
                It.IsAny<IEnumerable<PhoneNumber>>(),
                It.IsAny<IEnumerable<PhoneNumber>>(),
                It.IsAny<Func<PhoneNumber, string>>(),
                draftId,
                "WorkshopDraft",
                userId,
                "Contacts[Old Office].Phones",
                valueProjector.Object,
                true,
                null))
            .Returns(expectedLogs);

        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("LogContactCollections",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<ChangesLog>)methodInfo.Invoke(service,
        [
        oldContacts, newContacts, trackedProperties, draftId, userId
        ]);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Contacts[Old Office].Phones.Removed[0].Number", result[0].PropertyName);
        Assert.AreEqual("+0987654321", result[0].OldValue);
        Assert.IsNull(result[0].NewValue);
    }

    [Test]
    public void LogContactCollections_WithEmptyCollections_ReturnsEmptyList()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var userId = "test-user-id";
        var trackedProperties = new[] { "Contacts.Phones", "Contacts.Emails" };

        var oldContacts = new List<Contacts>();
        var newContacts = new List<Contacts>();

        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("LogContactCollections",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<ChangesLog>)methodInfo.Invoke(service, new object[]
        {
        oldContacts, newContacts, trackedProperties, draftId, userId
        });

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void LogContactCollections_WithUntrackedProperties_IgnoresUntrackedCollections()
    {
        // Arrange
        var draftId = Guid.NewGuid();
        var userId = "test-user-id";
        var trackedProperties = new[] { "Contacts.Phones" }; // Only phones tracked

        var oldContacts = new List<Contacts>();
        var newContacts = new List<Contacts>
    {
        new()
        {
            Title = "Office",
            Phones = new List<PhoneNumber>
            {
                new() { Number = "+1234567890", Type = "Work" }
            },
            Emails = new List<Email>  // This should be ignored
            {
                new() { Address = "test@example.com", Type = "Work" }
            },
            SocialNetworks = new List<SocialNetwork>  // This should be ignored
            {
                new() { Type = SocialNetworkContactType.Facebook, Url = "facebook.com/test" }
            }
        }
    };

        var expectedPhoneLogs = new List<ChangesLog>
    {
        new()
        {
            EntityType = "WorkshopDraft",
            EntityIdGuid = draftId,
            PropertyName = "Contacts[Office].Phones.Added[0].Number",
            OldValue = null,
            NewValue = "+1234567890",
            UserId = userId
        }
    };

        collectionChangeLoggerMock.Setup(l => l.CompareCollections(
                It.IsAny<IEnumerable<PhoneNumber>>(),
                It.IsAny<IEnumerable<PhoneNumber>>(),
                It.IsAny<Func<PhoneNumber, string>>(),
                draftId,
                "WorkshopDraft",
                userId,
                "Contacts[Office].Phones",
                valueProjector.Object,
                true,
                null))
            .Returns(expectedPhoneLogs);

        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("LogContactCollections",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<ChangesLog>)methodInfo.Invoke(service, new object[]
        {
        oldContacts, newContacts, trackedProperties, draftId, userId
        });

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.True(result.All(l => l.PropertyName.Contains("Phones")));
        Assert.False(result.Any(l => l.PropertyName.Contains("Emails")));
        Assert.False(result.Any(l => l.PropertyName.Contains("SocialNetworks")));
    }

    #endregion

    #region GetContactCollectionType Tests

    [Test]
    [TestCase("Contacts.Phones", ExpectedResult = ContactCollectionType.Phones)]
    [TestCase("Contacts.Emails", ExpectedResult = ContactCollectionType.Emails)]
    [TestCase("Contacts.SocialNetworks", ExpectedResult = ContactCollectionType.SocialNetworks)]
    public ContactCollectionType? GetContactCollectionType_WithValidProperty_ReturnsCorrectType(string trackedProperty)
    {
        // Arrange
        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactCollectionType",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var result = (ContactCollectionType?)methodInfo.Invoke(null, new object[] { trackedProperty });

        // Assert
        return result;
    }

    [Test]
    [TestCase("InvalidProperty")]
    [TestCase("Contacts.InvalidCollection")]
    [TestCase("")]
    [TestCase(null)]
    public void GetContactCollectionType_WithInvalidProperty_ReturnsNull(string trackedProperty)
    {
        // Arrange
        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactCollectionType",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var result = (ContactCollectionType?)methodInfo.Invoke(null, new object[] { trackedProperty });

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region GetContactIdentifier Tests

    [Test]
    public void GetContactIdentifier_WithNewContactTitle_ReturnsTitle()
    {
        // Arrange
        var oldContact = (Contacts)null;
        var newContact = new Contacts { Title = "Main Office" };
        var index = 0;

        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactIdentifier",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var result = (string)methodInfo.Invoke(null, new object[] { oldContact, newContact, index });

        // Assert
        Assert.AreEqual("Main Office", result);
    }

    [Test]
    public void GetContactIdentifier_WithOldContactTitle_ReturnsTitle()
    {
        // Arrange
        var oldContact = new Contacts { Title = "Old Office" };
        var newContact = (Contacts)null;
        var index = 1;

        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactIdentifier",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var result = (string)methodInfo.Invoke(null, [oldContact, newContact, index]);

        // Assert
        Assert.AreEqual("Old Office", result);
    }

    [Test]
    public void GetContactIdentifier_WithBothContactTitles_PrefersNewContactTitle()
    {
        // Arrange
        var oldContact = new Contacts { Title = "Old Office" };
        var newContact = new Contacts { Title = "New Office" };
        var index = 2;

        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactIdentifier",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var result = (string)methodInfo.Invoke(null, [oldContact, newContact, index]);

        // Assert
        Assert.AreEqual("New Office", result);
    }

    [Test]
    public void GetContactIdentifier_WithEmptyTitles_ReturnsIndexBasedIdentifier()
    {
        // Arrange
        var oldContact = new Contacts { Title = "" };
        var newContact = new Contacts { Title = "   " };
        var index = 3;

        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactIdentifier",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var result = (string)methodInfo.Invoke(null, [oldContact, newContact, index]);

        // Assert
        Assert.AreEqual("Index3", result);
    }

    [Test]
    public void GetContactIdentifier_WithNullContacts_ReturnsIndexBasedIdentifier()
    {
        // Arrange
        var oldContact = (Contacts)null;
        var newContact = (Contacts)null;
        var index = 5;

        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactIdentifier",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var result = (string)methodInfo.Invoke(null, [oldContact, newContact, index]);

        // Assert
        Assert.AreEqual("Index5", result);
    }

    #endregion

    #region GetContactCollectionLogs Tests

    [Test]
    public void GetContactCollectionLogs_WithPhonesCollection_ReturnsCorrectLogs()
    {
        // Arrange
        var oldContact = new Contacts
        {
            Phones = new List<PhoneNumber>
        {
            new() { Number = "+1234567890", Type = "Work" }
        }
        };

        var newContact = new Contacts
        {
            Phones = new List<PhoneNumber>
        {
            new() { Number = "+0987654321", Type = "Mobile" }
        }
        };

        var draftId = Guid.NewGuid();
        var userId = "test-user-id";
        var propertyPrefix = "Contacts[Office].Phones";

        var expectedLogs = new List<ChangesLog>
    {
        new()
        {
            EntityType = "WorkshopDraft",
            EntityIdGuid = draftId,
            PropertyName = "Contacts[Office].Phones.Changed[0].Number",
            OldValue = "+1234567890",
            NewValue = "+0987654321",
            UserId = userId
        }
    };

        collectionChangeLoggerMock.Setup(l => l.CompareCollections(
                oldContact.Phones,
                newContact.Phones,
                It.IsAny<Func<PhoneNumber, string>>(),
                draftId,
                "WorkshopDraft",
                userId,
                propertyPrefix,
                valueProjector.Object,
                true,
                null))
            .Returns(expectedLogs);

        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactCollectionLogs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<ChangesLog>)methodInfo.Invoke(service,
        [
        oldContact, newContact, ContactCollectionType.Phones, draftId, userId, propertyPrefix
        ]);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(expectedLogs[0].PropertyName, result[0].PropertyName);
        Assert.AreEqual(expectedLogs[0].OldValue, result[0].OldValue);
        Assert.AreEqual(expectedLogs[0].NewValue, result[0].NewValue);
    }

    [Test]
    public void GetContactCollectionLogs_WithEmailsCollection_ReturnsCorrectLogs()
    {
        // Arrange
        var oldContact = new Contacts
        {
            Emails = new List<Email>
        {
            new() { Address = "old@example.com", Type = "Work" }
        }
        };

        var newContact = new Contacts
        {
            Emails = new List<Email>
        {
            new() { Address = "new@example.com", Type = "Work" }
        }
        };

        var draftId = Guid.NewGuid();
        var userId = "test-user-id";
        var propertyPrefix = "Contacts[Office].Emails";

        var expectedLogs = new List<ChangesLog>
    {
        new()
        {
            EntityType = "WorkshopDraft",
            EntityIdGuid = draftId,
            PropertyName = "Contacts[Office].Emails.Changed[0].Address",
            OldValue = "old@example.com",
            NewValue = "new@example.com",
            UserId = userId
        }
    };

        collectionChangeLoggerMock.Setup(l => l.CompareCollections(
                oldContact.Emails,
                newContact.Emails,
                It.IsAny<Func<Email, string>>(),
                draftId,
                "WorkshopDraft",
                userId,
                propertyPrefix,
                valueProjector.Object,
                true,
                null))
            .Returns(expectedLogs);

        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactCollectionLogs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<ChangesLog>)methodInfo.Invoke(service,
        [
        oldContact, newContact, ContactCollectionType.Emails, draftId, userId, propertyPrefix
        ]);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(expectedLogs[0].PropertyName, result[0].PropertyName);
        Assert.AreEqual(expectedLogs[0].OldValue, result[0].OldValue);
        Assert.AreEqual(expectedLogs[0].NewValue, result[0].NewValue);
    }

    [Test]
    public void GetContactCollectionLogs_WithSocialNetworksCollection_ReturnsCorrectLogs()
    {
        // Arrange
        var oldContact = new Contacts
        {
            SocialNetworks =
        [
            new() { Type = SocialNetworkContactType.Facebook, Url = "facebook.com/old" }
        ]
        };

        var newContact = new Contacts
        {
            SocialNetworks =
        [
            new() { Type = SocialNetworkContactType.Instagram, Url = "instagram.com/new" }
        ]
        };

        var draftId = Guid.NewGuid();
        var userId = "test-user-id";
        var propertyPrefix = "Contacts[Office].SocialNetworks";

        var expectedLogs = new List<ChangesLog>
    {
        new()
        {
            EntityType = "WorkshopDraft",
            EntityIdGuid = draftId,
            PropertyName = "Contacts[Office].SocialNetworks.Changed[Facebook_facebook.com/old].Type",
            OldValue = "Facebook",
            NewValue = "Instagram",
            UserId = userId
        }
    };

        collectionChangeLoggerMock.Setup(l => l.CompareCollections(
                oldContact.SocialNetworks,
                newContact.SocialNetworks,
                It.IsAny<Func<SocialNetwork, string>>(),
                draftId,
                "WorkshopDraft",
                userId,
                propertyPrefix,
                valueProjector.Object,
                true,
                null))
            .Returns(expectedLogs);

        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactCollectionLogs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<ChangesLog>)methodInfo.Invoke(service,
        [
        oldContact, newContact, ContactCollectionType.SocialNetworks, draftId, userId, propertyPrefix
        ]);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(expectedLogs[0].PropertyName, result[0].PropertyName);
        Assert.AreEqual(expectedLogs[0].OldValue, result[0].OldValue);
        Assert.AreEqual(expectedLogs[0].NewValue, result[0].NewValue);
    }

    [Test]
    public void GetContactCollectionLogs_WithNullOldContact_HandlesGracefully()
    {
        // Arrange
        var oldContact = (Contacts)null;
        var newContact = new Contacts
        {
            Phones = new List<PhoneNumber>
        {
            new() { Number = "+1234567890", Type = "Work" }
        }
        };

        var draftId = Guid.NewGuid();
        var userId = "test-user-id";
        var propertyPrefix = "Contacts[Office].Phones";

        var expectedLogs = new List<ChangesLog>
    {
        new()
        {
            EntityType = "WorkshopDraft",
            EntityIdGuid = draftId,
            PropertyName = "Contacts[Office].Phones.Added[0].Number",
            OldValue = null,
            NewValue = "+1234567890",
            UserId = userId
        }
    };

        collectionChangeLoggerMock.Setup(l => l.CompareCollections(
                It.Is<IEnumerable<PhoneNumber>>(phones => !phones.Any()),
                newContact.Phones,
                It.IsAny<Func<PhoneNumber, string>>(),
                draftId,
                "WorkshopDraft",
                userId,
                propertyPrefix,
                valueProjector.Object,
                true,
                null))
            .Returns(expectedLogs);

        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactCollectionLogs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<ChangesLog>)methodInfo.Invoke(service,
        [
        oldContact, newContact, ContactCollectionType.Phones, draftId, userId, propertyPrefix
        ]);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(expectedLogs[0].PropertyName, result[0].PropertyName);
        Assert.IsNull(result[0].OldValue);
        Assert.AreEqual("+1234567890", result[0].NewValue);
    }

    [Test]
    public void GetContactCollectionLogs_WithInvalidCollectionType_ReturnsEmptyList()
    {
        // Arrange
        var oldContact = new Contacts();
        var newContact = new Contacts();
        var draftId = Guid.NewGuid();
        var userId = "test-user-id";
        var propertyPrefix = "Contacts[Office].Invalid";

        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("GetContactCollectionLogs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<ChangesLog>)methodInfo.Invoke(service,
        [
        oldContact, newContact, (ContactCollectionType)999, draftId, userId, propertyPrefix
        ]);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    #endregion

    #region LogImageDeletions

    [Test]
    public void LogImageDeletions_WithRemovedImages_LogsSuccessfully()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "Workshop";
        var userId = "test-user-id";

        var oldImageIds = new[] { "image1", "image2", "image3" };
        var newImageIds = new[] { "image1" };

        var service = GetChangesLogService();

        changesLogRepository.Setup(r => r.AddChangeLogsToDbContext(
                It.IsAny<IEnumerable<ChangesLog>>()))
            .Verifiable();

        // Act
        service.LogImageDeletions(oldImageIds, newImageIds, entityId, entityType, userId);

        // Assert
        changesLogRepository.Verify(r => r.AddChangeLogsToDbContext(
            It.Is<IEnumerable<ChangesLog>>(logs =>
                logs.Count() == 2 &&
                logs.All(l => l.EntityType == entityType && l.EntityIdGuid == entityId))),
            Times.Once);
    }

    [Test]
    public void LogImageDeletions_WithoutRemovedImages_DoesNotLogAnything()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "Workshop";
        var userId = "test-user-id";

        var oldImageIds = new[] { "image1", "image2" };
        var newImageIds = new[] { "image1", "image2" };

        var service = GetChangesLogService();

        // Act
        service.LogImageDeletions(oldImageIds, newImageIds, entityId, entityType, userId);

        // Assert
        changesLogRepository.Verify(r => r.AddChangeLogsToDbContext(It.IsAny<IEnumerable<ChangesLog>>()), Times.Never);
    }

    #endregion

    #region GetWorkshopDraftAccessPredicateAsync

    [Test]
    public async Task GetWorkshopDraftAccessPredicateAsync_MinistryAdmin_ReturnsCorrectPredicate()
    {
        // Arrange
        var userId = "ministry-admin-id";
        var institutionId = Guid.NewGuid();

        currentUserServiceMock.Setup(s => s.IsMinistryAdmin()).Returns(true);
        currentUserServiceMock.Setup(s => s.IsRegionAdmin()).Returns(false);
        currentUserServiceMock.Setup(s => s.IsAreaAdmin()).Returns(false);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var ministryAdmin = new MinistryAdminDto
        {
            InstitutionId = institutionId
        };

        ministryAdminServiceMock.Setup(s => s.GetByUserId(userId))
            .ReturnsAsync(ministryAdmin);

        // Create test data
        var workshopDrafts = new List<WorkshopDraft>
        {
            new() {
                Id = Guid.NewGuid(),
                Provider = new Provider { InstitutionId = institutionId }
            },
            new() {
                Id = Guid.NewGuid(),
                Provider = new Provider { InstitutionId = Guid.NewGuid() }
            }
        };

        var mockData = workshopDrafts.AsQueryable().BuildMock();

        // Use private method via reflection to test
        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("GetWorkshopDraftAccessPredicateAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var predicateTask = (Task<Expression<Func<WorkshopDraft, bool>>>)methodInfo.Invoke(service, null);
        var predicate = await predicateTask;

        var filteredResult = mockData.Where(predicate).ToList();

        // Assert
        Assert.IsNotNull(filteredResult);
        Assert.AreEqual(1, filteredResult.Count);
        Assert.AreEqual(institutionId, filteredResult[0].Provider.InstitutionId);
    }

    [Test]
    public async Task GetWorkshopDraftAccessPredicateAsync_RegionAdmin_ReturnsCorrectPredicate()
    {
        // Arrange
        var userId = "region-admin-id";
        var institutionId = Guid.NewGuid();
        var catottgId = 123L;
        var childCatottgId = 456L;

        currentUserServiceMock.Setup(s => s.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(s => s.IsRegionAdmin()).Returns(true);
        currentUserServiceMock.Setup(s => s.IsAreaAdmin()).Returns(false);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var regionAdmin = new RegionAdminDto
        {
            InstitutionId = institutionId,
            CATOTTGId = catottgId
        };

        regionAdminServiceMock.Setup(s => s.GetByUserId(userId))
            .ReturnsAsync(regionAdmin);

        codeficatorServiceMock.Setup(s => s.GetAllChildrenIdsByParentIdAsync(catottgId))
            .ReturnsAsync(new List<long> { childCatottgId });

        // Use private method via reflection to test
        var service = GetChangesLogService();
        var methodInfo = typeof(ChangesLogService).GetMethod("GetWorkshopDraftAccessPredicateAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var predicateTask = (Task<Expression<Func<WorkshopDraft, bool>>>)methodInfo.Invoke(service, null);

        var predicate = await predicateTask;

        // Assert
        Assert.IsNotNull(predicate);
    }

    #endregion

    private IOptions<ChangesLogConfig> CreateChangesLogOptions()
    {
        return Options.Create(new ChangesLogConfig
        {
            TrackedProperties = new Dictionary<string, string[]>
           {
               { "Provider", new[] { "FullTitle", "EdrpouIpn", "Director", "LegalAddress" } },
               { "WorkshopDraftContent", new[] { "Title", "ProviderTitle", "WorkshopDescriptionItems" } }
           },
        });
    }

    private IChangesLogService GetChangesLogService()
        => new ChangesLogService(
            CreateChangesLogOptions(),
            changesLogRepository.Object,
            providerRepository.Object,
            applicationRepository.Object,
            workshopDraftRepositoryMock.Object,
            employeeChangesLogRepository.Object,
            parentBlockedByAdminLogRepository.Object,
            workshopRepository.Object,
            logger.Object,
            valueProjector.Object,
            currentUserServiceMock.Object,
            ministryAdminServiceMock.Object,
            regionAdminServiceMock.Object,
            areaAdminServiceMock.Object,
            codeficatorServiceMock.Object,
            nestedObjectChangeLoggerMock.Object,
            collectionChangeLoggerMock.Object
            );
}