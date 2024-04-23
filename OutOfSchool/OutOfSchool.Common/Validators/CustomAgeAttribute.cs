using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Common.Validators;

/// <summary>
/// Validation attribute for dates. It allows only dates that are at least <see cref="MinAge"/> and at most <see cref="MaxAge"/>.
/// This attribute can be used on <see cref="DateTime"/> or <see cref="DateOnly"/> types.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class CustomAgeAttribute : DataTypeAttribute
{
    /// <summary>
    /// The approximate number of days in a year including leap years.
    /// </summary>
    public const double ApproximateDaysInYear = 365.2425;

    public CustomAgeAttribute()
        : base(DataType.Date)
    {
    }

    /// <summary>
    /// Gets the minimum age of the persom in years.
    /// </summary>
    public int MinAge { get; init; } = 0;

    /// <summary>
    /// Gets the maximum age of the persom in years.
    /// </summary>
    public int MaxAge { get; init; } = int.MaxValue;

    /// <summary>
    /// Gets a value indicating whether to use UTC now as beginning of time or not. Default value is <c>true</c>.
    /// </summary>
    public bool UseUTC { get; init; } = true;

    /// <summary>
    /// Gets the date time kind based on <see cref="UseUTC"/> value.
    /// </summary>
    private DateTimeKind DateTimeKind => UseUTC ? DateTimeKind.Utc : DateTimeKind.Local;

    /// <summary>
    /// Checks that the date is valid.
    /// </summary>
    /// <remarks>
    /// This method returns <c>true</c> if the <paramref name="value" /> is null.
    /// It is assumed the <see cref="RequiredAttribute" /> is used if the value may not be null.
    /// </remarks>
    /// <param name="value">Value to validate.</param>
    /// <returns><c>true</c> if valid, otherwise <c>false</c>.</returns>
    /// <exception cref="InvalidOperationException"> is thrown if the min and max ages are not valid.</exception>
    public override bool IsValid(object value)
    {
        EnsureThatMinAndMaxAgesAreValid();

        // If value is required then consumer should use RequiredAttribute with this attribute
        if (value is null)
        {
            return true;
        }

        var currentDate = UseUTC ? DateTime.UtcNow : DateTime.Now;

        DateTime? possibleBirthDate = value switch
        {
            DateTime date => date,
            DateOnly date => date.ToDateTime(TimeOnly.FromDateTime(currentDate), DateTimeKind),
            _ => null,
        };

        if (possibleBirthDate is not DateTime birthDate)
        {
            return false;
        }

        var totalDays = (currentDate - birthDate).TotalDays;
        var approximateAge = (int)(totalDays / ApproximateDaysInYear);

        return approximateAge >= MinAge && approximateAge <= MaxAge;
    }

    /// <summary>
    /// Ensures that min and max ages are valid.
    /// </summary>
    /// <exception cref="InvalidOperationException"> is thrown if the min and max ages are not valid.</exception>
    private void EnsureThatMinAndMaxAgesAreValid()
    {
        if (MinAge < 0)
        {
            throw new InvalidOperationException("MinAge cannot be less than zero.");
        }

        if (MaxAge < MinAge)
        {
            throw new InvalidOperationException("MaxAge cannot be less than MinAge.");
        }
    }
}
