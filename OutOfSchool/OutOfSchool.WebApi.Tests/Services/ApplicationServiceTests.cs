using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Serilog;

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
        private Mock<ILogger> logger;
        private OutOfSchoolDbContext context;

        private IEnumerable<ApplicationDto> applications;

        [SetUp]
        public void SetUp()
        {
            context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());

            applicationRepository = new ApplicationRepository(context);
            workshopRepository = new WorkshopRepository(context);
            childRepository = new EntityRepository<Child>(context);

            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger>();
            service = new ApplicationService(
                applicationRepository,
                logger.Object,
                localizer.Object,
                workshopRepository,
                childRepository);

            applications = FakeApplications();
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
        [TestCase(1)]
        public async Task GetApplicationById_WhenIdIsValid_ShouldReturnApplication(long id)
        {
            // Arrange
            var expected = await applicationRepository.GetById(id);

            // Act
            var result = await service.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Id, result.Id);
        }

        [Test]
        [TestCase(10)]
        public async Task GetApplicationById_WhenIdIsNotValid_ShouldReturnNull(long id)
        {
            // Act
            var result = await service.GetById(id).ConfigureAwait(false);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task CreateApplication_WhenCalled_ShouldReturnApplication()
        {
            // Arrange
            var toCreate = new ApplicationDto()
            {
                Id = 2,
                ChildId = 2,
                Status = ApplicationStatus.Pending,
                WorkshopId = 2,
                ParentId = 2,
                CreationTime = new DateTime(2021, 7, 9),
            };

            // Act
            var result = await service.Create(toCreate).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(toCreate, options =>
            options.Excluding(t => t.Workshop.Teachers));
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
            var toCreate = new ApplicationDto()
            {
                Id = 4,
                ChildId = 1,
                Status = ApplicationStatus.Pending,
                WorkshopId = 1,
                ParentId = 1,
                CreationTime = new DateTime(2021, 7, 9),
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(toCreate).ConfigureAwait(false));
        }

        [Test]
        public void CreateApplication_WhenParametersAreNotValid_ShouldThrowArgumentException()
        {
            // Arrange
            var toCreate = new ApplicationDto()
            {
                Id = 7,
                ChildId = 2,
                Status = ApplicationStatus.Pending,
                WorkshopId = 1,
                ParentId = 1,
                CreationTime = new DateTime(2021, 7, 9),
            };

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

        [Test]
        [TestCase(1)]
        public async Task GetAllByWokshop_WhenIdIsValid_ShouldReturnApplications(long id)
        {
            // Arrange
            var applicationFilter = new ApplicationFilter
            {
                Status = 1,
                OrderByAlphabetically = false,
                OrderByDateAscending = false,
                OrderByStatus = false,
            };

            Expression<Func<Application, bool>> filter = a => a.WorkshopId == id;
            var expected = await applicationRepository.GetByFilter(filter);
            expected = expected.Where(a => (int)a.Status == applicationFilter.Status);

            // Act
            var result = await service.GetAllByWorkshop(id, applicationFilter).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        }

        [Test]
        [TestCase(10)]
        public async Task GetAllByWorkshop_WhenIdIsNotValid_ShouldReturnEmptyCollection(long id)
        {
            // Act
            var applicationFilter = new ApplicationFilter { Status = 1 };
            var result = await service.GetAllByWorkshop(id, applicationFilter).ConfigureAwait(false);

            // Assert
            result.Count().Should().Be(0);
        }

        [Test]
        [TestCase(1)]
        public async Task GetAllByProvider_WhenIdIsValid_ShouldReturnApplications(long id)
        {
            // Arrange
            var applicationFilter = new ApplicationFilter
            {
                Status = 0,
                OrderByAlphabetically = false,
                OrderByStatus = false,
                OrderByDateAscending = false,
            };

            Expression<Func<Application, bool>> filter = a => a.Workshop.ProviderId == id;
            var expected = await applicationRepository.GetByFilter(filter);
            expected = expected.Where(a => (int)a.Status == applicationFilter.Status);

            // Act
            var result = await service.GetAllByProvider(id, applicationFilter).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        }

        [Test]
        [TestCase(10)]
        public async Task GetAllByProvider_WhenIdIsNotValid_ShouldReturnEmptyCollection(long id)
        {
            // Act
            var applicationFilter = new ApplicationFilter { Status = 1 };

            var result = await service.GetAllByProvider(id, applicationFilter).ConfigureAwait(false);

            // Assert
            result.Count().Should().Be(0);
        }

        [Test]
        [TestCase(1)]
        public async Task GetAllByParent_WhenIdIsValid_ShouldReturnApplications(long id)
        {
            // Arrange
            Expression<Func<Application, bool>> filter = a => a.ParentId == id;
            var expected = await applicationRepository.GetByFilter(filter);

            // Act
            var result = await service.GetAllByParent(id).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        }

        [Test]
        [TestCase(10)]
        public async Task GetAllByParent_WhenIdIsNotValid_ShouldReturnEmptyCollection(long id)
        {
            // Act
            var result = await service.GetAllByParent(id).ConfigureAwait(false);

            // Assert
            result.Count().Should().Be(0);
        }

        [Test]
        [TestCase(1)]
        public async Task GetAllByChild_WhenIdIsValid_ShouldReturnApplications(long id)
        {
            // Arrange
            Expression<Func<Application, bool>> filter = a => a.ChildId == id;
            var expected = await applicationRepository.GetByFilter(filter);

            // Act
            var result = await service.GetAllByChild(id).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        }

        [Test]
        [TestCase(10)]
        public async Task GetAllByChild_WhenIdIsNotValid_ShouldReturnEmptyCollection(long id)
        {
            // Act
            var result = await service.GetAllByChild(id).ConfigureAwait(false);

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
            var expected = new ApplicationDto()
            {
                Id = 1,
                Status = ApplicationStatus.Approved,
                ChildId = 1,
                WorkshopId = 1,
                ParentId = 1,
            };

            // Act
            var result = await service.Update(expected).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected, options =>
            options.Excluding(e => e.Parent)
                   .Excluding(e => e.Workshop)
                   .Excluding(e => e.Child));
        }

        [Test]
        public void UpdateApplication_WhenThereIsNoApplicationWithId_ShouldTrowArgumentException()
        {
            // Arrange
            var application = new ApplicationDto
            {
                Id = 10,
                Status = ApplicationStatus.Approved,
            };

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
        [TestCase(1)]
        public async Task DeleteApplication_WhenIdIsValid_ShouldDeleteApplication(long id)
        {
            // Arrange
            var countBeforeDelete = await context.Applications.CountAsync();

            // Act
            await service.Delete(id).ConfigureAwait(false);

            // Assert
            context.Applications.CountAsync().Result.Should().Be(countBeforeDelete - 1);
            context.Applications.FindAsync(id).Result.Should().BeNull();
        }

        [Test]
        [TestCase(10)]
        public void DeleteApplication_WhenIdIsNotValid_ShouldThrowArgumentException(long id)
        {
            // Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Delete(id).ConfigureAwait(false));
        }

        private IEnumerable<ApplicationDto> FakeApplications()
        {
            return new List<ApplicationDto>()
            {
                new ApplicationDto()
                {
                    Id = 5,
                    ChildId = 2,
                    Status = ApplicationStatus.Pending,
                    ParentId = 2,
                    WorkshopId = 1,
                },
                new ApplicationDto()
                {
                    Id = 6,
                    ChildId = 3,
                    Status = ApplicationStatus.Pending,
                    ParentId = 1,
                    WorkshopId = 2,
                },
            };
        }
    }
}
