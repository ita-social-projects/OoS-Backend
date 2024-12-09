using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Models.Emails;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ApplicationServiceTests
{
    private IApplicationService service;
    private Mock<IApplicationRepository> applicationRepositoryMock;
    private Mock<IWorkshopRepository> workshopRepositoryMock;
    private Mock<ILogger<ApplicationService>> logger;
    private Mock<IMapper> mapper;
    private Mock<INotificationService> notificationService;
    private Mock<IEmployeeService> providerAdminService;
    private Mock<IChangesLogService> changesLogService;
    private Mock<IWorkshopServicesCombiner> workshopServiceCombinerMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IAreaAdminService> areaAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<IRazorViewToStringRenderer> rendererMock;
    private Mock<IEmailSenderService> emailSenderMock;
    private Mock<IStringLocalizer<SharedResource>> localizerMock;
    private Mock<IOptions<HostsConfig>> hostsConfigMock;

    private Mock<IOptions<ApplicationsConstraintsConfig>> applicationsConstraintsConfig;

    [SetUp]
    public void SetUp()
    {
        applicationRepositoryMock = new Mock<IApplicationRepository>();
        workshopRepositoryMock = new Mock<IWorkshopRepository>();
        notificationService = new Mock<INotificationService>();
        providerAdminService = new Mock<IEmployeeService>();
        changesLogService = new Mock<IChangesLogService>();
        workshopServiceCombinerMock = new Mock<IWorkshopServicesCombiner>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        areaAdminServiceMock = new Mock<IAreaAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        rendererMock = new Mock<IRazorViewToStringRenderer>();
        emailSenderMock = new Mock<IEmailSenderService>();
        localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        hostsConfigMock = new Mock<IOptions<HostsConfig>>();

        logger = new Mock<ILogger<ApplicationService>>();
        mapper = new Mock<IMapper>();

        applicationsConstraintsConfig = new Mock<IOptions<ApplicationsConstraintsConfig>>();
        applicationsConstraintsConfig.Setup(x => x.Value)
            .Returns(new ApplicationsConstraintsConfig()
            {
                ApplicationsLimit = 2,
                ApplicationsLimitDays = 7,
            });

        var config = new HostsConfig();
        config.FrontendUrl = "http://localhost:4200";
        config.BackendUrl = "http://localhost:5443";
        hostsConfigMock.Setup(x => x.Value).Returns(config);

        rendererMock.Setup(x => x.GetHtmlPlainStringAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string.Empty, string.Empty));

        service = new ApplicationService(
            applicationRepositoryMock.Object,
            logger.Object,
            workshopRepositoryMock.Object,
            mapper.Object,
            applicationsConstraintsConfig.Object,
            notificationService.Object,
            providerAdminService.Object,
            changesLogService.Object,
            workshopServiceCombinerMock.Object,
            currentUserServiceMock.Object,
            ministryAdminServiceMock.Object,
            regionAdminServiceMock.Object,
            areaAdminServiceMock.Object,
            codeficatorServiceMock.Object,
            rendererMock.Object,
            emailSenderMock.Object,
            localizerMock.Object,
            hostsConfigMock.Object);
    }

    [Test]
    public async Task GetApplicationById_WhenIdIsValid_ShouldReturnApplication()
    {
        // Arrange
        var id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd");
        SetupGetById(WithApplication(id));

        // Act
        var result = await service.GetById(id).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedApplicationGetByIdSuccess(id));
    }

    [Test]
    public async Task GetApplicationById_WhenIdIsNotValid_ShouldReturnNull()
    {
        // Act
        var result = await service.GetById(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateApplication_WhenCalled_ShouldReturnApplication()
    {
        // Arrange
        var statusKey = "Status";
        var workshopList = WithWorkshopsList();

        var newApplication = ApplicationGenerator.Generate().WithWorkshop(WorkshopGenerator.Generate().WithProvider(ProvidersGenerator.Generate()));
        newApplication.WorkshopId = workshopList.FirstOrDefault(x => x.Status == WorkshopStatus.Open).Id;
        newApplication.Workshop.Id = newApplication.WorkshopId;
        newApplication.Status = ApplicationStatus.Pending;

        var applicationForCreation = new Application()
        {
            Id = newApplication.Id,
            WorkshopId = newApplication.WorkshopId,
            CreationTime = newApplication.CreationTime,
            Status = ApplicationStatus.Pending,
            ChildId = newApplication.ChildId,
            ParentId = newApplication.ParentId,
        };

        var applicationDto = new ApplicationDto()
        {
            Id = newApplication.Id,
            WorkshopId = newApplication.WorkshopId,
            CreationTime = newApplication.CreationTime,
            Status = ApplicationStatus.Pending,
            ChildId = newApplication.ChildId,
            ParentId = newApplication.ParentId,
        };

        applicationRepositoryMock.Setup(w => w.Create(applicationForCreation)).Returns(Task.FromResult(newApplication));
        mapper.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>())).Returns(applicationDto);
        mapper.Setup(m => m.Map<Application>(It.IsAny<ApplicationCreate>())).Returns(applicationForCreation);

        var input = new ApplicationCreate()
        {
            WorkshopId = newApplication.WorkshopId,
            ChildId = newApplication.ChildId,
            ParentId = newApplication.ParentId,
        };
        SetupCreate(newApplication);

        var recipientsIds = new List<string>()
        {
            newApplication.Workshop.Provider.UserId,
        };

        // Act
        var result = await service.Create(input).ConfigureAwait(false);

        // Assert
        notificationService.Verify(
            x => x.Create(
                NotificationType.Application,
                NotificationAction.Create,
                newApplication.Id,
                recipientsIds,
                It.Is<Dictionary<string, string>>(c => c.ContainsKey(statusKey) && c[statusKey] == newApplication.Status.ToString()),
                newApplication.Status.ToString()),
            Times.Once);

        result.Should().BeEquivalentTo(
            new ModelWithAdditionalData<ApplicationDto, int>
            {
                Model = ExpectedApplicationCreate(newApplication),
                AdditionalData = 0,
            });
    }

    [Test]
    public void CreateApplication_WhenModelIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        ApplicationCreate application = null;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.Create(application).ConfigureAwait(false));
    }

    [Test]
    public async Task CreateApplication_WhenLimitIsExceeded_ShouldThrowArgumentException()
    {
        // Arrange
        var application = new ApplicationCreate()
        {
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
        };

        currentUserServiceMock.Setup(x => x.UserHasRights(It.IsAny<ParentRights>())).Returns(() => Task.FromResult(true));

        workshopServiceCombinerMock.Setup(x => x.GetById(application.WorkshopId, It.IsAny<bool>())).Returns(Task.FromResult(new WorkshopDto()));

        var applications = new List<Application>
        {
            new Application(),
            new Application(),
            new Application(),
        };

        applicationRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Application, bool>>>(), It.IsAny<string>())).ReturnsAsync(applications.AsEnumerable());

        // Act
        var result = await service.Create(application);

        // Assert
        Assert.That(result.Model is null);
        Assert.That(result.Description != string.Empty);
    }

    [Test]
    public void CreateApplication_WhenParametersAreNotValid_ShouldThrowArgumentException()
    {
        // Arrange
        var application = new ApplicationCreate()
        {
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
        };

        // Act and Assert
        service.Invoking(w => w.Create(application)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public void CreateApplication_WhenStatusWorkshopIsClosed_ShouldReturnArgumentException()
    {
        var workshopList = WithWorkshopsList();

        var input = new ApplicationCreate()
        {
            WorkshopId = workshopList.FirstOrDefault(x => x.Status == WorkshopStatus.Closed).Id,
        };

        var mockWorkshop = workshopList.FirstOrDefault(x => x.Status == WorkshopStatus.Closed);

        workshopRepositoryMock.Setup(x => x.GetById(input.WorkshopId)).ReturnsAsync(mockWorkshop);

        // Act and assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await service.Create(input).ConfigureAwait(false));
    }

    [Test]
    public async Task GetAllByWorkshop_WhenIdIsValid_ShouldReturnApplications()
    {
        // Arrange
        var existingApplications = WithApplicationsList();
        SetupGetAllByWorkshop(existingApplications);
        var applicationFilter = new ApplicationFilter
        {
            Statuses = null,
            OrderByAlphabetically = false,
            OrderByStatus = false,
            OrderByDateAscending = false,
        };

        // Act
        var result = await service.GetAllByWorkshop(existingApplications.First().Id, existingApplications.First().Workshop.ProviderId, applicationFilter)
            .ConfigureAwait(false);

        // Assert
        result.Entities.Should().BeEquivalentTo(ExpectedApplicationsGetAll(existingApplications));
        currentUserServiceMock.Verify(
            a => a.UserHasRights(
                It.Is<IUserRights[]>(u => u.First() is ProviderRights && ((ProviderRights)u.First()).providerId == existingApplications.First().Workshop.ProviderId)));
    }

    [Test]
    public async Task GetAllByWorkshop_WhenIdIsNotValid_ShouldReturnEmptyCollection()
    {
        // Arrange
        SetupGetAllByWorkshopEmpty();
        var filter = new ApplicationFilter
        {
            Statuses = null,
            OrderByAlphabetically = false,
            OrderByStatus = false,
            OrderByDateAscending = false,
        };

        // Act
        var result = await service.GetAllByWorkshop(Guid.NewGuid(), Guid.NewGuid(), filter).ConfigureAwait(false);

        // Assert
        result.Entities.Should().BeEmpty();
    }

    [Test]
    public void GetAllByWorkshop_WhenFilterIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        ApplicationFilter filter = null;

        // Act and Assert
        service.Invoking(s => s.GetAllByWorkshop(Guid.NewGuid(), Guid.NewGuid(), filter)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task GetAllByProvider_WhenIdIsValid_ShouldReturnApplications()
    {
        // Arrange
        var existingApplications = WithApplicationsList();
        SetupGetAllByProvider(existingApplications);
        var applicationFilter = new ApplicationFilter
        {
            Statuses = null,
            OrderByAlphabetically = false,
            OrderByStatus = false,
            OrderByDateAscending = false,
        };

        // Act
        var result = await service.GetAllByProvider(existingApplications.First().Id, applicationFilter)
            .ConfigureAwait(false);

        // Assert
        result.Entities.Should().BeEquivalentTo(ExpectedApplicationsGetAll(existingApplications));
    }

    [Test]
    public async Task GetAllByProvider_WhenIdIsNotValid_ShouldReturnEmptyCollection()
    {
        // Arrange
        SetupGetAllByProviderEmpty();
        var filter = new ApplicationFilter
        {
            Statuses = null,
            OrderByAlphabetically = false,
            OrderByStatus = false,
            OrderByDateAscending = false,
        };

        // Act
        var result = await service.GetAllByProvider(Guid.NewGuid(), filter).ConfigureAwait(false);

        // Assert
        result.Entities.Should().BeEmpty();
    }

    [Test]
    public void GetAllByProvider_WhenFilterIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        ApplicationFilter filter = null;

        // Act and Assert
        service.Invoking(s => s.GetAllByProvider(Guid.NewGuid(), filter)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task GetAllByProviderAdmin_WhenIdIsValid_ShouldReturnApplications()
    {
        // Arrange
        var existingApplications = WithApplicationsList();
        var mappedDtos = existingApplications.Select(a => new ApplicationDto() { Id = a.Id }).ToList();
        var providerAdmin = new EmployeeProviderRelationDto()
        {
            UserId = Guid.NewGuid().ToString(),
            ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
        };
        providerAdminService.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(providerAdmin);
        currentUserServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        var applicationFilter = new ApplicationFilter
        {
            Statuses = null,
            OrderByAlphabetically = false,
            OrderByStatus = false,
            OrderByDateAscending = false,
        };
        var workshopsMock = WithWorkshopsList().AsQueryable().BuildMock();
        workshopRepositoryMock.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(workshopsMock)
            .Verifiable();
        var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();
        applicationRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(applicationsMock)
            .Verifiable();
        mapper.Setup(x => x.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(mappedDtos);

        // Act
        var result = await service.GetAllByEmployee(providerAdmin.UserId, applicationFilter)
            .ConfigureAwait(false);

        // Assert
        result.Entities.Should().BeEquivalentTo(ExpectedApplicationsGetAll(existingApplications));
    }

    [Test]
    public async Task GetAllByParent_WhenIdIsValid_ShouldReturnApplications()
    {
        // Arrange
        var existingApplications = WithApplicationsList();
        SetupGetAllBy(existingApplications);
        var filter = new ApplicationFilter();
        var mockQuery = existingApplications.AsTestAsyncEnumerableQuery();
        applicationRepositoryMock.Setup(r => r.Get(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<Application, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
            false)).Returns(mockQuery);

        // Act
        var result = await service.GetAllByParent(existingApplications.First().ParentId, filter).ConfigureAwait(false);

        // Assert
        result.Entities.Should().BeEquivalentTo(ExpectedApplicationsGetAll(existingApplications));
    }

    [Test]
    public async Task GetAllByParent_WhenIdIsNotValid_ShouldReturnEmptyCollection()
    {
        // Arrange
        var mockQuery = new List<Application>().AsTestAsyncEnumerableQuery();
        applicationRepositoryMock.Setup(r => r.Get(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<Application, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
            false)).Returns(mockQuery);

        // Act
        var filter = new ApplicationFilter();
        var result = await service.GetAllByParent(Guid.NewGuid(), filter).ConfigureAwait(false);

        // Assert
        Assert.That(result.Entities, Is.Null);
    }

    [Test]
    public async Task GetCountByParentId_WhenIdIsValid_ShouldReturnCount()
    {
        // Arrange
        currentUserServiceMock.Setup(c => c.IsEmployee()).Returns(true);
        var existingApplications = WithApplicationsList();
        var parentId = existingApplications.First().ParentId;
        var expectedCount = existingApplications.Count(x => x.ParentId == parentId);
        applicationRepositoryMock.Setup(a => a.Count(
                It.IsAny<Expression<Func<Application, bool>>>()))
            .Returns(Task.FromResult<int>(expectedCount));

        // Act
        var result = await service.GetCountByParentId(parentId).ConfigureAwait(false);

        // Assert
        result.Should().Be(expectedCount);
    }

    [Test]
    public async Task GetCountByParentId_WhenIdIsNotValid_ShouldReturnZero()
    {
        // Arrange
        currentUserServiceMock.Setup(c => c.IsEmployee()).Returns(true);

        // Act
        var result = await service.GetCountByParentId(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task GetCountByParentId_WhenUserNotAuthorized_ShouldThrowException()
    {
        // Act
        await service.Invoking(s => s.GetCountByParentId(Guid.NewGuid())).Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task GetAllByChild_WhenIdIsValid_ShouldReturnApplications()
    {
        // Arrange
        var existingApplications = WithApplicationsList();
        SetupGetAllBy(existingApplications);

        // Act
        var result = await service.GetAllByChild(existingApplications.First().ChildId).ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(ExpectedApplicationsGetAll(existingApplications));
    }

    [Test]
    public async Task GetAllByChild_WhenIdIsNotValid_ShouldReturnEmptyCollection()
    {
        // Act
        var result = await service.GetAllByChild(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    [TestCase("provider", ApplicationStatus.AcceptedForSelection, ApplicationStatus.Approved)]
    [TestCase("provider", ApplicationStatus.AcceptedForSelection, ApplicationStatus.Rejected)]
    [TestCase("provider", ApplicationStatus.Pending, ApplicationStatus.AcceptedForSelection)]
    [TestCase("parent", ApplicationStatus.Approved, ApplicationStatus.Left)]
    public async Task UpdateApplication_WhenIdIsValid_ShouldReturnApplication(string userRole, ApplicationStatus statusFrom, ApplicationStatus statusTo)
    {
        // Arrange
        var statusKey = "Status";
        var id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd");
        var changedEntity = WithApplication(id, statusFrom, true).WithParent(ParentGenerator.Generate());
        changedEntity.Parent.User = UserGenerator.Generate();
        changedEntity.Parent.UserId = changedEntity.Parent.User.Id;
        changedEntity.Workshop.WithProvider(ProvidersGenerator.Generate());

        var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();

        applicationRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(applicationsMock)
            .Verifiable();

        applicationRepositoryMock.Setup(a => a.Update(It.IsAny<Application>(), It.IsAny<Action<Application>>()))
            .ReturnsAsync(changedEntity);
        applicationRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(changedEntity);
        applicationRepositoryMock.Setup(a => a.Count(It.IsAny<Expression<Func<Application, bool>>>())).ReturnsAsync(int.MaxValue);
        applicationRepositoryMock.Setup(a => a.GetByFilter(It.IsAny<Expression<Func<Application, bool>>>(), It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<Application>>(new List<Application> { changedEntity }));

        mapper.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>())).Returns(new ApplicationDto() {Id = id});

        var expected = new ApplicationDto() {Id = id};
        var update = new ApplicationUpdate
        {
            Id = id,
            Status = statusTo,
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
            ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
        };

        workshopServiceCombinerMock.Setup(c =>
                c.GetById(It.Is<Guid>(i => i == update.WorkshopId), It.IsAny<bool>()))
            .ReturnsAsync(new WorkshopDto()
            {
                Id = update.WorkshopId,
                AvailableSeats = uint.MaxValue,
                Status = WorkshopStatus.Open,
            });

        workshopRepositoryMock.Setup(w => w.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Workshop>());

        workshopRepositoryMock.Setup(w => w.GetAvailableSeats(It.IsAny<Guid>())).ReturnsAsync(uint.MaxValue);

        currentUserServiceMock.Setup(c => c.UserRole).Returns(userRole);
        applicationRepositoryMock.Setup(a => a.Count(It.IsAny<Expression<Func<Application, bool>>>())).ReturnsAsync(int.MinValue);

        var recipientsIds = new List<string>();

        if (statusTo == ApplicationStatus.Approved
            || statusTo == ApplicationStatus.Rejected
            || statusTo == ApplicationStatus.AcceptedForSelection)
        {
            recipientsIds.Add(changedEntity.Parent.UserId);
        }
        else if (statusTo == ApplicationStatus.Left)
        {
            recipientsIds.Add(changedEntity.Workshop.Provider.UserId);
        }

        // Act
        var response = await service.Update(update, Guid.NewGuid()).ConfigureAwait(false);
        ApplicationDto result = new();
        response.Match(
            errResult => result = null,
            response => result = response);

        // Assert
        Assert.NotNull(result);
        AssertApplicationsDTOsAreEqual(expected, result);

        notificationService.Verify(
            x => x.Create(
                NotificationType.Application,
                NotificationAction.Update,
                changedEntity.Id,
                recipientsIds,
                It.Is<Dictionary<string, string>>(c => c.ContainsKey(statusKey) && c[statusKey] == changedEntity.Status.ToString()),
                changedEntity.Status.ToString()),
            Times.Once);
    }

    [Test]
    public async Task UpdateApplication_WhenIdIsValidAndNeedUpdateWorkshopStatus_ShouldReturnApplication()
    {
        // Arrange
        var id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd");
        var entity = WithApplication(id);
        var changedEntity = WithApplication(id, ApplicationStatus.Approved);
        var workshop = WithWorkshop(new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"));

        applicationRepositoryMock.Setup(a => a.Update(It.IsAny<Application>(), It.IsAny<Action<Application>>()))
            .ReturnsAsync(changedEntity);

        applicationRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(entity);
        applicationRepositoryMock.Setup(a => a.Count(x =>
                x.WorkshopId == workshop.Id &&
                (x.Status == ApplicationStatus.Approved || x.Status == ApplicationStatus.StudyingForYears)))
            .ReturnsAsync(1);

        workshopRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(workshop);
        mapper.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>())).Returns(new ApplicationDto()
            {Id = id, Status = ApplicationStatus.Approved});

        var expected = new ApplicationDto() {Id = id, Status = ApplicationStatus.Approved};
        var update = new ApplicationUpdate
        {
            Id = id,
            Status = ApplicationStatus.Approved,
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
            ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
        };

        workshopServiceCombinerMock.Setup(c =>
                c.GetById(It.Is<Guid>(i => i == update.WorkshopId), It.IsAny<bool>()))
            .ReturnsAsync(new WorkshopDto()
            {
                Id = update.WorkshopId,
                AvailableSeats = 5,
                Status = WorkshopStatus.Open,
            });

        workshopRepositoryMock.Setup(w => w.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Workshop>());

        workshopRepositoryMock.Setup(w => w.GetAvailableSeats(It.IsAny<Guid>())).ReturnsAsync(uint.MaxValue);

        currentUserServiceMock.Setup(c => c.UserRole).Returns("provider");

        // Act
        var response = await service.Update(update, Guid.NewGuid()).ConfigureAwait(false);
        ApplicationDto result = new ApplicationDto();
        response.Match(
            actionResult => result = null,
            succeed => result = succeed);

        // Assert
        Assert.NotNull(result);
        AssertApplicationsDTOsAreEqual(expected, result);
    }

    [Test]
    public async Task UpdateApplication_WhenThereIsNoApplicationWithId_ShouldReturnErrorResponse()
    {
        // Arrange
        var application = new ApplicationUpdate()
        {
            Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd"),
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
            Status = ApplicationStatus.Pending,
        };

        // Act
        var response = await service.Update(application, Guid.NewGuid());

        // Assert
        Assert.IsInstanceOf<ErrorResponse>(response.Match(
            error => error,
            succeed => null));
    }

    [Test]
    public async Task UpdateApplication_WhenThereIsAlreadyApprovedWorkshop_ShouldReturnErrorResponse()
    {
        // Arrange
        var id = new Guid("08da8609-a211-4d74-82c0-dc89ea1fb2a7");
        var entity = WithApplication(id);
        var changedEntity = WithApplication(id, ApplicationStatus.Approved);
        var workshop = WithWorkshop(new Guid("08da85ea-5b3c-4991-8416-2673d9421ca9"));

        applicationRepositoryMock.Setup(a => a.Update(It.IsAny<Application>(), It.IsAny<Action<Application>>()))
            .ReturnsAsync(changedEntity);

        applicationRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(entity);
        applicationRepositoryMock.Setup(a => a.Count(x =>
                x.WorkshopId == workshop.Id &&
                Application.ValidApplicationStatuses.Contains(x.Status)))
            .ReturnsAsync(1);
        workshopRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(workshop);
        mapper.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>())).Returns(new ApplicationDto()
        { Id = id, Status = ApplicationStatus.Approved });

        var update = new ApplicationUpdate
        {
            Id = id,
            Status = ApplicationStatus.Approved,
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
            ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
        };

        workshopServiceCombinerMock.Setup(c =>
                c.GetById(It.Is<Guid>(i => i == update.WorkshopId), It.IsAny<bool>()))
            .ReturnsAsync(new WorkshopDto()
            {
                Id = update.WorkshopId,
                AvailableSeats = 0,
                Status = WorkshopStatus.Open,
            });

        workshopRepositoryMock.Setup(w => w.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Workshop>()
            {
                new Workshop()
                {
                    Id = update.WorkshopId,
                },
            });

        workshopRepositoryMock.Setup(w => w.GetAvailableSeats(It.IsAny<Guid>())).ReturnsAsync(uint.MaxValue);

        currentUserServiceMock.Setup(c => c.UserRole).Returns("provider");

        applicationRepositoryMock.Setup(a => a.Count(It.IsAny<Expression<Func<Application, bool>>>())).ReturnsAsync(int.MaxValue);

        // Act
        var response = await service.Update(update, Guid.NewGuid());

        // Assert
        Assert.IsInstanceOf<ErrorResponse>(response.Match(
            error => error,
            succeed => null));
    }

    [Test]
    public async Task UpdateApplication_WhenThereIsNoAlreadyApprovedWorkshop_Succeeded()
    {
        // Arrange
        var id = new Guid("08da8609-a211-4d74-82c0-dc89ea1fb2a7");
        var entity = WithApplication(id);
        var changedEntity = WithApplication(id, ApplicationStatus.Approved);
        var workshop = WithWorkshop(new Guid("08da85ea-5b3c-4991-8416-2673d9421ca9"));

        applicationRepositoryMock.Setup(a => a.Update(It.IsAny<Application>(), It.IsAny<Action<Application>>()))
            .ReturnsAsync(changedEntity);

        applicationRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(entity);
        applicationRepositoryMock.Setup(a => a.Count(x =>
                x.WorkshopId == workshop.Id &&
                Application.ValidApplicationStatuses.Contains(x.Status)))
            .ReturnsAsync(1);
        workshopRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(workshop);
        mapper.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>())).Returns(new ApplicationDto()
        { Id = id, Status = ApplicationStatus.Approved });

        var update = new ApplicationUpdate
        {
            Id = id,
            Status = ApplicationStatus.Approved,
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
            ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
        };

        workshopServiceCombinerMock.Setup(c =>
                c.GetById(It.Is<Guid>(i => i == update.WorkshopId), It.IsAny<bool>()))
            .ReturnsAsync(new WorkshopDto()
            {
                Id = update.WorkshopId,
                AvailableSeats = 0,
                Status = WorkshopStatus.Open,
            });

        workshopRepositoryMock.Setup(w => w.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Workshop>()
            {
                new Workshop()
                {
                    Id = update.WorkshopId,
                },
            });

        workshopRepositoryMock.Setup(w => w.GetAvailableSeats(It.IsAny<Guid>())).ReturnsAsync(uint.MaxValue);

        currentUserServiceMock.Setup(c => c.UserRole).Returns("provider");

        applicationRepositoryMock.Setup(a => a.Count(It.IsAny<Expression<Func<Application, bool>>>())).ReturnsAsync(int.MinValue);

        // Act
        var response = await service.Update(update, Guid.NewGuid());

        // Assert
        Assert.IsInstanceOf<ApplicationDto>(response.Match(
            error => null,
            succeed => succeed));
    }

    [Test]
    public async Task UpdateApplication_WhenAmountOfApprovedBiggerThanAvailableSeats_Succeeded()
    {
        // Arrange
        var id = new Guid("08da8609-a211-4d74-82c0-dc89ea1fb2a7");
        var entity = WithApplication(id);
        var changedEntity = WithApplication(id, ApplicationStatus.Approved);
        var workshop = WithWorkshop(new Guid("08da85ea-5b3c-4991-8416-2673d9421ca9"));

        applicationRepositoryMock.Setup(a => a.Update(It.IsAny<Application>(), It.IsAny<Action<Application>>()))
            .ReturnsAsync(changedEntity);

        applicationRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(entity);
        applicationRepositoryMock.Setup(a => a.Count(x =>
                x.WorkshopId == workshop.Id &&
                Application.ValidApplicationStatuses.Contains(x.Status)))
            .ReturnsAsync(1);
        workshopRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(workshop);
        mapper.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>())).Returns(new ApplicationDto()
        { Id = id, Status = ApplicationStatus.Approved });

        var update = new ApplicationUpdate
        {
            Id = id,
            Status = ApplicationStatus.Approved,
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
            ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
        };

        workshopServiceCombinerMock.Setup(c =>
                c.GetById(It.Is<Guid>(i => i == update.WorkshopId), It.IsAny<bool>()))
            .ReturnsAsync(new WorkshopDto()
            {
                Id = update.WorkshopId,
                AvailableSeats = 0,
                Status = WorkshopStatus.Open,
            });

        workshopRepositoryMock.Setup(w => w.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Workshop>()
            {
                new Workshop()
                {
                    Id = update.WorkshopId,
                },
            });

        workshopRepositoryMock.Setup(w => w.GetAvailableSeats(It.IsAny<Guid>())).ReturnsAsync(uint.MinValue);

        currentUserServiceMock.Setup(c => c.UserRole).Returns("provider");

        applicationRepositoryMock.Setup(a => a.Count(It.IsAny<Expression<Func<Application, bool>>>())).ReturnsAsync(int.MaxValue);

        // Act
        var response = await service.Update(update, Guid.NewGuid());

        // Assert
        Assert.IsInstanceOf<ErrorResponse>(response.Match(
            error => error,
            succeed => null));
    }

    [Test]
    public void UpdateApplication_WhenModelIsNull_ShouldThrowArgumentException()
    {
        // Act and Assert
        service.Invoking(s => s.Update(null, Guid.NewGuid())).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    [TestCase(ApplicationStatus.Approved)]
    [TestCase(ApplicationStatus.Rejected)]
    [TestCase(ApplicationStatus.AcceptedForSelection)]
    public async Task UpdateApplication_WhenApplicationStatusIsApprovedRejectedAcceptedForSelection_ShouldSendEmail(ApplicationStatus status)
    {
        // Arrange
        var id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd");
        var changedEntity = WithApplication(id);
        if (status == ApplicationStatus.AcceptedForSelection)
        {
            changedEntity.Workshop.CompetitiveSelection = true;
        }

        var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();

        applicationRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(applicationsMock)
            .Verifiable();

        applicationRepositoryMock.Setup(a => a.Update(It.IsAny<Application>(), It.IsAny<Action<Application>>()))
            .ReturnsAsync(changedEntity);
        applicationRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(changedEntity);
        applicationRepositoryMock.Setup(a => a.Count(It.IsAny<Expression<Func<Application, bool>>>())).ReturnsAsync(int.MaxValue);
        mapper.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>())).Returns(new ApplicationDto() { Id = id });

        var expected = new ApplicationDto() { Id = id };
        var update = new ApplicationUpdate
        {
            Id = id,
            Status = status,
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
            ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
        };

        workshopServiceCombinerMock.Setup(c =>
                c.GetById(It.Is<Guid>(i => i == update.WorkshopId), It.IsAny<bool>()))
            .ReturnsAsync(new WorkshopDto()
            {
                Id = update.WorkshopId,
                AvailableSeats = uint.MaxValue,
                Status = WorkshopStatus.Open,
            });

        workshopRepositoryMock.Setup(w => w.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Workshop>());

        workshopRepositoryMock.Setup(w => w.GetAvailableSeats(It.IsAny<Guid>())).ReturnsAsync(uint.MaxValue);

        currentUserServiceMock.Setup(c => c.UserRole).Returns("provider");
        applicationRepositoryMock.Setup(a => a.Count(It.IsAny<Expression<Func<Application, bool>>>())).ReturnsAsync(int.MinValue);

        // Act
        await service.Update(update, Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        emailSenderMock.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<(string, string)>(), null), Times.Once);
    }

    [Test]
    [TestCase(Gender.Male, "ий")]
    [TestCase(Gender.Female, "а")]
    [TestCase(null, "ий/a")]
    public async Task UpdateApplication_WhenChildGenderMaleFemaleNull_ShouldSendEmailWithCorrectUaEnding(
        Gender? gender,
        string expectedEnding)
    {
        // Arrange
        var id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd");
        var changedEntity = WithApplication(id);
        changedEntity.Child.Gender = gender;

        var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();

        applicationRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(applicationsMock)
            .Verifiable();

        applicationRepositoryMock.Setup(a => a.Update(It.IsAny<Application>(), It.IsAny<Action<Application>>()))
            .ReturnsAsync(changedEntity);
        applicationRepositoryMock.Setup(a => a.GetById(It.IsAny<Guid>())).ReturnsAsync(changedEntity);
        applicationRepositoryMock.Setup(a => a.Count(It.IsAny<Expression<Func<Application, bool>>>())).ReturnsAsync(int.MaxValue);
        mapper.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>())).Returns(new ApplicationDto() { Id = id });

        var expected = new ApplicationDto() { Id = id };
        var update = new ApplicationUpdate
        {
            Id = id,
            Status = ApplicationStatus.Approved,
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
            ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
        };

        workshopServiceCombinerMock.Setup(c =>
                c.GetById(It.Is<Guid>(i => i == update.WorkshopId), It.IsAny<bool>()))
            .ReturnsAsync(new WorkshopDto()
            {
                Id = update.WorkshopId,
                AvailableSeats = uint.MaxValue,
                Status = WorkshopStatus.Open,
            });

        workshopRepositoryMock.Setup(w => w.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Workshop>());

        workshopRepositoryMock.Setup(w => w.GetAvailableSeats(It.IsAny<Guid>())).ReturnsAsync(uint.MaxValue);

        currentUserServiceMock.Setup(c => c.UserRole).Returns("provider");
        applicationRepositoryMock.Setup(a => a.Count(It.IsAny<Expression<Func<Application, bool>>>())).ReturnsAsync(int.MinValue);

        // Act
        await service.Update(update, Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        rendererMock.Verify(
            x => x.GetHtmlPlainStringAsync(
                "ApplicationApprovedEmail",
                It.Is<ApplicationStatusViewModel>(viewModel => viewModel.UaEnding == expectedEnding)),
            Times.Once);
        emailSenderMock.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<(string, string)>(), null), Times.Once);
    }

    private static void AssertApplicationsDTOsAreEqual(ApplicationDto expected, ApplicationDto actual)
    {
        Assert.Multiple(() =>
        {
            Assert.That(expected.Id, Is.EqualTo(actual.Id));
            Assert.That(expected.Status, Is.EqualTo(actual.Status));
            Assert.That(expected.CreationTime, Is.EqualTo(actual.CreationTime));
            Assert.That(expected.ChildId, Is.EqualTo(actual.ChildId));
            Assert.That(expected.ParentId, Is.EqualTo(actual.ParentId));
        });
    }

    #region Setup

    private void SetupCreate(Application application)
    {
        // var workshopMock = WithWorkshopsList().FirstOrDefault(x => x.Id == application.WorkshopId);
        var workshopMock = new WorkshopDto
        {
            Status = WorkshopStatus.Open,
        };

        applicationRepositoryMock.Setup(a => a.GetByFilter(
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<Application>>(new List<Application> {application}));
        workshopServiceCombinerMock.Setup(x => x.GetById(application.WorkshopId, It.IsAny<bool>())).ReturnsAsync(workshopMock);
    }

    private void SetupGetAll(List<Application> apps)
    {
        var mappedDtos = apps.Select(a => new ApplicationDto() {Id = a.Id }).ToList();
        applicationRepositoryMock.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(new List<Application> { apps.First() }.AsTestAsyncEnumerableQuery());
        mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(mappedDtos);
    }

    private void SetupGetAllByInstitutionId(List<Application> apps)
    {
        var mappedDtos = apps.Where(a => a.Workshop.InstitutionHierarchy.InstitutionId == new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"))
            .Select(a => new ApplicationDto()
            {
                Id = a.Id,
                Workshop = new WorkshopCard()
                {
                    InstitutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"),
                },
            })
            .ToList();
        applicationRepositoryMock.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(new List<Application> { apps.First() }.AsTestAsyncEnumerableQuery());
        mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(mappedDtos);
    }

    private void SetupGetAllBy(IEnumerable<Application> apps)
    {
        var mappedDtos = apps.Select(a => new ApplicationDto() {Id = a.Id}).ToList();

        applicationRepositoryMock.Setup(a => a.GetByFilter(
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<Application>>(new List<Application> {apps.First() }));
        mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(mappedDtos);
    }

    private void SetupGetAllByWorkshop(List<Application> apps)
    {
        var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();
        var mappedDtos = apps.Select(a => new ApplicationDto() {Id = a.Id}).ToList();

        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.UserHasRights(new IUserRights[] {new ProviderRights(apps.First().Workshop.ProviderId) }))
            .Verifiable();

        applicationRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(applicationsMock)
            .Verifiable();
        mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(mappedDtos);
    }

    private void SetupGetAllByWorkshopEmpty()
    {
        var emptyApplicationsList = new List<Application>().AsQueryable().BuildMock();
        var emptyApplicationDtosList = new List<ApplicationDto>();

        applicationRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(emptyApplicationsList)
            .Verifiable();
        mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(emptyApplicationDtosList);
    }

    private void SetupGetAllByProvider(IEnumerable<Application> apps)
    {
        var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();
        var workshopsMock = WithWorkshopsList().AsQueryable().BuildMock();
        var mappedDtos = apps.Select(a => new ApplicationDto() {Id = a.Id}).ToList();

        workshopRepositoryMock.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(workshopsMock)
            .Verifiable();
        applicationRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(applicationsMock)
            .Verifiable();
        mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(mappedDtos);
    }

    private void SetupGetAllByProviderEmpty()
    {
        var emptyWorkshopsList = new List<Workshop>().AsQueryable().BuildMock();
        var emptyApplicationsList = new List<Application>().AsQueryable().BuildMock();
        var emptyApplicationDtosList = new List<ApplicationDto>();

        workshopRepositoryMock.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(emptyWorkshopsList)
            .Verifiable();
        applicationRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(emptyApplicationsList)
            .Verifiable();
        mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(emptyApplicationDtosList);
    }

    private void SetupGetById(Application application)
    {
        var applicationId = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd");
        applicationRepositoryMock.Setup(a => a.GetById(applicationId)).ReturnsAsync(application);
        applicationRepositoryMock.Setup(a => a.GetByFilter(
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<string>()))
            .Returns(Task.FromResult<IEnumerable<Application>>(new List<Application> {application}));
        mapper.Setup(m => m.Map<ApplicationDto>(application)).Returns(new ApplicationDto() {Id = application.Id});
    }

    private void SetupDelete(Application application)
    {
        var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();

        applicationRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(applicationsMock)
            .Verifiable();
        applicationRepositoryMock.Setup(w => w.GetById(It.IsAny<Guid>())).ReturnsAsync(application);
        applicationRepositoryMock.Setup(a => a.Delete(It.IsAny<Application>())).Returns(Task.CompletedTask);
    }

    #endregion

    #region With

    private List<Application> WithApplicationsList()
    {
        return new List<Application>()
        {
            new Application()
            {
                Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8db"),
                IsBlockedByProvider = false,
                Status = ApplicationStatus.Pending,
                WorkshopId = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                ChildId = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"),
                ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
                Parent = new Parent()
                {
                    Id = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
                    User = new User()
                    {
                        LastName = "Petroffski",
                    },
                },
                Workshop = new Workshop()
                {
                    Id = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                    ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                    Status = WorkshopStatus.Open,
                    InstitutionHierarchy = new InstitutionHierarchy()
                    {
                        InstitutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"),
                    },
                },
            },
            new Application()
            {
                Id = new Guid("7c5f8f7c-d850-44d0-8d4e-fd2de99453be"),
                IsBlockedByProvider = false,
                Status = ApplicationStatus.Rejected,
                WorkshopId = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                ChildId = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"),
                ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
                Parent = new Parent()
                {
                    Id = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
                    User = new User()
                    {
                        LastName = "Petroffski",
                    },
                },
                Workshop = new Workshop()
                {
                    Id = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                    ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                    Status = WorkshopStatus.Open,
                    InstitutionHierarchy = new InstitutionHierarchy()
                    {
                        InstitutionId = Guid.NewGuid(),
                    },
                },
            },
            new Application()
            {
                Id = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
                IsBlockedByProvider = true,
                Status = ApplicationStatus.Pending,
                WorkshopId = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                ChildId = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"),
                ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
                Parent = new Parent()
                {
                    Id = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
                    User = new User()
                    {
                        LastName = "Petroffski",
                    },
                },
                Workshop = new Workshop()
                {
                    Id = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                    ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                    Status = WorkshopStatus.Open,
                    InstitutionHierarchy = new InstitutionHierarchy()
                    {
                        InstitutionId = Guid.NewGuid(),
                    },
                },
            },
        };
    }

    private Application WithApplication(Guid id, ApplicationStatus status = ApplicationStatus.Pending, bool CompetitiveSelection = false)
    {
        return new Application()
        {
            Id = id,
            WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
            ChildId = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"),
            ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
            Status = status,
            Parent = new Parent()
            {
                Id = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
                User = new User()
                {
                    LastName = "Petroffski",
                },
            },
            Child = new Child()
            {
                Id = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"),
                LastName = "Petroffski",
                FirstName = "Ivan",
                Gender = Gender.Male,
            },
            Workshop = new Workshop()
            {
                Id = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Status = WorkshopStatus.Open,
                CompetitiveSelection = CompetitiveSelection,
            },
        };
    }

    private Workshop WithWorkshop(Guid id)
    {
        return new Workshop()
        {
            Id = id,
            ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
            Status = WorkshopStatus.Open,
            AvailableSeats = 1,
        };
    }

    private List<Workshop> WithWorkshopsList()
    {
        return new List<Workshop>()
        {
            new Workshop()
            {
                Id = new Guid("b94f1989-c4e7-4878-ac86-21c4a402fb43"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Status = WorkshopStatus.Closed,
            },
            new Workshop()
            {
                Id = new Guid("8c14044b-e30d-4b14-a18b-5b3b859ad676"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                Status = WorkshopStatus.Open,
            },
            new Workshop()
            {
                Id = new Guid("3e8845a8-1359-4676-b6d6-5a6b29c122ea"),
                ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
            },
        };
    }

    private List<Child> WithChildList()
    {
        var fakeSocialGroups = new List<SocialGroup>()
        {
            new SocialGroup() {Id = 1, Name = "FakeSocialGroup1"},
            new SocialGroup() {Id = 2, Name = "FakeSocialGroup2"},
        };

        return new List<Child>()
        {
            new Child
            {
                Id = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"), FirstName = "fn1", LastName = "ln1",
                MiddleName = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male,
                ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"), SocialGroups = fakeSocialGroups
            },
            new Child
            {
                Id = new Guid("f29d0e07-e4f2-440b-b0fe-eaa11e31ddae"), FirstName = "fn2", LastName = "ln2",
                MiddleName = "mn2", DateOfBirth = new DateTime(2004, 11, 8), Gender = Gender.Female,
                ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"), SocialGroups = fakeSocialGroups
            },
            new Child
            {
                Id = new Guid("6ddd21d0-2f2e-48a0-beec-fefcb44cd3f0"), FirstName = "fn3", LastName = "ln3",
                MiddleName = "mn3", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male,
                ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"), SocialGroups = fakeSocialGroups
            },
        };
    }

    #endregion

    #region Expected

    private ApplicationDto ExpectedApplicationCreate(Application application)
    {
        return mapper.Object.Map<ApplicationDto>(application);
    }

    private ApplicationDto ExpectedApplicationGetByIdSuccess(Guid id)
    {
        return new ApplicationDto() {Id = id};
    }

    private List<ApplicationDto> ExpectedApplicationsGetAll(IEnumerable<Application> apps)
    {
        return mapper.Object.Map<List<ApplicationDto>>(apps);
    }

    #endregion
}