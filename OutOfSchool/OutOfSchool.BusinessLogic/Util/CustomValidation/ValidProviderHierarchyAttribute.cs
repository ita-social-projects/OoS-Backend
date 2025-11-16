using OutOfSchool.BusinessLogic.Models.Providers;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;
public class ValidProviderHierarchyAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is ProviderBaseDto dto)
        {
            if (dto.IsStructuralUnit && !dto.ParentProviderId.HasValue)
            {
                return new ValidationResult("Branch must have a parent provider.");
            }

            if (!dto.IsStructuralUnit && dto.ParentProviderId.HasValue)
            {
                return new ValidationResult("Main organization cannot have a parent.");
            }
        }
        return ValidationResult.Success!;
    }
}
