using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.StudySubjects;
public class StudySubjectCreateUpdateDto
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
}
