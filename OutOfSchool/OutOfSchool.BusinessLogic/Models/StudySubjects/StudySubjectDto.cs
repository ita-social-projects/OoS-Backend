using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;
public class StudySubjectDto
{
    public int Id { get; set; }
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

    /// <summary>
    /// Language of instruction (allows multiple selection)
    /// </summary>
    [Required(ErrorMessage = "The language of instruction is required.")]
    public List<Language> Languages { get; set; }
}
