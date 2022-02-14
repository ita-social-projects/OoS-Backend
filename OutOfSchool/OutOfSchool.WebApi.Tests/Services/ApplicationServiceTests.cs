using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ApplicationServiceTests
    {
        private IApplicationService service;
        private Mock<IApplicationRepository> applicationRepositoryMock;
        private Mock<IWorkshopRepository> workshopRepositoryMock;
        private Mock<IEntityRepository<Child>> childRepositoryMock;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger<ApplicationService>> logger;
        private Mock<IMapper> mapper;

        private Mock<IOptions<ApplicationsConstraintsConfig>> applicationsConstraintsConfig;

        [SetUp]
        public void SetUp()
        {
            applicationRepositoryMock = new Mock<IApplicationRepository>();
            workshopRepositoryMock = new Mock<IWorkshopRepository>();
            childRepositoryMock = new Mock<IEntityRepository<Child>>();

            localizer = new Mock<IStringLocalizer<SharedResource>>();
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
                localizer.Object,
                workshopRepositoryMock.Object,
                childRepositoryMock.Object,
                mapper.Object,
                applicationsConstraintsConfig.Object);
        }

        [Test]
        public async Task GetApplications_WhenCalled_ShouldReturnApplications()
        {
            // Arrange
            var application = WithApplicationsList();
            SetupGetAll(application);

            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert
            Assert.AreEqual(result.ToList().Count(), application.Count());
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
            var newApplication = new Application()
            {
                Id = new Guid("6d4caeae-f0c3-492e-99b0-c8c105693376"),
                WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
                CreationTime = new DateTimeOffset(2022, 01, 12, 12, 34, 15, TimeSpan.Zero),
                Status = ApplicationStatus.Pending,
                ChildId = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"),
                ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
            };
            SetupCreate(newApplication);

            // Act
            var result = await service.Create(newApplication.ToModel()).ConfigureAwait(false);

            // Assert
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
            ApplicationDto application = null;

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(application).ConfigureAwait(false));
        }

        [Test]
        public void CreateApplication_WhenLimitIsExceeded_ShouldThrowArgumentException()
        {
            // Arrange
            var id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd");
            var application = WithApplication(id);

            // Act and Assert
            service.Invoking(w => w.Create(application.ToModel())).Should().Throw<ArgumentException>();
        }

        [Test]
        public void CreateApplication_WhenParametersAreNotValid_ShouldThrowArgumentException()
        {
            // Arrange
            var id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd");
            var application = WithApplication(id);

            // Act and Assert
            service.Invoking(w => w.Create(application.ToModel())).Should().Throw<ArgumentException>();
        }       

        [Test]
        public async Task GetAllByWorkshop_WhenIdIsValid_ShouldReturnApplications()
        {
            // Arrange
            var existingApplications = WithApplicationsList();
            SetupGetAllByWorkshop(existingApplications);
            var applicationFilter = new ApplicationFilter
            {
                Status = 0,
                OrderByAlphabetically = false,
                OrderByStatus = false,
                OrderByDateAscending = false,
            };

            // Act
            var result = await service.GetAllByWorkshop(existingApplications.First().Id, applicationFilter).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(ExpectedApplicationsGetAll(existingApplications));
        }

        [Test]
        public async Task GetAllByWorkshop_WhenIdIsNotValid_ShouldReturnEmptyCollection()
        {
            // Arrange
            SetupGetAllByWorkshopEmpty();
            var filter = new ApplicationFilter
            {
                Status = 0,
                OrderByAlphabetically = false,
                OrderByStatus = false,
                OrderByDateAscending = false,
            };

            // Act
            var result = await service.GetAllByWorkshop(Guid.NewGuid(), filter).ConfigureAwait(false);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void GetAllByWorkshop_WhenFilterIsNull_ShouldThrowArgumentException()
        {
            // Arrange
            ApplicationFilter filter = null;

            // Act and Assert
            service.Invoking(s => s.GetAllByWorkshop(Guid.NewGuid(), filter)).Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task GetAllByProvider_WhenIdIsValid_ShouldReturnApplications()
        {
            // Arrange
            var existingApplications = WithApplicationsList();
            SetupGetAllByProvider(existingApplications);
            var applicationFilter = new ApplicationFilter
            {
                Status = 0,
                OrderByAlphabetically = false,
                OrderByStatus = false,
                OrderByDateAscending = false,
            };

            // Act
            var result = await service.GetAllByProvider(existingApplications.First().Id, applicationFilter).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(ExpectedApplicationsGetAll(existingApplications));
        }

        [Test]
        public async Task GetAllByProvider_WhenIdIsNotValid_ShouldReturnEmptyCollection()
        {
            // Arrange
            SetupGetAllByProviderEmpty();
            var filter = new ApplicationFilter
            {
                Status = 0,
                OrderByAlphabetically = false,
                OrderByStatus = false,
                OrderByDateAscending = false,
            };

            // Act
            var result = await service.GetAllByProvider(Guid.NewGuid(), filter).ConfigureAwait(false);

            // Assert
            result.Should().BeEmpty();
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
        public async Task GetAllByParent_WhenIdIsValid_ShouldReturnApplications()
        {
            // Arrange
            var existingApplications = WithApplicationsList();
            SetupGetAllBy(existingApplications);

            // Act
            var result = await service.GetAllByParent(existingApplications.First().ParentId).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(ExpectedApplicationsGetAll(existingApplications));
        }

        [Test]
        public async Task GetAllByParent_WhenIdIsNotValid_ShouldReturnEmptyCollection()
        {
            // Act
            var result = await service.GetAllByParent(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.Null);
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
        public async Task GetAllByStatus_WhenStatusIsValid_ShouldReturnApplications()
        {
            // Arrange
            var existingApplications = WithApplicationsList();
            var status = (int)existingApplications.First().Status;
            SetupGetAllBy(existingApplications);

            // Act
            var result = await service.GetAllByStatus(status).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(ExpectedApplicationsGetAll(existingApplications));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(10)]
        public async Task GetAllByStatus_WhenStatusIsNotValid_ShouldReturnEmptyCollection(int status)
        {
            // Act
            var result = await service.GetAllByStatus(status).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateApplication_WhenIdIsValid_ShouldReturnApplication()
        {
            // Arrange
            var id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd");
            var changedEntity = WithApplication(id);
            SetupUpdate(changedEntity);
            var expected = changedEntity.ToModel();

            // Act
            var result = await service.Update(expected).ConfigureAwait(false);

            // Assert
            AssertApplicationsDTOsAreEqual(expected, result);
        }

        [Test]
        public void UpdateApplication_WhenThereIsNoApplicationWithId_ShouldTrowArgumentException()
        {
            // Arrange
            var id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd");
            var application = WithApplication(id);

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Update(application.ToModel()).ConfigureAwait(false));
        }

        [Test]
        public void UpdateApplication_WhenModelIsNull_ShouldThrowArgumentException()
        {
            // Act and Assert
            service.Invoking(s => s.Update(null)).Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task DeleteApplication_WhenIdIsValid_ShouldTryToDelete()
        {
            // Arrange
            var id = Guid.NewGuid();
            SetupDelete(WithApplication(id));

            // Act
            await service.Delete(id).ConfigureAwait(false);

            // Assert
            applicationRepositoryMock.Verify(w => w.Delete(It.IsAny<Application>()), Times.Once);
        }

        [Test]
        public void DeleteApplication_WhenIdIsNotValid_ShouldThrowArgumentException()
        {
            // Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Delete(Guid.NewGuid()).ConfigureAwait(false));
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
            var childsMock = WithChildList().AsQueryable().BuildMock();

            applicationRepositoryMock.Setup(a => a.GetByFilter(
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<Application>>(new List<Application> { application }));
            childRepositoryMock.Setup(r => r.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Child, bool>>>(),
                    It.IsAny<Expression<Func<Child, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(childsMock.Object)
                .Verifiable();
            applicationRepositoryMock.Setup(
                    w => w.Create(It.IsAny<Application>()))
                .Returns(Task.FromResult(It.IsAny<Application>()));
            mapper.Setup(m => m.Map<ApplicationDto>(It.IsAny<Application>()))
                .Returns(new ApplicationDto() { Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd") });
        }

        private void SetupGetAll(IEnumerable<Application> apps)
        {
            var mappedDtos = apps.Select(a => new ApplicationDto() { Id = a.Id }).ToList();
            applicationRepositoryMock.Setup(w => w.GetAllWithDetails(
                    It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<Application>>(new List<Application> { apps.First() }));
            mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(mappedDtos);
        }

        private void SetupGetAllBy(IEnumerable<Application> apps)
        {
            var mappedDtos = apps.Select(a => new ApplicationDto() { Id = a.Id }).ToList();

            applicationRepositoryMock.Setup(a => a.GetByFilter(
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<Application>>(new List<Application> { apps.First() }));
            mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(mappedDtos);
        }

        private void SetupGetAllByWorkshop(IEnumerable<Application> apps)
        {
            var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();
            var mappedDtos = apps.Select(a => new ApplicationDto() { Id = a.Id }).ToList();

            applicationRepositoryMock.Setup(r => r.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<Expression<Func<Application, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(applicationsMock.Object)
                .Verifiable();
            mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(mappedDtos);
        }

        private void SetupGetAllByWorkshopEmpty()
        {
            var emptyApplicationsList = new List<Application>().AsQueryable().BuildMock();
            var emptyApplicationDtosList = new List<ApplicationDto>();

            applicationRepositoryMock.Setup(r => r.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<Expression<Func<Application, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(emptyApplicationsList.Object)
                .Verifiable();
            mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(emptyApplicationDtosList);
        }

        private void SetupGetAllByProvider(IEnumerable<Application> apps)
        {
            var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();
            var workshopsMock = WithWorkshopsList().AsQueryable().BuildMock();
            var mappedDtos = apps.Select(a => new ApplicationDto() { Id = a.Id }).ToList();

            workshopRepositoryMock.Setup(w => w.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Workshop, bool>>>(),
                    It.IsAny<Expression<Func<Workshop, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(workshopsMock.Object)
                .Verifiable();
            applicationRepositoryMock.Setup(r => r.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<Expression<Func<Application, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(applicationsMock.Object)
                .Verifiable();
            mapper.Setup(m => m.Map<List<ApplicationDto>>(It.IsAny<List<Application>>())).Returns(mappedDtos);
        }

        private void SetupGetAllByProviderEmpty()
        {
            var emptyWorkshopsList = new List<Workshop>().AsQueryable().BuildMock();
            var emptyApplicationsList = new List<Application>().AsQueryable().BuildMock();
            var emptyApplicationDtosList = new List<ApplicationDto>();

            workshopRepositoryMock.Setup(w => w.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Workshop, bool>>>(),
                    It.IsAny<Expression<Func<Workshop, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(emptyWorkshopsList.Object)
                .Verifiable();
            applicationRepositoryMock.Setup(r => r.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<Expression<Func<Application, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(emptyApplicationsList.Object)
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
                .Returns(Task.FromResult<IEnumerable<Application>>(new List<Application> { application }));
            mapper.Setup(m => m.Map<ApplicationDto>(application)).Returns(new ApplicationDto() { Id = application.Id });
        }

        private void SetupUpdate(Application application)
        {
            var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();

            applicationRepositoryMock.Setup(r => r.Get<It.IsAnyType>(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Expression<Func<Application, It.IsAnyType>>>(),
                It.IsAny<bool>()))
                .Returns(applicationsMock.Object)
                .Verifiable();

            applicationRepositoryMock.Setup(a => a.Update(mapper.Object.Map<Application>(application))).ReturnsAsync(application);
            mapper.Setup(m => m.Map<ApplicationDto>(application)).Returns(new ApplicationDto() { Id = application.Id });
        }

        private void SetupDelete(Application application)
        {
            var applicationsMock = WithApplicationsList().AsQueryable().BuildMock();

            applicationRepositoryMock.Setup(r => r.Get<It.IsAnyType>(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<Application, bool>>>(),
                    It.IsAny<Expression<Func<Application, It.IsAnyType>>>(),
                    It.IsAny<bool>()))
                .Returns(applicationsMock.Object)
                .Verifiable();
            applicationRepositoryMock.Setup(a => a.Delete(It.IsAny<Application>())).Returns(Task.CompletedTask);
        }
        #endregion

        #region With

        private IEnumerable<Application> WithApplicationsList()
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
                },
                new Application()
                {
                    Id = new Guid("7c5f8f7c-d850-44d0-8d4e-fd2de99453be"),
                    Status = ApplicationStatus.Rejected,
                    WorkshopId = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                    ChildId = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"),
                    ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
                },
                new Application()
                {
                    Id = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
                    Status = ApplicationStatus.Pending,
                    WorkshopId = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                    ChildId = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"),
                    ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"),
                },
            };
        }

        private Application WithApplication(Guid id)
        {
            return new Application()
            {
                Id = id,
                WorkshopId = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
                Status = ApplicationStatus.Pending,
            };
        }

        private IEnumerable<Workshop> WithWorkshopsList()
        {
            return new List<Workshop>()
            {
                new Workshop()
                {
                    Id = new Guid("b94f1989-c4e7-4878-ac86-21c4a402fb43"),
                    ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                },
                new Workshop()
                {
                    Id = new Guid("8c14044b-e30d-4b14-a18b-5b3b859ad676"),
                    ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                },
                new Workshop()
                {
                    Id = new Guid("3e8845a8-1359-4676-b6d6-5a6b29c122ea"),
                    ProviderId = new Guid("1aa8e8e0-d35f-45cb-b66d-a01faa8fe174"),
                },
            };
        }

        private IEnumerable<Child> WithChildList()
        {
            return new List<Child>()
                {
                    new Child { Id = new Guid("64988abc-776a-4ff8-961c-ba73c7db1986"), FirstName = "fn1", LastName = "ln1", MiddleName = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"), SocialGroupId = 2 },
                    new Child { Id = new Guid("f29d0e07-e4f2-440b-b0fe-eaa11e31ddae"), FirstName = "fn2", LastName = "ln2", MiddleName = "mn2", DateOfBirth = new DateTime(2004, 11, 8), Gender = Gender.Female, ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"), SocialGroupId = 1 },
                    new Child { Id = new Guid("6ddd21d0-2f2e-48a0-beec-fefcb44cd3f0"), FirstName = "fn3", LastName = "ln3", MiddleName = "mn3", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = new Guid("cce7dcbf-991b-4c8e-ba30-4e3cc9e952f3"), SocialGroupId = 1 },
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
            return new ApplicationDto() { Id = id };
        }

        private IEnumerable<ApplicationDto> ExpectedApplicationsGetAll(IEnumerable<Application> apps)
        {
            return mapper.Object.Map<List<ApplicationDto>>(apps);
        }

        #endregion
    }
}
