using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        private IWorkshopService workshopService;

        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext dbContext;

        private IWorkshopRepository workshopRepository;
        private Mock<ISubsubcategoryRepository> sscategoryRepositoryMoq;
        private IEntityRepository<Teacher> teacherRepository;
        private IEntityRepository<Address> addressRepository;

        private Mock<IRatingService> ratingServiceMoq;
        private Mock<ILogger> loggerMoq;
        private Mock<IStringLocalizer<SharedResource>> localizerMoq;

        private Workshop newWorkshop;
        private Subsubcategory sscategory;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolWorkshopTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            options = builder.Options;
            dbContext = new OutOfSchoolDbContext(options);

            workshopRepository = new WorkshopRepository(dbContext);
            sscategoryRepositoryMoq = new Mock<ISubsubcategoryRepository>();
            teacherRepository = new EntityRepository<Teacher>(dbContext);
            addressRepository = new EntityRepository<Address>(dbContext);

            ratingServiceMoq = new Mock<IRatingService>();
            loggerMoq = new Mock<ILogger>();
            localizerMoq = new Mock<IStringLocalizer<SharedResource>>();

            workshopService = new WorkshopService(
                workshopRepository,
                sscategoryRepositoryMoq.Object,
                teacherRepository,
                addressRepository,
                ratingServiceMoq.Object,
                loggerMoq.Object,
                localizerMoq.Object);

            SeedDatabase();
        }
#pragma warning disable SA1124 // Do not use regions

        #region Create
        [Test]
        public async Task Create_WhenEntityIsValid_ShouldCreateEntities()
        {
            // Arrange
            sscategoryRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
               .ReturnsAsync(sscategory);
            var teachersCount = dbContext.Teachers.Count();
            var addressecCount = dbContext.Addresses.Count();

            // Act
            var result = await workshopService.Create(newWorkshop.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(dbContext.Workshops.Last().Id, result.Id);
            Assert.AreEqual(6, result.Id);
            Assert.AreEqual(newWorkshop.Title, result.Title);

            Assert.AreEqual(sscategory.Id, result.SubsubcategoryId);
            Assert.AreEqual(sscategory.SubcategoryId, result.SubcategoryId);
            Assert.AreEqual(sscategory.Subcategory.CategoryId, result.CategoryId);

            Assert.AreEqual(newWorkshop.Teachers.Count, result.Teachers.Count());
            Assert.AreEqual(dbContext.Teachers.Count(), teachersCount + 2);

            Assert.AreEqual(dbContext.Addresses.Last().Id, result.Address.Id);
            Assert.AreEqual(dbContext.Addresses.Count(), addressecCount + 1);
        }

        [Test]
        public async Task Create_WhenCategoriesIdsSetWrong_ShouldCreateEntitiesWithRightCategoriesIds()
        {
            // Arrange
            sscategoryRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
               .ReturnsAsync(sscategory);
            newWorkshop.Title = "newWorkshopTitle2";
            newWorkshop.ProviderId = 7;
            newWorkshop.SubcategoryId = 10;
            newWorkshop.CategoryId = 90;

            // Act
            var result = await workshopService.Create(newWorkshop.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(dbContext.Workshops.Last().Id, result.Id);
            Assert.AreEqual(6, result.Id);
            Assert.AreEqual(newWorkshop.Title, result.Title);

            Assert.AreEqual(sscategory.Id, result.SubsubcategoryId);
            Assert.AreEqual(sscategory.SubcategoryId, result.SubcategoryId);
            Assert.AreEqual(sscategory.Subcategory.CategoryId, result.CategoryId);
        }

        [Test]
        public void Create_WhenSubsubcategoryIdSetWrong_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            sscategoryRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
               .ReturnsAsync(() => null);
            newWorkshop.Title = "newWorkshopTitle3";

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await workshopService.Create(newWorkshop.ToModel()).ConfigureAwait(false));
        }
        #endregion

        #region GetAll
        [Test]
        public async Task GetAll_WhenCalled_ShouldReturnAllEntities()
        {
            // Arrange
            var expected = await workshopRepository.GetAll();

            // Act
            var result = await workshopService.GetAll().ConfigureAwait(false);

            // Assert
            Assert.That(expected.Count(), Is.EqualTo(result.Count()));
        }
        #endregion

        #region GetById
        [Test]
        [TestCase(1)]
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
        [TestCase(0)]
        [TestCase(99)]
        public async Task GetById_WhenThereIsNoEntityWithId_ShouldReturnNull(long id)
        {
            // Act
            var result = await workshopService.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }
        #endregion

        #region GetWorkshopsByOrganization
        #endregion

        #region Update
        [Test]
        public async Task Update_WhenEntityIsValid_ShouldUpdateAllRelationalEntities()
        {
            // Arrange
            sscategoryRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
               .ReturnsAsync(sscategory);
            var changedFirstEntity = new Workshop()
            {
                Id = 1,
                Title = "ChangedTitle",
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
                    Latitude = 10,
                    Longitude = 10,
                },
                Teachers = new List<Teacher>
                        {
                            // deleteted first teacher
                            // changed teacher
                            new Teacher
                            {
                                Id = 2,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "Targaryen",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("2000-01-01"),
                                WorkshopId = 1,
                            },

                            // new teacher
                            new Teacher
                            {
                                Id = 0,
                                FirstName = "Daenerys",
                                LastName = "Targaryen",
                                MiddleName = "SomeMiddleName",
                                Description = "New",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("2000-01-01"),
                                WorkshopId = 1,
                            },
                        },
            };

            // Act
            var result = await workshopService.Update(changedFirstEntity.ToModel()).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(changedFirstEntity.Title, result.Title);

            Assert.AreEqual(sscategory.Id, result.SubsubcategoryId);
            Assert.AreEqual(sscategory.SubcategoryId, result.SubcategoryId);
            Assert.AreEqual(sscategory.Subcategory.CategoryId, result.CategoryId);

            Assert.AreEqual(changedFirstEntity.Teachers.Count, result.Teachers.Count());
            Assert.AreEqual(dbContext.Teachers.Where(x => x.WorkshopId == 1).Count(), result.Teachers.Count());
            Assert.AreEqual(0, dbContext.Teachers.Where(x => x.Id == 1).Count());
            Assert.AreEqual("Targaryen", dbContext.Teachers.Where(x => x.Id == 2).First().MiddleName);
            Assert.AreEqual("Daenerys", dbContext.Teachers.Where(x => x.Id == 11).First().FirstName);

            Assert.AreEqual(changedFirstEntity.Address.Latitude, result.Address.Latitude);
            Assert.AreEqual(10, dbContext.Addresses.Where(x => x.Id == 55).First().Latitude);
            Assert.AreEqual(dbContext.Addresses.Where(x => x.Id == 55).First().Latitude, result.Address.Latitude);
        }

        [Test]
        public void Update_WhenIdIsInvalid_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            sscategoryRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
               .ReturnsAsync(sscategory);
            var changedEntity = new WorkshopDTO()
            {
                Id = 99,
                Title = "Title1",
            };

            // Act and Assert
            Assert.That(
                async () => await workshopService.Update(changedEntity).ConfigureAwait(false),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        #endregion

        #region Delete
        [Test]
        [TestCase(1)]
        public async Task Delete_WhenIdIsValid_ShouldDeleteAllRelationalEntities(long id)
        {
            // Act
            var countWorkshopsBeforeDeleting = (await workshopService.GetAll().ConfigureAwait(false)).Count();
            var countAddressesBeforeDeleting = (await addressRepository.GetAll().ConfigureAwait(false)).Count();
            var countTeachersBeforeDeleting = (await teacherRepository.GetAll().ConfigureAwait(false)).Count();

            var appRepository = new EntityRepository<Application>(dbContext);
            var countAppsBeforeDeleting = (await appRepository.GetAll().ConfigureAwait(false)).Count();

            // Act
            await workshopService.Delete(id).ConfigureAwait(false);

            var countWorkshopsAfterDeleting = (await workshopService.GetAll().ConfigureAwait(false)).Count();
            var countAddressesAfterDeleting = (await addressRepository.GetAll().ConfigureAwait(false)).Count();
            var countTeachersAfterDeleting = (await teacherRepository.GetAll().ConfigureAwait(false)).Count();

            var countAppsAfterDeleting = (await appRepository.GetAll().ConfigureAwait(false)).Count();

            // Assert
            Assert.AreEqual(countWorkshopsBeforeDeleting - 1, countWorkshopsAfterDeleting);
            Assert.AreEqual(countAddressesBeforeDeleting - 1, countAddressesAfterDeleting);
            Assert.AreEqual(countTeachersBeforeDeleting - 2, countTeachersAfterDeleting);
            Assert.AreEqual(countAppsBeforeDeleting - 2, countAppsAfterDeleting);
        }

        [Test]
        [TestCase(7)]
        public void Delete_WhenIdIsInvalid_ShouldThrowNullReferenceException(long id)
        {
            // Assert
            Assert.That(
                async () => await workshopService.Delete(id).ConfigureAwait(false),
                Throws.Exception.TypeOf<NullReferenceException>());
        }
        #endregion
#pragma warning restore SA1124 // Do not use regions

        private void SeedDatabase()
        {
            newWorkshop = new Workshop()
            {
                Id = 0,
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
                ProviderId = 6,
                CategoryId = 1,
                SubsubcategoryId = 1,
                SubcategoryId = 1,
                AddressId = 0,
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
                        DateOfBirth = DateTime.Parse("2000-01-01"),
                        WorkshopId = 0,
                    },
                    new Teacher
                    {
                        FirstName = "John",
                        LastName = "Snow",
                        MiddleName = "SomeMiddleName",
                        Description = "Description",
                        Image = "Image",
                        DateOfBirth = DateTime.Parse("2000-01-01"),
                        WorkshopId = 0,
                    },
                },
            };
            sscategory = new Subsubcategory()
            {
                Id = 1,
                Title = "new SSC",
                SubcategoryId = 1,
                Subcategory = new Subcategory()
                {
                    Id = 1,
                    Title = "new SC",
                    CategoryId = 1,
                    Category = new Category()
                    {
                        Id = 1,
                        Title = "new C",
                    },
                },
            };

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
                                DateOfBirth = DateTime.Parse("2000-01-01"),
                                WorkshopId = 1,
                            },
                            new Teacher
                            {
                                Id = 2,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("2000-01-01"),
                                WorkshopId = 1,
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
                        ProviderId = 2,
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
                                Id = 3,
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("2000-01-01"),
                                WorkshopId = 2,
                            },
                            new Teacher
                            {
                                Id = 4,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("2000-01-01"),
                                WorkshopId = 2,
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
                        ProviderId = 3,
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
                                Id = 5,
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("2000-01-01"),
                                WorkshopId = 3,
                            },
                            new Teacher
                            {
                                Id = 6,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("2000-01-01"),
                                WorkshopId = 3,
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
                        ProviderId = 4,
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
                                Id = 7,
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("2020-01-01"),
                                WorkshopId = 4,
                            },
                            new Teacher
                            {
                                Id = 8,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("1990-01-01"),
                                WorkshopId = 4,
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
                        ProviderId = 5,
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
                                Id = 9,
                                FirstName = "Alex",
                                LastName = "Brown",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("1990-01-01"),
                                WorkshopId = 5,
                            },
                            new Teacher
                            {
                                Id = 10,
                                FirstName = "John",
                                LastName = "Snow",
                                MiddleName = "SomeMiddleName",
                                Description = "Description",
                                Image = "Image",
                                DateOfBirth = DateTime.Parse("1990-01-01"),
                                WorkshopId = 5,
                            },
                        },
                    },
                };

                var apps = new List<Application>() { new Application() { Id = 1, WorkshopId = 1 }, new Application() { Id = 2, WorkshopId = 1 }, new Application() { Id = 3, WorkshopId = 2 }, new Application() { Id = 4, WorkshopId = 2 } };
                context.Applications.AddRangeAsync(apps);

                var categories = new List<Category>() { new Category() { Title = "Category1" }, new Category() { Title = "Category2" } };
                context.Categories.AddRangeAsync(categories);
                var subcategories = new List<Subcategory>() { new Subcategory() { Title = "new1", CategoryId = 1 }, new Subcategory() { Title = "new2", CategoryId = 1 } };
                context.Subcategories.AddRangeAsync(subcategories);
                var subsubcategories = new List<Subsubcategory>() { new Subsubcategory() { Title = "new1", SubcategoryId = 1 }, new Subsubcategory() { Title = "new2", SubcategoryId = 1 } };
                context.Subsubcategories.AddRangeAsync(subsubcategories);

                context.Workshops.AddRangeAsync(workshops);
                context.SaveChangesAsync();
            }
        }
    }
}
