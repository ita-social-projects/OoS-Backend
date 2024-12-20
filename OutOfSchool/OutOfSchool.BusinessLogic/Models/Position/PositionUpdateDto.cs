using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Position;
public class PositionUpdateDto
{
    [MaxLength(30)]
    public string Language { get; set; }

    [MaxLength(Constants.MaxPositionDescriptionLength)]
    public string Description { get; set; }

    [MaxLength(60)]
    public string Department { get; set; }

    public int? SeatsAmount { get; set; }

    [MaxLength(Constants.NameMaxLength)]
    public string FullName { get; set; }

    [MaxLength(Constants.NameMaxLength)]
    public string ShortName { get; set; }

    [MaxLength(Constants.NameMaxLength)]
    public string GenitiveName { get; set; }

    public bool? IsTeachingPosition { get; set; }

    public float? Rate { get; set; }

    public float? Tariff { get; set; }

    [MaxLength(60)]
    public string ClassifierType { get; set; }

    public bool? IsForRuralAreas { get; set; }
}