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
        private IWorkshopRepository workshopRepository;
        private IWorkshopService workshopService;

        private Mock<ISubsubcategoryRepository> sscategoryRepositoryMoq;
        private Mock<IEntityRepository<Teacher>> teacherRepositoryMoq;
        private Mock<IEntityRepository<Address>> addressRepositoryMoq;

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

            workshopRepository = new WorkshopRepository(context);
            sscategoryRepositoryMoq = new Mock<ISubsubcategoryRepository>();
            teacherRepositoryMoq = new Mock<IEntityRepository<Teacher>>();
            addressRepositoryMoq = new Mock<IEntityRepository<Address>>();
            ratingService = new Mock<IRatingService>();
            logger = new Mock<ILogger>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            workshopService = new WorkshopService(
                workshopRepository,
                sscategoryRepositoryMoq.Object,
                teacherRepositoryMoq.Object,
                addressRepositoryMoq.Object,
                ratingService.Object,
                logger.Object,
                localizer.Object);

            SeedDatabase();
        }

        //[Test]
        //[Order(1)]
        public async Task Create_WhenEntityIsValid_ShouldReturnCreatedEntity()
        {
            // Arrange
            var expected = new Workshop()
            {
                Title = "Title",
                Phone = "1111111111",
                Description = "Desc",
                Price = 1000,
                WithDisabilityOptions = true,
                DaysPerWeek = 1,
                Head = "Head",
                HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                ProviderTitle = "ProviderTitle",
                DisabilityOptionsDesc = "Desc",
                Website = "website",
                Instagram = "insta",
                Facebook = "facebook",
                Email = "email1@gmail.com",
                MaxAge = 10,
                MinAge = 4,
                Logo = "image",
                ProviderId = 1,
                CategoryId = 1,
                SubsubcategoryId = 1,
                SubcategoryId = 1,
                Address = new Address
                {
                    Region = "Region",
                    District = "District",
                    City = "City",
                    Street = "Street",
                    BuildingNumber = "BuildingNumber",
                    Latitude = 0,
                    Longitude = 0,
                },
                Teachers = new List<Teacher>
                {
                    new Teacher
                    {
                        FirstName = "Alex",
                        LastName = "Brown",
                        MiddleName = "SomeMiddleName",
                        Description = "Description",
                        Image = "Image",
                    },
                    new Teacher
                    {
                        FirstName = "John",
                        LastName = "Snow",
                        MiddleName = "SomeMiddleName",
                        Description = "Description",
                        Image = "Image",
                    },
                },
            };

            // Act
            var result = await workshopService.Create(expected.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Title, result.Title);
            Assert.AreEqual(expected.Email, result.Email);
            Assert.AreEqual(expected.Phone, result.Phone);
            Assert.AreEqual(expected.Description, result.Description);
            Assert.AreEqual(expected.Head, result.Head);
        }

        [Test]
        [Order(2)]
        public async Task GetAll_WhenCalled_ShouldReturnAllEntities()
        {
            // Arrange
            var expected = await workshopRepository.GetAll();

            // Act
            var result = await workshopService.GetAll().ConfigureAwait(false);

            // Assert
            Assert.That(expected.Count(), Is.EqualTo(result.Count()));
        }

        [Test]
        [TestCase(1)]
        [Order(3)]
        public async Task GetById_WhenIdIsValid_ShouldReturnEntity(long id)
        {
            // Arrange
            var expected = await workshopRepository.GetById(id);

            // Act
            var result = await workshopService.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.Id, result.Id);
        }

        [Test]
        [TestCase(10)]
        [Order(4)]
        public void GetById_WhenIdIsInvalid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Assert
            Assert.That(async () => await workshopService.GetById(10), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        //[Test]
        //[Order(5)]
        public async Task Update_WhenEntityIsValid_ShouldUpdateExistedEntity()
        {
            // Arrange
            var changedEntity = new WorkshopDTO()
            {
                Id = 1,
                Title = "ChangedTitle2",
            };

            // Act
            var result = await workshopService.Update(changedEntity).ConfigureAwait(false);

            // Assert
            Assert.That(changedEntity.Title, Is.EqualTo(result.Title));
        }

        //[Test]
        //[Order(6)]
        public void Update_WhenEntityIsInvalid_ShouldThrowDbUpdateConcurrencyException()
        {
            // Arrange
            var changedEntity = new WorkshopDTO()
            {
                Title = "Title1",
            };

            // Assert
            Assert.That(
                async () => await workshopService.Update(changedEntity).ConfigureAwait(false),
                Throws.Exception.TypeOf<DbUpdateConcurrencyException>());
        }

        [Test]
        [TestCase(3)]
        [Order(7)]
        public async Task Delete_WhenIdIsValid_ShouldDeleteEntity(long id)
        {
            // Act
            var countBeforeDeleting = (await workshopService.GetAll().ConfigureAwait(false)).Count();

            await workshopService.Delete(id).ConfigureAwait(false);

            var countAfterDeleting = (await workshopService.GetAll().ConfigureAwait(false)).Count();

            // Assert
            Assert.That(countAfterDeleting, Is.Not.EqualTo(countBeforeDeleting));
        }

        [Test]
        [TestCase(10)]
        [Order(8)]
        public void Delete_WhenIdIsInvalid_ShouldThrowNullReferenceException(long id)
        {
            // Assert
            Assert.That(
                async () => await workshopService.Delete(id).ConfigureAwait(false),
                Throws.Exception.TypeOf<NullReferenceException>());
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
                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                        ProviderTitle = "ProviderTitle",
                        DisabilityOptionsDesc = "Desc1",
                        Website = "website1",
                        Instagram = "insta1",
                        Facebook = "facebook1",
                        Email = "email1@gmail.com",
                        MaxAge = 10,
                        MinAge = 4,
                        Logo = "image1",
                        ProviderId = 1,
                        CategoryId = 1,
                        SubsubcategoryId = 1,
                        SubcategoryId = 1,
                        AddressId = 55,
                        Address = new Address
                        {
                            Id = 55,
                            Region = "Region55",
                            District = "District55",
                            City = "City55",
                            Street = "Street55",
                            BuildingNumber = "BuildingNumber55",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        Teachers = new List<Teacher>
                        {
                            new Teacher
                            {
                                Id = 1,
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                            },
                            new Teacher
                            {
                                Id = 2,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                            },
                        },
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
                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                        ProviderTitle = "ProviderTitle",
                        DisabilityOptionsDesc = "Desc2",
                        Website = "website2",
                        Instagram = "insta2",
                        Facebook = "facebook2",
                        Email = "email2@gmail.com",
                        MaxAge = 10,
                        MinAge = 4,
                        Logo = "image2",
                        ProviderId = 1,
                        CategoryId = 1,
                        SubcategoryId = 1,
                        AddressId = 10,
                        Address = new Address
                        {
                            Id = 10,
                            Region = "Region10",
                            District = "District10",
                            City = "City10",
                            Street = "Street10",
                            BuildingNumber = "BuildingNumber10",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        Teachers = new List<Teacher>
                        {
                            new Teacher
                            {
                                Id = 4,
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                            },
                            new Teacher
                            {
                                Id = 5,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                            },
                        },
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
                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                        ProviderTitle = "ProviderTitle",
                        Website = "website3",
                        Instagram = "insta3",
                        Facebook = "facebook3",
                        Email = "email3@gmail.com",
                        MaxAge = 10,
                        MinAge = 4,
                        Logo = "image3",
                        ProviderId = 1,
                        CategoryId = 1,
                        SubcategoryId = 1,
                        AddressId = 11,
                        Address = new Address
                        {
                            Id = 11,
                            Region = "Region11",
                            District = "District11",
                            City = "City11",
                            Street = "Street11",
                            BuildingNumber = "BuildingNumber11",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        Teachers = new List<Teacher>
                        {
                            new Teacher
                            {
                                Id = 6,
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                            },
                            new Teacher
                            {
                                Id = 7,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                            },
                        },
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
                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                        ProviderTitle = "ProviderTitle",
                        DisabilityOptionsDesc = "Desc4",
                        Website = "website4",
                        Instagram = "insta4",
                        Facebook = "facebook4",
                        Email = "email4@gmail.com",
                        MaxAge = 10,
                        MinAge = 4,
                        Logo = "image4",
                        ProviderId = 1,
                        CategoryId = 1,
                        SubcategoryId = 1,
                        AddressId = 15,
                        Address = new Address
                        {
                            Id = 15,
                            Region = "Region15",
                            District = "District15",
                            City = "City15",
                            Street = "Street15",
                            BuildingNumber = "BuildingNumber15",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        Teachers = new List<Teacher>
                        {
                            new Teacher
                            {
                                Id = 8,
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                            },
                            new Teacher
                            {
                                Id = 9,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                            },
                        },
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
                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                        ProviderTitle = "ProviderTitle",
                        DisabilityOptionsDesc = "Desc5",
                        Website = "website5",
                        Instagram = "insta5",
                        Facebook = "facebook5",
                        Email = "email5@gmail.com",
                        MaxAge = 10,
                        MinAge = 4,
                        Logo = "image5",
                        ProviderId = 1,
                        CategoryId = 1,
                        SubcategoryId = 1,
                        AddressId = 17,
                        Address = new Address
                        {
                            Id = 17,
                            Region = "Region17",
                            District = "District17",
                            City = "City17",
                            Street = "Street17",
                            BuildingNumber = "BuildingNumber17",
                            Latitude = 0,
                            Longitude = 0,
                        },
                        Teachers = new List<Teacher>
                        {
                            new Teacher
                            {
                                Id = 10,
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                            },
                            new Teacher
                            {
                                Id = 11,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                            },
                        },
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