using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Individual;

public class UploadEmployeeRequestDto
{
    [Required(ErrorMessage = "FirstName is required")]
    [MinLength(Constants.MinIndividualNameLength)]
    [MaxLength(Constants.MaxIndividualNameLength)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "MiddleName is required")]
    [MinLength(Constants.MinIndividualNameLength)]
    [MaxLength(Constants.MaxIndividualNameLength)]
    public string MiddleName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    [MinLength(Constants.MinIndividualNameLength)]
    [MaxLength(Constants.MaxIndividualNameLength)]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Rnokpp is required")]
    public string Rnokpp { get; set; }

    [Required(ErrorMessage = "AssignedRole is required")]
    [MaxLength(60)]
    public string AssignedRole { get; set; }
}
