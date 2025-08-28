using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.SportsRegistryApiClient.Validators;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PhoneListValidationAttribute : ValidationAttribute
{
    override protected ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<string> phoneNumbers)
        {
            return new ValidationResult("sectionPhone must be a list of phone numbers");
        }

        var invalidPhones = phoneNumbers
           .Where(p => p.Length < 10 || p.Length > 12 || p.Any(c => !char.IsDigit(c)))
           .ToList();

        if (invalidPhones.Any())
        { 
            return new ValidationResult($"Invalid phone numbers format: {string.Join(", ", invalidPhones)}. Phone numbers must be between 10 and 12 digits long and contain only digits.");
        }
        return ValidationResult.Success;
    }
}
