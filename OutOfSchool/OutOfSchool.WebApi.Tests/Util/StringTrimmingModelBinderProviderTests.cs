using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
using OutOfSchool.WebApi.Util.ModelBinding;
using static OutOfSchool.Tests.Common.TestModelBinderHelpers;

namespace OutOfSchool.WebApi.Tests.Util;

[TestFixture]
public class StringTrimmingModelBinderProviderTests
{
    private StringTrimmingModelBinderProvider _provider;

    [SetUp]
    public void SetUp()
    {
        _provider = new StringTrimmingModelBinderProvider();
    }

    [Test]
    public void GetBinder_WithNullContext_ReturnsNull()
    {
        // Act
        var result = _provider.GetBinder(null);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithStringModelType_ReturnsProvider()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(string));
        var context = CreateContext(metadata);
        context.BindingInfo.BindingSource = BindingSource.Form;

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<StringTrimmingModelBinder>());
    }

    [Test]
    public void GetBinder_WithStringModelTypeAndBodyBindingSource_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(string));
        var context = CreateContext(metadata);
        context.BindingInfo.BindingSource = BindingSource.Body;

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithOtherModelType_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(int));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }
}