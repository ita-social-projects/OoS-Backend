using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.SportRegistryApiClient.Tests.Validators;

[TestFixture]
public class SectionScheduleRequestTests
{
    [Test]
    public void ValidData_ShouldPassValidation()
    {
        var model = new SectionScheduleRequest
        {
            SectionScheduleWeekday = Weekday.MONDAY,
            SectionScheduleTimeFrom = "09:00:00",
            SectionScheduleTimeTo = "11:00:00"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.IsTrue(isValid);
        Assert.IsEmpty(results);
    }

    [Test]
    [TestCase((Weekday)(-1))]
    [TestCase((Weekday)7)]
    public void InvalidWeekday_ShouldFailValidation(Weekday weekday)
    {
        var model = new SectionScheduleRequest
        {
            SectionScheduleWeekday = weekday,
            SectionScheduleTimeFrom = "09:00:00",
            SectionScheduleTimeTo = "10:00:00"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.IsFalse(isValid);
        Assert.That(results.Any(r => r.ErrorMessage.Contains("sectionScheduleWeekday")));
    }

    [Test]
    public void InvalidTimeFormat_ShouldFailValidation()
    {
        var model = new SectionScheduleRequest
        {
            SectionScheduleWeekday = Weekday.MONDAY,
            SectionScheduleTimeFrom = "23:00:00",
            SectionScheduleTimeTo = "24:00:00"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.IsFalse(isValid);
        Assert.That(results.Any(r => r.ErrorMessage.Contains("must be in HH:mm:ss format")));
    }

    [Test]
    public void TimeFromAfterTimeTo_ShouldFailValidation()
    {
        var model = new SectionScheduleRequest
        {
            SectionScheduleWeekday = Weekday.MONDAY,
            SectionScheduleTimeFrom = "12:00:00",
            SectionScheduleTimeTo = "10:00:00"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.IsFalse(isValid);
        Assert.That(results.Any(r => r.ErrorMessage.Contains("must be before")));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void NullTimeFrom_ShouldFailValidation(string scheduleTimeFrom)
    {
        var model = new SectionScheduleRequest
        {
            SectionScheduleWeekday = Weekday.MONDAY,
            SectionScheduleTimeFrom = scheduleTimeFrom,
            SectionScheduleTimeTo = "11:00:00"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.IsFalse(isValid);
        Assert.That(results.Any(r => r.ErrorMessage.Contains("sectionScheduleTimeFrom is required")));
    }

    [Test]
    public void EmptyTimeFromAndTimeTo_ShouldFailValidation()
    {
        var model = new SectionScheduleRequest
        {
            SectionScheduleWeekday = Weekday.MONDAY,
            SectionScheduleTimeFrom = "",
            SectionScheduleTimeTo = ""
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.IsFalse(isValid);
        Assert.That(results.Any(r => r.ErrorMessage.Contains("is required")));
    }
}