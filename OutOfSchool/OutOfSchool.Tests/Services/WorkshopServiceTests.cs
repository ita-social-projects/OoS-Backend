using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ResultModel;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.Tests.Services
{
    [TestFixture]
    public class WorkshopServiceTests
    {
        // private OutOfSchoolDbContext context;
        private DbContextOptionsBuilder<OutOfSchoolDbContext> optionsBuilder;
        private OutOfSchoolDbContext context;
        private IEntityRepository<Workshop> repo;
        private WorkshopService service;

        [SetUp]
        public void Setup()
        {
            repo = new EntityRepository<Workshop>(context);
            service = new WorkshopService(repo);
        }

        [OneTimeSetUp]
        public void Seed()
        {
            optionsBuilder = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase("TestOutOfSchoolDb");

            context = new OutOfSchoolDbContext(optionsBuilder.Options);

            context.Database.EnsureCreated();
            SeedDb();
        }

        // [TearDown]
        // public void DatabaseSettings()
        // {
        //     context.Database.EnsureDeleted();
        //     context.Database.EnsureCreated();
        // }

        [Test]
        [Order(1)]
        public async Task Create_WhenWorkshopEntityIsNotNull_ShouldCreateWorkshop()
        {
            // Arrange
            var entityToAdd = new WorkshopDTO()
            {
                Id = 3,
                Title = "Table tennis section",
                Description = "Section for young sportsmen",
                MinAge = 4,
                MaxAge = 15,
                DaysPerWeek = 3,
                Phone = "380508912343",
                Price = 3200,
                Head = "Ivanenko P.I.",
                HeadBirthDate = new DateTime(1980, 10, 28),
                DisabilityOptionsDesc = "We provide our service for people with disability as well",
                WithDisabilityOptions = true,
                Instagram = "sectioninsta",
                Facebook = "sectionfacebook",
                Website = "www.section.com.ua",
                Email = "section@gmail.com",
                Image = "sectionimage.png",
            };

            // Act
            var newEntity = await service.Create(entityToAdd).ConfigureAwait(false);

            // Assert
            Assert.That(newEntity.Data, Is.TypeOf(typeof(WorkshopDTO)));
            Assert.That(entityToAdd.Id, Is.EqualTo(newEntity.Data.Id));
            Assert.That(() => newEntity.Error.Code, Throws.Exception);
        }

        [Test]
        [Order(8)]
        public async Task Create_WhenWorkshopEntityIsNull_ShouldReturnInternalServerError()
        {
            // Arrange
            WorkshopDTO entityToAdd = null;

            var workshop = await service.Create(null).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ErrorCode.InternalServerError, workshop.Error.Code);
        }

        [Test]
        [Order(4)]
        public void GetAll_WhenCalled_ReturnAllWorkshops()
        {
            // Arrange
            var expected = repo.GetAll().Result.Select(entity => entity.ToModel()).ToList();

            // Act 
            var result = service.GetAll().Result.Data.ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Enumerable
                .Range(0, result.Count())
                .ToList()
                .ForEach(i =>
                {
                    Assert.AreEqual(expected[i].Id, result[i].Id);
                    Assert.AreEqual(expected[i].Title, result[i].Title);
                    Assert.AreEqual(expected[i].Description, result[i].Description);
                    Assert.AreEqual(expected[i].DaysPerWeek, result[i].DaysPerWeek);
                    Assert.AreEqual(expected[i].Phone, result[i].Phone);
                    Assert.AreEqual(expected[i].Price, result[i].Price);
                    Assert.AreEqual(expected[i].Email, result[i].Email);
                    Assert.AreEqual(expected[i].Instagram, result[i].Instagram);
                    Assert.AreEqual(expected[i].Facebook, result[i].Facebook);
                    Assert.AreEqual(expected[i].Website, result[i].Website);
                    Assert.AreEqual(expected[i].Head, result[i].Head);
                    Assert.AreEqual(expected[i].HeadBirthDate, result[i].HeadBirthDate);
                    Assert.AreEqual(expected[i].MaxAge, result[i].MaxAge);
                    Assert.AreEqual(expected[i].MinAge, result[i].MinAge);
                    Assert.AreEqual(expected[i].WithDisabilityOptions, result[i].WithDisabilityOptions);
                    Assert.AreEqual(expected[i].DisabilityOptionsDesc, result[i].DisabilityOptionsDesc);
                });
        }

        [Test]
        [TestCase(1)]
        [Order(2)]
        public async Task GetById_IdIsValid_ShouldReturnWorkshopWithThisId(long id)
        {
            // Act
            var result = await service.GetById(id).ConfigureAwait(false);

            // Asert
            Assert.That(result.Data, Is.TypeOf(typeof(WorkshopDTO)));
            Assert.That(result.Data.Id, Is.EqualTo(id));
        }

        [Test]
        [TestCase(200)]
        [Order(3)]
        public async Task GetById_IdIsInvalid_ShouldReturnNotFound(long id)
        {
            // Act
            var workshopById = await service.GetById(id).ConfigureAwait(false);

            // Asert
            Assert.AreEqual(ErrorCode.NotFound, workshopById.Error.Code);
        }

        [Test]
        [Order(5)]
        public async Task Update_WhenUpdatingWorkshopEntity_ShouldUpdateWorkshopEntity()
        {
            var expected = new Workshop()
            {
                Id = 1,
                Title = "Football section",
                Description =
                    "Our section teaches football skills, developing physical abilities, shaping personal qualities: communication, cohesion, patience, self-confidence.",
                DaysPerWeek = 3,
                Price = 500,
                WithDisabilityOptions = false,
            };

            context.Entry(await repo.GetById(expected.Id)).State = EntityState.Detached;

            // Act
            var updatedOrganization = await repo.Update(expected).ConfigureAwait(false);

            Assert.That(expected.DaysPerWeek, Is.EqualTo(updatedOrganization.DaysPerWeek));
        }

        [Test]
        [TestCase(1)]
        [Order(6)]
        public async Task Delete_WhenIdIsValid_ShouldDeleteWorkshopEntityWithThisId(long id)
        {
            // Act
            var countBeforeDeleting = service.GetAll().Result.Data.Count();

            await service.Delete(id).ConfigureAwait(false);

            var countAfterDeleting = await service.GetAll().ConfigureAwait(false);

            // Assert
            Assert.That(countBeforeDeleting, Is.Not.EqualTo(countAfterDeleting));
        }

        [Test]
        [TestCase(100)]
        [Order(7)]
        public async Task Delete_WhenIdIsNotValid_ShouldReturnNotFound(long id)
        {
            // Act
            var workshop = await service.Delete(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ErrorCode.NotFound, workshop.Error.Code);
        }


        private void SeedDb()
        {
            using var context = new OutOfSchoolDbContext(optionsBuilder.Options);

            var categories = new List<Category>
            {
                new Category()
                {
                    Id = 1, Title = "Sport", Subcategories = new List<Subcategory>(),
                },
                new Category()
                {
                    Id = 2, Title = "Programming", Subcategories = new List<Subcategory>(),
                },
                new Category()
                {
                    Id = 3, Title = "Preschool courses", Subcategories = new List<Subcategory>(),
                },
                new Category()
                {
                    Id = 4, Title = "Foreign languages", Subcategories = new List<Subcategory>(),
                },
            };
            var organizations = new List<Organization>
            {
                new Organization()
                {
                    Id = 1,
                    Title = "Kyiv Sport Centre",
                    Description = "Organization",
                    Phone = "380991023243",
                    Website = "website.com.ua",
                    Instagram = "organizationinst",
                    Facebook = "facebookfacebook",
                    EDRPOU = "12345678",
                    MFO = "123456",
                    Type = OrganizationType.Social,
                    INPP = "123456789123",
                },
            };
            var addresses = new List<Address>
            {
                new Address()
                {
                    Id = 1,
                    City = "Kyiv",
                    District = "Pecherskyi",
                    Street = "Lavrska",
                    BuildingNumb = "43/3",
                    Latitude = 32.342,
                    Longitude = 43.134,
                },
                new Address()
                {
                    Id = 2,
                    City = "Kyiv",
                    District = "Podilskyi",
                    Street = "Verhniy val",
                    BuildingNumb = "12a",
                    Latitude = 32.432,
                    Longitude = 42.879,
                },
                new Address()
                {
                    Id = 3,
                    City = "Kyiv",
                    District = "Desnianskyi",
                    Street = "Teodora Drayzera",
                    BuildingNumb = "34",
                    Latitude = 32.896,
                    Longitude = 43.543,
                },
            };
            var workshops = new List<Workshop>
            {
                new Workshop()
                {
                    Id = 1,
                    Title = "Football section",
                    Description =
                        "Our section teaches football skills, developing physical abilities, shaping personal qualities: communication, cohesion, patience, self-confidence.",
                    DaysPerWeek = 2,
                    Price = 500,
                    WithDisabilityOptions = false,
                },
                new Workshop()
                {
                    Id = 2,
                    Title = "English courses",
                    Description =
                        "Our highly qualified English teachers are by your side and provide you with the right tools to help you interact confidently in the real world and achieve the professional and personal success you are working towards.",
                    DaysPerWeek = 3,
                    Price = 5000,
                    WithDisabilityOptions = true,
                },
            };

            context.AddRange(workshops);
            context.AddRange(categories);

            context.SaveChanges();
        }
    }
}