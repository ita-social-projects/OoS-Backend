using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using OutOfSchool.WebApi.Util.ModelBinding;

namespace OutOfSchool.WebApi.Tests.Util;

[TestFixture]
public class EnumCollectionModelBinderTests
{
    private EnumCollectionModelBinder<TestEnum> _modelBinder;
    private ModelBindingContext _bindingContext;
    private TestValueProvider _valueProvider;
    private ModelStateDictionary _modelState;

    // Test enum for testing purposes
    public enum TestEnum
    {
        First = 1,
        Second = 2,
        Third = 3
    }

    [SetUp]
    public void SetUp()
    {
        _modelBinder = new EnumCollectionModelBinder<TestEnum>();
        _valueProvider = new TestValueProvider();
        _modelState = new ModelStateDictionary();

        _bindingContext = new DefaultModelBindingContext
        {
            ModelName = "testParam",
            ValueProvider = _valueProvider,
            ModelState = _modelState,
            ModelMetadata = new EmptyModelMetadataProvider()
                .GetMetadataForType(typeof(HashSet<TestEnum>))
        };
    }

    [Test]
    public void BindModelAsync_WithNullBindingContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _modelBinder.BindModelAsync(null));

        Assert.That(exception.ParamName, Is.EqualTo("bindingContext"));
    }

    [Test]
    public async Task BindModelAsync_WithEmptyValues_ReturnsEmptyHashSet()
    {
        // Arrange
        _valueProvider.SetValue("testParam", StringValues.Empty);

        // Act
        await _modelBinder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as HashSet<TestEnum>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
        Assert.That(_modelState.IsValid, Is.True);
    }

    [Test]
    public async Task BindModelAsync_WithValidEnumNames_ReturnsCorrectHashSet()
    {
        // Arrange
        _valueProvider.SetValue("testParam", new StringValues(["First", "Second"]));

        // Act
        await _modelBinder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as HashSet<TestEnum>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(TestEnum.First), Is.True);
        Assert.That(result.Contains(TestEnum.Second), Is.True);
        Assert.That(_modelState.IsValid, Is.True);
    }

    [Test]
    public async Task BindModelAsync_WithValidEnumNumbers_ReturnsCorrectHashSet()
    {
        // Arrange
        _valueProvider.SetValue("testParam", new StringValues(["1", "3"]));

        // Act
        await _modelBinder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as HashSet<TestEnum>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(TestEnum.First), Is.True);
        Assert.That(result.Contains(TestEnum.Third), Is.True);
        Assert.That(_modelState.IsValid, Is.True);
    }

    [Test]
    public async Task BindModelAsync_WithMixedValidValues_ReturnsCorrectHashSet()
    {
        // Arrange
        _valueProvider.SetValue("testParam", new StringValues(["First", "2", "Third"]));

        // Act
        await _modelBinder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as HashSet<TestEnum>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.Contains(TestEnum.First), Is.True);
        Assert.That(result.Contains(TestEnum.Second), Is.True);
        Assert.That(result.Contains(TestEnum.Third), Is.True);
        Assert.That(_modelState.IsValid, Is.True);
    }

    [Test]
    public async Task BindModelAsync_WithCaseInsensitiveNames_ReturnsCorrectHashSet()
    {
        // Arrange
        _valueProvider.SetValue("testParam", new StringValues(["first", "SECOND"]));

        // Act
        await _modelBinder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as HashSet<TestEnum>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(TestEnum.First), Is.True);
        Assert.That(result.Contains(TestEnum.Second), Is.True);
        Assert.That(_modelState.IsValid, Is.True);
    }

    [Test]
    public async Task BindModelAsync_WithInvalidValues_AddsModelErrors()
    {
        // Arrange
        _valueProvider.SetValue("testParam", new StringValues(["InvalidEnum", "999"]));

        // Act
        await _modelBinder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as HashSet<TestEnum>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
        Assert.That(_modelState.IsValid, Is.False);
        Assert.That(_modelState.ContainsKey("testParam"), Is.True);

        var errors = _modelState["testParam"].Errors;
        Assert.That(errors.Count, Is.EqualTo(1));

        var errorMessage = errors[0].ErrorMessage;
        Assert.That(errorMessage, Does.Contain("Invalid TestEnum values"));
        Assert.That(errorMessage, Does.Contain("InvalidEnum"));
        Assert.That(errorMessage, Does.Contain("999"));
        Assert.That(errorMessage, Does.Contain("Allowed values are"));
        Assert.That(errorMessage, Does.Contain("First"));
        Assert.That(errorMessage, Does.Contain("Second"));
        Assert.That(errorMessage, Does.Contain("Third"));
    }

    [Test]
    public async Task BindModelAsync_WithMixedValidAndInvalidValues_ReturnsValidValuesAndAddsErrors()
    {
        // Arrange
        _valueProvider.SetValue("testParam", new StringValues(["First", "InvalidEnum", "2", "999"]));

        // Act
        await _modelBinder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as HashSet<TestEnum>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(TestEnum.First), Is.True);
        Assert.That(result.Contains(TestEnum.Second), Is.True);

        Assert.That(_modelState.IsValid, Is.False);
        Assert.That(_modelState.ContainsKey("testParam"), Is.True);

        var errors = _modelState["testParam"].Errors;
        Assert.That(errors.Count, Is.EqualTo(1));

        var errorMessage = errors[0].ErrorMessage;
        Assert.That(errorMessage, Does.Contain("InvalidEnum"));
        Assert.That(errorMessage, Does.Contain("999"));
    }

    [Test]
    public async Task BindModelAsync_WithWhitespaceValues_IgnoresWhitespace()
    {
        // Arrange
        _valueProvider.SetValue("testParam", new StringValues(["First", "", "   ", "Second", null]));

        // Act
        await _modelBinder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as HashSet<TestEnum>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(TestEnum.First), Is.True);
        Assert.That(result.Contains(TestEnum.Second), Is.True);
        Assert.That(_modelState.IsValid, Is.True);
    }

    [Test]
    public async Task BindModelAsync_WithDuplicateValues_ReturnsUniqueValues()
    {
        // Arrange
        _valueProvider.SetValue("testParam", new StringValues(["First", "1", "FIRST"]));

        // Act
        await _modelBinder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as HashSet<TestEnum>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1)); // HashSet eliminates duplicates
        Assert.That(result.Contains(TestEnum.First), Is.True);
        Assert.That(_modelState.IsValid, Is.True);
    }

    [Test]
    public async Task BindModelAsync_WithZeroValue_HandlesCorrectly()
    {
        // Arrange
        _valueProvider.SetValue("testParam", new StringValues(["0"]));

        // Act
        await _modelBinder.BindModelAsync(_bindingContext);

        // Assert
        Assert.That(_bindingContext.Result.IsModelSet, Is.True);
        var result = _bindingContext.Result.Model as HashSet<TestEnum>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0)); // 0 is not defined in TestEnum
        Assert.That(_modelState.IsValid, Is.False);
        Assert.That(_modelState.ContainsKey("testParam"), Is.True);
    }

    // Helper class for testing
    private class TestValueProvider : IValueProvider
    {
        private readonly Dictionary<string, ValueProviderResult> _values = [];

        public void SetValue(string key, StringValues values)
        {
            _values[key] = new ValueProviderResult(values);
        }

        public bool ContainsPrefix(string prefix) => _values.ContainsKey(prefix);

        public ValueProviderResult GetValue(string key) =>
            _values.TryGetValue(key, out var value) ? value : ValueProviderResult.None;
    }
}
