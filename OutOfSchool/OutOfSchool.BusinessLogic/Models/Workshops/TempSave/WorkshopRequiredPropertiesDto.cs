using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.BusinessLogic.Models.Workshops.TempSave;

public class WorkshopRequiredPropertiesDto : WorkshopMainRequiredPropertiesDto
{
    [Required(ErrorMessage = "Should be indicated if the Workshop operates with funds from parents or benefactors")]
    public bool IsSelfFinanced { get; set; } = false;

    [EnumDataType(typeof(SpecialNeedsType), ErrorMessage = Constants.EnumErrorMessage)]
    public SpecialNeedsType SpecialNeedsType { get; set; } = SpecialNeedsType.None;

    [Required(ErrorMessage = "Property IsInclusive is required")]
    public bool IsInclusive { get; set; } = false;

    [Required(ErrorMessage = "Educational shift is required")]
    [EnumDataType(typeof(EducationalShift), ErrorMessage = Constants.EnumErrorMessage)]
    public EducationalShift EducationalShift { get; set; } = EducationalShift.First;

    [Required(ErrorMessage = "Type of age composition is required")]
    [EnumDataType(typeof(AgeComposition), ErrorMessage = Constants.EnumErrorMessage)]
    public AgeComposition AgeComposition { get; set; } = AgeComposition.SameAge;
    [EnumDataType(typeof(WorkshopType), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopType WorkshopType { get; set; } = WorkshopType.Workshop;

    public Guid? ParentWorkshopId { get; set; }

    [Required(ErrorMessage = "Property IsPaid is required")]
    public bool IsPaid { get; set; } = false;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    [RequiredIf(nameof(IsPaid), true, ErrorMessage = "Price is required")]
    public decimal? Price { get; set; } = default;

    [EnumDataType(typeof(PayRateType), ErrorMessage = Constants.EnumErrorMessage)]
    [RequiredIf(nameof(IsPaid), true, ErrorMessage = "PayRate is required")]
    public PayRateType? PayRate { get; set; } = PayRateType.Classes;

    public bool AreThereBenefits { get; set; } = default;

    [MaxLength(500)]
    [RequiredIf(nameof(AreThereBenefits), true, ErrorMessage = "PreferentialTermsOfParticipation is required")]
    public string PreferentialTermsOfParticipation { get; set; }
}