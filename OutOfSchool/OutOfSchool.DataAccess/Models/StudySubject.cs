using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

/// <summary>
/// Represents a subject in the educational system.
/// </summary>
public class StudySubject : BusinessEntity
{
    /// <summary>
    /// Name in Ukrainian
    /// </summary>
    [Required(ErrorMessage = "The name in Ukrainian is required.")]
    [MinLength(1, ErrorMessage = "The name in Ukrainian must be at least 1 character.")]
    [MaxLength(100, ErrorMessage = "The name in Ukrainian can't exceed 100 characters.")]
    public string NameInUkrainian { get; set; }

    /// <summary>
    /// Name in the language of instruction
    /// </summary>
    [Required(ErrorMessage = "The name in the language of instruction is required.")]
    [MinLength(1, ErrorMessage = "The name in the language of instruction must be at least 1 character.")]
    [MaxLength(100, ErrorMessage = "The name in the language of instruction can't exceed 100 characters.")]
    public string NameInInstructionLanguage { get; set; }

    /// <summary>
    /// Property determines if the primary language is Ukrainian
    /// </summary>
    [Required(ErrorMessage = "It's required to know if primary languge is Ukrainian.")]
    public bool IsLanguageUkrainian { get; set; }

    /// <summary>
    /// Primary language of the subject
    /// </summary>
    [Required(ErrorMessage = "The primary language's ID is required.")]
    public long LanguageId { get; set; }
    public virtual Language Language { get; set; }
}

