using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.BusinessLogic.Models.Workshops.Drafts;

public class WorkshopRequiredPropertiesDto : WorkshopMainRequiredPropertiesDto
{
    [Required(ErrorMessage = "Short stay is required")]
    public bool ShortStay { get; set; } = false;

    [Required(ErrorMessage = "Should be indicated if the Workshop operates with funds from parents or benefactors")]
    public bool IsSelfFinanced { get; set; } = false;

    [Required(ErrorMessage = "Property IsSpecial is required")]
    public bool IsSpecial { get; set; } = false;

    [EnumDataType(typeof(SpecialNeedsType), ErrorMessage = Constants.EnumErrorMessage)]
    public SpecialNeedsType SpecialNeedsType { get; set; } = SpecialNeedsType.None;

    [Required(ErrorMessage = "Property IsInclusive is required")]
    public bool IsInclusive { get; set; } = false;

    [Required(ErrorMessage = "Educational shift is required")]
    [EnumDataType(typeof(EducationalShift), ErrorMessage = Constants.EnumErrorMessage)]
    public EducationalShift EducationalShift { get; set; } = EducationalShift.First;

    [Required(ErrorMessage = "Language of education is required")]
    public uint LanguageOfEducationId { get; set; } = 0;

    [Required(ErrorMessage = "Type of age composition is required")]
    [EnumDataType(typeof(AgeComposition), ErrorMessage = Constants.EnumErrorMessage)]
    public AgeComposition AgeComposition { get; set; } = AgeComposition.SameAge;

    [Required(ErrorMessage = "Workshop type is required")]
    [EnumDataType(typeof(WorkshopType), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopType WorkshopType { get; set; }

    public Guid? ParentWorkshopId { get; set; }
}