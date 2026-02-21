using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

/// <summary>
/// Represents a language used in the educational system.
/// </summary>
public class Language : IKeyedEntity<long>
{
    public long Id { get; set; }

    /// <summary>
    /// ISO code of the language
    /// </summary>
    [Required(ErrorMessage = "The ISO code is required.")]
    [RegularExpression(@"^[a-zA-Z]{2,3}$", ErrorMessage = "The ISO code must be 2 or 3 alphabetic characters.")]
    public string Code { get; set; }

    /// <summary>
    /// Name of the language
    /// </summary>
    [Required(ErrorMessage = "The name is required.")]
    [MinLength(1, ErrorMessage = "The name must be at least 1 character.")]
    [MaxLength(50, ErrorMessage = "The name of instruction can't exceed 50 characters.")]
    public string Name { get; set; }
}

