using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;

public class SportsSectionUpdateRequest : SportsSectionBaseDto
{
    [Required]
    public Guid SectionId { get; set; }
}