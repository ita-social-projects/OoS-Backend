using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.WebApi.Tests.Validators;

[TestFixture]
public class CustomUkrainianNameAttributeAdapterTests
{
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private IModelMetadataProvider modelMetadataProvider;
    private ModelMetadata modelMetadata;
    private Mock<ActionContext> actionContext;

    [SetUp]
    public void SetUp()
    {
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        actionContext = new Mock<ActionContext>();
        modelMetadataProvider = new EmptyModelMetadataProvider();
        modelMetadata = modelMetadataProvider.GetMetadataForProperty(typeof(string), nameof(string.Length));
    }

    [Test]
    public void AddValidation_WhenContextIsNull_ShouldThrowException()
    {
        // Arrange
        var attribute = new CustomUkrainianNameAttribute();
        var adapter = new CustomUkrainianNameAttributeAdapter(attribute, localizer.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => adapter.AddValidation(null));
    }

    [Test]
    public void GetErrorMessage_WhenValidationContextIsNull_ShouldThrowException()
    {
        // Arrange
        var attribute = new CustomUkrainianNameAttribute();
        var adapter = new CustomUkrainianNameAttributeAdapter(attribute, localizer.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => adapter.GetErrorMessage(null));
    }

    [Test]
    public void AddValidation_ShouldPopulateContextAttributes()
    {
        // Arrange
        var expectedAddedAtributesCount = 3;
        var attributes = new Dictionary<string, string>();
        var attribute = new CustomUkrainianNameAttribute();
        var clientModelValidationContext = new Mock<ClientModelValidationContext>(actionContext.Object, modelMetadata, modelMetadataProvider, attributes);
        var adapter = new CustomUkrainianNameAttributeAdapter(attribute, localizer.Object);

        // Act
        adapter.AddValidation(clientModelValidationContext.Object);

        // Assert
        Assert.AreEqual(expectedAddedAtributesCount, attributes.Count);
        Assert.AreEqual(attributes["data-val"], "true");
    }
}
