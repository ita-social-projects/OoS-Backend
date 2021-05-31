using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class WorkshopServiceTests
    {
        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext context;
        private IWorkshopRepository repo;
        private IWorkshopService service;
        private Mock<IRatingService> ratingService;
        private Mock<ILogger> logger;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");
            options = builder.Options;
            context = new OutOfSchoolDbContext(options);

            repo = new WorkshopRepository(context);
            ratingService = new Mock<IRatingService>();
            logger = new Mock<ILogger>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            service = new WorkshopService(repo, ratingService.Object, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        public async Task Create_WhenEntityIsValid_ShouldReturnCreatedEntity()
        {
            // Arrange
            var expected = new Workshop()
            {
                Title = "NewTitle2",
                Description = "NewDescription2",
                MinAge = 4,
                MaxAge = 10,
                Price = 1000,
                Head = "NewHead2",
                HeadBirthDate = new DateTime(1980, 12, 10),
                SubsubcategoryId = 1,
            };

            // Act
            var result = await service.Create(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Title, result.Title);
            Assert.AreEqual(expected.Email, result.Email);
            Assert.AreEqual(expected.Phone, result.Phone);
            Assert.AreEqual(expected.Description, result.Description);
            Assert.AreEqual(expected.Head, result.Head);
        }

        [Test]
        public async Task GetAll_WhenCalled_ShouldReturnAllEntities()
        {
            // Arrange
            var expected = await repo.GetAll();

            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert
            Assert.That(expected.Count(), Is.EqualTo(result.Count()));
        }

        [Test]
        [TestCase(1)]
        public async Task GetById_WhenIdIsValid_ShouldReturnEntity(long id)
        {
            // Arrange
            var expected = await repo.GetById(id);

            // Act
            var result = await service.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Id, result.Id);
        }

        [Test]
        [TestCase(10)]
        public void GetById_WhenIdIsInvalid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Assert
            Assert.That(async () => await service.GetById(10), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public async Task Update_WhenEntityIsValid_ShouldUpdateExistedEntity()
        {
            // Arrange
            var changedEntity = new WorkshopDTO()
            {
                Id = 1,
                Title = "ChangedTitle2",
            };

            // Act
            var result = await service.Update(changedEntity).ConfigureAwait(false);

            // Assert
            Assert.That(changedEntity.Title, Is.EqualTo(result.Title));
        }

        [Test]
        public void Update_WhenEntityIsInvalid_ShouldThrowDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new WorkshopDTO()
            {
                Title = "Title1",
            };

            // Assert
            Assert.That(
                async () => await service.Update(changedEntity).ConfigureAwait(false),
                Throws.Exception.TypeOf<DbUpdateConcurrencyException>());
        }

        [Test]
        [TestCase(1)]
        public async Task Delete_WhenIdIsValid_ShouldDeleteEntity(long id)
        {
            // Act
            var countBeforeDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

            context.Entry<Workshop>(await repo.GetById(id).ConfigureAwait(false)).State = EntityState.Detached;

            await service.Delete(id).ConfigureAwait(false);

            var countAfterDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

            // Assert
            Assert.That(countAfterDeleting, Is.Not.EqualTo(countBeforeDeleting));
        }

        [Test]
        [TestCase(10)]
        public void Delete_WhenIdIsInvalid_ShouldThrowDbUpdateConcurrencyException(long id)
        {
            // Assert
            Assert.That(
                async () => await service.Delete(id).ConfigureAwait(false),
                Throws.Exception.TypeOf<DbUpdateConcurrencyException>());
        }

        private void SeedDatabase()
        {
            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var workshops = new List<Workshop>()
                {
                    new Workshop()
                    {
                        Id = 1,
                        Title = "Title1",
                        Phone = "1111111111",
                        Description = "Desc1",
                        Price = 1000,
                        WithDisabilityOptions = true,
                        DaysPerWeek = 1,
                        Head = "Head1",
                        HeadBirthDate = new DateTime(1980, month: 12, 28),
                        DisabilityOptionsDesc = "Desc1",
                        Website = "website1",
                        Instagram = "insta1",
                        Facebook = "facebook1",
                        Email = "email1@gmail.com",
                        MaxAge = 10,
                        MinAge = 4,
                    },
                    new Workshop()
                    {
                        Id = 2,
                        Title = "Title2",
                        Phone = "1111111111",
                        Description = "Desc2",
                        Price = 2000,
                        WithDisabilityOptions = true,
                        DaysPerWeek = 2,
                        Head = "Head2",
                        HeadBirthDate = new DateTime(1980, month: 12, 28),
                        DisabilityOptionsDesc = "Desc2",
                        Website = "website2",
                        Instagram = "insta2",
                        Facebook = "facebook2",
                        Email = "email2@gmail.com",
                        MaxAge = 10,
                        MinAge = 4,
                    },
                    new Workshop()
                    {
                        Id = 3,
                        Title = "Title3",
                        Phone = "1111111111",
                        Description = "Desc3",
                        Price = 3000,
                        WithDisabilityOptions = true,
                        DaysPerWeek = 3,
                        Head = "Head3",
                        HeadBirthDate = new DateTime(1980, month: 12, 28),
                        DisabilityOptionsDesc = "Desc3",
                        Website = "website3",
                        Instagram = "insta3",
                        Facebook = "facebook3",
                        Email = "email3@gmail.com",
                        MaxAge = 10,
                        MinAge = 4,
                    },
                    new Workshop()
                    {
                        Id = 4,
                        Title = "Title4",
                        Phone = "1111111111",
                        Description = "Desc4",
                        Price = 4000,
                        WithDisabilityOptions = true,
                        DaysPerWeek = 4,
                        Head = "Head4",
                        HeadBirthDate = new DateTime(1980, month: 12, 28),
                        DisabilityOptionsDesc = "Desc4",
                        Website = "website4",
                        Instagram = "insta4",
                        Facebook = "facebook4",
                        Email = "email4@gmail.com",
                        MaxAge = 10,
                        MinAge = 4,
                    },
                    new Workshop()
                    {
                        Id = 5,
                        Title = "Title5",
                        Phone = "1111111111",
                        Description = "Desc5",
                        Price = 5000,
                        WithDisabilityOptions = true,
                        DaysPerWeek = 5,
                        Head = "Head5",
                        HeadBirthDate = new DateTime(1980, month: 12, 12),
                        DisabilityOptionsDesc = "Desc5",
                        Website = "website5",
                        Instagram = "insta5",
                        Facebook = "facebook5",
                        Email = "email5@gmail.com",
                        MaxAge = 10,
                        MinAge = 4,
                    },
                };

                var categories = new List<Category>() { new Category() { Title = "Category1" }, new Category() { Title = "Category2" } };
                var subcategories = new List<Subcategory>() { new Subcategory() { Title = "new1", CategoryId = 1 }, new Subcategory() { Title = "new2", CategoryId = 1 } };
                var subsubcategories = new List<Subsubcategory>() { new Subsubcategory() { Title = "new1", SubcategoryId = 1 }, new Subsubcategory() { Title = "new2", SubcategoryId = 1 } };

                context.Categories.AddRangeAsync(categories);
                context.Subcategories.AddRangeAsync(subcategories);
                context.Subsubcategories.AddRangeAsync(subsubcategories);
                context.Workshops.AddRangeAsync(workshops);
                context.SaveChangesAsync();
            }
        }
    }
}