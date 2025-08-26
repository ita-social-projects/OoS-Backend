using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOfSchool.SportsRegistryApiClient.Validators;
public class PhoneListValidationAttribute : ValidationAttribute
{
    override protected ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<string> phoneNumbers)
        {
            return new ValidationResult("sectionPhone must be a list of phone numbers");
        }

        var invalidPhones = phoneNumbers
           .Where(p => !Regex.IsMatch(p, @"^\d{10,12}$"))
           .ToList();

        if (invalidPhones.Any())
        { 
            return new ValidationResult($"Invalid phone numbers format: {string.Join(", ", invalidPhones)}. Phone numbers must be between 10 and 12 digits long and contain only digits.");
        }
        return ValidationResult.Success;
    }
}
