using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;

[AttributeUsage(AttributeTargets.Property)]
public class ValidateSpecialNeedsAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var type = context.ObjectInstance.GetType();
        
        var isSpecialProperty = type.GetProperty("IsSpecial");
        var specialNeedsTypeProperty = type.GetProperty("SpecialNeedsType");

        if (isSpecialProperty == null || specialNeedsTypeProperty == null)
        {
            throw new InvalidOperationException(
                "ValidateSpecialNeeds attribute can only be used on types with both 'IsSpecial' and 'SpecialNeedsType' properties.");
        }

        var isSpecialValue = isSpecialProperty.GetValue(context.ObjectInstance);
        var specialNeedsTypeValue = specialNeedsTypeProperty.GetValue(context.ObjectInstance);

        if (isSpecialValue is not bool isSpecial)
        {
            throw new InvalidOperationException("IsSpecial property must be of type bool.");
        }

        if (specialNeedsTypeValue is not SpecialNeedsType specialNeedsType)
        {
            throw new InvalidOperationException("SpecialNeedsType property must be of type SpecialNeedsType enum.");
        }

        if (isSpecial && specialNeedsType == SpecialNeedsType.None)
        {
            return new ValidationResult(
                "Special needs type must be specified when IsSpecial is true",
                [specialNeedsTypeProperty.Name]);
        }

        return ValidationResult.Success;
    }
}