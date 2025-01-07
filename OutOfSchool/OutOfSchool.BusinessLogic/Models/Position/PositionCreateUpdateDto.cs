using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Position;

public class PositionCreateUpdateDto
{
    [Required]
    [MaxLength(30)]
    public string Language { get; set; }

    [MaxLength(Constants.MaxPositionDescriptionLength)]
    public string Description { get; set; }

    [Required]
    [MaxLength(60)]
    public string Department { get; set; }

    [Required]
    public int SeatsAmount { get; set; }

    [Required]
    [MaxLength(Constants.NameMaxLength)]
    public string FullName { get; set; }

    [MaxLength(Constants.NameMaxLength)]
    public string ShortName { get; set; }

    [Required]
    [MaxLength(Constants.NameMaxLength)]
    public string GenitiveName { get; set; }

    public bool IsTeachingPosition { get; set; }

    [Required]
    public float Rate { get; set; }

    [Required]
    public float Tariff { get; set; }

    [Required]
    [MaxLength(60)]
    public string ClassifierType { get; set; }

    public bool IsForRuralAreas { get; set; }
}