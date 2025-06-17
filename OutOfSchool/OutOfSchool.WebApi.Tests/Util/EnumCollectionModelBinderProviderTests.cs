using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
using OutOfSchool.WebApi.Util.ModelBinding;
using System;
using System.Collections.Generic;

namespace OutOfSchool.WebApi.Tests.Util;

[TestFixture]
public class EnumCollectionModelBinderProviderTests
{
    private EnumCollectionModelBinderProvider _provider;

    public enum TestEnum
    {
        Value1,
        Value2
    }

    public enum AnotherTestEnum
    {
        Option1,
        Option2
    }

    [SetUp]
    public void SetUp()
    {
        _provider = new EnumCollectionModelBinderProvider();
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
    public void GetBinder_WithNullMetadata_ReturnsNull()
    {
        // Arrange
        var context = CreateContext(null);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithNullModelType_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(null);
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithEnumHashSet_ReturnsCorrectModelBinder()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(HashSet<TestEnum>));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<EnumCollectionModelBinder<TestEnum>>());
    }

    [Test]
    public void GetBinder_WithDifferentEnumHashSet_ReturnsCorrectModelBinder()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(HashSet<AnotherTestEnum>));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<EnumCollectionModelBinder<AnotherTestEnum>>());
    }

    [Test]
    public void GetBinder_WithStringHashSet_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(HashSet<string>));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithIntHashSet_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(HashSet<int>));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithNonHashSetCollection_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(List<TestEnum>));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithEnumArray_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(TestEnum[]));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithIEnumerableEnum_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(IEnumerable<TestEnum>));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithHashSetOfMultipleGenericArguments_ReturnsNull()
    {
        // Arrange - Dictionary has 2 generic arguments, should return null
        var metadata = CreateModelMetadata(typeof(Dictionary<TestEnum, string>));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithNullableEnum_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(HashSet<TestEnum?>));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_WithStructThatIsNotEnum_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(HashSet<DateTime>));
        var context = CreateContext(metadata);

        // Act
        var result = _provider.GetBinder(context);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetBinder_CreatesNewInstanceEachTime()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(HashSet<TestEnum>));
        var context = CreateContext(metadata);

        // Act
        var result1 = _provider.GetBinder(context);
        var result2 = _provider.GetBinder(context);

        // Assert
        Assert.That(result1, Is.Not.Null);
        Assert.That(result2, Is.Not.Null);
        Assert.That(result1, Is.Not.SameAs(result2));
        Assert.That(result1, Is.InstanceOf<EnumCollectionModelBinder<TestEnum>>());
        Assert.That(result2, Is.InstanceOf<EnumCollectionModelBinder<TestEnum>>());
    }

    // Helper methods
    private static ModelMetadata CreateModelMetadata(Type modelType)
    {
        if (modelType == null)
            return null;

        var metadataProvider = new EmptyModelMetadataProvider();
        return metadataProvider.GetMetadataForType(modelType);
    }

    private static ModelBinderProviderContext CreateContext(ModelMetadata metadata)
    {
        var context = new TestModelBinderProviderContext();
        context.SetMetadata(metadata);
        return context;
    }

    // Custom implementation of ModelBinderProviderContext for testing
    private class TestModelBinderProviderContext : ModelBinderProviderContext
    {
        private ModelMetadata _metadata;

        public override ModelMetadata Metadata => _metadata;

        public void SetMetadata(ModelMetadata metadata)
        {
            _metadata = metadata;
        }

        public override BindingInfo BindingInfo => null;

        public override IModelBinder CreateBinder(ModelMetadata metadata)
        {
            throw new NotImplementedException("Not needed for these tests");
        }

        public override IModelMetadataProvider MetadataProvider => null;
    }
}