using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class CityControllerTests
    {
        private const int OkStatusCode = 200;
        private const int CreateStatusCode = 201;
        private const int NoContentStatusCode = 204;
        private const int BadRequestStatusCode = 400;

        private CityController controller;
        private Mock<ICityService> service;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<CityDto> cities;
        private CityDto city;

        [SetUp]
        public void Setup()
        {
            service = new Mock<ICityService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new CityController(service.Object, localizer.Object);

            cities = FakeCities();
            city = FakeCity();
        }

        [Test]
        public async Task GetCities_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(cities);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        public async Task GetCities_WhenEmptyCollection_ReturnsNoContentResult()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(new List<CityDto>());

            // Act
            var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NoContentStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetCityById_WhenIdIsValid_ReturnOkResultObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(cities.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase(-100)]
        public void GetCityById_WhenIdIsInvalid_ReturnsArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(cities.SingleOrDefault(x => x.Id == id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(100)]
        public async Task GetCityById_WhenIdIsNotValid_ReturnsEmptyObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(cities.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Value);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase("NoN")]
        public async Task GetCityByName_WhenNameIsValid_ReturnOkResultObject(string name)
        {
            // Arrange
            service.Setup(x => x.GetByCityName(name)).ReturnsAsync(cities.Where(x => x.Name.Contains(name)));

            // Act
            var result = await controller.GetByName(name).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase("")]
        public async Task GetCityByName_WhenNameIsNotValid_ReturnBadRequestResult(string name)
        {
            // Arrange
            service.Setup(x => x.GetByCityName(name)).ReturnsAsync(cities.Where(x => x.Name.Contains(name)));

            // Act
            var result = await controller.GetByName(name).ConfigureAwait(false) as BadRequestResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(BadRequestStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase("Test")]
        public async Task GetCityByName_WhenNameNotExist_ReturnsNoContentResult(string name)
        {
            // Arrange
            service.Setup(x => x.GetByCityName(name)).ReturnsAsync(cities.Where(x => x.Name.Contains(name)));

            // Act
            var result = await controller.GetByName(name).ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NoContentStatusCode, result.StatusCode);
        }

        [Test]
        public async Task CreateCity_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(city)).ReturnsAsync(city);

            // Act
            var result = await controller.Create(city).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(CreateStatusCode, result.StatusCode);
        }

        [Test]
        public async Task UpdateCity_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            service.Setup(x => x.Update(city)).ReturnsAsync(city);

            // Act
            var result = await controller.Update(city).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(OkStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteCity_WhenIdIsValid_ReturnsNoContentResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as NoContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(NoContentStatusCode, result.StatusCode);
        }

        [Test]
        [TestCase(-100)]
        public void DeleteCity_WhenIdIsInvalid_ReturnsArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.Delete(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteCity_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as OkObjectResult;

            // Assert
            Assert.IsNull(result);
        }

        private CityDto FakeCity()
        {
            return new CityDto()
            {
                Id = 1,
                Name = "Test",
                Region = "Test",
                District = "Test",
            };
        }

        private IEnumerable<CityDto> FakeCities()
        {
            return new List<CityDto>()
            {
                new CityDto()
                {
                    Id = 1,
                    Name = "NoName",
                    Region = "Test",
                    District = "Test",
                },
                new CityDto()
                {
                    Id = 2,
                    Name = "HaveName",
                    Region = "Test",
                    District = "Test",
                },
                new CityDto()
                {
                    Id = 3,
                    Name = "MissName",
                    Region = "Test",
                    District = "Test",
                },
            };
        }
    }
}
