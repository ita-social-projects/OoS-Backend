using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class StatusServiceTests
    {
        private IStatusService service;
        private OutOfSchoolDbContext context;
        private IEntityRepository<InstitutionStatus> repository;
        private DbContextOptions<OutOfSchoolDbContext> options;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);
            var localizer = new Mock<IStringLocalizer<SharedResource>>();
            repository = new EntityRepository<InstitutionStatus>(context);
            var logger = new Mock<ILogger<StatusService>>();
            service = new StatusService(repository, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        public async Task GetAll_WhenCalled_ReturnsAllInstitutionStatuses()
        {
            // Arrange
            var expected = (await repository.GetAll()).Select(s => s.ToModel());

            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert
            TestHelper.AssertTwoCollectionsEqualByValues(expected, result);
        }


        [Test]
        [TestCase(2)]
        public async Task GetById_WhenIdIsValid_ReturnsInstitutionStatus(long id)
        {
            // Arrange
            var expected = (await repository.GetById(id)).ToModel();

            // Act
            var result = await service.GetById(id).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [Test]
        [TestCase(13)]
        public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await service.GetById(id).ConfigureAwait(false));
        }

        [Test]
        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
        {
            // Arrange
            var entityToCreate = new InstitutionStatus() { Name = "TestName" };
            var expected = entityToCreate.ToModel();
            expected.Id = await repository.Count() + 1;

            // Act
            var result = await service.Create(entityToCreate.ToModel()).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [Test]
        public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
        {
            // Arrange
            var entityToUpdate = new InstitutionStatus()
            {
                Id = 1,
                Name = "TestName",
            };

            var expected = entityToUpdate.ToModel();

            // Act
            var result = await service.Update(entityToUpdate.ToModel()).ConfigureAwait(false);

            // Assert
            TestHelper.AssertDtosAreEqual(expected, result);
        }

        [Test]
        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new InstitutionStatusDTO()
            {
                Name = "TestName",
            };

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await service.Update(changedEntity).ConfigureAwait(false));
        }

        [Test]
        [TestCase(1)]
        public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
        {
            // Arrange
            var expected = await context.InstitutionStatuses.CountAsync();

            // Act
            await service.Delete(id).ConfigureAwait(false);

            // Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await service.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(13)]
        public void Delete_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await service.Delete(id).ConfigureAwait(false));
        }

        /// <summary>
        /// method to seed repository with entities to test.
        /// </summary>

        private void SeedDatabase()
        {
            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var institutionStatuses = SeedFakeStatuses();
                context.InstitutionStatuses.AddRangeAsync(institutionStatuses);

                context.SaveChangesAsync();
            }
        }

        private IEnumerable<InstitutionStatus> SeedFakeStatuses()
        {
            return new List<InstitutionStatus>()
            {
                new InstitutionStatus()
                {
                    Id = 1,
                    Name = "NoName",
                },
                new InstitutionStatus()
                {
                    Id = 2,
                    Name = "HaveName",
                },
                new InstitutionStatus()
                {
                    Id = 3,
                    Name = "MissName",
                },
            };
        }
    }
}
