using System;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Services.Enums;
using System.Linq;

namespace OutOfSchool.Tests.BusinessLogic.Models.Workshops;

[TestFixture]
public class WorkshopBaseDtoTests
{
    [Test]
    public void Validate_WhenNoAgeRestrictionsIsTrue_ShouldSetMinAndMaxAgeToDefaults()
    {
        // Arrange
        var dto = CreateDtoWithNoAgeRestrictions();

        // Act
        var validationContext = new ValidationContext(dto);
        var results = dto.Validate(validationContext).ToList();

        // Assert
        dto.MinAge.Should().Be(0);
        dto.MaxAge.Should().Be(120);
        results.Should().BeEmpty();
    }

    [Test]
    public void Validate_WhenNoAgeRestrictionsIsFalseAndAgesAreCorrect_ShouldReturnNoValidationErrors()
    {
        // Arrange
        var dto = CreateDtoWithCustomAges(5, 10);

        // Act
        var validationContext = new ValidationContext(dto);
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Validate_WhenNoAgeRestrictionsIsFalseAndMinAgeGreaterThanMaxAge_ShouldReturnValidationError()
    {
        // Arrange
        var dto = CreateDtoWithCustomAges(15, 10);

        // Act
        var validationContext = new ValidationContext(dto);
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Min age should be less than or equal to Max age"));
    }

    #region Helpers

    private static WorkshopBaseDto CreateDtoWithNoAgeRestrictions()
    {
        return new WorkshopBaseDto
        {
            NoAgeRestrictions = true,
            MinAge = null,
            MaxAge = null,
            StudyPeriodDates = new StudyPeriodDatesDto
            {
                StartDate = new DateOnly(2025, 9, 1),
                EndDate = new DateOnly(2026, 5, 31)
            },
            DateTimeRanges = [new() { StartTime = TimeSpan.Parse("10:00"), EndTime = TimeSpan.Parse("11:00"), Workdays = [DaysBitMask.Monday] }],
            WorkshopDescriptionItems = [new() { SectionName = "Section", Description = "Description" }],
            FormOfLearning = FormOfLearning.Offline,
            ProviderId = Guid.NewGuid(),
            ProviderTitle = "Test Provider",
            EducationalShift = EducationalShift.First,
            LanguageOfEducationId = 1,
            AgeComposition = AgeComposition.SameAge,
            WorkshopType = WorkshopType.Workshop,
            AvailableSeats = 10
        };
    }

    private static WorkshopBaseDto CreateDtoWithCustomAges(int min, int max)
    {
        var dto = CreateDtoWithNoAgeRestrictions();
        dto.NoAgeRestrictions = false;
        dto.MinAge = min;
        dto.MaxAge = max;
        return dto;
    }

    #endregion
}