using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Individual;

public class UploadEmployeeRequestDto
{
    [Required(ErrorMessage = "FirstName is required")]
    [MinLength(Constants.MinIndividualNameLength)]
    [MaxLength(Constants.MaxIndividualNameLength)]
    [RegularExpression("^[а-щА-ЩЬьЮюЯяЇїІіЄєҐґ'-]+$", ErrorMessage = "FirstName is not valid. It contains invalid characters.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "MiddleName is required")]
    [MinLength(Constants.MinIndividualNameLength)]
    [MaxLength(Constants.MaxIndividualNameLength)]
    [RegularExpression("^[а-щА-ЩЬьЮюЯяЇїІіЄєҐґ'-]+$", ErrorMessage = "MiddleName is not valid. It contains invalid characters.")]
    public string MiddleName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    [MinLength(Constants.MinIndividualNameLength)]
    [MaxLength(Constants.MaxIndividualNameLength)]
    [RegularExpression("^[а-щА-ЩЬьЮюЯяЇїІіЄєҐґ'-]+$", ErrorMessage = "LastName is not valid. It contains invalid characters.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Rnokpp is required")]
    [RegularExpression("[0-9]{10}|[АБВГДЕЄЖЗИІКЛМНОПРСТУФХЦЧШЩЮЯ]{2}[0-9]{6}|[0-9]{9}", ErrorMessage = "Rnokpp is not valid")]
    public string Rnokpp { get; set; }

    [Required(ErrorMessage = "AssignedRole is required")]
    [MaxLength(60)]
    [RegularExpression("^[-а-щА-ЩЬьЮюЯяЇїІіЄєҐґ'0-9 ]+$", ErrorMessage = "AssignedRole is not valid. It contains invalid characters.")]
    public string AssignedRole { get; set; }
}
