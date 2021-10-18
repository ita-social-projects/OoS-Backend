﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;

//using AutoMapper;

//using Microsoft.Extensions.Logging;

//using Moq;

//using NUnit.Framework;

//using OutOfSchool.Services.Models;
//using OutOfSchool.Services.Repository;
//using OutOfSchool.Tests.Common.TestDataGenerators;
//using OutOfSchool.WebApi.Extensions;
//using OutOfSchool.WebApi.Models;
//using OutOfSchool.WebApi.Services;

//namespace OutOfSchool.WebApi.Tests.Services.UnitTests
//{
//    [TestFixture]
//    public class WorkshopServiceUnitTests
//    {
//        private IWorkshopService workshopService;

//        private Mock<IWorkshopRepository> workshopRepositoryMoq;
//        private Mock<IClassRepository> classRepositoryMoq;

//        private Mock<IRatingService> ratingService;
//        private Mock<ILogger<WorkshopService>> logger;
//        private Mock<IMapper> mapperMoq;

//        private Workshop newWorkshop;
//        private List<Workshop> workshops;
//        private Class classEntity;

//        [SetUp]
//        public void SetUp()
//        {
//            workshopRepositoryMoq = new Mock<IWorkshopRepository>();
//            classRepositoryMoq = new Mock<IClassRepository>();
//            ratingService = new Mock<IRatingService>();
//            logger = new Mock<ILogger<WorkshopService>>();
//            mapperMoq = new Mock<IMapper>();

//            workshopService = new WorkshopService(
//                workshopRepositoryMoq.Object,
//                classRepositoryMoq.Object,
//                ratingService.Object,
//                logger.Object,
//                mapperMoq.Object);

//            FakeEntities();
//        }

//        #region Create
//        [Test]
//        public async Task Create_WhenEntityIsValid_ShouldRunInTransaction()
//        {
//            // Arrange
//            classRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
//                .ReturnsAsync(classEntity);
//            workshopRepositoryMoq.Setup(x => x.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
//                .ReturnsAsync(newWorkshop);

//            // Act
//            var result = await workshopService.Create(newWorkshop.ToModel()).ConfigureAwait(false);

//            // Assert
//            Assert.Multiple(() =>
//            {
//                workshopRepositoryMoq.Verify(x => x.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()), Times.Once());
//            });
//        }
//        #endregion

//        #region GetAll
//        [Test]
//        public async Task GetAll_WhenCalled_ShouldReturnAllEntities()
//        {
//            // Arrange
//            var queryable = new EnumerableQuery<Workshop>(workshops);
//            workshopRepositoryMoq.Setup(x => x.Count(It.IsAny<Expression<Func<Workshop, bool>>>())).ReturnsAsync(workshops.Count);
//            workshopRepositoryMoq.Setup(x => x.Get<long>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<Expression<Func<Workshop, long>>>(), It.IsAny<bool>()))
//                .Returns(queryable);

//            // Act
//            var result = await workshopService.GetAll(new OffsetFilter()).ConfigureAwait(false);

//            // Assert
//            workshopRepositoryMoq.Verify(x => x.Get<long>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<Expression<Func<Workshop, long>>>(), It.IsAny<bool>()), Times.Once);
//            Assert.AreEqual(workshops.Count(), result.TotalAmount);
//            Assert.AreEqual(workshops.First().Id, result.Entities.First().Id);
//        }
//        #endregion

//        #region GetById
//        [TestCase(1)]
//        public async Task GetById_WhenIdIsValid_ShouldReturnEntity(long id)
//        {
//            // Arrange
//            workshopRepositoryMoq.Setup(x => x.GetById(id))
//                .ReturnsAsync(workshops.First());

//            // Act
//            var result = await workshopService.GetById(id).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(workshops.First().Id, result.Id);
//        }

//        [TestCase(10)]
//        public async Task GetById_WhenThereIsNoEntityWithId_ShouldReturnNull(long id)
//        {
//            // Arrange
//            workshopRepositoryMoq.Setup(x => x.GetById(id))
//                .ReturnsAsync(() => null);

//            // Act
//            var result = await workshopService.GetById(id).ConfigureAwait(false);

//            // Assert
//            Assert.IsNull(result);
//        }
//        #endregion

//        #region GetWorkshopsByOrganization
//        [TestCase(1)]
//        public async Task GetWorkshopsByOrganization_WhenIdIsValid_ShouldReturnEntities(long id)
//        {
//            // Arrange
//            workshopRepositoryMoq.Setup(z => z.GetByFilter(x => x.ProviderId == id, It.IsAny<string>()))
//                .ReturnsAsync(workshops);

//            // Act
//            var result = await workshopService.GetByProviderId(id).ConfigureAwait(false);

//            // Assert
//            Assert.Multiple(() =>
//            {
//                workshopRepositoryMoq.Verify(x => x.GetByFilter(x => x.ProviderId == id, It.IsAny<string>()), Times.Once());
//                Assert.That(workshops.Count(), Is.EqualTo(result.Count()));
//            });
//        }

//        [TestCase(10)]
//        public async Task GetWorkshopsByOrganization_WhenThereIsNoWorkshop_ShouldReturnEmptyList(long id)
//        {
//            // Arrange
//            var emptyList = new List<Workshop>();
//            workshopRepositoryMoq.Setup(z => z.GetByFilter(x => x.ProviderId == id, It.IsAny<string>()))
//                .ReturnsAsync(emptyList);

//            // Act
//            var result = await workshopService.GetByProviderId(id).ConfigureAwait(false);

//            // Assert
//            Assert.Multiple(() =>
//                {
//                    workshopRepositoryMoq.Verify(x => x.GetByFilter(x => x.ProviderId == id, It.IsAny<string>()), Times.Once());
//                    Assert.That(emptyList.Count(), Is.EqualTo(result.Count()));
//                });
//        }
//        #endregion

//        #region Update
//        [Test]
//        public async Task Update_WhenEntityIsValid_ShouldRunInTransaction([Random(2, 5, 1)] int teachersNumber)
//        {
//            // Arrange
//            var changedFirstEntity = new Workshop()
//            {
//                Id = 1,
//                Title = "ChangedTitle",
//                Phone = "1111111111",
//                Description = "Desc1",
//                Price = 1000,
//                WithDisabilityOptions = true,
//                Head = "Head1",
//                HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                ProviderTitle = "ProviderTitle",
//                DisabilityOptionsDesc = "Desc1",
//                Website = "website1",
//                Instagram = "insta1",
//                Facebook = "facebook1",
//                Email = "email1@gmail.com",
//                MaxAge = 10,
//                MinAge = 4,
//                Logo = "image1",
//                ProviderId = 1,
//                DirectionId = 1,
//                ClassId = 1,
//                DepartmentId = 1,
//                AddressId = 55,
//                Address = AddressGenerator.Generate(),
//            };
//            changedFirstEntity.Teachers = TeachersGenerator.Generate(teachersNumber).WithWorkshopId(changedFirstEntity.Id);

//            IQueryable<Workshop> quer = new EnumerableQuery<Workshop>(workshops);
//            workshopRepositoryMoq.Setup(z => z.GetByFilterNoTracking(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
//                .Returns(quer);
//            classRepositoryMoq.Setup(x => x.GetById(It.IsAny<long>()))
//                .ReturnsAsync(classEntity);

//            workshopRepositoryMoq.Setup(x => x.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
//                .ReturnsAsync(changedFirstEntity);

//            // Act
//            var result = await workshopService.Update(changedFirstEntity.ToModel()).ConfigureAwait(false);

//            // Assert
//            Assert.Multiple(() =>
//            {
//                workshopRepositoryMoq.Verify(x => x.GetByFilterNoTracking(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()), Times.Once());
//                classRepositoryMoq.Verify(x => x.GetById(It.IsAny<long>()), Times.Once());
//                workshopRepositoryMoq.Verify(x => x.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()), Times.Once());

//                Assert.IsNotNull(result);
//                Assert.IsInstanceOf<WorkshopDTO>(result);
//            });
//        }
//        #endregion

//        #region Delete
//        [TestCase(1)]
//        public async Task Delete_WhenIdIsValid_ShouldRunInTransactionDelete(long id)
//        {
//            // Arrange
//            var moqObject = new Workshop() { Id = default };
//            workshopRepositoryMoq.Setup(x => x.GetById(id))
//                .ReturnsAsync(workshops.First());
//            workshopRepositoryMoq.Setup(x => x.Delete(It.IsAny<Workshop>()))
//                .Returns(Task.CompletedTask);
//            workshopRepositoryMoq.Setup(x => x.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()))
//                .ReturnsAsync(moqObject);

//            // Act
//            await workshopService.Delete(id).ConfigureAwait(false);

//            // Assert
//            Assert.Multiple(() =>
//            {
//                workshopRepositoryMoq.Verify(x => x.GetById(id), Times.Once());
//                workshopRepositoryMoq.Verify(x => x.RunInTransaction(It.IsAny<Func<Task<Workshop>>>()), Times.Once());
//            });
//        }

//        [TestCase(1)]
//        public void Delete_WhenIdIsInvalid_ShouldThrowNullReferenceException(long id)
//        {
//            // Arrange
//            workshopRepositoryMoq.Setup(x => x.GetById(id))
//                .Returns(() => null);

//            // Act and Assert
//            Assert.That(
//                async () => await workshopService.Delete(id).ConfigureAwait(false),
//                Throws.Exception.TypeOf<NullReferenceException>());
//        }
//        #endregion

//        private void FakeEntities()
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
//                ProviderId = 1,
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
//                Teachers = TeachersGenerator.Generate(2).WithWorkshopId(0),
//            };
//            workshops =
//                new List<Workshop>()
//                {
//                    new Workshop()
//                    {
//                        Id = 1,
//                        Title = "Title1",
//                        Phone = "1111111111",
//                        Description = "Desc1",
//                        Price = 1000,
//                        WithDisabilityOptions = true,
//                        Head = "Head1",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "ProviderTitle",
//                        DisabilityOptionsDesc = "Desc1",
//                        Website = "website1",
//                        Instagram = "insta1",
//                        Facebook = "facebook1",
//                        Email = "email1@gmail.com",
//                        MaxAge = 10,
//                        MinAge = 4,
//                        Logo = "image1",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        ClassId = 1,
//                        DepartmentId = 1,
//                        AddressId = 55,
//                        Address = AddressGenerator.Generate(),
//                        Teachers = TeachersGenerator.Generate(2).WithWorkshopId(1),
//                    },
//                    new Workshop()
//                    {
//                        Id = 2,
//                        Title = "Title2",
//                        Phone = "1111111111",
//                        Description = "Desc2",
//                        Price = 2000,
//                        WithDisabilityOptions = true,
//                        Head = "Head2",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "ProviderTitle",
//                        DisabilityOptionsDesc = "Desc2",
//                        Website = "website2",
//                        Instagram = "insta2",
//                        Facebook = "facebook2",
//                        Email = "email2@gmail.com",
//                        MaxAge = 10,
//                        MinAge = 4,
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
//                            City = "City10",
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
//                        Title = "Title3",
//                        Phone = "1111111111",
//                        Description = "Desc3",
//                        Price = 3000,
//                        WithDisabilityOptions = true,
//                        Head = "Head3",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "ProviderTitle",
//                        Website = "website3",
//                        Instagram = "insta3",
//                        Facebook = "facebook3",
//                        Email = "email3@gmail.com",
//                        MaxAge = 10,
//                        MinAge = 4,
//                        Logo = "image3",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        DepartmentId = 1,
//                        AddressId = 11,
//                        Address = new Address
//                        {
//                            Id = 11,
//                            Region = "Region11",
//                            District = "District11",
//                            City = "City11",
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
//                        Title = "Title4",
//                        Phone = "1111111111",
//                        Description = "Desc4",
//                        Price = 4000,
//                        WithDisabilityOptions = true,
//                        Head = "Head4",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "ProviderTitle",
//                        DisabilityOptionsDesc = "Desc4",
//                        Website = "website4",
//                        Instagram = "insta4",
//                        Facebook = "facebook4",
//                        Email = "email4@gmail.com",
//                        MaxAge = 10,
//                        MinAge = 4,
//                        Logo = "image4",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        DepartmentId = 1,
//                        AddressId = 15,
//                        Address = new Address
//                        {
//                            Id = 15,
//                            Region = "Region15",
//                            District = "District15",
//                            City = "City15",
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
//                        Title = "Title5",
//                        Phone = "1111111111",
//                        Description = "Desc5",
//                        Price = 5000,
//                        WithDisabilityOptions = true,
//                        Head = "Head5",
//                        HeadDateOfBirth = new DateTime(1980, month: 12, 28),
//                        ProviderTitle = "ProviderTitle",
//                        DisabilityOptionsDesc = "Desc5",
//                        Website = "website5",
//                        Instagram = "insta5",
//                        Facebook = "facebook5",
//                        Email = "email5@gmail.com",
//                        MaxAge = 10,
//                        MinAge = 4,
//                        Logo = "image5",
//                        ProviderId = 1,
//                        DirectionId = 1,
//                        DepartmentId = 1,
//                        AddressId = 17,
//                        Address = new Address
//                        {
//                            Id = 17,
//                            Region = "Region17",
//                            District = "District17",
//                            City = "City17",
//                            Street = "Street17",
//                            BuildingNumber = "BuildingNumber17",
//                            Latitude = 0,
//                            Longitude = 0,
//                        },
//                        Teachers = TeachersGenerator.Generate(2).WithWorkshopId(5),
//                    },
//                };

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
//        }
//    }
//}