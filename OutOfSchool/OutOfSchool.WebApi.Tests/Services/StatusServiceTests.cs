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
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class StatusServiceTests
    {
        private IStatusService service;
        private OutOfSchoolDbContext context;
        private IEntityRepository<InstitutionStatus> repository;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger<StatusService>> logger;
        private DbContextOptions<OutOfSchoolDbContext> options;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            repository = new EntityRepository<InstitutionStatus>(context);
            logger = new Mock<ILogger<StatusService>>();
            service = new StatusService(repository, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        public async Task GetAll_WhenCalled_ReturnsAllInstitutionStatuses()
        {
            // Arrange
            var expected = await repository.GetAll();

            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert
            Assert.AreEqual(result.ToList().Count(), expected.Count());
        }



        private void SeedDatabase()
        {
            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var institutionStatuses = new List<InstitutionStatus>()
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

                context.InstitutionStatuses.AddRangeAsync(institutionStatuses);
                context.SaveChangesAsync();
            }
        }
    }
}
