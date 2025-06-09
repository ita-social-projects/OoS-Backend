using NUnit.Framework;
using OutOfSchool.QuartzJobs.Api.Util;
using System;

namespace OutOfSchool.WebApi.Tests.QuartzJobs.Monitoring;

[TestFixture]
public class CronHelperTests
{
    [Test]
    public void GetUpcomingExecutions_WithInvalidCronExpression_ReturnsEmptyList()
    {
        // Arrange
        var invalidCron = "invalid cron";
        var count = 5;

        // Act
        var result = CronHelper.GetUpcomingExecutions(invalidCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetUpcomingExecutions_WithValidCronExpression_ReturnsCorrectNumberOfExecutions()
    {
        // Arrange
        var validCron = "0 0 12 * * ?"; // Noon every day
        var count = 5;

        // Act
        var result = CronHelper.GetUpcomingExecutions(validCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(count));
    }

    [Test]
    public void GetUpcomingExecutions_WithZeroCount_ReturnsEmptyList()
    {
        // Arrange
        var validCron = "0 0 12 * * ?"; // Noon every day
        var count = 0;

        // Act
        var result = CronHelper.GetUpcomingExecutions(validCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetUpcomingExecutions_WithNegativeCount_ReturnsEmptyList()
    {
        // Arrange
        var validCron = "0 0 12 * * ?"; // Noon every day
        var count = -1;

        // Act
        var result = CronHelper.GetUpcomingExecutions(validCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetUpcomingExecutions_WithDailyCron_ReturnsCorrectSequentialDates()
    {
        // Arrange
        var dailyCron = "0 0 12 * * ?"; // Noon every day
        var count = 3;

        // Act
        var result = CronHelper.GetUpcomingExecutions(dailyCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(count));

        // Check that dates are sequential with 1 day difference
        for (int i = 1; i < result.Count; i++)
        {
            var difference = result[i].Date - result[i - 1].Date;
            Assert.That(difference.Days, Is.EqualTo(1),
                $"Expected 1 day difference between {result[i - 1]} and {result[i]}");
        }

        // Check that all times are at noon in local time
        foreach (var date in result)
        {
            Assert.That(date.Hour, Is.EqualTo(12), "Hour should be 12 (noon)");
            Assert.That(date.Minute, Is.EqualTo(0), "Minute should be 0");
            Assert.That(date.Second, Is.EqualTo(0), "Second should be 0");
        }
    }

    [Test]
    public void GetUpcomingExecutions_WithHourlyCron_ReturnsCorrectSequentialHours()
    {
        // Arrange
        var hourlyCron = "0 0 * * * ?"; // Every hour at minute 0
        var count = 5;

        // Act
        var result = CronHelper.GetUpcomingExecutions(hourlyCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(count));

        // Check that times are sequential with 1 hour difference
        for (int i = 1; i < result.Count; i++)
        {
            var difference = result[i] - result[i - 1];
            Assert.That(Math.Round(difference.TotalHours), Is.EqualTo(1),
                $"Expected 1 hour difference between {result[i - 1]} and {result[i]}");
        }

        // Check that all times are at minute 0, second 0
        foreach (var date in result)
        {
            Assert.That(date.Minute, Is.EqualTo(0), "Minute should be 0");
            Assert.That(date.Second, Is.EqualTo(0), "Second should be 0");
        }
    }

    [Test]
    public void GetUpcomingExecutions_WithWeeklyCron_ReturnsCorrectWeeklyDates()
    {
        // Arrange
        var weeklyCron = "0 0 12 ? * MON"; // Noon every Monday
        var count = 3;

        // Act
        var result = CronHelper.GetUpcomingExecutions(weeklyCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(count));

        // Check that all dates are Mondays
        foreach (var date in result)
        {
            Assert.That(date.DayOfWeek, Is.EqualTo(DayOfWeek.Monday), "Day should be Monday");
            Assert.That(date.Hour, Is.EqualTo(12), "Hour should be 12 (noon)");
            Assert.That(date.Minute, Is.EqualTo(0), "Minute should be 0");
            Assert.That(date.Second, Is.EqualTo(0), "Second should be 0");
        }

        // Check that dates are sequential with 7 days difference
        for (int i = 1; i < result.Count; i++)
        {
            var difference = result[i].Date - result[i - 1].Date;
            Assert.That(difference.Days, Is.EqualTo(7),
                $"Expected 7 days difference between {result[i - 1]} and {result[i]}");
        }
    }

    [Test]
    public void GetUpcomingExecutions_WithMonthlyLastDayCron_ReturnsLastDaysOfMonths()
    {
        // Arrange - Cron for noon on the last day of each month
        var monthlyCron = "0 0 12 L * ?";
        var count = 3;

        // Act
        var result = CronHelper.GetUpcomingExecutions(monthlyCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(count));

        // Check that all dates are the last day of their month
        foreach (var date in result)
        {
            var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);
            Assert.That(date.Day, Is.EqualTo(lastDayOfMonth),
                $"Expected last day of month ({lastDayOfMonth}) for {date}");
            Assert.That(date.Hour, Is.EqualTo(12), "Hour should be 12 (noon)");
            Assert.That(date.Minute, Is.EqualTo(0), "Minute should be 0");
            Assert.That(date.Second, Is.EqualTo(0), "Second should be 0");
        }
    }

    [Test]
    public void GetUpcomingExecutions_WithComplexCron_ReturnsCorrectDates()
    {
        // Arrange - 15th of each month at 10:30 AM
        var complexCron = "0 30 10 15 * ?";
        var count = 4;

        // Act
        var result = CronHelper.GetUpcomingExecutions(complexCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(count), "Expected to get exactly 4 execution times");

        // Check that all times are at 10:30
        foreach (var date in result)
        {
            Assert.That(date.Hour, Is.EqualTo(10), "Hour should be 10");
            Assert.That(date.Minute, Is.EqualTo(30), "Minute should be 30");
            Assert.That(date.Second, Is.EqualTo(0), "Second should be 0");
            Assert.That(date.Day, Is.EqualTo(15), "Day should be 15th of month");
        }
    
        // Check that the dates are consecutive with a difference of 1 month
        for (int i = 1; i < result.Count; i++)
        {
            var difference = result[i].Month - result[i - 1].Month;
            // Taking into account the transition between years
            if (difference < 0)
                difference += 12;
            
            Assert.That(difference, Is.EqualTo(1), 
                $"Expected 1 month difference between {result[i-1]} and {result[i]}");
        }
    }

    [Test]
    public void GetUpcomingExecutions_WithNoMoreExecutions_ReturnsPartialList()
    {
        // Arrange - Cron for specific past dates that won't have enough future occurrences
        // This is a contrived example to demonstrate behavior when there aren't enough occurrences
        // 12:00 on February 29th (leap day)
        var specificCron = "0 0 12 29 2 ? 2024"; // Only valid for Feb 29, 2024
        var count = 5; // We ask for 5 but expect fewer

        // Act
        var result = CronHelper.GetUpcomingExecutions(specificCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);

        // The result count will depend on when the test is run:
        // - If run before Feb 29, 2024: expect 1 result
        // - If run after Feb 29, 2024: expect 0 results

        // Instead of time-dependent logic, we'll just verify each result makes sense
        foreach (var date in result)
        {
            Assert.That(date.Day, Is.EqualTo(29), "Day should be 29");
            Assert.That(date.Month, Is.EqualTo(2), "Month should be February (2)");
            Assert.That(date.Year, Is.EqualTo(2024), "Year should be 2024");
            Assert.That(date.Hour, Is.EqualTo(12), "Hour should be 12");
            Assert.That(date.Minute, Is.EqualTo(0), "Minute should be 0");
            Assert.That(date.Second, Is.EqualTo(0), "Second should be 0");
        }
    }

    [Test]
    public void GetUpcomingExecutions_ReturnedDatesAreInLocalTimeZone()
    {
        // Arrange
        var dailyCron = "0 0 12 * * ?"; // Noon every day
        var count = 1;

        // Act
        var result = CronHelper.GetUpcomingExecutions(dailyCron, count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(count));

        // Verify the result is in local time by comparing offset with local offset
        var localOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
        Assert.That(result[0].Offset, Is.EqualTo(localOffset),
            "The DateTimeOffset should have the local timezone offset");
    }
}