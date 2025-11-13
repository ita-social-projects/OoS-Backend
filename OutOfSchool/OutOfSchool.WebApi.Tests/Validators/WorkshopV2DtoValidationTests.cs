using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.Tests.BusinessLogic.Models.Workshops;

[TestFixture]
public class WorkshopV2DtoValidationTests
{
    private WorkshopV2Dto validDto;

    [SetUp]
    public void SetUp()
    {
        validDto = WorkshopV2DtoGenerator.Generate();
        validDto.DateTimeRanges = [new() { StartTime = TimeSpan.Parse("18:00"), EndTime = TimeSpan.Parse("19:45"), Workdays = [DaysBitMask.Monday] }];
    }

    [Test]
    public void Validate_WhenAvailableSeatsEqualsUintMaxValue_ShouldReturnNoValidationErrors()
    {
        // Arrange
        var dto = validDto;
        dto.AvailableSeats = uint.MaxValue;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Validate_WhenAvailableSeatsGreaterThan100000_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.AvailableSeats = 100500;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("AvailableSeats field should be in the range from 1 to 100000"));
    }

    [Test]
    public void Validate_WhenAvailableSeatsFewerThanOne_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.AvailableSeats = 0;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("AvailableSeats field should be in the range from 1 to 100000"));
    }

    [Test]
    public void Validate_WhenDateTimeRangeEndTimeEarlierStartTime_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        (dto.DateTimeRanges[0].StartTime, dto.DateTimeRanges[0].EndTime) = (dto.DateTimeRanges[0].EndTime, dto.DateTimeRanges[0].StartTime);
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("The end date cannot be equal to or earlier than the start date"));
    }

    [Test]
    public void Validate_WhenDateTimeRangeWorkdaysIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.DateTimeRanges[0].Workdays = [];
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Workdays are required"));
    }

    [Test]
    public void Validate_WhenDateTimeRangeWorkdaysContainsDuplications_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.DateTimeRanges[0].Workdays = [DaysBitMask.Monday, DaysBitMask.Monday];
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Workdays contain duplications"));
    }

    [Test]
    public void Validate_WhenNoAgeRestrictionsIsTrueAndMinAndMaxAgeHaveAnyValue_ShouldReturnNoValidationErrors()
    {
        // Arrange
        var dto = validDto;
        dto.MinAge = 10;
        dto.MaxAge = -10;
        dto.NoAgeRestrictions = true;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        dto.MinAge.Should().Be(10);
        dto.MaxAge.Should().Be(-10);
        results.Should().BeEmpty();
    }

    [Test]
    public void Validate_WhenNoAgeRestrictionsIsFalseAndAgesAreCorrect_ShouldReturnNoValidationErrors()
    {
        // Arrange
        var dto = validDto;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Validate_WhenNoAgeRestrictionsIsFalseAndMinAgeGreaterThanMaxAge_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.MinAge = 15;
        dto.MaxAge = 10;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Min age should be less than Max age"));
    }

    [Test]
    public void Validate_WhenIsPaidEqualsTrueAndPayRateIsNull_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.IsPaid = true;
        dto.PayRate = null;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Pay rate must be specified when the workshop is paid."));
    }

    [Test]
    public void Validate_WhenIsPaidEqualsTrueAndPayRateIsNone_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.IsPaid = true;
        dto.PayRate = PayRateType.None;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Pay rate must be specified when the workshop is paid."));
    }

    [Test]
    public void Validate_WhenIsPaidEqualsTrueAndPriceIsNull_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.IsPaid = true;
        dto.Price = null;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Price must be specified and must be in the range from 0.01 to 100000.00 when the workshop is paid."));
    }

    [Test]
    public void Validate_WhenIsPaidEqualsTrueAndPriceLessOneCent_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.IsPaid = true;
        dto.Price = 0.00M;
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Price must be specified and must be in the range from 0.01 to 100000.00 when the workshop is paid."));
    }

    [Test]
    public void Validate_WhenAnyKeywordIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.Keywords = [string.Empty];
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Keyword cannot be empty or whitespace."));
    }

    [Test]
    public void Validate_WhenAnyKeywordGreaterThanMaxLengthOfOneKeywordConst_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.Keywords = [new string('c', Constants.MaxLengthOfOneKeyword + 1)];
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains($"Keyword must be no longer than {Constants.MaxLengthOfOneKeyword} characters."));
    }

    [Test]
    public void Validate_WhenKeywordsLengthGreaterThanMaxKeywordsLengthConst_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.Keywords = [new string('c', 1), new string('c', Constants.MaxKeywordsLength)];
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains($"The length of all keywords must not exceed {Constants.MaxKeywordsLength} characters."));
    }

    [Test]
    public void Validate_WhenKeywordsListContainsDuplicates_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.Keywords = [new string('c', 5), new string('c', 5)];
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Keywords list contains duplicates."));
    }

    [Test]
    public void Validate_WhenImageFilesContainEmptyFiles_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.ImageFiles = [FakeFile("img.jpg", 0)];
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("ImageFiles must not contain empty files."));
    }

    [Test]
    public void Validate_WhenImageIdsContainEmptyOrWhitespaceStrings_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.ImageIds = [Guid.NewGuid().ToString(), string.Empty];
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("ImageIds must not contain empty or whitespace strings."));
    }

    [Test]
    public void Validate_WhenCoverImageAndCoverImageIdValuesAreNotNull_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.CoverImage = FakeFile();
        dto.CoverImageId = Guid.NewGuid().ToString();
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("Either CoverImage or CoverImageId should be provided, not both."));
    }

    [Test]
    public void Validate_WhenImageFilesAndImageIdsAreEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.ImageFiles = [];
        dto.ImageIds = [];
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains("At least one of the ImageFiles or ImageIds fields must be filled in."));
    }

    [Test]
    public void Validate_WhenCountOfImageFilesAndImageIdsMoreThanAllowed_ShouldReturnValidationError()
    {
        // Arrange
        var dto = validDto;
        dto.ImageIds = [Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
                        Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
                        Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),];
        dto.ImageFiles = [FakeFile()];
        var validationContext = new ValidationContext(dto);

        // Act
        var results = dto.Validate(validationContext).ToList();

        // Assert
        results.Should().ContainSingle(result =>
            result.ErrorMessage.Contains($"A maximum of {Constants.MaxCountOfImagesForWorkshop} images are allowed for a workshop."));
    }

    private static IFormFile FakeFile(string name = "img.jpg", int size = 10)
    {
        var bytes = new byte[size];
        var ms = new MemoryStream(bytes);
        return new FormFile(ms, 0, bytes.Length, "file", name);
    }
}