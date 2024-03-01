using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Application;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Services;
namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ApplicationSensitiveServiceTests
{
    private ISensitiveApplicationService service;
    private ISensitiveApplicationService sensitiveApplicationService;
    private Mock<IApplicationRepository> applicationRepositoryMock;
    private Mock<IWorkshopRepository> workshopRepositoryMock;
    private Mock<ILogger<ApplicationService>> logger;
    private Mock<IMapper> mapper;
    private Mock<INotificationService> notificationService;
    private Mock<IProviderAdminService> providerAdminService;
    private Mock<IChangesLogService> changesLogService;
    private Mock<IWorkshopServicesCombiner> workshopServiceCombinerMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IAreaAdminService> areaAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;

    private Mock<IOptions<ApplicationsConstraintsConfig>> applicationsConstraintsConfig;

    [SetUp]
    public void SetUp()
    {
        applicationRepositoryMock = new Mock<IApplicationRepository>();
        workshopRepositoryMock = new Mock<IWorkshopRepository>();
        notificationService = new Mock<INotificationService>();
        providerAdminService = new Mock<IProviderAdminService>();
        changesLogService = new Mock<IChangesLogService>();
        workshopServiceCombinerMock = new Mock<IWorkshopServicesCombiner>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        areaAdminServiceMock = new Mock<IAreaAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();

        logger = new Mock<ILogger<ApplicationService>>();
        mapper = new Mock<IMapper>();

        applicationsConstraintsConfig = new Mock<IOptions<ApplicationsConstraintsConfig>>();
        applicationsConstraintsConfig.Setup(x => x.Value)
            .Returns(new ApplicationsConstraintsConfig()
            {
                ApplicationsLimit = 2,
                ApplicationsLimitDays = 7,
            });

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
            codeficatorServiceMock.Object);
    }

    [Test]
    public async Task GetApplications_WhenCalled_ShouldReturnApplications()
    {
        // Arrange
        var application = WithApplicationsList();
        SetupGetAll(application);
        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(false);

        // Act
        var result = await service.GetAll(new ApplicationFilter());

        // Assert
        Assert.AreEqual(result.Entities.Count, application.Count);
    }

    [Test]
    public async Task GetApplications_WhenMinistryAdminCalled_ShouldReturnApplications()
    {
        // Arrange
        var institutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4");
        var applications = WithApplicationsList();
        SetupGetAllByInstitutionId(applications);

        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(false);
        ministryAdminServiceMock
            .Setup(m => m.GetByIdAsync(It.IsAny<string>()))
            .Returns(Task.FromResult<MinistryAdminDto>(new MinistryAdminDto()
            {
                InstitutionId = institutionId,
            }));

        // Act
        var result = await service.GetAll(new ApplicationFilter());

        // Assert
        Assert.That(result.Entities.Count, Is.EqualTo(1));
        Assert.That(result.Entities.FirstOrDefault().Workshop.InstitutionId, Is.EqualTo(institutionId));
    }

    [Test]
    public async Task GetApplications_WhenRegionAdminCalled_ShouldReturnApplications()
    {
        // Arrange
        var institutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4");
        long catottgId = 31737;
        var applications = WithApplicationsList();
        SetupGetAllByInstitutionId(applications);

        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(true);
        regionAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<RegionAdminDto>(new RegionAdminDto()
            {
                InstitutionId = institutionId,
                CATOTTGId = catottgId,
            }));

        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(true);
        regionAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<RegionAdminDto>(new RegionAdminDto()
            {
                InstitutionId = institutionId,
            }));

        codeficatorServiceMock
            .Setup(x => x.GetAllChildrenIdsByParentIdAsync(It.IsAny<long>()))
            .Returns(Task.FromResult((IEnumerable<long>)new List<long> { catottgId }));

        // Act
        var result = await service.GetAll(new ApplicationFilter());

        // Assert
        Assert.That(result.Entities.Count, Is.EqualTo(1));
        Assert.That(result.Entities.FirstOrDefault().Workshop.InstitutionId, Is.EqualTo(institutionId));
    }

    [Test]
    public async Task GetApplications_WhenAreaAdminCalled_ShouldReturnApplications()
    {
        // Arrange
        var institutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4");
        long catottgId = 31737;
        var applications = WithApplicationsList();
        SetupGetAllByInstitutionId(applications);

        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.IsAreaAdmin()).Returns(true);
        areaAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<AreaAdminDto>(new AreaAdminDto()
            {
                InstitutionId = institutionId,
                CATOTTGId = catottgId,
            }));

        currentUserServiceMock.Setup(c => c.IsAreaAdmin()).Returns(true);
        areaAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<AreaAdminDto>(new AreaAdminDto()
            {
                InstitutionId = institutionId,
            }));

        codeficatorServiceMock
            .Setup(x => x.GetAllChildrenIdsByParentIdAsync(It.IsAny<long>()))
            .Returns(Task.FromResult((IEnumerable<long>)new List<long> { catottgId }));

        // Act
        var result = await service.GetAll(new ApplicationFilter());

        // Assert
        Assert.That(result.Entities.Count, Is.EqualTo(1));
        Assert.That(result.Entities.FirstOrDefault().Workshop.InstitutionId, Is.EqualTo(institutionId));
    }

    private List<Application> WithApplicationsList()
    {
        return new List<Application>()
        {
            new Application()
            {
                Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8db"),
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
}