using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Extensions;

namespace OutOfSchool.Common.Validators;

/// <summary>
/// Validation attribute for Ukrainian name.
/// </summary>
public class CustomUkrainianNameAttribute : DataTypeAttribute
{
    public CustomUkrainianNameAttribute()
        : base(DataType.Text)
    {
    }

    /// <summary>
    /// Checks that the value is valid Ukrainian name.
    /// </summary>
    /// <remarks>
    /// This method returns <c>true</c> if the <paramref name="value" /> is null or empty.
    /// It is assumed the <see cref="RequiredAttribute" /> is used if the value may not be null or empty.
    /// </remarks>
    /// <param name="value">Value to validate.</param>
    /// <returns> <c>true</c> if valid, otherwise <c>false</c>.</returns>
    public override bool IsValid(object value) => value switch
    {
        null => true,
        string { Length: 0 } => true,
        string str => str.AsSpan().IsUkrainianName(),
        _ => false,
    };
}
