using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
using OutOfSchool.WebApi.Util.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Util;

[TestFixture]
public class StringTrimmingModelBinderTests
{
    private StringTrimmingModelBinder _binder;
    private DefaultModelBindingContext _bindingContext;
    private SimpleValueProvider _valueProvider;
    private ModelStateDictionary _modelState;

    [SetUp]
    public void SetUp()
    {
        _binder = new StringTrimmingModelBinder();
        _valueProvider = new SimpleValueProvider();
        _modelState = new ModelStateDictionary();

        _bindingContext = new DefaultModelBindingContext
        {
            ModelName = "testModel",
            ValueProvider = _valueProvider,
            ModelState = _modelState,
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(string))
        };
    }

    [Test]
    public void BindModelAsync_WithNullBindingContext_ThrowsNullArgumentException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _binder.BindModelAsync(null));

        Assert.That(exception.ParamName, Is.EqualTo("bindingContext"));
    }

    [Test]
    public async Task BindModelAsync_WithNullValue_ReturnsNullModel()
    {
        // Arrange
        _valueProvider.SetValue("testModel", null);

        // Act
        await _binder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as string;
        Assert.That(result, Is.Null);
        Assert.That(_modelState.IsValid, Is.True);
    }

    [Test]
    public async Task BindModelAsync_WithEmptyStringValue_ReturnsNullModel()
    {
        // Arrange
        _valueProvider.SetValue("testModel", "");

        // Act
        await _binder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as string;
        Assert.That(result, Is.Null);
        Assert.That (_modelState.IsValid, Is.True);
    }

    [Test]
    public async Task BindModelAsync_WithWhitespaceString_TrimsAndReturnsValue()
    {
        // Arrange
        var value = "   example string   ";
        var expectedValue = value.Trim();
        _valueProvider.SetValue("testModel", value);

        // Act
        await _binder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as string;
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedValue));
        Assert.That(_modelState.IsValid, Is.True);
    }

    private class SimpleValueProvider : IValueProvider
    {
        private readonly Dictionary<string, string> _values = new();

        public void SetValue(string key, string value)
        {
            _values[key] = value;
        }

        public bool ContainsPrefix(string prefix)
        {
            return _values.ContainsKey(prefix);
        }

        public ValueProviderResult GetValue(string key)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return new ValueProviderResult(value);
            }

            return ValueProviderResult.None;
        }
    }
}