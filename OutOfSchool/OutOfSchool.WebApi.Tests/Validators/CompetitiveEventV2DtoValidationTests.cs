using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Validators;

[TestFixture]
public class CompetitiveEventV2DtoValidationTests
{
    private DateTime date1;
    private DateTime date2;
    private DateTime date3;
    private DateTime date4;

    [SetUp]
    public void SetUp()
    {
        date1 = new DateTime(2025, 9, 1);
        date2 = date1.Add(new TimeSpan(15, 0, 0, 0));
        date3 = date2.Add(new TimeSpan(3, 0, 0, 0));
        date4 = date3.Add(new TimeSpan(30, 0, 0, 0));
    }

        [Test]
    public void Validate_WhenAllPropertiesAreCorrect_ShouldReturnNoValidationErrors()
    {
        // Arrange
        var dto = CreateTheCorrectDto();
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Validate_WhenRegistrationStartTimeLaterThanRegistrationEndTime_ShouldReturnValidationError()
    {
        // Arrange
        var dto = CreateTheCorrectDto();
        dto.RegistrationStartTime = date2;
        dto.RegistrationEndTime = date1;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("The registration start time cannot be equal to or earlier than the registration end time"));
    }

    [Test]
    public void Validate_WhenScheduledStartTimeLaterThanScheduledEndTime_ShouldReturnValidationError()
    {
        // Arrange
        var dto = CreateTheCorrectDto();
        dto.ScheduledStartTime = date4;
        dto.ScheduledEndTime = date3;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("The scheduled start time cannot be equal to or earlier than the scheduled end time"));
    }

    [Test]
    public void Validate_WhenRegistrationEndTimeLaterThanScheduledStartTime_ShouldReturnValidationError()
    {
        // Arrange
        var dto = CreateTheCorrectDto();
        dto.RegistrationEndTime = date3;
        dto.ScheduledStartTime = date2;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("The scheduled start time cannot be equal to or earlier than the registration end time"));
    }

    [Test]
    public void Validate_WhenNumberOfSeatsGreaterThan100000_ShouldReturnValidationError()
    {
        // Arrange
        var dto = CreateTheCorrectDto();
        dto.NumberOfSeats = 100100;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("NumberOfSeats field should be in the range from 1 to 100000."));
    }

    [Test]
    public void Validate_WhenMinimumAgeGreaterThanMaximumAge_ShouldReturnValidationError()
    {
        // Arrange
        var dto = CreateTheCorrectDto();
        dto.MinimumAge = 10;
        dto.MaximumAge = 5;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Minimum age should be less than Maximum age"));
    }

    #region Helpers
    // 
    private CompetitiveEventV2Dto CreateTheCorrectDto()
    {
        var dto = CompetitiveEventV2DtoGenerator.Generate();
        dto.RegistrationStartTime = date1;
        dto.RegistrationEndTime = date2;
        dto.ScheduledStartTime = date3;
        dto.ScheduledEndTime = date4;
        dto.NumberOfSeats = 100;
        dto.MaximumAge = 10;
        dto.MinimumAge = 5;

        return dto;
    }

    #endregion
}