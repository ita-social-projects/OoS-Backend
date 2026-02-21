using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon;
using OutOfSchool.AuthCommon.Validators;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.AuthServer.Tests.Validators;

[TestFixture]
public class CustomClientValidationProviderTests
{
    private IValidationAttributeAdapterProvider adapterProvider;
    private Mock<IStringLocalizer<SharedResource>> localizer;

    [SetUp]
    public void SetUp()
    {
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        adapterProvider = new CustomClientValidationProvider();
    }
    
    [Test]
    public void GetAttributeAdapter_WithCustomPasswordAttribute_ShouldReturnCustomPasswordAdapter()
    {
        // Arrange
        var attribute = new CustomPasswordValidationAttribute();

        // Act
        var result = adapterProvider.GetAttributeAdapter(attribute, localizer.Object);

        // Assert
        Assert.IsInstanceOf<CustomPasswordValidationAdapter>(result);
    }

    [Test]
    public void GetAttributeAdapter_WithCustomUkrainianNameAttribute_ShouldReturnCustomUkrainianNameAttribute()
    {
        // Arrange
        var attribute = new CustomUkrainianNameAttribute();

        // Act
        var result = adapterProvider.GetAttributeAdapter(attribute, localizer.Object);

        // Assert
        Assert.IsInstanceOf<CustomUkrainianNameAttributeAdapter>(result);
    }
}