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
    public bool IsPrimaryLanguageUkrainian { get; set; }

    /// <summary>
    /// Language of instruction (allows multiple selection)
    /// </summary>
    [Required(ErrorMessage = "The language of instruction is required.")]
    public List<StudySubjectCreateUpdateLanguage> Languages { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Languages == null || !Languages.Any())
        {
            yield return new ValidationResult("Languages cannot be null or empty.", new[] { nameof(Languages) });
        }
        else
        {
            if (Languages.Count(l => l.IsPrimary) != 1)
                yield return new ValidationResult("Languages must contain primary language.", new[] { nameof(Languages) });

            if (Languages.Count() != Languages.Distinct().Count())
                yield return new ValidationResult("Languages cannot contain duplicates.", new[] { nameof(Languages) });

            if (Languages.Any(l => l.Id <= 0))
                yield return new ValidationResult("Languages cannot contain values smaller than 1.", new[] { nameof(Languages) });
        }
    }
}
