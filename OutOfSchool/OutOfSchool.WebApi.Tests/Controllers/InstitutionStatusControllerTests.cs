﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class InstitutionStatusControllerTests
{
    private InstitutionStatusController controller;
    private Mock<IStatusService> service;
    private IEnumerable<InstitutionStatus> institutionStatuses;
    private InstitutionStatus institutionStatus;
    private IMapper mapper;

    [SetUp]
    public void Setup()
    {
        // setup controller
        service = new Mock<IStatusService>();
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        controller = new InstitutionStatusController(service.Object, localizer.Object);

        // generate random collection of statuses and single entity to use in test cases.
        institutionStatuses = InstitutionStatusGenerator.Generate(5);
        institutionStatus = InstitutionStatusGenerator.Generate();
    }

    [Test]
    public async Task GetInstitutionStatuses_WhenCalled_ReturnsOkResultObject()
    {
        // Arrange
        var expected = institutionStatuses.Select(x => mapper.Map<InstitutionStatusDTO>(x));

        service.Setup(x => x.GetAll(It.IsAny<LocalizationType>())).ReturnsAsync(institutionStatuses.Select(x => mapper.Map<InstitutionStatusDTO>(x)));

        // Act
        var response = await controller.Get().ConfigureAwait(false);

        // Assert
        response.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetInstitutionStatuses_WhenEmptyCollection_ReturnsNoContentResult()
    {
        // Arrange
        service.Setup(x => x.GetAll(It.IsAny<LocalizationType>())).ReturnsAsync(new List<InstitutionStatusDTO>());

        // Act
        var response = await controller.Get().ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(response);
    }

    [Test]
    public async Task GetInstitutionStatusById_WhenIdIsValid_ReturnOkResultObject()
    {
        // Arrange
        var existingId = TestDataHelper.RandomItem(institutionStatuses as ICollection<InstitutionStatus>).Id;
        var expected = mapper.Map<InstitutionStatusDTO>(institutionStatuses.First(x => x.Id == existingId));

        service.Setup(x => x.GetById(existingId, It.IsAny<LocalizationType>()))
            .ReturnsAsync(mapper.Map<InstitutionStatusDTO>(institutionStatuses.First(x => x.Id == existingId)));

        // Act
        var response = await controller.GetById(existingId).ConfigureAwait(false);

        // Assert
        response.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetInstitutionStatusById_WhenIdIsInvalid_ReturnsBadRequestWithExceptionMessage()
    {
        // Arrange
        var invalidId = TestDataHelper.GetNegativeInt();
        var exceptedResponse = new BadRequestObjectResult(TestDataHelper.GetRandomWords());
        service.Setup(x => x.GetById(invalidId, It.IsAny<LocalizationType>()))
            .ReturnsAsync(mapper.Map<InstitutionStatusDTO>(institutionStatuses.SingleOrDefault(x => x.Id == invalidId)));

        // Act
        var response = await controller.GetById(invalidId).ConfigureAwait(false);

        // Assert
        response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(exceptedResponse);
    }

    [Test]
    public async Task GetById_WhenIdDoesntExist_ReturnsBadRequestWithExceptionMessage()
    {
        // Arrange
        var notExistId = TestDataHelper.GetPositiveInt(institutionStatuses.Count(), int.MaxValue);
        var expectedResponse = new BadRequestObjectResult(TestDataHelper.GetRandomWords());

        service.Setup(x => x.GetById(notExistId, It.IsAny<LocalizationType>())).Throws<ArgumentOutOfRangeException>();

        // Act
        var response = await controller.GetById(notExistId).ConfigureAwait(false);

        // Assert
        response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expectedResponse);
    }

    [Test]
    public async Task CreateInstitutionStatus_WhenModelIsValid_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var expected = mapper.Map<InstitutionStatusDTO>(institutionStatus);
        var expectedResponse = new CreatedAtActionResult(
            nameof(controller.GetById),
            nameof(controller),
            new { id = expected.Id },
            expected);
        service.Setup(x => x.Create(expected)).ReturnsAsync(mapper.Map<InstitutionStatusDTO>(institutionStatus));

        // Act
        var response = await controller.Create(expected).ConfigureAwait(false);

        // Assert
        response.AssertExpectedResponseTypeAndCheckDataInside<CreatedAtActionResult>(expectedResponse);
    }

    [Test]
    public async Task UpdateInstitutionStatus_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        var expected = mapper.Map<InstitutionStatusDTO>(institutionStatus);
        service.Setup(x => x.Update(expected, It.IsAny<LocalizationType>())).ReturnsAsync(mapper.Map<InstitutionStatusDTO>(institutionStatus));

        // Act
        var response = await controller.Update(expected).ConfigureAwait(false);

        // Assert
        response.AssertResponseOkResultAndValidateValue(expected);
    }


    [Test]
    public async Task DeleteInstitutionStatus_WhenIdIsValid_ReturnsNoContentResult()
    {
        // Arrange
        var idToDelete = TestDataHelper.RandomItem(institutionStatuses as ICollection<InstitutionStatus>).Id;
        service.Setup(x => x.Delete(idToDelete));

        // Act
        var response = await controller.Delete(idToDelete);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(response);
    }

    [Test]
    public async Task Delete_WhenIdIsInvalid_ReturnsBadRequestWithExceptionMessageAsync()
    {
        // Arrange
        var invalidId = TestDataHelper.GetNegativeInt();
        var expected = new BadRequestObjectResult(TestDataHelper.GetRandomWords());
        service.Setup(x => x.Delete(invalidId));

        // Act
        var response = await controller.Delete(invalidId).ConfigureAwait(false);

        // Assert
        response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
    }

    [Test]
    public async Task DeleteInstitutionStatus_WhenIdDoesntExists_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var notExistId = TestDataHelper.GetPositiveInt(institutionStatuses.Count(), int.MaxValue);
        var expected = new BadRequestObjectResult(TestDataHelper.GetRandomWords());
        service.Setup(x => x.Delete(notExistId)).Throws<ArgumentOutOfRangeException>();

        // Act
        var response = await controller.Delete(notExistId).ConfigureAwait(false);

        // Assert
        response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
    }
}