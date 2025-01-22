using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DateOrderAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is WorkshopDraftBaseDto dto && dto.ActiveFrom > dto.ActiveTo)
        {
            return new ValidationResult("ActiveFrom cannot be later than ActiveTo.");
        }

        return ValidationResult.Success;
    }
}
