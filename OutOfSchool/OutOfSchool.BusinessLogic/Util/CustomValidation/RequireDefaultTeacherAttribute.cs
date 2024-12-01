using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class RequireDefaultTeacherAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is List<TeacherDraftCreateDto> teachers)
        {
            if (!teachers.Any())
            {
                return new ValidationResult("The list of teachers cannot be empty. At least one teacher is required.");
            }

            var defaultTeachersCount = teachers.Count(t => t.IsDefaultTeacher);

            if (defaultTeachersCount == 1)
            {
                return ValidationResult.Success;
            }
            else if (defaultTeachersCount == 0)
            {
                return new ValidationResult("At least one teacher must be marked as default.");
            }
            else
            {
                return new ValidationResult("Only one teacher can be marked as default.");
            }
        }

        return new ValidationResult("Invalid data format. Expected a list of teachers.");
    }
}
