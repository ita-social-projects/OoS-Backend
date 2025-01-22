using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;

/// <summary>
/// Custom validation attribute for validating DateTimeRangeDraftDto items.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CollectionValidationAttribute : ValidationAttribute
{
    public Type ValidatorType { get; }
    public string ValidationMethodName { get; }

    public CollectionValidationAttribute(Type validatorType, string validationMethodName)
    {
        ValidatorType = validatorType;
        ValidationMethodName = validationMethodName;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {     
        if (value is not IEnumerable collection)
        {
            return new ValidationResult(ErrorMessage ?? "Invalid type. The property must be a collection.");
        }

        var validator = validationContext.GetService(ValidatorType) ?? Activator.CreateInstance(ValidatorType);

        if (validator == null)
        {
            throw new InvalidOperationException($"Cannot instantiate or find service of type {ValidatorType.FullName}");
        }

        var validationMethod = ValidatorType.GetMethod(ValidationMethodName);

        if (validationMethod == null)
        {
            throw new InvalidOperationException($"Method '{ValidationMethodName}' not found in {ValidatorType.FullName}");
        }

        var validationErrors = new List<string>();

        foreach (var item in collection)
        {
            var result = validationMethod.Invoke(validator, new[] { item }) as IEnumerable<ValidationResult>;

            if (result != null)
            {
                validationErrors.AddRange(result.Select(e => e.ErrorMessage));
            }
        }

        if (validationErrors.Any())
        {
            return new ValidationResult(string.Join("; ", validationErrors));
        }

        return ValidationResult.Success;
    }
}
