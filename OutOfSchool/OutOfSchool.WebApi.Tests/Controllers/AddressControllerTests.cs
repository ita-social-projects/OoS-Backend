using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class AddressControllerTests
{
    private AddressController controller;
    private Mock<IAddressService> service;
    private Mock<IStringLocalizer<SharedResource>> localizer;

    private IEnumerable<AddressDto> addresses;
    private AddressDto address;

    [SetUp]
    public void Setup()
    {
        service = new Mock<IAddressService>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();

        controller = new AddressController(service.Object, localizer.Object);

        addresses = FakeAddresses();
        address = FakeAddress();
    }

    [Test]
    public async Task GetAddresses_WhenCalled_ReturnsOkResultObject()
    {
        // Arrange
        service.Setup(x => x.GetAll()).ReturnsAsync(addresses);

        // Act
        var result = await controller.GetAddresses().ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.StatusCode, 200);
    }

    [Test]
    [TestCase(1)]
    public async Task GetAddressById_WhenIdIsValid_ReturnsOkObjectResult(long id)
    {
        // Arrange
        service.Setup(x => x.GetById(id)).ReturnsAsync(addresses.SingleOrDefault(x => x.Id == id));

        // Act
        var result = await controller.GetAddressById(id).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.StatusCode, 200);
    }

    [Test]
    [TestCase(0)]
    public void GetAddressById_WhenIdIsInvalid_ReturnsBadRequestObjectResult(long id)
    {
        // Arrange
        service.Setup(x => x.GetById(id)).ReturnsAsync(addresses.SingleOrDefault(x => x.Id == id));

        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await controller.GetAddressById(id).ConfigureAwait(false));
    }

    [Test]
    [TestCase(10)]
    public async Task GetAddressById_WhenIdIsTooBig_ReturnsEmptyObject(long id)
    {
        // Arrange
        service.Setup(x => x.GetById(id)).ReturnsAsync(addresses.SingleOrDefault(x => x.Id == id));

        // Act
        var result = await controller.GetAddressById(id).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result.Value, Is.Null);
        Assert.AreEqual(result.StatusCode, 200);
    }

    [Test]
    public async Task CreateAddress_WhenModelIsValid_ReturnsCreatedAtActionResult()
    {
        // Arrange
        service.Setup(x => x.Create(address)).ReturnsAsync(address);

        // Act
        var result = await controller.Create(address).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.StatusCode, 201);
    }

    [Test]
    public async Task CreateAddress_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        controller.ModelState.AddModelError("CreateAddress", "Invalid model state.");

        // Act
        var result = await controller.Create(address).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateAddress_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        var changedAddress = new AddressDto()
        {
            Id = 1,
            CATOTTGId = 4970,
        };
        service.Setup(x => x.Update(changedAddress)).ReturnsAsync(changedAddress);

        // Act
        var result = await controller.Update(changedAddress).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.StatusCode, 200);
    }

    [Test]
    public async Task UpdateAddress_WhenModelIsInvalid_ReturnsBadRequestObjectResult()
    {
        // Arrange
        controller.ModelState.AddModelError("UpdateAddress", "Invalid model state.");

        // Act
        var result = await controller.Update(address).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That((result as BadRequestObjectResult).StatusCode, Is.EqualTo(400));
    }

    [Test]
    [TestCase(1)]
    public async Task DeleteAddress_WhenIdIsValid_ReturnsNoContentResult(long id)
    {
        // Arrange
        service.Setup(x => x.Delete(id));

        // Act
        var result = await controller.Delete(id) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.StatusCode, 204);
    }

    [Test]
    [TestCase(0)]
    public void DeleteAddress_WhenIdIsInvalid_ReturnsBadRequestObjectResult(long id)
    {
        // Arrange
        service.Setup(x => x.Delete(id));

        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await controller.Delete(id).ConfigureAwait(false));
    }

    [Test]
    [TestCase(10)]
    public async Task DeleteAddress_WhenIdIsInvalid_ReturnsNull(long id)
    {
        // Arrange
        service.Setup(x => x.Delete(id));

        // Act
        var result = await controller.Delete(id) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Null);
    }

    private AddressDto FakeAddress()
    {
        return new AddressDto()
        {
            Id = 6,
            CATOTTGId = 4970,
            Street = "Street6",
            BuildingNumber = "BuildingNumber6",
            Latitude = 60.45383,
            Longitude = 65.56765,
        };
    }

    private IEnumerable<AddressDto> FakeAddresses()
    {
        return new List<AddressDto>()
        {
            new AddressDto()
            {
                Id = 1,
                CATOTTGId = 4970,
                Street = "Street1",
                BuildingNumber = "BuildingNumber1",
                Latitude = 41.45383,
                Longitude = 51.56765,
            },
            new AddressDto()
            {
                Id = 2,
                CATOTTGId = 4970,
                Street = "Street2",
                BuildingNumber = "BuildingNumber2",
                Latitude = 42.45383,
                Longitude = 52.56765,
            },
            new AddressDto()
            {
                Id = 3,
                CATOTTGId = 4970,
                Street = "Street3",
                BuildingNumber = "BuildingNumber3",
                Latitude = 43.45383,
                Longitude = 53.56765,
            },
            new AddressDto()
            {
                Id = 4,
                CATOTTGId = 4970,
                Street = "Street4",
                BuildingNumber = "BuildingNumber4",
                Latitude = 44.45383,
                Longitude = 54.56765,
            },
            new AddressDto()
            {
                Id = 5,
                CATOTTGId = 4970,
                Street = "Street5",
                BuildingNumber = "BuildingNumber5",
                Latitude = 45.45383,
                Longitude = 55.56765,
            },
        };
    }
}