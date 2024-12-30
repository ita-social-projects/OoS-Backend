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
    public List<long> LanguageIds { get; set; }

    /// <summary>
    /// Primary language of the subject
    /// </summary>
    [Required(ErrorMessage = "Primary language's id is required.")]
    public int PrimaryLanguageId { get; set; }

    /// <summary>
    /// Id of the related workshop
    /// </summary>
    [Required(ErrorMessage = "The workshop's id is required.")]
    public Guid WorkshopId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PrimaryLanguageId <= 0)
        {
            yield return new ValidationResult("PrimaryLanguageId cannot be smaller than one.", new[] { nameof(PrimaryLanguageId) });
        }

        if (LanguageIds == null || !LanguageIds.Any())
        {
            yield return new ValidationResult("LanguageIds cannot be null or empty.", new[] { nameof(LanguageIds) });
        }
        else
        {
            if (!LanguageIds.Contains(PrimaryLanguageId))
                yield return new ValidationResult("LanguageIds must contain PrimaryLanguageId.", new[] { nameof(LanguageIds) });

            if (LanguageIds.Count() != LanguageIds.Distinct().Count())
                yield return new ValidationResult("LanguageIds cannot contain duplicates.", new[] { nameof(LanguageIds) });

            if (LanguageIds.Any(id => id <= 0))
                yield return new ValidationResult("LanguageIds cannot contain values smaller than 1.", new[] { nameof(LanguageIds) });
        }
    }
}
