using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Validators;

public class RequiredIfMinAndMaxLengthAttributes
{
    /// <summary>
    /// An attribute to conditionally set the minimum length of a property when the required property has a certain value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredIfMinLengthAttribute(string otherProperty, bool requiredValue, int minLength) : ValidationAttribute
    {
        private readonly string otherProperty = otherProperty;
        private readonly bool requiredValue = requiredValue;
        private readonly int minLength = minLength;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyInfo = validationContext.ObjectType.GetProperty(otherProperty);

            if (propertyInfo == null)
            {
                return new ValidationResult($"Property {otherProperty} not found.");
            }

            var otherPropertyValue = propertyInfo.GetValue(validationContext.ObjectInstance);

            if ((otherPropertyValue is bool otherValue) && otherValue == requiredValue)
            {
                var result = value switch
                {
                    null => new ValidationResult(ErrorMessage ?? $"This field is required when {otherProperty} = {requiredValue}."),
                    string str when string.IsNullOrWhiteSpace(str) => new ValidationResult(ErrorMessage ?? $"This field is required when {otherProperty} = {requiredValue}."),
                    string str when str.Length < minLength => new ValidationResult(ErrorMessage ?? $"The field must be at least {minLength} characters long when {otherProperty} = {requiredValue}."),
                    ICollection collection when collection.Count < minLength => new ValidationResult(ErrorMessage ?? $"The collection must contain at least {minLength} items when {otherProperty} = {requiredValue}."),
                    _ => ValidationResult.Success
                };

                return result;
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// An attribute to conditionally set the maximum length of a property when the required property has a certain value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredIfMaxLengthAttribute(string otherProperty, bool requiredValue, int maxLength) : ValidationAttribute
    {
        private readonly string otherProperty = otherProperty;
        private readonly bool requiredValue = requiredValue;
        private readonly int maxLength = maxLength;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyInfo = validationContext.ObjectType.GetProperty(otherProperty);

            if (propertyInfo == null)
            {
                return new ValidationResult($"Property {otherProperty} not found.");
            }

            var otherPropertyValue = propertyInfo.GetValue(validationContext.ObjectInstance);

            if ((otherPropertyValue is bool otherValue) && otherValue == requiredValue)
            {
                var result = value switch
                {
                    null => new ValidationResult(ErrorMessage ?? $"This field is required when {otherProperty} = {requiredValue}."),
                    string str when string.IsNullOrWhiteSpace(str) => new ValidationResult(ErrorMessage ?? $"This field is required when {otherProperty} = {requiredValue}."),
                    string str when str.Length > maxLength => new ValidationResult(ErrorMessage ?? $"The field must not be greater than {maxLength} characters long when {otherProperty} = {requiredValue}."),
                    ICollection collection when collection.Count > maxLength => new ValidationResult(ErrorMessage ?? $"The collection must not contain greater than {maxLength} items when {otherProperty} = {requiredValue}."),
                    _ => ValidationResult.Success
                };

                return result;
            }

            return ValidationResult.Success;
        }
    }
}
