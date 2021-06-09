﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class WorkshopControllerTests
    {
        private WorkshopController controller;
        private Mock<IWorkshopService> service;
        private Mock<IEntityRepository<Workshop>> repo;

        private IEnumerable<WorkshopDTO> workshops;
        private WorkshopDTO workshop;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        [SetUp]
        public void Setup()
        {
            repo = new Mock<IEntityRepository<Workshop>>();
            service = new Mock<IWorkshopService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new WorkshopController(service.Object, localizer.Object);
            workshops = FakeWorkshops();
            workshop = FakeWorkshop();
        }

        [Test]
        public async Task GetWorkshops_WhenCalled_ShouldReturnOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(workshops);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetWorkshopById_WhenIdIsValid_ShouldReturnOkResultObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(workshops.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(0)]
        public void GetWorkshopById_WhenIdIsInvalid_ShouldThrowArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(workshops.SingleOrDefault(x => x.Id == id));

            // Assert
            Assert.That(
                async () => await controller.GetById(id),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase(10)]
        public async Task GetWorkshopById_WhenIdIsInvalid_ShouldReturnNull(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(workshops.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task CreateWorkshop_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(workshop)).ReturnsAsync(workshop);

            // Act
            var result = await controller.Create(workshop).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(201, result.StatusCode);
        }

        [Test]
        public async Task CreateWorkshop_WhenModelIsInvalid_ShouldReturnBadRequestObjectResult()
        {
            // Arrange
            var newWorkshop = new WorkshopDTO()
            {
                Title = string.Empty,
                Price = 2000,
                Description = "description",
            };

            service.Setup(x => x.Create(newWorkshop)).ReturnsAsync(newWorkshop);
            controller.ModelState.AddModelError("CreateWorkshop", "Invalid model state.");

            // Act
            var result = await controller.Create(newWorkshop).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateWorkshop_WhenModelIsValid_ShouldReturnOkObjectResult()
        {
            // Arrange
            var changedWorkshop = new WorkshopDTO()
            {
                Id = 1,
                Title = "ChangedTitle",
            };
            service.Setup(x => x.Update(changedWorkshop)).ReturnsAsync(changedWorkshop);

            // Act
            var result = await controller.Update(changedWorkshop).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task UpdateWorkshop_WhenModelIsInvalid_ShouldReturnBadRequestObjectResult()
        {
            // Arrange
            var newWorkshop = new WorkshopDTO()
            {
                Id = 1,
                Title = string.Empty,
            };

            service.Setup(x => x.Update(newWorkshop)).ReturnsAsync(newWorkshop);
            controller.ModelState.AddModelError("CreateWorkshop", "Invalid model state.");

            // Act
            var result = await controller.Update(newWorkshop).ConfigureAwait(false);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteWorkshop_WhenIdIsValid_ShouldReturnNoContentResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(0)]
        public void DeleteWorkshop_WhenIdIsInvalid_ShouldReturnBadRequestObjectResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Assert
            Assert.That(
                async () => await controller.Delete(id),
                Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteWorkshop_WhenIdIsInvalid_ShouldReturnNull(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        private WorkshopDTO FakeWorkshop()
        {
            return new WorkshopDTO()
            {
                Id = 6,
                Title = "Title6",
                Phone = "1111111111",
                Description = "Desc6",
                Price = 6000,
                WithDisabilityOptions = true,
                DaysPerWeek = 6,
                Head = "Head6",
                HeadDateOfBirth = new DateTime(1980, month: 12, 28),
                DisabilityOptionsDesc = "Desc6",
                Website = "website6",
                Instagram = "insta6",
                Facebook = "facebook6",
                Email = "email6@gmail.com",
                MaxAge = 10,
                MinAge = 4,
                Logo = "image6",
                ProviderId = 1,
                CategoryId = 1,
                SubcategoryId = 1,
                AddressId = 55,
                Address = new AddressDto
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
                Teachers = new List<TeacherDTO>
                {
                    new TeacherDTO
                    {
                        Id = 1,
                        FirstName = "Alex",
                        LastName = "Brown",
                        MiddleName = "SomeMiddleName",
                        Description = "Description",
                        Image = "Image",
                    },
                    new TeacherDTO
                    {
                        Id = 2,
                        FirstName = "John",
                        LastName = "Snow",
                        MiddleName = "SomeMiddleName",
                        Description = "Description",
                        Image = "Image",
                    },
                },
            };
        }

        private IEnumerable<WorkshopDTO> FakeWorkshops()
        {
            return new List<WorkshopDTO>()
            {
                new WorkshopDTO()
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
                    DisabilityOptionsDesc = "Desc1",
                    Website = "website1",
                    Instagram = "insta1",
                    Facebook = "facebook1",
                    Email = "email1@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    Logo = "image1",
                },
                new WorkshopDTO()
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
                    DisabilityOptionsDesc = "Desc2",
                    Website = "website2",
                    Instagram = "insta2",
                    Facebook = "facebook2",
                    Email = "email2@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    Logo = "image2",
                },
                new WorkshopDTO()
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
                    DisabilityOptionsDesc = "Desc3",
                    Website = "website3",
                    Instagram = "insta3",
                    Facebook = "facebook3",
                    Email = "email3@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    Logo = "image3",
                },
                new WorkshopDTO()
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
                    DisabilityOptionsDesc = "Desc4",
                    Website = "website4",
                    Instagram = "insta4",
                    Facebook = "facebook4",
                    Email = "email4@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    Logo = "image4",
                },
                new WorkshopDTO()
                {
                    Id = 5,
                    Title = "Title5",
                    Phone = "1111111111",
                    Description = "Desc5",
                    Price = 5000,
                    WithDisabilityOptions = true,
                    DaysPerWeek = 5,
                    Head = "Head5",
                    HeadDateOfBirth = new DateTime(1980, month: 12, 12),
                    DisabilityOptionsDesc = "Desc5",
                    Website = "website5",
                    Instagram = "insta5",
                    Facebook = "facebook5",
                    Email = "email5@gmail.com",
                    MaxAge = 10,
                    MinAge = 4,
                    Logo = "image5",
                },
            };
        }
    }
}