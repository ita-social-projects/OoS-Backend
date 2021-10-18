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
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ApplicationServiceTests
    {
        private IApplicationService service;
        private IApplicationRepository applicationRepository;
        private IWorkshopRepository workshopRepository;
        private IEntityRepository<Child> childRepository;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger<ApplicationService>> logger;
        private OutOfSchoolDbContext context;

        private List<ApplicationDto> applications;
        private Mock<IMapper> mapper;

        [SetUp]
        public void SetUp()
        {
            context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());

            applicationRepository = new ApplicationRepository(context);
            workshopRepository = new WorkshopRepository(context);
            childRepository = new EntityRepository<Child>(context);

            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger<ApplicationService>>();
            mapper = new Mock<IMapper>();
            service = new ApplicationService(
                applicationRepository,
                logger.Object,
                localizer.Object,
                workshopRepository,
                childRepository, mapper.Object);

            applications = ApplicationDTOsGenerator.Generate(5);
        }

        [Test]
        public async Task GetApplications_WhenCalled_ShouldReturnApplications()
        {
            // Arrange
            var expected = await applicationRepository.GetAll();

            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert
            Assert.AreEqual(result.ToList().Count(), expected.Count());
        }

        [Test]
        public async Task GetApplicationById_WhenIdIsValid_ShouldReturnApplication()
        {
            // Arrange
            var expected = applications.First();

            // Act
            var result = await service.GetById(expected.Id).ConfigureAwait(false);

            // Assert
            AssertApplicationsDTOsAreEqual(expected, result);
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
            var toCreate = ApplicationDTOsGenerator.Generate();

            // Act
            var result = await service.Create(toCreate).ConfigureAwait(false);

            // Assert
            AssertApplicationsDTOsAreEqual(toCreate, result);
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
            var toCreate = ApplicationDTOsGenerator.Generate();

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(toCreate).ConfigureAwait(false));
        }

        [Test]
        public void CreateApplication_WhenParametersAreNotValid_ShouldThrowArgumentException()
        {
            // Arrange
            var toCreate = ApplicationDTOsGenerator.Generate();
            //var toCreate = new ApplicationDto()
            //{
            //    Id = 7,
            //    ChildId = 2,
            //    Status = ApplicationStatus.Pending,
            //    WorkshopId = 1,
            //    ParentId = 1,
            //    CreationTime = new DateTime(2021, 7, 9),
            //};

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(toCreate).ConfigureAwait(false));
        }

        [Test]
        public async Task CreateMultipleApplications_WhenModelIsValid_ShouldReturnApplication()
        {
            // Act
            var result = await service.Create(applications).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(applications);
        }

        [Test]
        public void CreateMultipleApplications_WhenCollectionIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            IEnumerable<ApplicationDto> applications = new List<ApplicationDto>();

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(applications).ConfigureAwait(false));
        }

        [Test]
        public void CreateMultipleApplications_WhenModelIsNull_ShouldThrowArgumentException()
        {
            // Arrange
            ApplicationDto application = null;
            IEnumerable<ApplicationDto> applications = new List<ApplicationDto>()
            {
                application,
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(applications).ConfigureAwait(false));
        }

        //[Test]
        //[TestCase(1)]
        //public async Task GetAllByWokshop_WhenIdIsValid_ShouldReturnApplications(long id)
        //{
        //    // Arrange
        //    var applicationFilter = new ApplicationFilter
        //    {
        //        Status = (ApplicationStatus)1,
        //        OrderByAlphabetically = false,
        //        OrderByDateAscending = false,
        //        OrderByStatus = false,
        //    };

        //    Expression<Func<Application, bool>> filter = a => a.WorkshopId == id;
        //    var expected = await applicationRepository.GetByFilter(filter);
        //    expected = expected.Where(a => a.Status == applicationFilter.Status);

        //    // Act
        //    var result = await service.GetAllByWorkshop(id, applicationFilter).ConfigureAwait(false);

        //    // Assert
        //    result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        //}

        //[Test]
        //[TestCase(10)]
        //public async Task GetAllByWorkshop_WhenIdIsNotValid_ShouldReturnEmptyCollection(long id)
        //{
        //    // Act
        //    var applicationFilter = new ApplicationFilter { Status = (ApplicationStatus)1 };
        //    var result = await service.GetAllByWorkshop(id, applicationFilter).ConfigureAwait(false);

        //    // Assert
        //    result.Count().Should().Be(0);
        //}

        //[Test]
        //[TestCase(1)]
        //public void GetAllByWorkshop_WhenFilterIsNull_ShouldThrowArgumentException(long id)
        //{
        //    // Arrange
        //    ApplicationFilter filter = null;

        //    // Act and Assert
        //    service.Invoking(s => s.GetAllByWorkshop(id, filter)).Should().ThrowAsync<ArgumentException>();
        //}

        //[Test]
        //[TestCase(1)]
        //public async Task GetAllByProvider_WhenIdIsValid_ShouldReturnApplications(long id)
        //{
        //    // Arrange
        //    var applicationFilter = new ApplicationFilter
        //    {
        //        Status = 0,
        //        OrderByAlphabetically = false,
        //        OrderByStatus = false,
        //        OrderByDateAscending = false,
        //    };

        //    Expression<Func<Application, bool>> filter = a => a.Workshop.ProviderId == id;
        //    var expected = await applicationRepository.GetByFilter(filter);
        //    expected = expected.Where(a => a.Status == applicationFilter.Status);

        //    // Act
        //    var result = await service.GetAllByProvider(id, applicationFilter).ConfigureAwait(false);

        //    // Assert
        //    result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        //}

        //[Test]
        //[TestCase(10)]
        //public async Task GetAllByProvider_WhenIdIsNotValid_ShouldReturnEmptyCollection(long id)
        //{
        //    // Act
        //    var applicationFilter = new ApplicationFilter { Status = (ApplicationStatus)1 };

        //    var result = await service.GetAllByProvider(id, applicationFilter).ConfigureAwait(false);

        //    // Assert
        //    result.Count().Should().Be(0);
        //}

        //[Test]
        //[TestCase(1)]
        //public void GetAllByProvider_WhenFilterIsNull_ShouldThrowArgumentException(long id)
        //{
        //    // Arrange
        //    ApplicationFilter filter = null;

        //    // Act and Assert
        //    service.Invoking(s => s.GetAllByProvider(id, filter)).Should().ThrowAsync<ArgumentException>();
        //}

        //[Test]
        //[TestCase(1)]
        //public async Task GetAllByParent_WhenIdIsValid_ShouldReturnApplications(long id)
        //{
        //    // Arrange
        //    Expression<Func<Application, bool>> filter = a => a.ParentId == id;
        //    var expected = await applicationRepository.GetByFilter(filter);

        //    // Act
        //    var result = await service.GetAllByParent(id).ConfigureAwait(false);

        //    // Assert
        //    result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        //}

        //[Test]
        //[TestCase(10)]
        //public async Task GetAllByParent_WhenIdIsNotValid_ShouldReturnEmptyCollection(long id)
        //{
        //    // Act
        //    var result = await service.GetAllByParent(id).ConfigureAwait(false);

        //    // Assert
        //    result.Count().Should().Be(0);
        //}

        [Test]
        public async Task GetAllByChild_WhenIdIsValid_ShouldReturnApplications()
        {
            // Arrange
            var existingApplication = applications.RandomItem();
            Expression<Func<Application, bool>> filter = a => a.ChildId == existingApplication.ChildId;
            var expected = await applicationRepository.GetByFilter(filter);

            // Act
            var result = await service.GetAllByChild(existingApplication.ChildId).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        }

        [Test]
        public async Task GetAllByChild_WhenIdIsNotValid_ShouldReturnEmptyCollection()
        {
            // Act
            var result = await service.GetAllByChild(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            result.Count().Should().Be(0);
        }

        [Test]
        [TestCase(0)]
        public async Task GetAllByStatus_WhenStatusIsValid_ShouldReturnApplications(int status)
        {
            // Arrange
            Expression<Func<Application, bool>> filter = a => (int)a.Status == status;
            var expected = await applicationRepository.GetByFilter(filter);

            // Act
            var result = await service.GetAllByStatus(status).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        }

        [Test]
        [TestCase(2)]
        public async Task GetAllByStatus_WhenStatusIsNotValid_ShouldReturnEmptyCollection(int status)
        {
            // Act
            var result = await service.GetAllByStatus(status).ConfigureAwait(false);

            // Assert
            result.Count().Should().Be(0);
        }

        [Test]
        public async Task UpdateApplication_WhenIdIsValid_ShouldReturnApplication()
        {
            // Arrange
            var expected = ApplicationDTOsGenerator.Generate();
            expected.Id = applications.First().Id;

            // Act
            var result = await service.Update(expected).ConfigureAwait(false);

            // Assert
            AssertApplicationsDTOsAreEqual(expected, result);
        }

        [Test]
        public void UpdateApplication_WhenThereIsNoApplicationWithId_ShouldTrowArgumentException()
        {
            // Arrange
            var application = ApplicationDTOsGenerator.Generate();

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Update(application).ConfigureAwait(false));
        }

        [Test]
        public void UpdateApplication_WhenModelIsNull_ShouldThrowArgumentException()
        {
            // Act and Assert
            service.Invoking(s => s.Update(null)).Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task DeleteApplication_WhenIdIsValid_ShouldDeleteApplication()
        {
            // Arrange
            var applicationToDelete = applications.RandomItem();

            // Act
            await service.Delete(applicationToDelete.Id).ConfigureAwait(false);

            // Assert
            Assert.That(applications.Find(app => app.Id.Equals(applicationToDelete.Id)), Is.Null);
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
    }
}
