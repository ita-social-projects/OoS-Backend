using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ApplicationSensitiveServiceTests
{
    private ISensitiveApplicationService service;
    private string currentUserId;
    private Mock<IApplicationRepository> applicationRepositoryMock;
    private Mock<IMapper> mapper;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IAreaAdminService> areaAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;

    [SetUp]
    public void SetUp()
    {
        applicationRepositoryMock = new Mock<IApplicationRepository>();
        mapper = new Mock<IMapper>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        areaAdminServiceMock = new Mock<IAreaAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();

        service = new ApplicationService(
            applicationRepositoryMock.Object,
            new Mock<ILogger<ApplicationService>>().Object,
            new Mock<IWorkshopRepository>().Object,
            mapper.Object,
            new Mock<IOptions<ApplicationsConstraintsConfig>>().Object,
            new Mock<INotificationService>().Object,
            new Mock<IEmployeeService>().Object,
            new Mock<IChangesLogService>().Object,
            new Mock<IWorkshopServicesCombiner>().Object,
            currentUserServiceMock.Object,
            ministryAdminServiceMock.Object,
            regionAdminServiceMock.Object,
            areaAdminServiceMock.Object,
            codeficatorServiceMock.Object,
            new Mock<IRazorViewToStringRenderer>().Object,
            new Mock<IEmailSenderService>().Object,
            new Mock<IStringLocalizer<SharedResource>>().Object,
            new Mock<IOptions<HostsConfig>>().Object);

        currentUserId = Guid.NewGuid().ToString();
        currentUserServiceMock.SetupGet(c => c.UserId).Returns(currentUserId);
    }

    [Test]
    public void GetAll_WhenNotAdmin_Throws()
    {
        // Arrange
        SetupAdminRights(isAdmin: false);

        // Act, Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await service.GetAll(new ApplicationFilter()));
    }

    [Test]
    public async Task GetAll_WhenMinistryAdmin_ReturnsApplications()
    {
        // Arrange
        var applications = GenerateApplicationsForSameWorkshop();

        SetupAdminRights(isAdmin: true, isMinistry: true);

        var appFilter = new ApplicationFilter();
        SetupRepositoryWithVerification(appFilter, applications);

        ministryAdminServiceMock.Setup(m => m.GetByIdAsync(currentUserId))
            .Returns(Task.FromResult(new MinistryAdminDto() { InstitutionId = applications[0].Workshop.InstitutionHierarchy.InstitutionId }));

        // Act
        var result = await service.GetAll(appFilter);

        // Assert
        mapper.VerifyAll();
        currentUserServiceMock.VerifyAll();
        applicationRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task GetAll_WhenMinistryAdmin_ReturnsRelatedApplications()
    {
        // Arrange
        var applications = GenerateApplicationsForSameWorkshop();
        var institutionId = applications[0].Workshop.InstitutionHierarchy.InstitutionId;

        var mnistryAdmin = AdminGenerator.GenerateMinistryAdminDto();
        mnistryAdmin.InstitutionId = institutionId;

        var otherInstitutionHierarchy = InstitutionHierarchyGenerator.Generate();
        var otherWorkshop = WorkshopGenerator.Generate().WithInstitutionHierarchy(otherInstitutionHierarchy);

        var wrongApplications = applications.Take(1).ToList().WithWorkshop(otherWorkshop);
        var correctApplications = applications.Skip(wrongApplications.Count).ToList();

        SetupAdminRights(isAdmin: true, isMinistry: true);

        ministryAdminServiceMock.Setup(m => m.GetByIdAsync(currentUserId)).Returns(Task.FromResult(mnistryAdmin));

        var appFilter = new ApplicationFilter();
        SetupRepositoryWithVerification(appFilter, correctApplications, wrongApplications);

        // Act
        var result = await service.GetAll(appFilter);

        // Assert
        mapper.VerifyAll();
        currentUserServiceMock.VerifyAll();
        ministryAdminServiceMock.VerifyAll();
        applicationRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task GetAll_WhenRegionAdmin_ReturnsRelatedApplications()
    {
        // Arrange
        var applications = GenerateApplicationsForSameWorkshop();
        var workshop = applications[0].Workshop;
        var institutionId = workshop.InstitutionHierarchy.InstitutionId;
        var catottgId = workshop.Provider.LegalAddress.CATOTTGId;

        var regionAdmin = AdminGenerator.GenerateRegionAdminDto();
        regionAdmin.InstitutionId = institutionId;

        var childrenCatottgIds = new List<long> { catottgId } as IEnumerable<long>;

        var otherInstitutionHierarchyWorkshop = WorkshopGenerator.Generate().WithInstitutionHierarchy(InstitutionHierarchyGenerator.Generate()).WithProvider(workshop.Provider);
        var otherProviderWorkshop = WorkshopGenerator.Generate().WithInstitutionHierarchy(workshop.InstitutionHierarchy).WithProvider(ProvidersGenerator.Generate());

        var wrongApplications = new List<Application> 
        {
            applications[0].WithWorkshop(otherInstitutionHierarchyWorkshop),
            applications[1].WithWorkshop(otherProviderWorkshop),
        };
        var correctApplications = applications.Skip(wrongApplications.Count).ToList();

        SetupAdminRights(isAdmin: true, isRegion: true);

        var appFilter = new ApplicationFilter();
        SetupRepositoryWithVerification(appFilter, correctApplications, wrongApplications);

        regionAdminServiceMock.Setup(m => m.GetByUserId(currentUserId)).Returns(Task.FromResult(regionAdmin));

        codeficatorServiceMock
            .Setup(x => x.GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId))
            .Returns(Task.FromResult(childrenCatottgIds));

        // Act
        var result = await service.GetAll(appFilter);

        // Assert
        mapper.VerifyAll();
        currentUserServiceMock.VerifyAll();
        regionAdminServiceMock.VerifyAll();
        codeficatorServiceMock.VerifyAll();
        applicationRepositoryMock.VerifyAll();
    }

    [Test]
    public async Task GetAll_WhenAreaAdmin_ReturnsRelatedApplications()
    {
        // Arrange
        var applications = GenerateApplicationsForSameWorkshop();
        var workshop = applications[0].Workshop;
        var institutionId = workshop.InstitutionHierarchy.InstitutionId;
        var catottgId = workshop.Provider.LegalAddress.CATOTTGId;

        var areaAdmin = AdminGenerator.GenerateAreaAdminDto();
        areaAdmin.InstitutionId = institutionId;

        var childrenCatottgIds = new List<long> { catottgId } as IEnumerable<long>;

        var otherInstitutionHierarchyWorkshop = WorkshopGenerator.Generate().WithInstitutionHierarchy(InstitutionHierarchyGenerator.Generate()).WithProvider(workshop.Provider);
        var otherProviderWorkshop = WorkshopGenerator.Generate().WithInstitutionHierarchy(workshop.InstitutionHierarchy).WithProvider(ProvidersGenerator.Generate());

        var wrongApplications = new List<Application> 
        {
            applications[0].WithWorkshop(otherInstitutionHierarchyWorkshop),
            applications[1].WithWorkshop(otherProviderWorkshop),
        };
        var correctApplications = applications.Skip(wrongApplications.Count).ToList();

        SetupAdminRights(isAdmin: true, isArea: true);

        var appFilter = new ApplicationFilter();
        SetupRepositoryWithVerification(appFilter, correctApplications, wrongApplications);

        areaAdminServiceMock.Setup(m => m.GetByUserId(currentUserId)).Returns(Task.FromResult(areaAdmin));

        codeficatorServiceMock
            .Setup(x => x.GetAllChildrenIdsByParentIdAsync(areaAdmin.CATOTTGId))
            .Returns(Task.FromResult(childrenCatottgIds));

        // Act
        var result = await service.GetAll(appFilter);

        // Assert
        mapper.VerifyAll();
        currentUserServiceMock.VerifyAll();
        areaAdminServiceMock.VerifyAll();
        codeficatorServiceMock.VerifyAll();
        applicationRepositoryMock.VerifyAll();
    }

    private void SetupAdminRights(bool isAdmin, bool isMinistry = false, bool isRegion = false, bool isArea = false)
    {
        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(isAdmin);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(isMinistry);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(isRegion);
        currentUserServiceMock.Setup(c => c.IsAreaAdmin()).Returns(isArea);
    }

    private List<Application> GenerateApplicationsForSameWorkshop()
    {
        var institutionHierarchy = InstitutionHierarchyGenerator.Generate();
        var workshop = WorkshopGenerator.Generate().WithInstitutionHierarchy(institutionHierarchy).WithProvider();
        var parent = ParentGenerator.Generate();
        var child = ChildGenerator.Generate().WithParent(parent);

        return ApplicationGenerator.Generate(6).WithWorkshop(workshop).WithParent(parent).WithChild(child);
    }

    private void SetupRepositoryWithVerification(ApplicationFilter appFilter, List<Application> correctApplications, List<Application> wrongApplications = null)
    {
        applicationRepositoryMock.Setup(
            w => w.Get(
                appFilter.From,
                appFilter.Size,
                It.IsAny<string>(),
                It.Is<Expression<Func<Application, bool>>>(
                    // here main magic goes - verification that predicate built in service and passed to applicationRepository should behave correctly on "correct" and "wrong" applications
                    expr => correctApplications.All(expr.Compile())
                        && (wrongApplications == null || !wrongApplications.Any(expr.Compile()))
                ),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(correctApplications.AsTestAsyncEnumerableQuery())
            .Verifiable("Wrong expression was built in service and passed to applicationRepository.Get() whereExpression argument. Probably some conditions aren't tested");

        mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>()))
            .Returns(correctApplications.Select(app => new ApplicationDto()).ToList());
    }
}