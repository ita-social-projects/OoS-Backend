﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
using OutOfSchool.BusinessLogic.Services.Memento.Models;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class WorkshopMementoControllerTests
{
    private WorkshopMementoController controller;
    private Mock<IMementoService<RequiredWorkshopMemento>> mementoService;
    private ClaimsPrincipal user;

    private IEnumerable<RequiredWorkshopMemento> mementos;
    private RequiredWorkshopMemento memento;

    [SetUp]
    public void Setup()
    {
        mementoService = new Mock<IMementoService<RequiredWorkshopMemento>>();
        controller = new WorkshopMementoController(mementoService.Object);
        user = new ClaimsPrincipal(new ClaimsIdentity());
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        memento = FakeMemento();
    }

    [Test]
    public async Task RestoreMemento_WhenMementoExistsInCache_ReturnsMementoAtActionResult()
    {
        // Arrange
        mementoService.Setup(rm => rm.RestoreMemento(It.IsAny<string>())).ReturnsAsync(memento);

        // Act
        var result = await controller.RestoreMemento();

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().NotBe(default(string));
    }

    [Test]
    public async Task RestoreMemento_WhenMementoIsAbsentInCache_ReturnsDefaultMementoAtActionResult()
    {
        // Arrange
        mementoService.Setup(ms => ms.RestoreMemento(It.IsAny<string>())).ReturnsAsync(default(RequiredWorkshopMemento));

        // Act
        var result = await controller.RestoreMemento();

        // Assert
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(default(string));
    }

    private RequiredWorkshopMemento FakeMemento()
    {
        return new RequiredWorkshopMemento()
        {
            Title = "title1",
            Email = "myemail1@gmail.com",
            Phone = "+380670000001",
        };
    }
}