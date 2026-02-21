using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Common.Validators;

public static class ConfigurationValidationHelper
{
    /// <summary>
    /// Validates the provided configuration object using data annotations.
    /// </summary>
    /// <typeparam name="T">The type of configuration object to validate.</typeparam>
    /// <param name="instance">The configuration instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when instance is null.</exception>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public static void ValidateConfigurationObject<T>(T instance) where T : class
    {
        var validationContext = new ValidationContext(instance);
        Validator.ValidateObject(instance, validationContext, true);
    }
}