using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon;
using OutOfSchool.AuthCommon.Validators;

namespace OutOfSchool.AuthServer.Tests.Validators;

[TestFixture]
public class CustomPasswordValidationAdapterTests
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
    public void AddValidation_ShouldPopulateContextAttributes()
    {
        // Arrange
        var expectedAddedAtributesCount = 7;
        var expectedPrefixCount = 6;
        var attributes = new Dictionary<string, string>();
        var attribute = new CustomPasswordValidationAttribute();
        var clientModelValidationContext = new Mock<ClientModelValidationContext>(actionContext.Object, modelMetadata, modelMetadataProvider, attributes);
        var adapter = new CustomPasswordValidationAdapter(attribute, localizer.Object);
        
        // Act
        adapter.AddValidation(clientModelValidationContext.Object);
        
        // Assert
        Assert.AreEqual(expectedAddedAtributesCount, attributes.Count);
        Assert.AreEqual(expectedPrefixCount, attributes.Keys.Count(key => key.StartsWith("data-val-validpass")));
        Assert.AreEqual(attributes["data-val"], "true");
    }

}