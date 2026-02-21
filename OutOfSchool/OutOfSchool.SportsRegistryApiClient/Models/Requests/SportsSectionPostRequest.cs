using System.ComponentModel.DataAnnotations;
namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;

public class SportsSectionPostRequest : SportsSectionBaseDto
{
    [Required(ErrorMessage = "organizationCode is required.")]
    [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "Sport organization code must be exactly 8 digits.")]
    public string OrganizationCode { get; set; } = null!;
}