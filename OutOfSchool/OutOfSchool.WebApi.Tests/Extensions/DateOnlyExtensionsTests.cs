using NUnit.Framework;
using OutOfSchool.BusinessLogic.Extensions;
using System;

namespace OutOfSchool.WebApi.Tests;

[TestFixture]
public class DateOnlyExtensionsTests
{
    [Test]
    public void ToStudyPeriodDate_WhenCalled_SetsYearTo2000_AndPreservesMonthAndDay()
    {
        // Arrange
        var originalDate = new DateOnly(2025, 9, 1);

        // Act
        var result = originalDate.ToStudyPeriodDate();

        // Assert
        Assert.AreEqual(new DateOnly(2000, 9, 1), result);
    }

    [Test]
    public void ToStudyPeriodDate_WhenDateIsLeapDay_SetsYearTo2000()
    {
        // Arrange
        var leapDate = new DateOnly(2024, 2, 29);

        // Act
        var result = leapDate.ToStudyPeriodDate();

        // Assert
        Assert.AreEqual(new DateOnly(2000, 2, 29), result);
    }

    [Test]
    public void ToStudyPeriodDate_WhenDifferentDates_ReturnCorrectNormalizedDates()
    {
        // Arrange & Act & Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(new DateOnly(2000, 1, 1), new DateOnly(2023, 1, 1).ToStudyPeriodDate());
            Assert.AreEqual(new DateOnly(2000, 12, 31), new DateOnly(9999, 12, 31).ToStudyPeriodDate());
            Assert.AreEqual(new DateOnly(2000, 5, 15), new DateOnly(1980, 5, 15).ToStudyPeriodDate());
        });
    }
}
