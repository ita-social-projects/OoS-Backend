using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Official;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Controllers.V1;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class OfficialControllerTests
{
    private OfficialController controller;
    private Guid providerId;
    private Mock<IOfficialService> service;

    [SetUp]
    public void SetUp()
    {
        providerId = Guid.NewGuid();
        service = new Mock<IOfficialService>();

        controller = new OfficialController(service.Object);
    }

    [Test]
    public async Task Get_ReturnsNoContent_WhenListIsEmpty()
    {
        // Arrange
        var emptyResult = new SearchResult<OfficialDto>();
        service.Setup(s => s.GetByFilter(providerId, null)).ReturnsAsync(emptyResult);

        // Act
        var result = await controller.Get(providerId, null).ConfigureAwait(false) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public async Task Get_ReturnsOk_WhenListIsNotEmpty()
    {
        // Arrange
        var searchResult = new SearchResult<OfficialDto>()
        {
            Entities = new List<OfficialDto>
            {
                FakeOfficialDto(),
                FakeOfficialDto(),
                FakeOfficialDto()
            },
            TotalAmount = 3
        };
        service.Setup(s => s.GetByFilter(providerId, null)).ReturnsAsync(searchResult);

        // Act
        var result = await controller.Get(providerId, null).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));

        var returnedSearchResult = result.Value as SearchResult<OfficialDto>;
        Assert.That(returnedSearchResult, Is.Not.Null);
        Assert.That(returnedSearchResult.TotalAmount, Is.EqualTo(searchResult.TotalAmount));
        Assert.That(returnedSearchResult.Entities, Is.EqualTo(searchResult.Entities));
    }

    private OfficialDto FakeOfficialDto()
    {
        return new OfficialDto()
        {
            ActiveFrom = DateOnly.FromDateTime(DateTime.Now),
            ActiveTo = DateOnly.FromDateTime(DateTime.Now),
            Id = Guid.NewGuid(),
            EmploymentType = EmploymentType.PartTime,
            Individual = new OfficialIndividualDto()
            {
                FirstName = "Test",
                LastName = "Testov",
                MiddleName = "Testovich",
                Rnokpp = "1234567890"
            },
            Position = new OfficialPositionDto()
            {
                Id = Guid.NewGuid(),
                FullName = "Test"
            }
        };
    }
}
