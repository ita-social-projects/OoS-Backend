using System.Collections.Generic;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.Memento;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
using OutOfSchool.BusinessLogic.Services.Memento.Models;

namespace OutOfSchool.WebApi.Tests.Services.Memento;

[TestFixture]
public class MementoServiceTests
{
    private Mock<IMemento> mementoMock;
    private IMementoService<RequiredWorkshopMemento> mementoService;

    [SetUp]
    public void SetUp()
    {
        mementoMock = new Mock<IMemento>();
        mementoService = new MementoService<RequiredWorkshopMemento>(mementoMock.Object);
    }

    [Test]
    public void RestoreMemento_WhenMementoExistsInCache_ShouldSetAppropriatedEntityToState()
    {
        // Arrange
        var workshopMemento = new RequiredWorkshopMemento()
        {
            Title = "title",
            Email = "myemail@gmail.com",
            Phone = "+380670000000",
        };

        // Act
        mementoService.RestoreMemento(new KeyValuePair<string, string?>(
            "ExpectedKey",
            "{\"Title\":\"title\",\"Email\":\"myemail@gmail.com\",\"Phone\":\"+380670000000\"}"));

        // Assert
        Assert.AreEqual(workshopMemento.Title, mementoService.State.Title);
        Assert.AreEqual(workshopMemento.Email, mementoService.State.Email);
        Assert.AreEqual(workshopMemento.Phone, mementoService.State.Phone);
    }

    [Test]
    public void RestoreMemento_WhenMementoIsAbsentInCache_ShouldSetDefaultEntityToStateValue()
    {
        // Arrange
        var workshopMemento = default(RequiredWorkshopMemento);

        // Act
        mementoService.RestoreMemento(new KeyValuePair<string, string?>(
            "ExpectedKey",
            null));

        // Assert
        Assert.AreEqual(null, workshopMemento);
    }

    [Test]
    public void CreateMemento_ShouldReturnCreatedMementoInStateProperty()
    {
        // Arrange
        var workshopMemento = new RequiredWorkshopMemento()
        {
            Title = "title",
            Email = "myemail@gmail.com",
            Phone = "+380670000000",
        };
        var expectedValue = JsonConvert.SerializeObject(workshopMemento);
        mementoMock.Setup(c => c.State).Returns(
            new KeyValuePair<string, string?>(
                mementoService.GetMementoKey("ExpectedKey"),
                JsonConvert.SerializeObject(workshopMemento)));

        // Act
        var result = mementoService.CreateMemento("ExpectedKey", workshopMemento);

        // Assert
        Assert.AreEqual("ExpectedKey_RequiredWorkshopMemento", result.State.Key);
        Assert.AreEqual(expectedValue, result.State.Value);
    }

    [Test]
    public void GetMementoKey_ShouldReturnCreatedMementoKey()
    {
        // Arrange
        var expectedKey = "ExpectedKey_RequiredWorkshopMemento";

        // Act
        var result = mementoService.GetMementoKey("ExpectedKey");

        // Assert
        Assert.AreEqual(expectedKey, result);
    }
}
