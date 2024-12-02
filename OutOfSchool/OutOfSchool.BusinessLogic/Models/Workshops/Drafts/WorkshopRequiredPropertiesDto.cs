using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Workshops.Drafts;

public class WorkshopRequiredPropertiesDto : WorkshopMainRequiredPropertiesDto
{
    [Required(ErrorMessage = "Short stay is required")]
    public bool ShortStay { get; set; } = false;

    [Required(ErrorMessage = "Should be indicated if the Workshop operates with funds from parents or benefactors")]
    public bool IsSelfFinanced { get; set; } = false;

    [Required(ErrorMessage = "Property IsSpecial is required")]
    public bool IsSpecial { get; set; } = false;

    public uint SpecialNeedsId { get; set; } = 0;

    [Required(ErrorMessage = "Property IsInclusive is required")]
    public bool IsInclusive { get; set; } = false;

    [Required(ErrorMessage = "Educational shift is required")]
    public uint EducationalShiftId { get; set; } = 0;

    [Required(ErrorMessage = "Language of education is required")]
    public uint LanguageOfEducationId { get; set; } = 0;

    [Required(ErrorMessage = "Type of age composition is required")]
    public uint TypeOfAgeCompositionId { get; set; } = 0;

    [Required(ErrorMessage = "Educational disciplines is required")]
    public Guid EducationalDisciplines { get; set; } = Guid.Empty;

    [Required(ErrorMessage = "Category is required")]
    public uint CategoryId { get; set; } = 0;

    [Required(ErrorMessage = "GropeType is required")]
    public uint GropeTypeId { get; set; } = 0;

    public Guid? MemberOfWorkshopId { get; set; }
}