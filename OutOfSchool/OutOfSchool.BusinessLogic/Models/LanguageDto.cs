using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;
public class LanguageDto
{
    public int Id { get; set; }

    /// <summary>
    /// ISO code of the language
    /// </summary>
    [Required(ErrorMessage = "The ISO code is required.")]
    [RegularExpression(@"^[a-zA-Z]{2,3}$", ErrorMessage = "The ISO code must be 2 or 3 alphabetic characters.")]
    public string Code { get; set; }

    /// <summary>
    /// Title of the language
    /// </summary>
    [Required(ErrorMessage = "The title is required.")]
    [MinLength(1, ErrorMessage = "The title must be at least 1 character.")]
    [MaxLength(50, ErrorMessage = "The title of instruction can't exceed 50 characters.")]
    public string Title { get; set; }
}
