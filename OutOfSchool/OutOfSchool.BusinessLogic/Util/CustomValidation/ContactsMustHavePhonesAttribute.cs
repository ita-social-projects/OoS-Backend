using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;

/// <summary>
/// Validates that each contact in the list has at least one phone number.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ContactsMustHavePhonesAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null)
        {
            return true; // Let Required attribute handle null checks
        }

        if (value is not IEnumerable<ContactsDto> contacts)
        {
            return false;
        }

        foreach (var contact in contacts)
        {
            if (contact.Phones == null || contact.Phones.Count == 0)
            {
                return false;
            }
        }

        return true;
    }

    public override string FormatErrorMessage(string name)
    {
        return ErrorMessage ?? $"Each contact in {name} must have at least one phone number.";
    }
}

