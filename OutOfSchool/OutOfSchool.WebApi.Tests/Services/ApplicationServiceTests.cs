﻿using System;
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
        private IApplicationRepository repository;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger> logger;
        private OutOfSchoolDbContext context;

        private IEnumerable<ApplicationDto> applications;

        [SetUp]
        public void SetUp()
        {
            context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
            repository = new ApplicationRepository(context);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            logger = new Mock<ILogger>();
            service = new ApplicationService(repository, logger.Object, localizer.Object);

            applications = FakeApplications();
        }

        [Test]
        public async Task GetApplications_WhenCalled_ShouldReturnApplications()
        {
            // Arrange
            var expected = await repository.GetAll();

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
            var expected = await repository.GetById(id);

            // Act
            var result = await service.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Id, result.Id);
        }

        [Test]
        [TestCase(10)]
        public void GetApplicationById_WhenIdIsNotValid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await service.GetById(id).ConfigureAwait(false));
        }

        [Test]
        public async Task CreateApplication_WhenCalled_ShouldReturnApplication()
        {
            // Arrange
            var expected = new ApplicationDto()
            {
                Id = 2,
                ChildId = 2,
                Status = ApplicationStatus.Pending,
                WorkshopId = 2,
                UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEFA6",
            };

            // Act
            var result = await service.Create(expected).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected);
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
        public void CreateApplication_WhenModelAlreadyExists_ShouldThrowArgumentException()
        {
            // Arrange
            var expected = new ApplicationDto()
            {
                Id = 4,
                ChildId = 1,
                Status = ApplicationStatus.Pending,
                WorkshopId = 1,
                UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6",
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Create(expected).ConfigureAwait(false));
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
        public void CreateMultipleApplications_WhenModelAlreadyExists_ShouldThrowArgumentException()
        {
            // Arrange
            IEnumerable<ApplicationDto> applications = new List<ApplicationDto>()
            {
                new ApplicationDto
                {
                    Id = 4,
                    ChildId = 1,
                    Status = ApplicationStatus.Pending,
                    WorkshopId = 1,
                    UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6",
                },
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
            Expression<Func<Application, bool>> filter = a => a.WorkshopId == id;
            var expected = await repository.GetByFilter(filter);

            // Act
            var result = await service.GetAllByWorkshop(id).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        }

        [Test]
        [TestCase(10)]
        public void GetAllByWorkshop_WhenIdIsNotValid_ShouldThrowArgumentException(long id)
        {
            // Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.GetAllByWorkshop(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase("de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6")]
        public async Task GetAllByUser_WhenIdIsValid_ShouldReturnApplications(string id)
        {
            // Arrange
            Expression<Func<Application, bool>> filter = a => a.UserId.Equals(id);
            var expected = await repository.GetByFilter(filter);

            // Act
            var result = await service.GetAllByUser(id).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected.Select(a => a.ToModel()));
        }

        [Test]
        [TestCase("string")]
        public void GetAllByUser_WhenIdIsNotValid_ShouldThrowArgumentException(string id)
        {
            // Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.GetAllByUser(id).ConfigureAwait(false));
        }

        [Test]
        public async Task UpdateApplication_WhenIdIsValid_ShouldReturnApplication()
        {
            // Arrange
            var expected = new ApplicationDto()
            {
                Id = 1,
                Status = ApplicationStatus.Approved,
            };

            // Act
            var result = await service.Update(expected).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void UpdateApplication_WhenIdIsNotValid_ShouldThrowDbUpdateConcurrencyException()
        {
            // Arrange
            var changedApplication = new Application()
            {
                Status = ApplicationStatus.Approved,
            };

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await service.Update(changedApplication.ToModel()).ConfigureAwait(false));
        }

        [Test]
        public void UpdateApplication_WhenModelIsNull_ShouldThrowArgumentException()
        {
            // Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.Update(null).ConfigureAwait(false));
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
        public void DeleteApplication_WhenIdIsNotValid_ShouldThrowDbUpdateConcurrencyException(long id)
        {
            // Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await service.Delete(id).ConfigureAwait(false));
        }

        private IEnumerable<ApplicationDto> FakeApplications()
        {
            return new List<ApplicationDto>()
            {
                new ApplicationDto()
                {
                    Id = 5,
                    ChildId = 3,
                    Status = ApplicationStatus.Pending,
                    UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6",
                    WorkshopId = 2,
                },
                new ApplicationDto()
                {
                    Id = 6,
                    ChildId = 3,
                    Status = ApplicationStatus.Pending,
                    UserId = "de909VV5-5eb7-4b7a-bda8-40a5bfda96a6",
                    WorkshopId = 2,
                },
            };
        }
    }
}
