using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.StudySubjects;
public class StudySubjectCreateUpdateDto : IValidatableObject
{
    public Guid Id { get; set; }

    /// <summary>
    /// Name in Ukrainian
    /// </summary>
    [Required(ErrorMessage = "The name in Ukrainian is required.")]
    public string NameInUkrainian { get; set; }

    /// <summary>
    /// Name in the language of instruction
    /// </summary>
    [Required(ErrorMessage = "The name in the language of instruction is required.")]
    public string NameInInstructionLanguage { get; set; }

    [Required(ErrorMessage = "It's required to know if primary languge is Ukrainian.")]
    public bool IsLanguageUkrainian { get; set; }

    /// <summary>
    /// Primary language of the subject
    /// </summary>
    [Required(ErrorMessage = "The primary language is required.")]
    public LanguageDto Language { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Language == null)
        {
            yield return new ValidationResult("Language cannot be null.", new[] { nameof(Language) });
        }
        else if (Language.Id <= 0)
        {
            yield return new ValidationResult("Language ID must be greater than 0.", new[] { nameof(Language.Id) });
        }
    }
}
