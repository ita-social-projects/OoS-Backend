using System;
using NUnit.Framework;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Util;

[TestFixture]
public class ModelValidationHelperTests
{
    [Test]
    public void ValidateExcludedIdFilter_WhenNull_Should_ThrowException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ModelValidationHelper.ValidateExcludedIdFilter(null));
    }

    [Test]
    public void ValidateExcludedIdFilter_WhenGuidIsEMpty_Should_ThrowException()
    {
        // Arrange
        var filter = new ExcludeIdFilter() { ExcludedId = Guid.Empty };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => ModelValidationHelper.ValidateExcludedIdFilter(filter));
    }

    [Test]
    public void ValidateExcludedIdFilter_WhenOffsetFilterIsIncorrect_Should_ThrowException()
    {
        // Arrange
        var filter = new ExcludeIdFilter() { From = -1 };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => ModelValidationHelper.ValidateExcludedIdFilter(filter));
    }

    [Test]
    public void ValidateExcludedIdFilter_WhenValidFilter_Should_Not_ThrowException()
    {
        // Arrange
        var filter = new ExcludeIdFilter() { From = 1, ExcludedId = Guid.NewGuid() };

        // Act and Assert
        Assert.DoesNotThrow(() => ModelValidationHelper.ValidateExcludedIdFilter(filter));
    }
}
