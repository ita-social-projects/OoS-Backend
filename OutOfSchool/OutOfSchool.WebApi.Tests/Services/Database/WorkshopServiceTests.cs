//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;
//using OutOfSchool.Services;
//using OutOfSchool.Services.Enums;
//using OutOfSchool.Services.Models;
//using OutOfSchool.Services.Repository;
//using OutOfSchool.Tests.Common.TestDataGenerators;
//using OutOfSchool.WebApi.Enums;
//using OutOfSchool.WebApi.Extensions;
//using OutOfSchool.WebApi.Models;
//using OutOfSchool.WebApi.Services;

//namespace OutOfSchool.WebApi.Tests.Services
//{
//    [TestFixture]
//    public class WorkshopServiceTests
//    {
//        private IWorkshopService workshopService;

//        private DbContextOptions<OutOfSchoolDbContext> options;
//        private OutOfSchoolDbContext dbContext;

//        private IWorkshopRepository workshopRepository;
//        private Mock<IClassRepository> classRepositoryMoq;

//        private Mock<IRatingService> ratingServiceMoq;
//        private Mock<ILogger<WorkshopService>> loggerMoq;
//        private Mock<IMapper> mapperMoq;

//        private Workshop newWorkshop;
//        private Class classEntity;

//        [SetUp]
//        public void SetUp()
//        {
//            var builder =
//                new DbContextOptionsBuilder<OutOfSchoolDbContext>()
//                .UseInMemoryDatabase(databaseName: "OutOfSchoolWorkshopTestDB")
//                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

//            options = builder.Options;
//            dbContext = new OutOfSchoolDbContext(options);

//            workshopRepository = new WorkshopRepository(dbContext);
//            classRepositoryMoq = new Mock<IClassRepository>();

//            ratingServiceMoq = new Mock<IRatingService>();
//            loggerMoq = new Mock<ILogger<WorkshopService>>();
//            mapperMoq = new Mock<IMapper>();

//            workshopService = new WorkshopService(
//                workshopRepository,
//                classRepositoryMoq.Object,
//                ratingServiceMoq.Object,
//                loggerMoq.Object,
//                mapperMoq.Object);

//            SeedDatabase();
//        }

//        #region Create
//        [Test]
//        public async Task Create_WhenEntityIsValid_ShouldCreateEntities()
//        {
//            // Arrange
//            classRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
//               .ReturnsAsync(classEntity);
//            var teachersCount = dbContext.Teachers.Count();
//            var addressecCount = dbContext.Addresses.Count();

//            // Act
//            var result = await workshopService.Create(newWorkshop.ToModel()).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(dbContext.Workshops.Last().Id, result.Id);
//            Assert.AreEqual(6, result.Id);
//            Assert.AreEqual(newWorkshop.Title, result.Title);

//            Assert.AreEqual(classEntity.Id, result.ClassId);
//            Assert.AreEqual(classEntity.DepartmentId, result.DepartmentId);
//            Assert.AreEqual(classEntity.Department.DirectionId, result.DirectionId);

//            Assert.AreEqual(newWorkshop.Teachers.Count, result.Teachers.Count());
//            Assert.AreEqual(dbContext.Teachers.Count(), teachersCount + 2);

//            Assert.AreEqual(dbContext.Addresses.Last().Id, result.Address.Id);
//            Assert.AreEqual(dbContext.Addresses.Count(), addressecCount + 1);
//        }

//        [Test]
//        public async Task Create_WhenDirectionsIdsAreWrong_ShouldCreateEntitiesWithRightDirectionsIds()
//        {
//            // Arrange
//            classRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
//               .ReturnsAsync(classEntity);
//            newWorkshop.Title = "newWorkshopTitle2";
//            newWorkshop.ProviderId = 7;
//            newWorkshop.DepartmentId = 10;
//            newWorkshop.DirectionId = 90;

//            // Act
//            var result = await workshopService.Create(newWorkshop.ToModel()).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(dbContext.Workshops.Last().Id, result.Id);
//            Assert.AreEqual(6, result.Id);
//            Assert.AreEqual(newWorkshop.Title, result.Title);

//            Assert.AreEqual(classEntity.Id, result.ClassId);
//            Assert.AreEqual(classEntity.DepartmentId, result.DepartmentId);
//            Assert.AreEqual(classEntity.Department.DirectionId, result.DirectionId);
//        }

//        [Test]
//        public void Create_WhenThereIsNoClassId_ShouldThrowArgumentOutOfRangeException()
//        {
//            // Arrange
//            classRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
//               .ReturnsAsync(() => null);
//            newWorkshop.Title = "newWorkshopTitle3";

//            // Act and Assert
//            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
//                async () => await workshopService.Create(newWorkshop.ToModel()).ConfigureAwait(false));
//        }
//        #endregion

//        #region GetAll
//        [Test]
//        public async Task GetAll_WhenCalled_ShouldReturnAllEntities()
//        {
//            // Arrange
//            var expected = await workshopRepository.GetAll();
//            var filter = new OffsetFilter();

//            // Act
//            var result = await workshopService.GetAll(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected.Count(), result.TotalAmount);
//            Assert.AreEqual(expected.First().Id, result.Entities.First().Id);
//        }
//        #endregion

//        #region GetById
//        [Test]
//        [TestCase(1)]
//        public async Task GetById_WhenIdIsValid_ShouldReturnEntity(long id)
//        {
//            // Arrange
//            var expected = await workshopRepository.GetById(id);

//            // Act
//            var result = await workshopService.GetById(id).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected.Id, result.Id);
//        }

//        [Test]
//        [TestCase(0)]
//        [TestCase(99)]
//        public async Task GetById_WhenThereIsNoEntityWithId_ShouldReturnNull(long id)
//        {
//            // Act
//            var result = await workshopService.GetById(id).ConfigureAwait(false);

//            // Assert
//            Assert.IsNull(result);
//        }
//        #endregion

//        #region GetByProviderId
//        [Test]
//        [TestCase(1)]
//        public async Task GetByProviderId_WhenIdIsValid_ShouldReturnEntities(long id)
//        {
//            // Arrange
//            var expected = await workshopRepository.GetByFilter(x => x.ProviderId == id).ConfigureAwait(false);

//            // Act
//            var result = await workshopService.GetByProviderId(id).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected.First().Id, result.First().Id);
//        }

//        [Test]
//        [TestCase(0)]
//        [TestCase(99)]
//        public async Task GetByProviderId_WhenThereIsNoEntityWithId_ShouldReturnEmptyList(long id)
//        {
//            // Arrange
//            var expected = await workshopRepository.GetByFilter(x => x.ProviderId == id).ConfigureAwait(false);

//            // Act
//            var result = await workshopService.GetByProviderId(id).ConfigureAwait(false);

//            // Assert
//            Assert.IsEmpty(result);
//        }
//        #endregion

//        #region Update

//        // TODO: refactor this test
//        //[Test]
//        //public async Task Update_WhenEntityIsValid_ShouldUpdateAllRelationalEntities([Random(2,5,1)] int teachersInWorkshop)
//        //{
//        //    // Arrange
//        //    classRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
//        //       .ReturnsAsync(classEntity);

//        //    var changedFirstEntity = new Workshop()
//        //    {
//        //        Id = 1,
//        //        Title = "ChangedTitle",
//        //        Phone = "1111111111",
//        //        Description = "Desc1",
//        //        Price = 1000,
//        //        WithDisabilityOptions = true,
//        //        Head = "Head1",
//        //        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//        //        ProviderTitle = "ProviderTitle",
//        //        DisabilityOptionsDesc = "Desc1",
//        //        Website = "website1",
//        //        Instagram = "insta1",
//        //        Facebook = "facebook1",
//        //        Email = "email1@gmail.com",
//        //        MaxAge = 10,
//        //        MinAge = 4,
//        //        Logo = "image1",
//        //        ProviderId = 1,
//        //        DirectionId = 1,
//        //        ClassId = 1,
//        //        DepartmentId = 1,
//        //        AddressId = 55,
//        //        Address = new Address
//        //        {
//        //            Id = 55,
//        //            Region = "Region55",
//        //            District = "District55",
//        //            City = "City55",
//        //            Street = "Street55",
//        //            BuildingNumber = "BuildingNumber55",
//        //            Latitude = 10,
//        //            Longitude = 10,
//        //        },
//        //    };
//        //    var teachers = TeachersGenerator.Generate(teachersInWorkshop).WithWorkshopId(changedFirstEntity.Id);
//        //    changedFirstEntity.Teachers = teachers;

//        //    // Act
//        //    var result = await workshopService.Update(changedFirstEntity.ToModel()).ConfigureAwait(false);

//        //    // Assert
//        //    Assert.AreEqual(changedFirstEntity.Title, result.Title);

//        //    Assert.AreEqual(classEntity.Id, result.ClassId);
//        //    Assert.AreEqual(classEntity.DepartmentId, result.DepartmentId);
//        //    Assert.AreEqual(classEntity.Department.DirectionId, result.DirectionId);

//        //    Assert.AreEqual(changedFirstEntity.Teachers.Count, result.Teachers.Count());
//        //    Assert.AreEqual(dbContext.Teachers.Where(x => x.WorkshopId == 1).Count(), result.Teachers.Count());

//        //    //Assert.AreEqual(0, dbContext.Teachers.Where(x => x.Id == 1).Count());

//        //    //Assert.AreEqual("Targaryen", dbContext.Teachers.Where(x => x.Id == 2).First().MiddleName);

//        //    //Assert.AreEqual("Daenerys", dbContext.Teachers.Where(x => x.Id == 11).First().FirstName);

//        //    Assert.AreEqual(changedFirstEntity.Address.Latitude, result.Address.Latitude);
//        //    Assert.AreEqual(10, dbContext.Addresses.Where(x => x.Id == 55).First().Latitude);
//        //    Assert.AreEqual(dbContext.Addresses.Where(x => x.Id == 55).First().Latitude, result.Address.Latitude);
//        //}

//        [Test]
//        public void Update_WhenIdIsInvalid_ShouldThrowArgumentOutOfRangeException()
//        {
//            // Arrange
//            classRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
//               .ReturnsAsync(classEntity);
//            var changedEntity = new WorkshopDTO()
//            {
//                Id = 99,
//                Title = "Title1",
//            };

//            // Act and Assert
//            Assert.That(
//                async () => await workshopService.Update(changedEntity).ConfigureAwait(false),
//                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
//        }

//        #endregion

//        #region Delete
//        [Test]
//        [TestCase(1)]
//        public async Task Delete_WhenIdIsValid_ShouldDeleteAllRelationalEntities(long id)
//        {
//            // Act
//            var countWorkshopsBeforeDeleting = (await workshopService.GetAll(new OffsetFilter()).ConfigureAwait(false)).TotalAmount;

//            var appRepository = new EntityRepository<Application>(dbContext);
//            var countAppsBeforeDeleting = (await appRepository.GetAll().ConfigureAwait(false)).Count();

//            // Act
//            await workshopService.Delete(id).ConfigureAwait(false);

//            var countWorkshopsAfterDeleting = (await workshopService.GetAll(new OffsetFilter()).ConfigureAwait(false)).TotalAmount;

//            var countAppsAfterDeleting = (await appRepository.GetAll().ConfigureAwait(false)).Count();

//            // Assert
//            Assert.AreEqual(countWorkshopsBeforeDeleting - 1, countWorkshopsAfterDeleting);
//            Assert.AreEqual(countAppsBeforeDeleting - 2, countAppsAfterDeleting);
//        }

//        [Test]
//        [TestCase(7)]
//        public void Delete_WhenIdIsInvalid_ShouldThrowNullReferenceException(long id)
//        {
//            // Assert
//            Assert.That(
//                async () => await workshopService.Delete(id).ConfigureAwait(false),
//                Throws.Exception.TypeOf<NullReferenceException>());
//        }
//        #endregion

//        #region GetByFilter
//        [Test]
//        public async Task GetByFilter_WhenFilterIsNull_ShouldReturnFirstPortionOfAllEntities()
//        {
//            // Arrange
//            var expected = await workshopRepository.GetAll();

//            // Act
//            var result = await workshopService.GetByFilter(null).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected.Count(), result.TotalAmount);
//            Assert.AreEqual(expected.First().Id, result.Entities.First().Id);
//        }

//        [Test]
//        public async Task GetByFilter_WhenIdsIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 2;
//            var filter = new WorkshopFilter()
//            {
//                Ids = new List<long>() { 1, 2 },
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => x.Id == 1 || x.Id == 2));
//        }

//        [Test]
//        public async Task GetByFilter_WhenNotOnlyIdsAreSet_ShouldReturnEntitiesOnlyByIdsSearch()
//        {
//            // Arrange
//            var expected = 2;
//            var filter = new WorkshopFilter()
//            {
//                Ids = new List<long>() { 1, 2 },
//                IsFree = true,
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => x.Id == 1 || x.Id == 2));
//            Assert.IsFalse(result.Entities.All(x => x.Price == 0));
//        }

//        [Test]
//        public async Task GetByFilter_WhenCityIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 3;
//            var filter = new WorkshopFilter()
//            {
//                City = "Київ",
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//        }

//        [Test]
//        public async Task GetByFilter_WhenSearchTextIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 2;
//            var filter = new WorkshopFilter()
//            {
//                SearchText = "діти",
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => x.Keywords.Contains(filter.SearchText)));
//        }

//        [Test]
//        public async Task GetByFilter_WhenAgeIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 3;
//            var filter = new WorkshopFilter()
//            {
//                MinAge = 3,
//                MaxAge = 6,
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => x.MaxAge >= filter.MinAge));
//            Assert.IsTrue(result.Entities.All(x => x.MinAge <= filter.MaxAge));
//        }

//        [Test]
//        public async Task GetByFilter_WhenDirectionIdsIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 4;
//            var filter = new WorkshopFilter()
//            {
//                DirectionIds = new List<long>() { 1 },
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => x.DirectionId == 1));
//        }

//        [Test]
//        public async Task GetByFilter_WhenWithDisabilityOptionsIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 3;
//            var filter = new WorkshopFilter()
//            {
//                WithDisabilityOptions = true,
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => x.WithDisabilityOptions));
//        }

//        [Test]
//        public async Task GetByFilter_WhenIsFreeIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 2;
//            var filter = new WorkshopFilter()
//            {
//                IsFree = true,
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => x.Price == 0));
//        }

//        [Test]
//        public async Task GetByFilter_WhenMinPriceIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 2;
//            var filter = new WorkshopFilter()
//            {
//                MinPrice = 100,
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => x.Price >= filter.MinPrice));
//        }

//        [Test]
//        public async Task GetByFilter_WhenMaxPriceIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 4;
//            var filter = new WorkshopFilter()
//            {
//                MaxPrice = 100,
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => x.Price <= filter.MaxPrice));
//        }

//        [Test]
//        public async Task GetByFilter_WhenMinMaxPriceIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 2;
//            var filter = new WorkshopFilter()
//            {
//                MinPrice = 50,
//                MaxPrice = 100,
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => x.Price >= filter.MinPrice && x.Price <= filter.MaxPrice));
//        }

//        [Test]
//        public async Task GetByFilter_WhenIsFreeMinMaxPriceIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 3;
//            var filter = new WorkshopFilter()
//            {
//                IsFree = true,
//                MinPrice = 100,
//                MaxPrice = 500,
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => (x.Price >= filter.MinPrice && x.Price <= filter.MaxPrice) || x.Price == 0));
//        }

//        [Test]
//        public async Task GetByFilter_WhenMultipleParamsAreSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 1;
//            var filter = new WorkshopFilter()
//            {
//                City = "Чернігів",
//                SearchText = "танці",
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.IsTrue(result.Entities.All(x => (x.Price >= filter.MinPrice && x.Price <= filter.MaxPrice) || x.Price == 0));
//            Assert.IsTrue(result.Entities.All(x => x.Keywords.Contains(filter.SearchText)));
//        }

//        [Test]
//        public async Task GetByFilter_WhenFilterIsSet_ShouldReturnValidResults()
//        {
//            // Arrange
//            var expected = 3;
//            var filter = new WorkshopFilter()
//            {
//                City = "Київ",
//                OrderByField = OrderBy.Alphabet.ToString(),
//            };

//            // Act
//            var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected, result.TotalAmount);
//            Assert.AreEqual("Вишивання", result.Entities.First().Title);
//        }

//        #endregion

//        private void SeedDatabase()
//        {
//            newWorkshop = new Workshop()
//            {
//                Id = 0,
//                Title = "Title",
//                Phone = "1111111111",
//                Description = "Desc",
//                Price = 1000,
//                WithDisabilityOptions = true,
//                Head = "Head",
//                HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                ProviderTitle = "ProviderTitle",
//                DisabilityOptionsDesc = "Desc",
//                Website = "website",
//                Instagram = "insta",
//                Facebook = "facebook",
//                Email = "email1@gmail.com",
//                MaxAge = 10,
//                MinAge = 4,
//                Logo = "image",
//                ProviderId = 6,
//                DirectionId = 1,
//                ClassId = 1,
//                DepartmentId = 1,
//                AddressId = 0,
//                Address = new Address
//                {
//                    Region = "Region",
//                    District = "District",
//                    City = "City",
//                    Street = "Street",
//                    BuildingNumber = "BuildingNumber",
//                    Latitude = 0,
//                    Longitude = 0,
//                },
//                Teachers = new List<Teacher>
//                {
//                    new Teacher
//                    {
//                        FirstName = "Alex",
//                        LastName = "Brown",
//                        MiddleName = "SomeMiddleName",
//                        Description = "Description",
//                        Image = "Image",
//                        DateOfBirth = DateTime.Parse("2000-01-01"),
//                        WorkshopId = 0,
//                    },
//                    new Teacher
//                    {
//                        FirstName = "John",
//                        LastName = "Snow",
//                        MiddleName = "SomeMiddleName",
//                        Description = "Description",
//                        Image = "Image",
//                        DateOfBirth = DateTime.Parse("2000-01-01"),
//                        WorkshopId = 0,
//                    },
//                },
//            };
//            classEntity = new Class()
//            {
//                Id = 1,
//                Title = "new SSC",
//                DepartmentId = 1,
//                Department = new Department()
//                {
//                    Id = 1,
//                    Title = "new SC",
//                    DirectionId = 1,
//                    Direction = new Direction()
//                    {
//                        Id = 1,
//                        Title = "new C",
//                    },
//                },
//            };

//            using var context = new OutOfSchoolDbContext(options);
//            {
//                context.Database.EnsureDeleted();
//                context.Database.EnsureCreated();

//                var workshops = new List<Workshop>()
//                {
//                    new Workshop()
//                    {
//                        Id = 1,
//                        Title = "Шаффл",
//                        Phone = "1111111111",
//                        Description = "Танці",
//                        Keywords = "шаффл",
//                        Price = 0,
//                        WithDisabilityOptions = true,
//                        Head = "Head1",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "Школа танцю",
//                        DisabilityOptionsDesc = "пандус",
//                        Website = "website1",
//                        Instagram = "insta1",
//                        Facebook = "facebook1",
//                        Email = "email1@gmail.com",
//                        MaxAge = 6,
//                        MinAge = 3,
//                        Logo = "image1",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        ClassId = 1,
//                        DepartmentId = 1,
//                        AddressId = 55,
//                        Address = new Address
//                        {
//                            Id = 55,
//                            Region = "Київ",
//                            District = "Київ55",
//                            City = "Київ",
//                            Street = "Street55",
//                            BuildingNumber = "BuildingNumber55",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = TeachersGenerator.Generate(2).WithWorkshopId(1),
//                    },
//                    new Workshop()
//                    {
//                        Id = 2,
//                        Title = "Шаффл для дорослих",
//                        Phone = "1111111111",
//                        Description = "Танці",
//                        Keywords = "танці¤діти¤шаффл",
//                        Price = 50,
//                        WithDisabilityOptions = false,
//                        Head = "Head2",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "Школа танцю",
//                        DisabilityOptionsDesc = "Desc2",
//                        Website = "website2",
//                        Instagram = "insta2",
//                        Facebook = "facebook2",
//                        Email = "email2@gmail.com",
//                        MaxAge = 10,
//                        MinAge = 7,
//                        Logo = "image2",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        DepartmentId = 1,
//                        AddressId = 10,
//                        Address = new Address
//                        {
//                            Id = 10,
//                            Region = "Region10",
//                            District = "District10",
//                            City = "Чернігів",
//                            Street = "Street10",
//                            BuildingNumber = "BuildingNumber10",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = TeachersGenerator.Generate(2).WithWorkshopId(2),
//                    },
//                    new Workshop()
//                    {
//                        Id = 3,
//                        Title = "Футбол",
//                        Phone = "1111111111",
//                        Description = "гра з м'ячем",
//                        Keywords = "м'яч¤біг¤ноги¤діти¤дорослі¤спорт",
//                        Price = 100,
//                        WithDisabilityOptions = true,
//                        Head = "Head3",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "ProviderTitle",
//                        Website = "website3",
//                        Instagram = "insta3",
//                        Facebook = "facebook3",
//                        Email = "email3@gmail.com",
//                        MaxAge = 10,
//                        MinAge = 7,
//                        Logo = "image3",
//                        ProviderId = 3,
//                        DirectionId = 1,
//                        DepartmentId = 1,
//                        AddressId = 11,
//                        Address = new Address
//                        {
//                            Id = 11,
//                            Region = "Region11",
//                            District = "District11",
//                            City = "Київ",
//                            Street = "Street11",
//                            BuildingNumber = "BuildingNumber11",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = TeachersGenerator.Generate(2).WithWorkshopId(3),
//                    },
//                    new Workshop()
//                    {
//                        Id = 4,
//                        Title = "Футбол",
//                        Phone = "1111111111",
//                        Description = "Desc4",
//                        Keywords = "м'яч¤біг¤спорт",
//                        Price = 1000,
//                        WithDisabilityOptions = false,
//                        Head = "Head4",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "ProviderTitle",
//                        DisabilityOptionsDesc = "Desc4",
//                        Website = "website4",
//                        Instagram = "insta4",
//                        Facebook = "facebook4",
//                        Email = "email4@gmail.com",
//                        MaxAge = 6,
//                        MinAge = 3,
//                        Logo = "image4",
//                        ProviderId = 4,
//                        DirectionId = 1,
//                        DepartmentId = 1,
//                        AddressId = 15,
//                        Address = new Address
//                        {
//                            Id = 15,
//                            Region = "Region15",
//                            District = "District15",
//                            City = "Чернігів",
//                            Street = "Street15",
//                            BuildingNumber = "BuildingNumber15",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = TeachersGenerator.Generate(2).WithWorkshopId(4),
//                    },
//                    new Workshop()
//                    {
//                        Id = 5,
//                        Title = "Вишивання",
//                        Phone = "1111111111",
//                        Description = "Desc5",
//                        Keywords = "рукоділля¤вишивка",
//                        Price = 0,
//                        WithDisabilityOptions = true,
//                        Head = "Head5",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "ProviderTitle",
//                        DisabilityOptionsDesc = "Desc5",
//                        Website = "website5",
//                        Instagram = "insta5",
//                        Facebook = "facebook5",
//                        Email = "email5@gmail.com",
//                        MaxAge = 100,
//                        MinAge = 5,
//                        Logo = "image5",
//                        ProviderId = 5,
//                        DirectionId = 2,
//                        DepartmentId = 1,
//                        AddressId = 17,
//                        Address = new Address
//                        {
//                            Id = 17,
//                            Region = "Region17",
//                            District = "District17",
//                            City = "Київ",
//                            Street = "Street17",
//                            BuildingNumber = "BuildingNumber17",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = TeachersGenerator.Generate(2).WithWorkshopId(5),
//                    },
//                };

//                var apps = new List<Application>()
//                {
//                    new Application() { Id = Guid.NewGuid(), WorkshopId = 1 },
//                    new Application() { Id = Guid.NewGuid(), WorkshopId = 1 },
//                    new Application() { Id = Guid.NewGuid(), WorkshopId = 2 },
//                    new Application() { Id = Guid.NewGuid(), WorkshopId = 2 },
//                };

//                context.Applications.AddRangeAsync(apps);

//                var directions = new List<Direction>() { new Direction() { Title = "Direction1" }, new Direction() { Title = "Direction2" } };
//                context.Directions.AddRangeAsync(directions);
//                var departments = new List<Department>() { new Department() { Title = "new1", DirectionId = 1 }, new Department() { Title = "new2", DirectionId = 1 } };
//                context.Departments.AddRangeAsync(departments);
//                var classes = new List<Class>() { new Class() { Title = "new1", DepartmentId = 1 }, new Class() { Title = "new2", DepartmentId = 1 } };
//                context.Classes.AddRangeAsync(classes);

//                context.Workshops.AddRangeAsync(workshops);
//                context.SaveChangesAsync();
//            }
//        }
//    }
//}
