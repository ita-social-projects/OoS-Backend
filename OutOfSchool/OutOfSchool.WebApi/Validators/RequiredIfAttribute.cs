using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Validators;

public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string otherProperty;
    private readonly bool requiredValue;

    public RequiredIfAttribute(string otherProperty, bool requiredValue)
    {
        this.otherProperty = otherProperty;
        this.requiredValue = requiredValue;
    }

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
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}

