using System;
using NUnit.Framework;
using OutOfSchool.Common.Extensions;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class DateTimeExtensionsTests
{
    [TestCase("9999-12-31T12:34:56", "9999-12-31T23:59:59.9999999")]
    [TestCase("9999-12-30T15:23:01", "9999-12-31T00:00:00")]
    [TestCase("2023-10-15T12:12:43", "2023-10-16T00:00:00")]
    [TestCase("0001-01-01T00:00:01", "0001-01-02T00:00:00")]
    public void NextDayStart_ShouldReturnNextDayStartDateTime(DateTime initialDateTime, DateTime expectedValue)
    {
        // Act
        var result = initialDateTime.NextDayStart();

        // Assert
        Assert.AreEqual(expectedValue, result);
    }
}
