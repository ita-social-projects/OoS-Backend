using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.Directions;

public class SubDirectionsInfoDto : IExternalInfo<Guid>
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }
    
    public List<long> DirectionIds { get; set; }
}