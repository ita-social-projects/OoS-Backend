using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;
public class RequireDefaultTeacherAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is List<TeacherDraftCreateDto> teachers && teachers.Any(t => t.IsDefaultTeacher))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("At least one teacher must be marked as default.");
    }
}
