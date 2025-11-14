using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Util;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Util;

[TestFixture]
public class DecimalDotModelBinderTests
{
    private DecimalDotModelBinder _binder;

    [SetUp]
    public void SetUp()
    {
        _binder = new DecimalDotModelBinder();
    }

    [Test]
    public void BindModelAsync_WithNullBindingContext_ThrowsArgumentException()
    {
        // Arrange
        ModelBindingContext bindingContext = null;

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _binder.BindModelAsync(bindingContext));
        Assert.That(ex.ParamName, Is.EqualTo(null));
    }

    [Test]
    public async Task BindModelAsync_WithDotDecimalString_SetsModelResult()
    {
        // Arrange
        var expected = 1234.56m;
        var value = "1234.56";
        var bindingContext = GetBindingContext(value);

        // Act
        await _binder.BindModelAsync(bindingContext);

        // Assert
        Assert.That(bindingContext.Result.IsModelSet, Is.True);
        Assert.That(bindingContext.Result.Model, Is.EqualTo(expected));
    }

    [Test]
    public async Task BindModelAsync_WithCommaDecimalString_SetsModelResult()
    {
        // Arrange
        var expected = 1234.56m;
        var value = "1234,56";
        var bindingContext = GetBindingContext(value);

        // Act
        await _binder.BindModelAsync(bindingContext);

        // Assert
        Assert.That(bindingContext.Result.IsModelSet, Is.True);
        Assert.That(bindingContext.Result.Model, Is.EqualTo(expected));
    }

    [Test]
    public async Task BindModelAsync_WithNullValue_SetsModelResultToNull()
    {
        // Arrange
        string value = null;
        var bindingContext = GetBindingContext(value);

        // Act
        await _binder.BindModelAsync(bindingContext);

        // Assert
        Assert.That(bindingContext.Result.IsModelSet, Is.False);
        Assert.That(bindingContext.Result.Model, Is.Null);
    }

    [Test]
    public async Task BindModelAsync_WithInvalidDecimalString_SetsModelStateError()
    {
        // Arrange
        var value = "invalid_decimal";
        var bindingContext = GetBindingContext(value);

        // Act
        await _binder.BindModelAsync(bindingContext);

        // Assert
        Assert.That(bindingContext.Result.IsModelSet, Is.False);
        Assert.That(bindingContext.ModelState.ContainsKey("Price"), Is.True);
        var errors = bindingContext.ModelState["Price"].Errors;
        Assert.That(errors.Count, Is.GreaterThan(0));
    }

    private DefaultModelBindingContext GetBindingContext(string value)
    {
        var modelName = "Price";
        var bindingContext = new DefaultModelBindingContext
        {
            ModelName = modelName,
            ModelState = new ModelStateDictionary(),
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(decimal?))
        };

        var valueProviderMock = new Mock<IValueProvider>();

        if (value != null)
        {
            var valueProviderResult = new ValueProviderResult(
                new StringValues(value),
                CultureInfo.InvariantCulture);

            valueProviderMock
                .Setup(vp => vp.GetValue(modelName))
                .Returns(valueProviderResult);
        }
        else
        {
            valueProviderMock
                .Setup(vp => vp.GetValue(modelName))
                .Returns(ValueProviderResult.None);
        }

        bindingContext.ValueProvider = valueProviderMock.Object;

        return bindingContext;
    }
}