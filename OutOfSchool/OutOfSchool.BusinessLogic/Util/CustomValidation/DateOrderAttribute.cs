using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;
public class DateOrderAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is WorkshopDraftBaseDto dto)
        {
            if (dto.ActiveFrom > dto.ActiveTo)
            {
                return new ValidationResult("ActiveFrom cannot be later than ActiveTo.");
            }
        }

        return ValidationResult.Success;
    }
}
