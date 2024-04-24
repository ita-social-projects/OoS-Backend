﻿using System;
using System.Buffers;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Common.Validators;

/// <summary>
/// Validation attribute for phone number. It allows only digits and '+' sign, length must be between 7 and 15 symbols.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class CustomPhoneNumberAttribute : DataTypeAttribute
{
    public CustomPhoneNumberAttribute()
        : base(DataType.PhoneNumber)
    {
    }

    private static SearchValues<char> DigitSearchValues { get; } = SearchValues.Create("0123456789");

    /// <summary>
    /// Checks that the phone number is valid.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <returns><c>true</c> if valid, otherwise <c>false</c>.</returns>
    /// <exception cref="InvalidOperationException"> is thrown if the current attribute is ill-formed. Inherited from <see cref="DataTypeAttribute.IsValid(object)"/>.</exception>
    public override bool IsValid(object value)
    {
        // If value is required then consumer should use RequiredAttribute with this attribute
        if (value is null)
        {
            return true;
        }

        if (value is not string phoneNumber)
        {
            return false;
        }

        var phoneNumberSpan = phoneNumber.AsSpan();

        if (phoneNumberSpan is not ['+', .. var possibleDigits])
        {
            return false;
        }

        if (possibleDigits is { Length: < Constants.MinPhoneNumberLength or > Constants.MaxPhoneNumberLength })
        {
            return false;
        }

        if (possibleDigits.IndexOfAnyExcept(DigitSearchValues) != -1)
        {
            return false;
        }

        // base.IsValid - always returns true
        return base.IsValid(value);
    }
}