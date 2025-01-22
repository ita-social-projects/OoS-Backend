using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.Directions;

public class DirectionInfoDto : IExternalInfo<long>
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }

    [MaxLength(500)]
    [DataType(DataType.Text)]
    public string Description { get; set; } = string.Empty;
}